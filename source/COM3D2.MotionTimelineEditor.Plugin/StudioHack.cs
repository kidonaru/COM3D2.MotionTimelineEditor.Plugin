using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioHack : StudioHackBase
    {
        private PoseEditWindow poseEditWindow = null;
        private PlacementWindow placementWindow = null;
        private MotionWindow motionWindow = null;
        private ObjectManagerWindow objectManagerWindow = null;
        private BGWindow bgWindow = null;
        private PhotoWindowManager photoManager = null;
        private WindowPartsBoneCheckBox bodyBoneCheckBox = null;
        private Dictionary<IKManager.BoneType, WFCheckBox> boneCheckBoxMap = new Dictionary<IKManager.BoneType, WFCheckBox>(78);

        private FieldInfo fieldDataDic = null;
        private FieldInfo fieldBoneDic = null;
        private FieldInfo fieldIkboxVisibleDic = null;

        public WFCheckBox ikBoxVisibleRoot = null;
        public WFCheckBox ikBoxVisibleBody = null;

        public override int priority
        {
            get
            {
                return 0;
            }
        }

        public override Maid selectedMaid
        {
            get
            {
                return photoManager.select_maid;
            }
        }

        public CreateBGObjectSubWindow createBgObjectWindow
        {
            get
            {
                return objectManagerWindow.createBgObjectWindow;
            }
        }

        private List<Maid> _allMaids = new List<Maid>();
        public override List<Maid> allMaids
        {
            get
            {
                var characterMgr = GameMain.Instance.CharacterMgr;
                _allMaids.Clear();
                for (int i = 0; i < characterMgr.GetMaidCount(); i++)
                {
                    var maid = characterMgr.GetMaid(i);
                    if (maid != null)
                    {
                        _allMaids.Add(maid);
                    }
                }
                return _allMaids;
            }
        }

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();

        public override List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                foreach (var targetObj in objectManagerWindow.GetTargetList())
                {
                    if (targetObj.obj == null || targetObj.type != PhotoTransTargetObject.Type.Prefab)
                    {
                        continue;
                    }

                    var displayName = targetObj.draw_name;
                    var modelName = targetObj.draw_name;
                    int myRoomId = 0;
                    long bgObjectId = 0;
                    var transform = targetObj.obj.transform;
                    var attachMaidSlotNo = GetMaidSlotNo(targetObj.attach_maid_guid);

                    var model = modelManager.CreateModelStat(
                        displayName,
                        modelName,
                        myRoomId,
                        bgObjectId,
                        transform,
                        targetObj.attachi_point,
                        attachMaidSlotNo,
                        targetObj);

                    _modelList.Add(model);
                }

                return _modelList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get
            {
                return allMaids.IndexOf(selectedMaid);
            }
        }

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

                SetMaidStoreDataAll(ikBoxVisibleRoot.name, value.ToString());
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

                SetMaidStoreDataAll(ikBoxVisibleBody.name, value.ToString());
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
                if (value && isMotionPlaying)
                {
                    isMotionPlaying = false;
                }

                poseEditWindow.CheckbtnUse.check = value;
                poseEditWindow.OnClickUseCheckRun(value);
                maidStoreData["use"] = value.ToString();
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
                if (value && isPoseEditing)
                {
                    isPoseEditing = false;
                }

                motionWindow.CheckbtnStop.check = !value;
                motionWindow.OnClickStopCheckRun(!value);

                SetMaidStoreDataAll("is_stop", value.ToString());
                maidManager.SetMotionPlayingAll(value);
            }
        }

        private Dictionary<string, string> maidStoreData
        {
            get
            {
                return poseEditWindow.GetMaidStoreData(selectedMaid);
            }
        }

        public override float motionSliderRate
        {
            set
            {
                var maxNum = motionWindow.Slider.MaxNum;
                var current = Mathf.Clamp01(value) * maxNum;
                motionWindow.Slider.value = current;
            }
        }

        public override bool useMuneKeyL
        {
            set
            {
                SetMaidStoreDataAll("use_mune_key_l", value.ToString());

                if (bodyBoneCheckBox != null)
                {
                    bodyBoneCheckBox.CheckBoxMuneLPhysics.check = value;
                    bodyBoneCheckBox.OnClickCheckBoxMunePhysics(bodyBoneCheckBox.CheckBoxMuneLPhysics);
                }
            }
        }

        public override bool useMuneKeyR
        {
            set
            {
                SetMaidStoreDataAll("use_mune_key_r", value.ToString());

                if (bodyBoneCheckBox != null)
                {
                    bodyBoneCheckBox.CheckBoxMuneRPhysics.check = value;
                    bodyBoneCheckBox.OnClickCheckBoxMunePhysics(bodyBoneCheckBox.CheckBoxMuneRPhysics);
                }
            }
        }

        public override bool Init()
        {
            PluginUtils.Log("StudioHack初期化中...");

            if (!base.Init())
            {
                return false;
            }

            {
                BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

                fieldDataDic = typeof(WindowPartsBoneCheckBox).GetField("data_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldDataDic != null, "fieldDataDic is null");

                fieldBoneDic = typeof(IKManager).GetField("bone_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldBoneDic != null, "fieldBoneDic is null");

                fieldIkboxVisibleDic = typeof(PoseEditWindow).GetField("ikbox_visible_dic_", bindingAttr);
                PluginUtils.AssertNull(fieldIkboxVisibleDic != null, "fieldIkboxVisibleDic is null");
            }

            return true;
        }

        private MethodInfo methodSelectMaid = null;
        private void SetSelectMaid(Maid maid)
        {
            if (methodSelectMaid == null)
            {
                methodSelectMaid = typeof(PlacementWindow).GetMethod("SetSelectMaid", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(methodSelectMaid != null, "methodSelectMaid is null");
            }

            methodSelectMaid.Invoke(placementWindow, new object[] { maid });
        }

        private MethodInfo methodAddObject = null;
        private void AddObject(PhotoBGObjectData add_bg_data, string create_time)
        {
            if (methodAddObject == null)
            {
                methodAddObject = typeof(CreateBGObjectSubWindow).GetMethod("AddObject", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(methodAddObject != null, "methodAddObject is null");
            }

            methodAddObject.Invoke(createBgObjectWindow, new object[] { add_bg_data, create_time });
        }

        public override void ChangeMaid(Maid maid)
        {
            SetSelectMaid(maid);
        }

        public override void OnSceneActive()
        {
            base.OnSceneActive();

            {
                var gameObject = GameObject.Find("PoseEditWindow");
                poseEditWindow = gameObject.GetComponent<PoseEditWindow>();
                PluginUtils.AssertNull(poseEditWindow != null, "poseEditWindow is null");
            }

            {
                var gameObject = GameObject.Find("PlacementWindow");
                placementWindow = gameObject.GetComponent<PlacementWindow>();
                PluginUtils.AssertNull(placementWindow != null, "placementWindow is null");
            }

            {
                var gameObject = GameObject.Find("MotionWindow");
                motionWindow = gameObject.GetComponent<MotionWindow>();
                PluginUtils.AssertNull(motionWindow != null, "motionWindow is null");
            }
            
            {
                var gameObject = GameObject.Find("ObjectManagerWindow");
                objectManagerWindow = gameObject.GetComponent<ObjectManagerWindow>();
                PluginUtils.AssertNull(objectManagerWindow != null, "objectManagerWindow is null");
            }

            {
                var gameObject = GameObject.Find("BGWindow");
                bgWindow = gameObject.GetComponent<BGWindow>();
                PluginUtils.AssertNull(bgWindow != null, "bgWindow is null");
            }

            photoManager = poseEditWindow.mgr;
            PluginUtils.AssertNull(photoManager != null, "photoManager is null");

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

            PluginUtils.AssertNull(bodyBoneCheckBox != null, "bodyBoneCheckBox is null");

            {
                var ikboxVisibleDic = (Dictionary<string, WFCheckBox>) fieldIkboxVisibleDic.GetValue(poseEditWindow);
                PluginUtils.AssertNull(ikboxVisibleDic != null, "ikboxVisibleDic is null");

                ikBoxVisibleRoot = ikboxVisibleDic["ik_box_visible_Root"];
                PluginUtils.AssertNull(ikBoxVisibleRoot != null, "ikBoxVisibleRoot is null");

                ikBoxVisibleBody = ikboxVisibleDic["ik_box_visible_Body"];
                PluginUtils.AssertNull(ikBoxVisibleBody != null, "ikBoxVisibleBody is null");
            }
        }

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "ScenePhotoMode";
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

            SetMaidStoreDataAll("rotate_visible_" + boneType.ToString(), visible.ToString());
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

        public override void ClearPoseHistory()
        {
            maidManager.ikManager.HistoryClear();
        }

        public override void DeleteAllModels()
        {
            var modelList = this.modelList;
            foreach (var model in modelList)
            {
                DeleteModel(model);
            }
        }

        public override void DeleteModel(StudioModelStat model)
        {
            createBgObjectWindow.RemoveObject(model.transform.gameObject);
        }

        public override void CreateModel(StudioModelStat model)
        {
            if (model.info.type == StudioModelType.MyRoom)
            {
                return;
            }

            var bgObject = PhotoBGObjectData.Get(model.info.bgObjectId);
            if (bgObject == null)
            {
                return;
            }

            var date = DateTime.Now;
            var dateFormat = "yyyyMMddHHmmss";
            var targetList = objectManagerWindow.GetTargetList();
            var timeStr = date.ToString(dateFormat) + targetList.Count.ToString("D3");
            AddObject(bgObject, timeStr);

            foreach (var target in targetList)
            {
                if (target == null || target.obj == null || target.obj.name != timeStr)
                {
                    continue;
                }

                model.transform = target.obj.transform;
                model.obj = target;
                UpdateAttachPoint(model);
                break;
            }
        }

        public override void UpdateAttachPoint(StudioModelStat model)
        {
            var target = model.obj as PhotoTransTargetObject;
            if (target != null)
            {
                target.Attach(model.attachPoint, false, GetMaid(model.attachMaidSlotNo));
            }
        }

        private static Dictionary<string, PhotoBGData> bgDataMap = null;

        private static PhotoBGData GetPhotoBGData(string bgName)
        {
            if (bgDataMap == null)
            {
                bgDataMap = new Dictionary<string, PhotoBGData>();
                foreach (var _data in PhotoBGData.data)
                {
                    bgDataMap[_data.create_prefab_name] = _data;
                }
            }

            PhotoBGData data;
            if (bgDataMap.TryGetValue(bgName, out data))
            {
                return data;
            }

            return null;
        }

        public override void ChangeBackground(string bgName)
        {
            var bgData = GetPhotoBGData(bgName);
            if (bgData == null)
            {
                PluginUtils.LogError("背景が見つかりません: " + bgName);
                return;
            }

            var woldStoreData = bgWindow.GetWoldStoreData();
            var dictionary = woldStoreData["ベース背景"];
            dictionary["id"] = bgData.id;
            dictionary["visible"] = true.ToString();
            dictionary["position"] = Vector3.zero.ToString("G9");
            dictionary["rotation"] = Quaternion.identity.ToString("G9");
            dictionary["scale"] = Vector3.one.ToString("G9");
            dictionary["背景色"] = GameMain.Instance.MainCamera.camera.backgroundColor.ToString("G9");

            bgWindow.OnDeserializeEvent();
        }

        public override void SetBackgroundVisible(bool visible)
        {
            bgWindow.CheckSolidColor.check = !visible;
            bgWindow.OnClickCheckSolidColorCheckBox(bgWindow.CheckSolidColor);
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

        private void SetMaidStoreDataAll(string key, string value)
        {
            foreach (var maid in allMaids)
            {
                var maidStoreData = poseEditWindow.GetMaidStoreData(maid);
                maidStoreData[key] = value;
            }
        }
    }
}