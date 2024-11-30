using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BGModelInfo
    {
        public string sourceName;
        public int depth;
        public GameObject gameObject;
        public string displayName;

        public bool initialVisible;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 initialScale;

        public BGModelInfo(string sourceName, int depth, GameObject gameObject)
        {
            this.sourceName = sourceName;
            this.depth = depth;
            this.gameObject = gameObject;
            this.displayName = BoneUtils.ConvertToBoneName(sourceName);

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
                if (_modelInfoMap.Count > 0)
                {
                    return _modelInfoMap;
                }

                var bgObject = this.bgObject;
                if (bgObject != null)
                {
                    foreach (Transform child in bgObject.transform)
                    {
                        AddModelInfo(child.gameObject);
                    }
                }

                return _modelInfoMap;
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
            var info = GetModelInfo(sourceName);
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

        private void AddModelInfo(GameObject go, string parentName = "", int depth = 0)
        {
            if (go == null)
            {
                return;
            }

            var name = parentName == "" ? go.name : parentName + "/" + go.name;
            var info = new BGModelInfo(name, depth, go);
            _modelInfoMap[name] = info;

            foreach (Transform child in go.transform)
            {
                AddModelInfo(child.gameObject, name, depth + 1);
            }
        }

        private void SortModels()
        {
            models.Sort((a, b) => 
            {
                var cmp = a.sourceName.CompareTo(b.sourceName);
                if (cmp != 0)
                {
                    return cmp;
                }

                return a.group - b.group;
            });

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