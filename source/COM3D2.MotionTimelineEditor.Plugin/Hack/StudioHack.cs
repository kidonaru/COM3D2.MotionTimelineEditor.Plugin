using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public partial class StudioHack : StudioHackBase
    {
        private StudioWrapper studio = new StudioWrapper();

        public override string pluginName => "StudioMode";
        public override int priority => 0;
        public override Maid selectedMaid => studio.photoManager.select_maid;

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
                var bgObjectMap = studio.GetBgObjectMap();

                foreach (var targetObj in studio.objectManagerWindow.GetTargetList())
                {
                    if (targetObj.obj == null || targetObj.type != PhotoTransTargetObject.Type.Prefab)
                    {
                        continue;
                    }

                    var bgObject = bgObjectMap.GetOrDefault(targetObj.obj.name);
                    if (bgObject == null)
                    {
                        continue;
                    }

                    var bgObjectData = bgObject.data;
                    bgObjectData.GetFileNameAndType(out var modelName, out var _);

                    var displayName = bgObjectData.name;
                    int myRoomId = 0;
                    long bgObjectId = bgObjectData.id;
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
                        targetObj,
                        pluginName,
                        targetObj.obj.activeSelf);

                    _modelList.Add(model);
                }

                return _modelList;
            }
        }

        private List<StudioLightStat> _lightList = new List<StudioLightStat>();
        public override List<StudioLightStat> lightList
        {
            get
            {
                _lightList.Clear();

                int index = 0;
                foreach (var targetObj in studio.lightWindow.GetTargetList())
                {
                    Light light;
                    Transform transform;

                    if (targetObj.obj == null)
                    {
                        light = GameMain.Instance.MainLight.GetComponent<Light>();
                        transform = GameMain.Instance.MainLight.transform;
                    }
                    else
                    {
                        light = targetObj.obj.GetComponentInChildren<Light>(false);
                        transform = targetObj.obj.transform;
                    }

                    if (light == null || transform == null)
                    {
                        continue;
                    }

                    var stat = new StudioLightStat(light, transform, targetObj, index++);
                    _lightList.Add(stat);
                }

                return _lightList;
            }
        }

        public override int selectedMaidSlotNo
        {
            get => allMaids.IndexOf(selectedMaid);
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

        public override bool hasIkBoxVisible => true;

        public override bool isIkBoxVisibleRoot
        {
            get => studio.ikBoxVisibleRoot.check;
            set
            {
                studio.ikBoxVisibleRoot.check = value;
                studio.poseEditWindow.OnIKBOXVixibleCheckBox(studio.ikBoxVisibleRoot);

                SetMaidStoreDataAll(studio.ikBoxVisibleRoot.name, value.ToString());
            }
        }

        public override bool isIkBoxVisibleBody
        {
            get => studio.ikBoxVisibleBody.check;
            set
            {
                studio.ikBoxVisibleBody.check = value;
                studio.poseEditWindow.OnIKBOXVixibleCheckBox(studio.ikBoxVisibleBody);

                SetMaidStoreDataAll(studio.ikBoxVisibleBody.name, value.ToString());
            }
        }

        public override bool isPoseEditing
        {
            get => studio.poseEditWindow.CheckbtnUse.check;
            set
            {
                MTEUtils.LogDebug("isPoseEditing: {0} -> {1}", isPoseEditing, value);

                if (value && isAnmPlaying)
                {
                    isAnmPlaying = false;
                }

                var savedUseMuneKeyL = _useMuneKeyL;
                var savedUseMuneKeyR = _useMuneKeyR;

                studio.poseEditWindow.CheckbtnUse.check = value;
                studio.poseEditWindow.OnClickUseCheckRun(value);
                maidStoreData["use"] = value.ToString();

                useMuneKeyL = savedUseMuneKeyL;
                useMuneKeyR = savedUseMuneKeyR;
            }
        }

        public override bool isIKVisible
        {
            get => isIkBoxVisibleRoot || isIkBoxVisibleBody;
            set
            {
                isIkBoxVisibleRoot = value;
                isIkBoxVisibleBody = value;
            }
        }

        public override bool isAnmEnabled
        {
            get => !studio.motionWindow.CheckbtnStop.check;
            set
            {
                MTEUtils.LogDebug("isAnmEnabled: {0} -> {1}", isAnmEnabled, value);

                studio.motionWindow.CheckbtnStop.check = !value;
                studio.motionWindow.OnClickStopCheckRun(!value);

                SetMaidStoreDataAll("is_stop", value.ToString());
                maidManager.SetAnmEnabledAll(value);
            }
        }

        private Dictionary<string, string> maidStoreData
        {
            get => studio.poseEditWindow.GetMaidStoreData(selectedMaid);
        }

        public override float motionSliderRate
        {
            set
            {
                var maxNum = studio.motionWindow.Slider.MaxNum;
                var current = Mathf.Clamp01(value) * maxNum;
                studio.motionWindow.Slider.value = current;
            }
        }

        public override bool useMuneKeyL
        {
            set
            {
                SetMaidStoreDataAll("use_mune_key_l", value.ToString());

                if (studio.bodyBoneCheckBox != null)
                {
                    studio.bodyBoneCheckBox.CheckBoxMuneLPhysics.check = value;
                    studio.bodyBoneCheckBox.OnClickCheckBoxMunePhysics(studio.bodyBoneCheckBox.CheckBoxMuneLPhysics);
                }
            }
        }

        public override bool useMuneKeyR
        {
            set
            {
                SetMaidStoreDataAll("use_mune_key_r", value.ToString());

                if (studio.bodyBoneCheckBox != null)
                {
                    studio.bodyBoneCheckBox.CheckBoxMuneRPhysics.check = value;
                    studio.bodyBoneCheckBox.OnClickCheckBoxMunePhysics(studio.bodyBoneCheckBox.CheckBoxMuneRPhysics);
                }
            }
        }

        private bool _useMuneKeyL
        {
            get
            {
                if (selectedMaid != null)
                {
                    return !selectedMaid.body0.jbMuneL.enabled;
                }
                return false;
            }
        }

        private bool _useMuneKeyR
        {
            get
            {
                if (selectedMaid != null)
                {
                    return !selectedMaid.body0.jbMuneR.enabled;
                }
                return false;
            }
        }

        public override Camera subCamera => null;

        public override bool isUIVisible
        {
            get => true;
            set {}
        }

        public override bool Init()
        {
            MTEUtils.Log("StudioHack初期化中...");

            if (!base.Init())
            {
                return false;
            }

            if (!studio.Init())
            {
                return false;
            }

            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void ChangeMaid(Maid maid)
        {
            studio.SetSelectMaid(maid);
        }

        public override void OnSceneActive()
        {
            base.OnSceneActive();
            studio.OnSceneActive();
        }

        public override void OnSceneDeactive()
        {
            base.OnSceneDeactive();
        }

        public override void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            base.OnChangedSceneLevel(sceneName, sceneMode);
            isSceneActive = sceneName.name == "ScenePhotoMode";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!studio.active)
            {
                _errorMessage = "スタジオモードの初期化に失敗しました。";
                return false;
            }

            return true;
        }

        public override bool HasBoneRotateVisible(IKManager.BoneType boneType)
        {
            return studio.boneCheckBoxMap.ContainsKey(boneType);
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
                if (studio.boneCheckBoxMap.TryGetValue(boneType, out checkBox))
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
            foreach (var pair in studio.boneCheckBoxMap)
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
            studio.createBgObjectWindow.RemoveObject(model.transform.gameObject);
        }

        public override void CreateModel(StudioModelStat model)
        {
            model.info.Dump();

            if (model.info.type == StudioModelType.MyRoom)
            {
                return;
            }

            if (model.info.type == StudioModelType.Mod &&
                config.autoResisterBackgroundCustom &&
                backgroundCustomManager.IsValid())
            {
                var fileName = Path.GetFileNameWithoutExtension(model.info.fileName);
                var menu = ModMenuLoader.Load(fileName);
                if (menu == null)
                {
                    return;
                }

                backgroundCustomManager.RegisterObject(menu.name, fileName);
                var info = modelManager.FindOfficialObject(menu.name, fileName, 0, 0);
                if (info != null)
                {
                    model.ReplaceInfo(info);
                }
            }

            var bgObject = PhotoBGObjectData.Get(model.info.bgObjectId);
            if (bgObject == null)
            {
                MTEUtils.LogWarning("背景オブジェクトが見つかりません: " + model.name);
                return;
            }

            var date = DateTime.Now;
            var dateFormat = "yyyyMMddHHmmss";
            var targetList = studio.objectManagerWindow.GetTargetList();
            var timeStr = date.ToString(dateFormat) + targetList.Count.ToString("D3");
            studio.AddObject(bgObject, timeStr);

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

        public override bool CanCreateLight()
        {
            return false;
        }

        public override void DeleteAllLights()
        {
            foreach (var light in lightList)
            {
                DeleteLight(light);
            }
        }

        public override void DeleteLight(StudioLightStat stat)
        {
            if (stat.index == 0)
            {
                return;
            }

            var targetObj = stat.obj as PhotoTransTargetObject;
            if (targetObj != null && targetObj.obj != null && stat.light != null)
            {
                stat.light.enabled = false;
                //studio.lightWindow.RemoveTransTargetObject(targetObj.obj);
            }
        }

        public override void CreateLight(StudioLightStat stat)
        {
            var targetList = studio.lightWindow.GetTargetList();
            if (stat.index >= targetList.Count)
            {
                MTEUtils.LogError("ライト数が足りません: " + stat.index);
                return;
            }

            var targetObj = targetList[stat.index];
            if (targetObj != null && targetObj.obj != null)
            {
                stat.light = targetObj.obj.GetComponentInChildren<Light>(false);
                stat.transform = targetObj.obj.transform;
                stat.obj = targetObj;
                ChangeLight(stat);
                ApplyLight(stat);
            }
        }

        public override void ChangeLight(StudioLightStat stat)
        {
            var targetObj = stat.obj as PhotoTransTargetObject;
            if (targetObj == null || stat.light == null)
            {
                MTEUtils.LogError("ライトが見つかりません: " + stat.name);
                return;
            }

            MTEUtils.LogDebug("ChangeLight: {0} {1} -> {2}",
                stat.name, stat.light.type, stat.type);
            if (stat.light.type != stat.type)
            {
                if (stat.type == LightType.Spot)
                {
                    UTY.GetChildObject(targetObj.obj, "LightPoint", false).SetActive(false);
                    UTY.GetChildObject(targetObj.obj, "LightSpot", false).SetActive(true);
                }
                else if (stat.type == LightType.Point)
                {
                    UTY.GetChildObject(targetObj.obj, "LightPoint", false).SetActive(true);
                    UTY.GetChildObject(targetObj.obj, "LightSpot", false).SetActive(false);
                }
                stat.light = targetObj.obj.GetComponentInChildren<Light>(false);
                stat.transform = targetObj.obj.transform;
            }
        }

        public override void ApplyLight(StudioLightStat stat)
        {
            var targetObj = stat.obj as PhotoTransTargetObject;
            if (targetObj == null || stat.light == null)
            {
                MTEUtils.LogError("ライトが見つかりません: " + stat.name);
                return;
            }

            if (stat.type == LightType.Directional)
            {
                var directionalLightWindow = studio.directionalLightWindow;
                directionalLightWindow.OnChangetIntensityValue(null, stat.light.intensity);
                directionalLightWindow.OnChangetShadowValue(null, stat.light.shadowStrength);
                directionalLightWindow.OnChangetColorValue(null, stat.light.color);
            }

            stat.light.enabled = stat.visible;
        }

        public override void SetLightCompatibilityMode(bool isLightCompatibilityMode)
        {
            var targetList = studio.lightWindow.GetTargetList();
            foreach (var targetObj in targetList)
            {
                if (targetObj.obj == null)
                {
                    continue;
                }

                var lightSpot = UTY.GetChildObject(targetObj.obj, "LightSpot", false);
                if (lightSpot == null)
                {
                    continue;
                }

                lightSpot.transform.localEulerAngles =
                        isLightCompatibilityMode ? Vector3.zero : new Vector3(90f, 0f, 0f);
            }
        }

        public override void ChangeBackground(string bgName)
        {
            var bgData = photoBGManager.GetPhotoBGData(bgName);
            if (bgData == null)
            {
                MTEUtils.LogError("背景が見つかりません: " + bgName);
                return;
            }

            var woldStoreData = studio.bgWindow.GetWoldStoreData();
            var dictionary = woldStoreData["ベース背景"];
            dictionary["id"] = bgData.id;
            dictionary["visible"] = true.ToString();
            dictionary["position"] = Vector3.zero.ToString("G9");
            dictionary["rotation"] = Quaternion.identity.ToString("G9");
            dictionary["scale"] = Vector3.one.ToString("G9");
            dictionary["背景色"] = PluginUtils.MainCamera.backgroundColor.ToString("G9");

            studio.bgWindow.OnDeserializeEvent();
        }

        public override void SetBackgroundVisible(bool visible)
        {
            var bgWindow = studio.bgWindow;
            if (bgWindow != null)
            {
                bgWindow.CheckSolidColor.check = !visible;
                bgWindow.OnClickCheckSolidColorCheckBox(bgWindow.CheckSolidColor);
            }
        }

        public override void OnMotionUpdated(Maid maid)
        {
            studio.motionWindow.UpdateAnimationData(maid);
            studio.poseEditWindow.OnMotionUpdate(maid);
            base.OnMotionUpdated(maid);
        }

        public override void OnUpdateMyPose(string anmPath, bool isExist)
        {
            if (!isExist)
            {
                studio.motionWindow.AddMyPose(anmPath);
            }
            else
            {
                studio.motionWindow.OnUpdateMyPose(anmPath);
            }
            base.OnUpdateMyPose(anmPath, isExist);
        }

        private void SetMaidStoreDataAll(string key, string value)
        {
            foreach (var maid in allMaids)
            {
                var maidStoreData = studio.poseEditWindow.GetMaidStoreData(maid);
                maidStoreData[key] = value;
            }
        }

        public override bool IsIKDragging(IKHoldType iKHoldType)
        {
            var maidCache = maidManager.maidCache;
            if (maidCache != null)
            {
                return maidCache.GetAxisObj(iKHoldType).is_drag;
            }
            return false;
        }
    }
}