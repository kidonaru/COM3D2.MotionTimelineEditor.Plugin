using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyRoomCustom;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class OfficialObjectInfo
    {
        public StudioModelType type;
        public string label;
        public string fileName;
        public string prefabName;
        public int myRoomId;
        public long bgObjectId;

        public string fileNameOrId
        {
            get
            {
                if (type == StudioModelType.MyRoom)
                {
                    return myRoomId.ToString();
                }
                return fileName;
            }
        }

        public void Dump()
        {
            MTEUtils.LogDebug("OfficialObjectInfo: type={0} label={1} fileName={2} prefabName={3} myRoomId={4} bgObjectId={5}",
                type, label, fileName, prefabName, myRoomId, bgObjectId);
        }
    }

    public class StudioModelManager : ManagerBase
    {
        private Dictionary<string, StudioModelStat> modelMap = new Dictionary<string, StudioModelStat>();
        public Dictionary<string, ModelBone> boneMap = new Dictionary<string, ModelBone>();
        public Dictionary<string, ModelBlendShape> blendShapeMap = new Dictionary<string, ModelBlendShape>();
        public Dictionary<string, ModelMaterial> materialMap = new Dictionary<string, ModelMaterial>(); 

        public List<StudioModelStat> models = new List<StudioModelStat>();
        public List<string> modelNames = new List<string>();
        public List<string> boneNames = new List<string>();
        public List<string> blendShapeNames = new List<string>();
        public List<string> materialNames = new List<string>();

        public static event UnityAction<StudioModelStat> onModelAdded;
        public static event UnityAction<StudioModelStat> onModelRemoved;
        public static event UnityAction<StudioModelStat> onModelUpdated;

        private static StudioModelManager _instance = null;
        public static StudioModelManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StudioModelManager();
                }
                return _instance;
            }
        }

        private Dictionary<string, OfficialObjectInfo> _officialObjectLabelMap = null;
        private Dictionary<string, OfficialObjectInfo> _bgObjectFileNameMap = null;
        private Dictionary<int, OfficialObjectInfo> _myRoomObjectIdMap = null;
        private Dictionary<long, OfficialObjectInfo> _bgObjectIdMap = null;

        public Dictionary<string, OfficialObjectInfo> OfficialObjectLabelMap
        {
            get
            {
                InitOfficialObjectMap();
                return _officialObjectLabelMap;
            }
        }

        private Dictionary<string, OfficialObjectInfo> BGObjectFileNameMap
        {
            get
            {
                InitOfficialObjectMap();
                return _bgObjectFileNameMap;
            }
        }

        public Dictionary<int, OfficialObjectInfo> MYRoomObjectIdMap
        {
            get
            {
                InitOfficialObjectMap();
                return _myRoomObjectIdMap;
            }
        }

        public Dictionary<long, OfficialObjectInfo> BGObjectIdMap
        {
            get
            {
                InitOfficialObjectMap();
                return _bgObjectIdMap;
            }
        }

        private StudioModelManager()
        {
        }

        public StudioModelStat GetModel(string name)
        {
            StudioModelStat model;
            if (modelMap.TryGetValue(name, out model))
            {
                return model;
            }
            return null;
        }

        public StudioModelStat GetModel(int index)
        {
            if (index < 0 || index >= modelNames.Count)
            {
                return null;
            }
            return GetModel(modelNames[index]);
        }

        public ModelBone GetBone(string name)
        {
            ModelBone bone;
            if (boneMap.TryGetValue(name, out bone))
            {
                return bone;
            }
            return null;
        }

        public ModelBone GetBone(int index)
        {
            if (index < 0 || index >= boneNames.Count)
            {
                return null;
            }
            return GetBone(boneNames[index]);
        }

        public ModelBlendShape GetBlendShape(string name)
        {
            ModelBlendShape blendShape;
            if (blendShapeMap.TryGetValue(name, out blendShape))
            {
                return blendShape;
            }
            return null;
        }

        public ModelBlendShape GetBlendShape(int index)
        {
            if (index < 0 || index >= blendShapeNames.Count)
            {
                return null;
            }
            return GetBlendShape(blendShapeNames[index]);
        }

        public ModelMaterial GetMaterial(string name)
        {
            ModelMaterial material;
            if (materialMap.TryGetValue(name, out material))
            {
                return material;
            }
            return null;
        }

        public ModelMaterial GetMaterial(int index)
        {
            if (index < 0 || index >= materialNames.Count)
            {
                return null;
            }
            return GetMaterial(materialNames[index]);
        }

        private int _prevUpdateFrame = -1;

        public override void LateUpdate()
        {
            LateUpdate(false);
        }

        public void LateUpdate(bool force)
        {
            if (!force)
            {
                if (Time.frameCount < _prevUpdateFrame + 30 || currentLayer.isAnmPlaying)
                {
                    return;
                }
            }
            _prevUpdateFrame = Time.frameCount;

            var modelList = modelHackManager.modelList;

            var addedModels = new List<StudioModelStat>();
            var removedModels = new List<StudioModelStat>();
            var updatedModels = new List<StudioModelStat>();
            var refresh = false;

            foreach (var model in modelList)
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    continue;
                }

                if (model.transform == null)
                {
                    MTEUtils.LogWarning("StudioModelManager: transformがありません: name={0}", model.name);
                    continue;
                }

                StudioModelStat cachedModel;
                if (!modelMap.TryGetValue(model.name, out cachedModel))
                {
                    cachedModel = new StudioModelStat();
                    modelMap[model.name] = cachedModel;
                    addedModels.Add(cachedModel);
                }

                if (cachedModel.transform != model.transform ||
                    cachedModel.pluginName != model.pluginName)
                {
                    cachedModel.FromModel(model);
                    updatedModels.Add(cachedModel);
                    refresh = true;
                    continue;
                }

                if (cachedModel.attachPoint != model.attachPoint ||
                    cachedModel.attachMaidSlotNo != model.attachMaidSlotNo ||
                    cachedModel.visible != model.visible)
                {
                    cachedModel.FromModel(model);
                    updatedModels.Add(cachedModel);
                    continue;
                }
            }

            if (modelList.Count < modelMap.Count)
            {
                var names = new List<string>(modelMap.Keys);
                foreach (var name in names)
                {
                    if (!modelList.Exists(model => model.name == name))
                    {
                        removedModels.Add(modelMap[name]);
                        modelMap.Remove(name);
                        refresh = true;
                    }
                }
            }

            if (refresh)
            {
                models.Clear();
                modelNames.Clear();
                boneMap.Clear();
                blendShapeMap.Clear();
                materialMap.Clear();
                boneNames.Clear();
                blendShapeNames.Clear();
                materialNames.Clear();

                models.AddRange(modelMap.Values);

                models.Sort((model1, model2) =>
                {
                    var c = string.Compare(model1.info.fileName, model2.info.fileName);
                    if (c != 0)
                    {
                        return c;
                    }
                    return model1.group - model2.group;
                });

                foreach (var model in models)
                {
                    modelNames.Add(model.name);

                    foreach (var bone in model.bones)
                    {
                        boneMap[bone.name] = bone;
                        boneNames.Add(bone.name);
                    }

                    foreach (var blendShape in model.blendShapes)
                    {
                        blendShapeMap[blendShape.name] = blendShape;
                        blendShapeNames.Add(blendShape.name);
                    }

                    foreach (var material in model.materials)
                    {
                        materialMap[material.name] = material;
                        materialNames.Add(material.name);
                    }
                }

                MTEUtils.LogDebug("StudioModelManager: Model list updated");

                foreach (var model in models)
                {
                    MTEUtils.LogDebug("model: type={0} displayName={1} name={2} label={3} fileName={4} myRoomId={5} bgObjectId={6} pluginName={7}",
                        model.info.type, model.displayName, model.name, model.info.label, model.info.fileName, model.info.myRoomId, model.info.bgObjectId, model.pluginName);

                    foreach (var bone in model.bones)
                    {
                        MTEUtils.LogDebug("  bone: name={0}", bone.name);
                    }

                    foreach (var blendShape in model.blendShapes)
                    {
                        MTEUtils.LogDebug("  blendShape: name={0}", blendShape.name);
                    }

                    foreach (var material in model.materials)
                    {
                        MTEUtils.LogDebug("  material: name={0}", material.name);
                    }
                }
            }

            if (addedModels.Count > 0 || removedModels.Count > 0 || updatedModels.Count > 0)
            {
                UpdateTimelineModels();
            }

            foreach (var model in addedModels)
            {
                onModelAdded?.Invoke(model);
            }

            foreach (var model in removedModels)
            {
                onModelRemoved?.Invoke(model);
            }

            foreach (var model in updatedModels)
            {
                onModelUpdated?.Invoke(model);
            }
        }

        public void SetupModels(List<TimelineModelData> modelDataList)
        {
            MTEUtils.LogDebug("SetupModels: count={0}", modelDataList.Count);

            var modelList = modelHackManager.modelList.ToList();

            foreach (var modelData in modelDataList)
            {
                var model = modelList.Find(m => m.name == modelData.name);
                if (model == null)
                {
                    model = CreateModelStat(
                        modelData.name,
                        null,
                        modelData.attachPoint,
                        modelData.attachMaidSlotNo,
                        null,
                        modelData.pluginName,
                        true);
                    modelHackManager.CreateModel(model);

                    MTEUtils.Log("Create model: type={0} displayName={1} name={2} label={3} fileName={4} myRoomId={5} bgObjectId={6}",
                        model.info.type, model.displayName, model.name, model.info.label, model.info.fileName, model.info.myRoomId, model.info.bgObjectId);
                }
                else
                {
                    model.attachPoint = modelData.attachPoint;
                    model.attachMaidSlotNo = modelData.attachMaidSlotNo;

                    if (model.pluginName != modelData.pluginName)
                    {
                        modelHackManager.ChangePluginName(model, modelData.pluginName);
                    }
                    else
                    {
                        modelHackManager.UpdateAttachPoint(model);
                    }
                }
            }

            foreach (var model in modelList)
            {
                if (modelDataList.FindIndex(data => data.name == model.name) < 0)
                {
                    modelHackManager.DeleteModel(model);

                    MTEUtils.Log("Remove model: type={0} displayName={1} name={2} label={3} fileName={4} myRoomId={5} bgObjectId={6}",
                        model.info.type, model.displayName, model.name, model.info.label, model.info.fileName, model.info.myRoomId, model.info.bgObjectId);
                }
            }

            LateUpdate(true);
        }

        public override void OnLoad()
        {
            SetupModels(timeline.models);
        }

        public override void OnPluginDisable()
        {
            Reset();
        }

        public void Reset()
        {
            modelMap.Clear();
            models.Clear();
            boneMap.Clear();
            blendShapeMap.Clear();
            materialMap.Clear();
            modelNames.Clear();
            boneNames.Clear();
            blendShapeNames.Clear();
            materialNames.Clear();
            _prevUpdateFrame = -1;

            modelHackManager.DeleteAllModels();
        }

        public StudioModelStat CreateModelStat(
            string displayName,
            string modelName,
            int myRoomId,
            long bgObjectId,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName,
            bool visible)
        {
            var label = displayName;
            var fileName = modelName;

            var group = PluginUtils.ExtractGroup(label);
            if (group != 0)
            {
                label = PluginUtils.RemoveGroupSuffix(label);
                fileName = PluginUtils.RemoveGroupSuffix(fileName);
            }

            if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("MYR_", System.StringComparison.Ordinal))
            {
                myRoomId = PluginUtils.ExtractMyRoomId(fileName);
            }

            var info = FindOfficialObject(label, fileName, myRoomId, bgObjectId);
            return new StudioModelStat(info, group, transform, attachPoint, attachMaidSlotNo, obj, pluginName, visible);
        }

        public StudioModelStat CreateModelStat(
            string modelName,
            Transform transform,
            AttachPoint attachPoint,
            int attachMaidSlotNo,
            object obj,
            string pluginName,
            bool visible)
        {
            int myRoomId = 0;
            long bgObjectId = 0;

            return CreateModelStat(
                modelName,
                modelName,
                myRoomId,
                bgObjectId,
                transform,
                attachPoint,
                attachMaidSlotNo,
                obj,
                pluginName,
                visible);
        }

        public void DeleteModel(StudioModelStat model)
        {
            modelHackManager.DeleteModel(model);
            LateUpdate(true);

            timelineManager.RequestHistory("モデルの削除: " + model.displayName);
        }

        public void CreateModel(StudioModelStat model)
        {
            modelHackManager.CreateModel(model);
            LateUpdate(true);

            timelineManager.RequestHistory("モデルの追加: " + model.displayName);
        }

        public void UpdateAttachPoint(StudioModelStat model)
        {
            modelHackManager.UpdateAttachPoint(model);
            LateUpdate(true);

            //timelineManager.RequestHistory("アタッチ変更: " + model.displayName);
        }

        public void SetModelVisible(StudioModelStat model, bool visible)
        {
            modelHackManager.SetModelVisible(model, visible);
        }

        public void ChangePluginName(StudioModelStat model, string pluginName)
        {
            modelHackManager.ChangePluginName(model, pluginName);
            LateUpdate(true);

            timelineManager.RequestHistory("プラグイン変更: " + model.displayName);
        }

        public OfficialObjectInfo FindOfficialObject(
            string label,
            string fileName,
            int myRoomId,
            long bgObjectId)
        {
            OfficialObjectInfo objectInfo;

            //MTEUtils.LogDebug("FindOfficialObject: label={0} fileName={1} myRoomId={2} bgObjectId={3}",
            //    label, fileName, myRoomId, bgObjectId);

            if (!string.IsNullOrEmpty(fileName))
            {
                if (BGObjectFileNameMap.TryGetValue(fileName, out objectInfo))
                {
                    return objectInfo;
                }
            }

            if (myRoomId != 0)
            {
                if (MYRoomObjectIdMap.TryGetValue(myRoomId, out objectInfo))
                {
                    return objectInfo;
                }
            }

            if (bgObjectId != 0)
            {
                if (BGObjectIdMap.TryGetValue(bgObjectId, out objectInfo))
                {
                    return objectInfo;
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                if (OfficialObjectLabelMap.TryGetValue(label, out objectInfo))
                {
                    return objectInfo;
                }
            }

            if (label.EndsWith(".menu", System.StringComparison.Ordinal))
            {
                label = Path.GetFileName(label);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(fileName);

                if (GameUty.IsExistFile(fileName))
                {
                    var menu = ModMenuLoader.Load(fileName);
                    if (menu != null)
                    {
                        label = menu.name;
                    }
                }
            }

            var info = new OfficialObjectInfo
            {
                type = StudioModelType.Mod,
                label = label,
                fileName = string.IsNullOrEmpty(fileName) ? label : fileName,
                myRoomId = myRoomId,
                bgObjectId = bgObjectId,
            };
            _officialObjectLabelMap[label] = info;

            return info;
        }

        public void RegisterPhotoBGObject(PhotoBGObjectData data)
        {
            var info = new OfficialObjectInfo
            {
                label = data.name,
                bgObjectId = data.id,
            };

            data.GetFileNameAndType(out info.fileName, out info.type);

            //MTEUtils.LogDebug("PhotoBGObjectData: label={0} bgObjectId={1} fileName={2} type={3}",
            //    info.label, info.bgObjectId, info.fileName, info.type);

            _officialObjectLabelMap[info.label] = info;

            if (!string.IsNullOrEmpty(info.fileName))
            {
                _bgObjectFileNameMap[info.fileName] = info;
            }
            if (info.bgObjectId != 0)
            {
                _bgObjectIdMap[info.bgObjectId] = info;
            }
        }

        private void InitOfficialObjectMap()
        {
            if (_officialObjectLabelMap != null)
            {
                return;
            }

            var length = 256;
            _officialObjectLabelMap = new Dictionary<string, OfficialObjectInfo>(length);
            _bgObjectFileNameMap = new Dictionary<string, OfficialObjectInfo>(length);
            _myRoomObjectIdMap = new Dictionary<int, OfficialObjectInfo>(length);
            _bgObjectIdMap = new Dictionary<long, OfficialObjectInfo>(length);

            foreach (var data in PhotoBGObjectData.data)
            {
                RegisterPhotoBGObject(data);
            }

            foreach (var data in PlacementData.GetDatas(_ => true))
            {
                var prefabName = (!string.IsNullOrEmpty(data.resourceName)) ? data.resourceName : data.assetName;
                var fileName = string.Format("MYR_{0}", data.ID);

                var info = new OfficialObjectInfo
                {
                    label = data.drawName,
                    myRoomId = data.ID,
                    prefabName = prefabName,
                    fileName = fileName,
                    type = StudioModelType.MyRoom,
                };

                //MTEUtils.LogDebug("PlacementData: label={0} myRoomId={1} prefabName={2} fileName={3}",
                //    info.label, info.myRoomId, info.prefabName, info.fileName);

                _officialObjectLabelMap[info.label] = info;
                _myRoomObjectIdMap[info.myRoomId] = info;
            }

            LoadExtraModel(PluginUtils.ExtraModelCsvPath);
        }

        void LoadExtraModel(string path)
        {
            if (!File.Exists(path))
            {
                MTEUtils.LogWarning("CSVファイルが見つかりません path={0}", path);
                return;
            }

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    bool isFirstLine = true;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // ヘッダー行をスキップ
                        }

                        if (string.IsNullOrEmpty(line))
                        {
                            continue; // 空行をスキップ
                        }

                        string[] values = line.Split(',');
                        if (values.Length < 4)
                        {
                            MTEUtils.LogWarning("CSVファイルの形式が不正です line={0}", line);
                            continue;
                        }

                        if (values[2] == "" || values[3] == "")
                        {
                            continue; // 空行をスキップ
                        }

                        var info = new OfficialObjectInfo
                        {
                            label = values[2],
                            fileName = values[3],
                            type = (StudioModelType) int.Parse(values[0]),
                        };

                        if (_bgObjectFileNameMap.ContainsKey(info.fileName))
                        {
                            MTEUtils.LogWarning("ファイル名が重複していたのでスキップ fileName={0}", info.fileName);
                            continue;
                        }

                        _officialObjectLabelMap[info.label] = info;
                        _bgObjectFileNameMap[info.fileName] = info;
                    }
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
            }
        }

        private void UpdateTimelineModels()
        {
            if (timeline == null)
            {
                return;
            }

            MTEUtils.LogDebug("UpdateTimelineModels");

            var models = this.models;
            var timelineModels = timeline.models;

            foreach (var model in models)
            {
                model.info.Dump();
            }

            if (models.Count != timelineModels.Count)
            {
                timelineModels.Clear();
                foreach (var model in models)
                {
                    var timelineModel = new TimelineModelData(model);
                    timelineModels.Add(timelineModel);
                }
            }
            else
            {
                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var timelineModel = timelineModels[i];
                    timelineModel.FromModel(model);
                }
            }
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            Reset();
        }
    }
}