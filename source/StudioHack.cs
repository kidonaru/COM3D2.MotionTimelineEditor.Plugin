using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public static class StudioHack
    {
        public static PoseEditWindow poseEditWindow = null;
        public static MotionWindow motionWindow = null;
        public static PhotoWindowManager photoManager = null;
        public static WindowPartsBoneCheckBox bodyBoneCheckBox = null;
        public static Dictionary<IKManager.BoneType, WFCheckBox> boneCheckBoxMap = new Dictionary<IKManager.BoneType, WFCheckBox>(78);

        public static FieldInfo fieldAnimeState = null;
        public static FieldInfo fieldLimbControlList = null;
        public static FieldInfo fieldJointDragPoint = null;
        public static FieldInfo fieldTipDragPoint = null;
        public static FieldInfo fieldPathDic = null;
        public static FieldInfo fieldDataDic = null;
        public static FieldInfo fieldBoneDic = null;
        public static FieldInfo fieldIkboxVisibleDic = null;

        private static string[] _saveBonePaths = null;
        public static string[] saveBonePaths
        {
            get
            {
                if (_saveBonePaths == null)
                {
                    var methodInfo = typeof(CacheBoneDataArray).GetMethod("GetSaveBonePathArray", BindingFlags.NonPublic | BindingFlags.Static);
                    _saveBonePaths = (string[]) methodInfo.Invoke(null, null);
                    Extensions.AssertNull(saveBonePaths != null);
                }
                return _saveBonePaths;
            }
        }

        public static WFCheckBox ikBoxVisibleRoot = null;
        public static WFCheckBox ikBoxVisibleBody = null;

        public static bool isIkBoxVisibleRoot
        {
            get
            {
                return ikBoxVisibleRoot.check;
            }
            set
            {
                ikBoxVisibleRoot.check = value;
                poseEditWindow.OnIKBOXVixibleCheckBox(ikBoxVisibleRoot);
            }
        }

        public static bool isIkBoxVisibleBody
        {
            get
            {
                return ikBoxVisibleBody.check;
            }
            set
            {
                ikBoxVisibleBody.check = value;
                poseEditWindow.OnIKBOXVixibleCheckBox(ikBoxVisibleBody);
            }
        }

        public static bool isPoseEditing
        {
            get
            {
                return poseEditWindow.CheckbtnUse.check;
            }
            set
            {
                if (value == isPoseEditing)
                {
                    return;
                }

                if (value && isMotionPlaying)
                {
                    motionWindow.CheckbtnStop.check = true;
                    motionWindow.OnClickStopCheckRun(true);
                }

                poseEditWindow.CheckbtnUse.check = value;
                poseEditWindow.OnClickUseCheckRun(value);
            }
        }

        public static bool isMotionPlaying
        {
            get
            {
                return !motionWindow.CheckbtnStop.check;
            }
            set
            {
                if (value == isMotionPlaying)
                {
                    return;
                }

                if (value && isPoseEditing)
                {
                    poseEditWindow.CheckbtnUse.check = false;
                    poseEditWindow.OnClickUseCheckRun(false);
                }

                motionWindow.CheckbtnStop.check = !value;
                motionWindow.OnClickStopCheckRun(!value);
            }
        }

        public static Maid maid
        {
            get
            {
                return photoManager.select_maid;
            }
        }

        public static Dictionary<string, string> maidStoreData
        {
            get
            {
                return poseEditWindow.GetMaidStoreData(maid);
            }
        }

        public static CameraMain mainCamera
        {
            get
            {
                return GameMain.Instance.MainCamera;
            }
        }

        public static AnimationState animeState
        {
            get
            {
                AnimationState result = null;
                if (fieldAnimeState != null)
                {
                    result = (AnimationState) fieldAnimeState.GetValue(motionWindow);
                }
                return result;
            }
        }

        public static float anmSpeed
        {
            get
            {
                return animeState != null ? animeState.speed : 1f;
            }
            set
            {
                if (animeState != null)
                {
                    animeState.speed = value;
                }
            }
        }

        public static IKManager ikManager
        {
            get
            {
                return poseEditWindow.ik_mgr;
            }
        }

        public static float motionSliderRate
        {
            get
            {
                var current = motionWindow.Slider.value;
                var maxNum = motionWindow.Slider.MaxNum;
                return current / maxNum;
            }
            set
            {
                var maxNum = motionWindow.Slider.MaxNum;
                var current = Mathf.Clamp01(value) * maxNum;
                motionWindow.Slider.value = current;
            }
        }

        public static List<LimbControl> limbControlList
        {
            get
            {
                if (fieldLimbControlList != null)
                {
                    return (List<LimbControl>) fieldLimbControlList.GetValue(ikManager);
                }
                return new List<LimbControl>();
            }
        }

        public static bool useMuneKeyL
        {
            get
            {
                if (bodyBoneCheckBox != null)
                {
                    return bodyBoneCheckBox.CheckBoxMuneLPhysics.check;
                }
                return false;
            }
            set
            {
                if (bodyBoneCheckBox != null)
                {
                    bodyBoneCheckBox.CheckBoxMuneLPhysics.check = value;
                    bodyBoneCheckBox.OnClickCheckBoxMunePhysics(bodyBoneCheckBox.CheckBoxMuneLPhysics);
                }
            }
        }

        public static bool useMuneKeyR
        {
            get
            {
                if (bodyBoneCheckBox != null)
                {
                    return bodyBoneCheckBox.CheckBoxMuneRPhysics.check;
                }
                return false;
            }
            set
            {
                if (bodyBoneCheckBox != null)
                {
                    bodyBoneCheckBox.CheckBoxMuneRPhysics.check = value;
                    bodyBoneCheckBox.OnClickCheckBoxMunePhysics(bodyBoneCheckBox.CheckBoxMuneRPhysics);
                }
            }
        }

        public static Dictionary<IKManager.BoneType, KeyValuePair<IKManager.BoneSetType, GameObject>> boneDic
        {
            get
            {
                if (fieldBoneDic != null)
                {
                    return (Dictionary<IKManager.BoneType, KeyValuePair<IKManager.BoneSetType, GameObject>>) fieldBoneDic.GetValue(ikManager);
                }
                return new Dictionary<IKManager.BoneType, KeyValuePair<IKManager.BoneSetType, GameObject>>();
            }
        }

        public static LimbControl GetLimbControl(LimbControl.Type type)
        {
            return limbControlList.Find(l => l.type == type);
        }

        public static IKDragPoint GetDragPoint(LimbControl.Type type, FieldInfo field)
        {
            var limbControl = GetLimbControl(type);
            if (limbControl != null)
            {
                return (IKDragPoint) field.GetValue(limbControl);
            }
            return null;
        }

        public static IKDragPoint GetDragPoint(IKHoldType type)
        {
            switch (type)
            {
                case IKHoldType.Arm_R_Joint:
                    return GetDragPoint(LimbControl.Type.Arm_R, fieldJointDragPoint);
                case IKHoldType.Arm_R_Tip:
                    return GetDragPoint(LimbControl.Type.Arm_R, fieldTipDragPoint);
                case IKHoldType.Arm_L_Joint:
                    return GetDragPoint(LimbControl.Type.Arm_L, fieldJointDragPoint);
                case IKHoldType.Arm_L_Tip:
                    return GetDragPoint(LimbControl.Type.Arm_L, fieldTipDragPoint);
                case IKHoldType.Foot_R_Joint:
                    return GetDragPoint(LimbControl.Type.Foot_R, fieldJointDragPoint);
                case IKHoldType.Foot_R_Tip:
                    return GetDragPoint(LimbControl.Type.Foot_R, fieldTipDragPoint);
                case IKHoldType.Foot_L_Joint:
                    return GetDragPoint(LimbControl.Type.Foot_L, fieldJointDragPoint);
                case IKHoldType.Foot_L_Tip:
                    return GetDragPoint(LimbControl.Type.Foot_L, fieldTipDragPoint);
            }
            return null;
        }

        public static Dictionary<string, CacheBoneDataArray.BoneData> GetBonePathDic(
            CacheBoneDataArray cacheBoneDataArray)
        {
            if (fieldPathDic != null)
            {
                return (Dictionary<string, CacheBoneDataArray.BoneData>) fieldPathDic.GetValue(cacheBoneDataArray);
            }
            return null;
        }

        public static bool HasBoneRotateVisible(IKManager.BoneType boneType)
        {
            return boneCheckBoxMap.ContainsKey(boneType);
        }

        public static bool IsBoneRotateVisible(IKManager.BoneType boneType)
        {
            var maidStoreData = StudioHack.maidStoreData;
            string value;
            if (maidStoreData.TryGetValue("rotate_visible_" + boneType.ToString(), out value))
            {
                return bool.Parse(value);
            }
            return false;
        }

        public static void SetBoneRotateVisible(IKManager.BoneType boneType, bool visible)
        {
            if (isPoseEditing)
            {
                WFCheckBox checkBox;
                if (boneCheckBoxMap.TryGetValue(boneType, out checkBox))
                {
                    if (checkBox.check == visible)
                    {
                        return;
                    }

                    checkBox.check = visible;
                    foreach (var action in checkBox.onClick)
                    {
                        action(checkBox);
                    }
                }
            }
            else
            {
                var maidStoreData = StudioHack.maidStoreData;
                maidStoreData["rotate_visible_" + boneType.ToString()] = visible.ToString();
            }
        }

        public static void ClearBoneRotateVisible()
        {
            foreach (var pair in boneCheckBoxMap)
            {
                if (pair.Value.check)
                {
                    pair.Value.check = false;
                    foreach (var action in pair.Value.onClick)
                    {
                        action(pair.Value);
                    }
                }
            }
        }

        public static void SelectBoneRotateVisible(IKManager.BoneType boneType, bool isMultiSelect)
        {
            if (!isMultiSelect)
            {
                ClearBoneRotateVisible();
            }

            SetBoneRotateVisible(boneType, true);
        }

        public static void UIHide()
        {
            var methodInfo = typeof(CameraMain).GetMethod("UIHide", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(mainCamera, null);
        }

        public static void UIResume()
        {
            var methodInfo = typeof(CameraMain).GetMethod("UIResume", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(mainCamera, null);
        }

        public static void Init()
        {
            Extensions.Log("StudioHack初期化中...");
            {
                var gameObject = GameObject.Find("PoseEditWindow");
                poseEditWindow = gameObject.GetComponent<PoseEditWindow>();
                Extensions.AssertNull(poseEditWindow != null);
            }

            {
                var gameObject = GameObject.Find("MotionWindow");
                motionWindow = gameObject.GetComponent<MotionWindow>();
                Extensions.AssertNull(motionWindow != null);
            }

            photoManager = poseEditWindow.mgr;
            Extensions.AssertNull(photoManager != null);

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldAnimeState = typeof(MotionWindow).GetField("anime_state_", bindingAttr);
                Extensions.AssertNull(fieldAnimeState != null);

                fieldLimbControlList = typeof(IKManager).GetField("limb_control_list_", bindingAttr);
                Extensions.AssertNull(fieldLimbControlList != null);

                fieldJointDragPoint = typeof(LimbControl).GetField("joint_drag_point_", bindingAttr);
                Extensions.AssertNull(fieldJointDragPoint != null);

                fieldTipDragPoint = typeof(LimbControl).GetField("tip_drag_point_", bindingAttr);
                Extensions.AssertNull(fieldTipDragPoint != null);

                fieldPathDic = typeof(CacheBoneDataArray).GetField("path_dic_", bindingAttr);
                Extensions.AssertNull(fieldPathDic != null);

                fieldDataDic = typeof(WindowPartsBoneCheckBox).GetField("data_dic_", bindingAttr);
                Extensions.AssertNull(fieldDataDic != null);

                fieldBoneDic = typeof(IKManager).GetField("bone_dic_", bindingAttr);
                Extensions.AssertNull(fieldBoneDic != null);

                fieldIkboxVisibleDic = typeof(PoseEditWindow).GetField("ikbox_visible_dic_", bindingAttr);
                Extensions.AssertNull(fieldIkboxVisibleDic != null);
            }

            foreach (var boneCheckBox in poseEditWindow.RotateCheckBoxArray)
            {
                if (boneCheckBox == null)
                {
                    continue;
                }

                if (boneCheckBox.BoneSetType == IKManager.BoneSetType.Body)
                {
                    bodyBoneCheckBox = boneCheckBox;
                }

                var dataDic = (Dictionary<IKManager.BoneType, WFCheckBox>) fieldDataDic.GetValue(boneCheckBox);
                foreach (var pair in dataDic)
                {
                    boneCheckBoxMap[pair.Key] = pair.Value;
                }
            }

            Extensions.AssertNull(bodyBoneCheckBox != null);

            {
                var ikboxVisibleDic = (Dictionary<string, WFCheckBox>) fieldIkboxVisibleDic.GetValue(poseEditWindow);
                Extensions.AssertNull(ikboxVisibleDic != null);

                ikBoxVisibleRoot = ikboxVisibleDic["ik_box_visible_Root"];
                Extensions.AssertNull(ikBoxVisibleRoot != null);

                ikBoxVisibleBody = ikboxVisibleDic["ik_box_visible_Body"];
                Extensions.AssertNull(ikBoxVisibleBody != null);
            }
        }
    }
}