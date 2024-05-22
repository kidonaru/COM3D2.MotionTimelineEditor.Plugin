using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MyRoomCustom;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

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
    }

    public class StudioModelManager
    {
        public Dictionary<string, StudioModelStat> modelMap = new Dictionary<string, StudioModelStat>();
        public Dictionary<string, StudioModelBoneStat> boneMap = new Dictionary<string, StudioModelBoneStat>();
        public List<string> modelNames = new List<string>();
        public List<string> boneNames = new List<string>();

        public static event UnityAction<StudioModelStat> onModelAdded;
        public static event UnityAction<StudioModelStat> onModelRemoved;

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

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
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
            SceneManager.sceneLoaded += OnChangedSceneLevel;
            TimelineManager.onRefresh += OnRefresh;
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

        public StudioModelBoneStat GetBone(string name)
        {
            StudioModelBoneStat bone;
            if (boneMap.TryGetValue(name, out bone))
            {
                return bone;
            }
            return null;
        }

        public StudioModelBoneStat GetBone(int index)
        {
            if (index < 0 || index >= boneNames.Count)
            {
                return null;
            }
            return GetBone(boneNames[index]);
        }

        private Dictionary<string, int> _modelGroupMap = new Dictionary<string, int>();

        public void LateUpdate()
        {
            var modelList = studioHack.modelList;
            FixGroup(modelList);

            var addedModels = new List<StudioModelStat>();
            var removedModels = new List<StudioModelStat>();
            var updated = false;

            foreach (var model in modelList)
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    continue;
                }

                StudioModelStat cachedModel;
                if (!modelMap.TryGetValue(model.name, out cachedModel))
                {
                    cachedModel = new StudioModelStat();
                    modelMap[model.name] = cachedModel;
                    addedModels.Add(cachedModel);
                }

                if (cachedModel.transform != model.transform)
                {
                    cachedModel.FromModel(model);
                    updated = true;
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
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                modelNames.Clear();
                modelNames.AddRange(modelMap.Keys);

                boneMap.Clear();
                boneNames.Clear();

                foreach (var model in modelMap.Values)
                {
                    foreach (var bone in model.bones)
                    {
                        boneMap[bone.name] = bone;
                        boneNames.Add(bone.name);
                    }
                }

                PluginUtils.Log("StudioModelManager: Model list updated");

                foreach (var model in modelMap.Values)
                {
                    PluginUtils.Log("model: type={0} displayName={1} name={2} label={3} fileName={4} myRoomId={5} bgObjectId={6}",
                        model.info.type, model.displayName, model.name, model.info.label, model.info.fileName, model.info.myRoomId, model.info.bgObjectId);

                    foreach (var bone in model.bones)
                    {
                        PluginUtils.Log("  bone: name={0}", bone.name);
                    }
                }

                foreach (var model in addedModels)
                {
                    if (onModelAdded != null)
                    {
                        onModelAdded.Invoke(model);
                    }
                }

                foreach (var model in removedModels)
                {
                    if (onModelRemoved != null)
                    {
                        onModelRemoved.Invoke(model);
                    }
                }
            }
        }

        public void OnPluginDisable()
        {
            Reset();
        }

        public void OnPluginEnable()
        {
            LateUpdate();
        }

        public void Reset()
        {
            modelMap.Clear();
            boneMap.Clear();
            modelNames.Clear();
            boneNames.Clear();
        }

        public StudioModelStat CreateModelStat(
            string displayName,
            string modelName,
            int myRoomId,
            long bgObjectId,
            Transform transform)
        {
            var label = displayName;
            var fileName = modelName;

            var group = ExtractGroup(label);
            if (group != 0)
            {
                label = RemoveGroupSuffix(label);
                fileName = RemoveGroupSuffix(fileName);
            }

            if (fileName.StartsWith("MYR_", System.StringComparison.Ordinal))
            {
                myRoomId = ExtractMyRoomId(fileName);
            }

            var info = FindOfficialObject(label, fileName, myRoomId, bgObjectId);
            return new StudioModelStat(info, group, transform);
        }

        public StudioModelStat CreateModelStat(
            string modelName,
            Transform transform)
        {
            int myRoomId = 0;
            long bgObjectId = 0;

            return CreateModelStat(
                modelName,
                modelName,
                myRoomId,
                bgObjectId,
                transform);
        }

        private OfficialObjectInfo FindOfficialObject(
            string label,
            string fileName,
            int myRoomId,
            long bgObjectId)
        {
            OfficialObjectInfo objectInfo;

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

            if (fileName.EndsWith(".menu", System.StringComparison.Ordinal))
            {
                fileName = Path.GetFileName(fileName);
            }

            var info = new OfficialObjectInfo
            {
                type = StudioModelType.Mod,
                label = label,
                fileName = string.IsNullOrEmpty(fileName) ? label : fileName,
                myRoomId = myRoomId,
                bgObjectId = bgObjectId,
            };
            OfficialObjectLabelMap[label] = info;

            return info;
        }

        /// <summary>
        /// groupの修正
        /// </summary>
        /// <param name="models"></param>
        private void FixGroup(List<StudioModelStat> models)
        {
            _modelGroupMap.Clear();

            foreach (var model in models)
            {
                //PluginUtils.Log("FixGroup: type={0} displayName={1} name={2} label={3} fileName={4} myRoomId={5} bgObjectId={6}",
                //        model.info.type, model.displayName, model.name, model.info.label, model.info.fileName, model.info.myRoomId, model.info.bgObjectId);
                int group = model.group;
                if (group != 0)
                {
                    continue;
                }

                if (_modelGroupMap.TryGetValue(model.info.fileName, out group))
                {
                    group++;
                    if (group == 1) group++; // 1は使わない
                }

                model.SetGroup(group);
                _modelGroupMap[model.info.fileName] = group;
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
                var info = new OfficialObjectInfo
                {
                    label = data.name,
                    bgObjectId = data.id,
                };
                if (!string.IsNullOrEmpty(data.create_prefab_name))
                {
                    info.fileName = data.create_prefab_name;
                    info.type = StudioModelType.Prefab;
                }
                else if (!string.IsNullOrEmpty(data.create_asset_bundle_name))
                {
                    info.fileName = data.create_asset_bundle_name;
                    info.type = StudioModelType.Asset;
                }
                else
                {
                    info.fileName = data.name;
                    info.type = StudioModelType.Mod;
                }

                //PluginUtils.LogDebug("PhotoBGObjectData: label={0} bgObjectId={1} fileName={2} type={3}",
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

                //PluginUtils.LogDebug("PlacementData: label={0} myRoomId={1} prefabName={2} fileName={3}",
                //    info.label, info.myRoomId, info.prefabName, info.fileName);

                _officialObjectLabelMap[info.label] = info;
                _myRoomObjectIdMap[info.myRoomId] = info;
            }
        }

        private static readonly Regex _regexGroup = new Regex(@"\(\d+\)$", RegexOptions.Compiled);
        private static readonly Regex _regexMyRoomId = new Regex(@"MYR_\d+", RegexOptions.Compiled);

        public static int ExtractGroup(string input)
        {
            var match = _regexGroup.Match(input);
            if (match.Success)
            {
                return int.Parse(match.Value.Substring(1, match.Value.Length - 2));
            }
            return 0;
        }

        public static string RemoveGroupSuffix(string input)
        {
            return _regexGroup.Replace(input, "");
        }

        public static int ExtractMyRoomId(string input)
        {
            var match = _regexMyRoomId.Match(input);
            if (match.Success)
            {
                return int.Parse(match.Value.Substring(4));
            }
            return 0;
        }

        public static string GetGroupSuffix(int group)
        {
            if (group > 0)
            {
                return string.Format(" ({0})", group);
            }
            return "";
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }

        private void OnRefresh()
        {
            //Reset();
        }
    }
}