using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BGModelInfo
    {
        public string sourceName;
        public int group;
        public string name;
        public int depth;
        public GameObject gameObject;
        public string displayName;

        public bool initialVisible;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 initialScale;

        public BGModelInfo(string sourceName, int depth, GameObject gameObject, int group = 0)
        {
            this.sourceName = sourceName;
            this.group = group;
            this.name = sourceName + PluginUtils.GetGroupSuffix(group);
            this.depth = depth;
            this.gameObject = gameObject;
            this.displayName = BoneUtils.ConvertToBoneName(name);

            this.initialVisible = gameObject.activeSelf;
            this.initialPosition = gameObject.transform.localPosition;
            this.initialRotation = gameObject.transform.localRotation;
            this.initialScale = gameObject.transform.localScale;
        }
    }

    public class BGModelManager
    {
        public List<BGModelStat> models = new List<BGModelStat>();
        public List<string> modelNames = new List<string>();
        private Dictionary<string, BGModelStat> _modelMap = new Dictionary<string, BGModelStat>();
        private Dictionary<string, List<BGModelStat>> _modelsMap = new Dictionary<string, List<BGModelStat>>();

        private GameObject _prevBgObject = null;

        private Dictionary<string, BGModelInfo> _modelInfoMap = new Dictionary<string, BGModelInfo>(64);
        public Dictionary<string, BGModelInfo> modelInfoMap
        {
            get
            {
                SetupModelInfo();
                return _modelInfoMap;
            }
        }

        private List<BGModelInfo> _modelInfoList = new List<BGModelInfo>(64);
        public List<BGModelInfo> modelInfoList
        {
            get
            {
                SetupModelInfo();
                return _modelInfoList;
            }
        }

        private GameObject bgObject => GameMain.Instance.BgMgr?.BgObject;

        public static event UnityAction onModelSetup;
        public static event UnityAction<BGModelStat> onModelAdded;
        public static event UnityAction<BGModelStat> onModelRemoved;

        private static BGModelManager _instance = null;
        public static BGModelManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BGModelManager();
                }
                return _instance;
            }
        }

        private static TimelineManager timelineManager => TimelineManager.instance;
        private static TimelineData timeline => timelineManager.timeline;

        private BGModelManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
        }

        public BGModelStat GetModel(string name)
        {
            BGModelStat model;
            if (_modelMap.TryGetValue(name, out model))
            {
                return model;
            }
            return null;
        }

        public BGModelStat GetModel(int index)
        {
            if (index < 0 || index >= modelNames.Count)
            {
                return null;
            }
            return GetModel(modelNames[index]);
        }

        public List<BGModelStat> GetModels(string sourceName)
        {
            List<BGModelStat> models;
            if (_modelsMap.TryGetValue(sourceName, out models))
            {
                return models;
            }
            return new List<BGModelStat>();
        }

        public void LateUpdate()
        {
            var bgObject = this.bgObject;

            if (bgObject != _prevBgObject)
            {
                _prevBgObject = bgObject;
                _modelInfoMap.Clear();
                _modelInfoList.Clear();
                Reset();
                SetupModels(timeline.bgModels);
            }
        }

        public void SetupModels(List<TimelineBGModelData> modelDataList)
        {
            foreach (var modelData in modelDataList)
            {
                var model = GetModel(modelData.name);
                if (model == null)
                {
                    model = AddModel(modelData.sourceName, modelData.group, false);
                }
            }

            var storedModels = models.ToArray();
            foreach (var model in storedModels)
            {
                if (modelDataList.FindIndex(
                    data => data.sourceName == model.sourceName && data.group == model.group) < 0)
                {
                    DeleteModel(model, false);
                }
            }

            _prevBgObject = bgObject;

            onModelSetup?.Invoke();
        }

        public void OnPluginDisable()
        {
            Reset();
        }

        public void OnPluginEnable()
        {
            // SetupModelsが呼ばれるので不要
        }

        public void Reset()
        {
            foreach (var model in models)
            {
                model.Destroy();
            }

            models.Clear();
            modelNames.Clear();
            _modelMap.Clear();
            _modelsMap.Clear();
        }

        public BGModelInfo GetModelInfo(string sourceName)
        {
            return modelInfoMap.GetOrNull(sourceName);
        }

        public BGModelStat AddModel(string sourceName, int group, bool notify = true)
        {
            var name = sourceName + PluginUtils.GetGroupSuffix(group);
            var info = GetModelInfo(name);
            if (info == null && group > 0)
            {
                var sourceInfo = GetModelInfo(sourceName);
                if (sourceInfo != null)
                {
                    info = CopyModelInfo(sourceInfo, group);
                }
            }

            var model = new BGModelStat(sourceName, group, info);

            models.Add(model);
            modelNames.Add(model.name);
            _modelMap[model.name] = model;
            _modelsMap.GetOrCreate(sourceName).Add(model);

            SortModels();

            if (notify)
            {
                onModelAdded?.Invoke(model);
                timelineManager.RequestHistory("背景モデルの追加: " + model.displayName);
            }

            PluginUtils.LogDebug("背景モデルを追加しました: " + model.displayName);

            return model;
        }

        public BGModelStat AddModelBySourceName(string sourceName, bool notify = true)
        {
            var models = GetModels(sourceName);

            var group = 0;
            while (models.FindIndex(model => model.group == group) >= 0)
            {
                group++;
                if (group == 1) group++; // 1は使わない
            }

            return AddModel(sourceName, group, notify);
        }

        public void DeleteModel(BGModelStat model, bool notify = true)
        {
            models.Remove(model);
            modelNames.Remove(model.name);
            _modelMap.Remove(model.name);
            _modelsMap.GetOrNull(model.sourceName)?.Remove(model);

            if (notify)
            {
                onModelRemoved?.Invoke(model);
                timelineManager.RequestHistory("背景モデルの削除: " + model.displayName);
            }

            model.Destroy();

            var info = model.info;
            if (info.group > 0)
            {
                DeleteModelInfo(info.name);
            }

            PluginUtils.LogDebug("背景モデルを削除しました: " + model.displayName);
        }

        public void DeleteModelBySourceName(string sourceName, bool notify = true)
        {
            var models = GetModels(sourceName);
            if (models.Count > 0)
            {
                DeleteModel(models[models.Count - 1], notify);
            }
        }

        private void SetupModelInfo()
        {
            if (_modelInfoMap.Count > 0)
            {
                return;
            }

            var bgObject = this.bgObject;
            if (bgObject != null)
            {
                foreach (Transform child in bgObject.transform)
                {
                    AddModelInfo(child.gameObject);
                }
                SortModelInfoList();
            }
        }

        private BGModelInfo AddModelInfo(GameObject go, string parentName = "", int depth = 0)
        {
            if (go == null)
            {
                return null;
            }

            var name = parentName == "" ? go.name : parentName + "/" + go.name;
            var info = new BGModelInfo(name, depth, go);
            _modelInfoMap[name] = info;
            _modelInfoList.Add(info);

            foreach (Transform child in go.transform)
            {
                AddModelInfo(child.gameObject, name, depth + 1);
            }

            return info;
        }

        private BGModelInfo AddModelInfo(
            GameObject go,
            string parentName,
            int depth,
            string sourceName,
            int group)
        {
            if (go == null)
            {
                return null;
            }

            var name = parentName == "" ? go.name : parentName + "/" + go.name;
            var info = new BGModelInfo(sourceName, depth, go, group);
            _modelInfoMap[name] = info;
            _modelInfoList.Add(info);

            foreach (Transform child in go.transform)
            {
                AddModelInfo(child.gameObject, name, depth + 1);
            }

            return info;
        }

        private void DeleteModelInfo(string name)
        {
            var info = _modelInfoMap.GetOrNull(name);
            if (info == null)
            {
                return;
            }

            _modelInfoMap.Remove(name);
            _modelInfoList.Remove(info);

            foreach (Transform child in info.gameObject.transform)
            {
                var childName = name + "/" + child.name;
                DeleteModelInfo(childName);
            }

            Object.Destroy(info.gameObject);
        }

        private BGModelInfo CopyModelInfo(BGModelInfo sourceinfo, int group)
        {
            var name = sourceinfo.sourceName + PluginUtils.GetGroupSuffix(group);

            var sourceObj = sourceinfo.gameObject;
            var go = Object.Instantiate(sourceObj);
            var transform = go.transform;
            transform.SetParent(sourceObj.transform.parent, false);
            transform.localPosition = sourceinfo.initialPosition;
            transform.localRotation = sourceinfo.initialRotation;
            transform.localScale = sourceinfo.initialScale;

            go.name = BoneUtils.ConvertToBoneName(name);
            var parentPath = BoneUtils.ConvertToParentPath(name);

            var info = AddModelInfo(go, parentPath, sourceinfo.depth, sourceinfo.sourceName, group);
            SortModelInfoList();

            return info;
        }

        private static int ComparePathNames(string nameA, string nameB)
        {
            var aParts = nameA.Split('/');
            var bParts = nameB.Split('/');

            // 共通のパス部分で先に比較
            for (int i = 0; i < Mathf.Min(aParts.Length, bParts.Length); i++)
            {
                var cmp = string.Compare(aParts[i], bParts[i], System.StringComparison.Ordinal);
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            if (aParts.Length != bParts.Length)
            {
                return aParts.Length - bParts.Length;
            }

            return string.Compare(nameA, nameB, System.StringComparison.Ordinal);
        }

        private void SortModelInfoList()
        {
            _modelInfoList.Sort((a, b) => ComparePathNames(a.name, b.name));
        }

        private void SortModels()
        {
            models.Sort((a, b) => ComparePathNames(a.name, b.name));

            modelNames.Clear();
            foreach (var model in models)
            {
                modelNames.Add(model.name);
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}