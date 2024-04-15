using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioHack : StudioHackBase
    {
        private PoseEditWindow poseEditWindow = null;
        private MotionWindow motionWindow = null;
        private PhotoWindowManager photoManager = null;
        private WindowPartsBoneCheckBox bodyBoneCheckBox = null;
        private Dictionary<IKManager.BoneType, WFCheckBox> boneCheckBoxMap = new Dictionary<IKManager.BoneType, WFCheckBox>(78);

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

        public override bool hasIkBoxVisible
        {
            get
            {
                return true;
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

        public override bool useMuneKeyL
        {
            get
            {
                if (maid != null)
                {
                    return !maid.body0.jbMuneL.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneL.enabled = !value;
                    maid.body0.MuneYureL(maid.body0.jbMuneL.enabled ? 1 : 0);
                    maidStoreData["use_mune_key_l"] = value.ToString();
                }
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
                if (maid != null)
                {
                    return !maid.body0.jbMuneR.enabled;
                }
                return false;
            }
            set
            {
                if (maid != null)
                {
                    maid.body0.jbMuneR.enabled = !value;
                    maid.body0.MuneYureR(maid.body0.jbMuneR.enabled ? 1 : 0);
                    maidStoreData["use_mune_key_r"] = value.ToString();
                }
                if (bodyBoneCheckBox != null)
                {
                    bodyBoneCheckBox.CheckBoxMuneRPhysics.check = value;
                    bodyBoneCheckBox.OnClickCheckBoxMunePhysics(bodyBoneCheckBox.CheckBoxMuneRPhysics);
                }
            }
        }

        public override void OnSceneActive()
        {
            PluginUtils.Log("StudioHack初期化中...");
            base.OnSceneActive();

            {
                var gameObject = GameObject.Find("PoseEditWindow");
                poseEditWindow = gameObject.GetComponent<PoseEditWindow>();
                PluginUtils.AssertNull(poseEditWindow != null);
            }

            {
                var gameObject = GameObject.Find("MotionWindow");
                motionWindow = gameObject.GetComponent<MotionWindow>();
                PluginUtils.AssertNull(motionWindow != null);
            }

            photoManager = poseEditWindow.mgr;
            PluginUtils.AssertNull(photoManager != null);

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldDataDic = typeof(WindowPartsBoneCheckBox).GetField("data_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldDataDic != null);

                fieldBoneDic = typeof(IKManager).GetField("bone_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldBoneDic != null);

                fieldIkboxVisibleDic = typeof(PoseEditWindow).GetField("ikbox_visible_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldIkboxVisibleDic != null);
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

            PluginUtils.AssertNull(bodyBoneCheckBox != null);

            {
                var ikboxVisibleDic = (Dictionary<string, WFCheckBox>) fieldIkboxVisibleDic.GetValue(poseEditWindow);
                PluginUtils.AssertNull(ikboxVisibleDic != null);

                ikBoxVisibleRoot = ikboxVisibleDic["ik_box_visible_Root"];
                PluginUtils.AssertNull(ikBoxVisibleRoot != null);

                ikBoxVisibleBody = ikboxVisibleDic["ik_box_visible_Body"];
                PluginUtils.AssertNull(ikBoxVisibleBody != null);
            }
        }

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "ScenePhotoMode";
        }

        public override Maid activeMaid
        {
            get
            {
                return photoManager.select_maid;
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