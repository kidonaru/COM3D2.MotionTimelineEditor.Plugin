using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CM3D2.SceneCapture.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor_SceneCapture.Plugin
{
    using SCMenuInfo = CM3D2.SceneCapture.Plugin.MenuInfo;
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class ModelPane
    {
        public string name;
        public SCMenuInfo menu;
        public GameObject go;
        public object orgPane;

        public ModelPane(string name, SCMenuInfo menu, GameObject go, object orgPane)
        {
            this.name = name;
            this.menu = menu;
            this.go = go;
            this.orgPane = orgPane;
        }
    }

    public class SceneCaptureHack : ModelHackBase
    {
        private SceneCapture sceneCapture = null;

        private FieldInfo _modelViewField = null;

        private object modelWindow
        {
            get
            {
                if (_modelViewField == null)
                {
                    _modelViewField = typeof(SceneCapture).GetField("modelView", BindingFlags.NonPublic | BindingFlags.Instance);
                    PluginUtils.AssertNull(_modelViewField != null, "_modelViewField is null");
                }

                return _modelViewField.GetValue(sceneCapture);
            }
        }

        private FieldInfo _orgModelPanes = null;

        private IList orgModelPanes
        {
            get
            {
                if (_orgModelPanes == null)
                {
                    _orgModelPanes = modelWindow.GetType().GetField("modelPanes", BindingFlags.NonPublic | BindingFlags.Instance);
                    PluginUtils.AssertNull(_orgModelPanes != null, "_modelPanes is null");
                }

                return (IList)_orgModelPanes.GetValue(modelWindow);
            }
        }

        private List<ModelPane> _modelPanes = new List<ModelPane>();

        private List<ModelPane> modelPanes
        {
            get
            {
                _modelPanes.Clear();

                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

                foreach (var orgPane in orgModelPanes)
                {
                    var name = (string)orgPane.GetType().GetField("name", bindingFlags).GetValue(orgPane);
                    var menu = (SCMenuInfo)orgPane.GetType().GetField("menu", bindingFlags).GetValue(orgPane);
                    var go = (GameObject)orgPane.GetType().GetField("go", bindingFlags).GetValue(orgPane);

                    _modelPanes.Add(new ModelPane(name, menu, go, orgPane));
                }

                return _modelPanes;
            }
        }

        private FieldInfo _initializedField = null;

        private bool initialized
        {
            get
            {
                if (_initializedField == null)
                {
                    _initializedField = typeof(SceneCapture).GetField("initialized", BindingFlags.NonPublic | BindingFlags.Instance);
                    PluginUtils.AssertNull(_initializedField != null, "_initializedField is null");
                }

                return (bool)_initializedField.GetValue(sceneCapture);
            }
        }

        public override string pluginName
        {
            get
            {
                return "SceneCapture";
            }
        }

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();
        public override List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                foreach (var modelPane in modelPanes)
                {
                    if (modelPane != null)
                    {
                        AttachPoint attachPoint = AttachPoint.Null;
                        int attachMaidSlotNo = -1;

                        var parent = modelPane.go.transform.parent;
                        if (parent != null && BoneUtils.IsDefaultBoneName(parent.name))
                        {
                            var boneType = BoneUtils.GetBoneTypeByName(parent.name);
                            attachPoint = BoneUtils.GetAttachPoint(boneType);

                            if (attachPoint != AttachPoint.Null)
                            {
                                var maidCaches = maidManager.maidCaches;
                                for (int i = 0; i < maidCaches.Count; ++i)
                                {
                                    var maidCache = maidCaches[i];
                                    if (maidCache.ikManager == null)
                                    {
                                        continue;
                                    }

                                    var bone = maidCache.ikManager.GetBone(boneType);
                                    if (bone == parent.gameObject)
                                    {
                                        attachMaidSlotNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        var displayName = modelPane.name;
                        var modelName = modelPane.menu.menuFileName;
                        int myRoomId = modelPane.menu.myRoomObjectId;
                        long bgObjectId = modelPane.menu.BGObjectId;
                        var transform = modelPane.go.transform;

                        if (string.IsNullOrEmpty(modelName))
                        {
                            modelName = displayName;
                        }

                        var model = modelManager.CreateModelStat(
                            displayName,
                            modelName,
                            myRoomId,
                            bgObjectId,
                            transform,
                            attachPoint,
                            attachMaidSlotNo,
                            modelPane,
                            pluginName,
                            modelPane.go.activeSelf);

                        _modelList.Add(model);
                    }
                }

                return _modelList;
            }
        }

        public SceneCaptureHack()
        {
        }

        public override bool Init()
        {
            PluginUtils.Log("SceneCaptureHack: 初期化中...");

            if (!base.Init())
            {
                return false;
            }

            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                sceneCapture = gameObject.GetComponent<SceneCapture>();
                PluginUtils.AssertNull(sceneCapture != null, "sceneCapture is null");
            }

            if (sceneCapture == null)
            {
                PluginUtils.LogError("SceneCaptureプラグインが見つかりませんでした");
                return false;
            }

            return true;
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (!initialized)
            {
                _errorMessage = "SceneCaptureが初期化されていません";
                return false;
            }

            return true;
        }

        private MethodInfo _clearModelsInfo;

        private void _ClearModels()
        {
            if (_clearModelsInfo == null)
            {
                _clearModelsInfo = modelWindow.GetType().GetMethod("ClearModels", BindingFlags.Public | BindingFlags.Instance);
                PluginUtils.AssertNull(_clearModelsInfo != null, "_clearModelsInfo is null");
            }

            _clearModelsInfo.Invoke(modelWindow, null);
        }

        private MethodInfo _checkForModelUpdates;

        private void _CheckForModelUpdates()
        {
            if (_checkForModelUpdates == null)
            {
                _checkForModelUpdates = modelWindow.GetType().GetMethod("CheckForModelUpdates", BindingFlags.Public | BindingFlags.Instance);
                PluginUtils.AssertNull(_checkForModelUpdates != null, "_checkForModelUpdates is null");
            }

            _checkForModelUpdates.Invoke(modelWindow, null);
        }

        private MethodInfo _deleteModelInfo;

        private void _DeleteModel(ModelPane modelPane)
        {
            if (_deleteModelInfo == null)
            {
                _deleteModelInfo = modelPane.orgPane.GetType().GetMethod("DeleteModel", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(_deleteModelInfo != null, "_deleteModelInfo is null");
            }

            _deleteModelInfo.Invoke(modelPane.orgPane, new object[] { null, null });
            _CheckForModelUpdates();
        }

        public override void DeleteAllModels()
        {
            _ClearModels();
        }

        public override void DeleteModel(StudioModelStat model)
        {
            var modelPane = model.obj as ModelPane;
            if (modelPane != null)
            {
                _DeleteModel(modelPane);
            }
        }

        private MethodInfo _addModelInfo = null;

        private void _AddModel(SCMenuInfo menu)
        {
            if (_addModelInfo == null)
            {
                _addModelInfo = modelWindow.GetType().GetMethod("AddModel", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(_addModelInfo != null, "_addModelInfo is null");
            }

            _addModelInfo.Invoke(modelWindow, new object[] { menu });
        }

        public override void CreateModel(StudioModelStat model)
        {
            try
            {
                SCMenuInfo menuInfo = null;
                if (model.info.type == StudioModelType.Mod)
                {
                    menuInfo = AssetLoader.LoadMenu(model.info.fileName);
                }
                else if (model.info.type == StudioModelType.Prefab ||
                        model.info.type == StudioModelType.Asset)
                {
                    menuInfo = SCMenuInfo.MakeBGObjectMenu(model.info.bgObjectId);
                }
                else if (model.info.type == StudioModelType.MyRoom)
                {
                    menuInfo = SCMenuInfo.MakeMyRoomObjectMenu(model.info.myRoomId);
                    //menuInfo = SCMenuInfo.MakeBackgroundMenu(model.info.modelName);
                    //menuInfo = SCMenuInfo.MakeMyRoomMenu(model.info.myRoomId);
                }

                _AddModel(menuInfo);

                foreach (var modelPane in modelPanes)
                {
                    if (modelPane.menu == menuInfo)
                    {
                        model.obj = modelPane;
                        model.transform = modelPane.go.transform;
                        break;
                    }
                }

                if (model.obj == null)
                {
                    PluginUtils.LogError("CreateModel: モデルの追加に失敗しました" + model.name);
                    return;
                }

                UpdateAttachPoint(model);
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public override void UpdateAttachPoint(StudioModelStat model)
        {
            var attachPoint = model.attachPoint;
            var attachMaidSlotNo = model.attachMaidSlotNo;
            var maidCache = maidManager.GetMaidCache(attachMaidSlotNo);
            AttachItem(maidCache, model.transform, attachPoint, false);
        }

        public void AttachItem(MaidCache maidCache, Transform item, AttachPoint point, bool keepWorldPosition)
        {
            Transform parent = maidCache != null ? maidCache.GetAttachPointTransform(point) : null;
            Quaternion quaternion = item.rotation;
            Vector3 localScale = item.localScale;

            item.SetParent(parent, keepWorldPosition);
            if (keepWorldPosition)
            {
                item.rotation = quaternion;
            }
            else
            {
                item.localPosition = Vector3.zero;
                item.rotation = Quaternion.identity;
            }
            item.localScale = localScale;
        }
    }
}