using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ModelHackManager
    {
        private Dictionary<string, IModelHack> modelHackMap = new Dictionary<string, IModelHack>();

        private List<StudioModelStat> _modelList = new List<StudioModelStat>();
        public List<StudioModelStat> modelList
        {
            get
            {
                _modelList.Clear();

                _modelList.AddRange(studioHack.modelList);

                foreach (var modelHack in modelHackMap.Values)
                {
                    if (modelHack.IsValid())
                    {
                        _modelList.AddRange(modelHack.modelList);
                    }
                }

                return _modelList;
            }
        }

        private List<string> _pluginNames = new List<string>();
        public List<string> pluginNames
        {
            get
            {
                _pluginNames.Clear();

                _pluginNames.Add(studioHack.pluginName);

                foreach (var modelHack in modelHackMap.Values)
                {
                    if (modelHack.IsValid())
                    {
                        _pluginNames.Add(modelHack.pluginName);
                    }
                }

                return _pluginNames;
            }
        }

        private static ModelHackManager _instance;
        public static ModelHackManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModelHackManager();
                }

                return _instance;
            }
        }

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private ModelHackManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
        }

        public void Register(ModelHackBase studioHack)
        {
            if (studioHack == null || !studioHack.Init())
            {
                return;
            }

            modelHackMap[studioHack.pluginName] = studioHack;
        }

        public IModelHack GetOrDefault(string pluginName)
        {
            IModelHack modelHack;
            if (!string.IsNullOrEmpty(pluginName) &&
                modelHackMap.TryGetValue(pluginName, out modelHack))
            {
                if (modelHack.IsValid())
                {
                    return modelHack;
                }
            }

            // 見つからない場合はアクティブなStudioHackを返す
            return studioHack;
        }

        public void DeleteAllModels()
        {
            foreach (var modelHack in modelHackMap.Values)
            {
                modelHack.DeleteAllModels();
            }
        }

        public void DeleteModel(StudioModelStat model)
        {
            var modelHack = GetOrDefault(model.pluginName);
            if (modelHack != null)
            {
                modelHack.DeleteModel(model);
            }
        }

        public void CreateModel(StudioModelStat model)
        {
            var modelHack = GetOrDefault(model.pluginName);
            if (modelHack != null)
            {
                modelHack.CreateModel(model);
            }
        }

        public void UpdateAttachPoint(StudioModelStat model)
        {
            var modelHack = GetOrDefault(model.pluginName);
            if (modelHack != null)
            {
                modelHack.UpdateAttachPoint(model);
            }
        }

        public void ChangePluginName(StudioModelStat model, string pluginName)
        {
            var prevModelHack = GetOrDefault(model.pluginName);
            var nextModelHack = GetOrDefault(pluginName);

            if (nextModelHack != prevModelHack)
            {
                prevModelHack.DeleteModel(model);
                nextModelHack.CreateModel(model);
            }
        }

        public void Update()
        {
        }

        public void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            if (!config.pluginEnabled)
            {
                return;
            }

            foreach (var modelHack in modelHackMap.Values)
            {
                modelHack.OnChangedSceneLevel(sceneName, sceneMode);
            }
        }
    }
}