using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioHack : MaidHackBase
    {
        private PoseEditWindow poseEditWindow = null;
        private MotionWindow motionWindow = null;
        private PhotoWindowManager photoManager = null;
        private WindowPartsBoneCheckBox bodyBoneCheckBox = null;
        private Dictionary<IKManager.BoneType, WFCheckBox> boneCheckBoxMap = new Dictionary<IKManager.BoneType, WFCheckBox>(78);

        private FieldInfo fieldAnimeState = null;
        private FieldInfo fieldLimbControlList = null;
        private FieldInfo fieldIkFabrik = null;
        private FieldInfo fieldJointDragPoint = null;
        private FieldInfo fieldTipDragPoint = null;
        private FieldInfo fieldDataDic = null;
        private FieldInfo fieldBoneDic = null;
        private FieldInfo fieldIkboxVisibleDic = null;

        public WFCheckBox ikBoxVisibleRoot = null;
        public WFCheckBox ikBoxVisibleBody = null;

        public override string outputAnmPath
        {
            get
            {
                var path = PhotoModePoseSave.folder_path;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public override bool isIkBoxVisibleRoot
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

        public override bool isIkBoxVisibleBody
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

        public override bool isPoseEditing
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

        public override bool isMotionPlaying
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

        private Dictionary<string, string> maidStoreData
        {
            get
            {
                return poseEditWindow.GetMaidStoreData(maid);
            }
        }

        private IKManager ikManager
        {
            get
            {
                return poseEditWindow.ik_mgr;
            }
        }

        public override float motionSliderRate
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

        private List<LimbControl> limbControlList
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

        public override bool useMuneKeyL
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

        public override bool useMuneKeyR
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

        public override void Init()
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

                fieldIkFabrik = typeof(LimbControl).GetField("ik_fabrik_", bindingAttr);
                Extensions.AssertNull(fieldIkFabrik != null);

                fieldJointDragPoint = typeof(LimbControl).GetField("joint_drag_point_", bindingAttr);
                Extensions.AssertNull(fieldJointDragPoint != null);

                fieldTipDragPoint = typeof(LimbControl).GetField("tip_drag_point_", bindingAttr);
                Extensions.AssertNull(fieldTipDragPoint != null);

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

        public override bool IsValid()
        {
            _errorMessage = "";

            if (GetMaid() == null)
            {
                _errorMessage = "メイドを配置してください";
                return false;
            }

            return true;
        }

        protected override Maid GetMaid()
        {
            return photoManager.select_maid;
        }

        public override void Update()
        {
            base.Update();
        }

        private LimbControl GetLimbControl(LimbControl.Type type)
        {
            return limbControlList.Find(l => l.type == type);
        }

        private FABRIK GetIkFabrik(LimbControl.Type type)
        {
            var limbControl = GetLimbControl(type);
            if (limbControl != null)
            {
                return (FABRIK) fieldIkFabrik.GetValue(limbControl);
            }
            return null;
        }

        private FABRIK GetIkFabrik(IKHoldType type)
        {
            switch (type)
            {
                case IKHoldType.Arm_R_Joint:
                case IKHoldType.Arm_R_Tip:
                    return GetIkFabrik(LimbControl.Type.Arm_R);
                case IKHoldType.Arm_L_Joint:
                case IKHoldType.Arm_L_Tip:
                    return GetIkFabrik(LimbControl.Type.Arm_L);
                case IKHoldType.Foot_R_Joint:
                case IKHoldType.Foot_R_Tip:
                    return GetIkFabrik(LimbControl.Type.Foot_R);
                case IKHoldType.Foot_L_Joint:
                case IKHoldType.Foot_L_Tip:
                    return GetIkFabrik(LimbControl.Type.Foot_L);
            }
            return null;
        }

        private IKDragPoint GetDragPoint(LimbControl.Type type, FieldInfo field)
        {
            var limbControl = GetLimbControl(type);
            if (limbControl != null)
            {
                return (IKDragPoint) field.GetValue(limbControl);
            }
            return null;
        }

        private IKDragPoint GetDragPoint(IKHoldType type)
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

        public override Vector3 GetIkPosition(IKHoldType holdType)
        {
            var dragPoint = GetDragPoint(holdType);
            if (dragPoint != null)
            {
                return dragPoint.target_ik_point_trans.position;
            }
            return Vector3.zero;
        }

        public override void UpdateIkPosition(IKHoldType holdType, Vector3 targetPosition)
        {
            var ikFabrik = GetIkFabrik(holdType);
            var dragPoint = GetDragPoint(holdType);
            if (ikFabrik != null && dragPoint != null)
            {
                dragPoint.drag_start_event.Invoke();
                dragPoint.transform.position = targetPosition;
                ikFabrik.solver.Update();
                dragPoint.drag_end_event.Invoke();
            }
        }

        public override bool HasBoneRotateVisible(IKManager.BoneType boneType)
        {
            return boneCheckBoxMap.ContainsKey(boneType);
        }

        public override bool IsBoneRotateVisible(IKManager.BoneType boneType)
        {
            var maidStoreData = this.maidStoreData;
            string value;
            if (maidStoreData.TryGetValue("rotate_visible_" + boneType.ToString(), out value))
            {
                return bool.Parse(value);
            }
            return false;
        }

        public override void SetBoneRotateVisible(IKManager.BoneType boneType, bool visible)
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
                var maidStoreData = this.maidStoreData;
                maidStoreData["rotate_visible_" + boneType.ToString()] = visible.ToString();
            }
        }

        public override void ClearBoneRotateVisible()
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

        public override void OnMotionUpdated(Maid maid)
        {
            motionWindow.UpdateAnimationData(maid);
            poseEditWindow.OnMotionUpdate(maid);
            base.OnMotionUpdated(maid);
        }

        public override void OnUpdateMyPose(string anmPath, bool isExist)
        {
            if (!isExist)
            {
                motionWindow.AddMyPose(anmPath);
            }
            else
            {
                motionWindow.OnUpdateMyPose(anmPath);
            }
            base.OnUpdateMyPose(anmPath, isExist);
        }
    }
}