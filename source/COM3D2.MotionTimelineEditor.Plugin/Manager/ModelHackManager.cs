using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ModelHackManager : ManagerBase
    {
        private Dictionary<string, IModelHack> modelHackMap = new Dictionary<string, IModelHack>();
        private Dictionary<string, int> _modelGroupMap = new Dictionary<string, int>();

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

                FixGroup(_modelList);

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

        private ModelHackManager()
        {
        }

        public void Register(IModelHack modelHack)
        {
            if (modelHack == null || !modelHack.Init())
            {
                return;
            }

            modelHackMap[modelHack.pluginName] = modelHack;
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
            if (studioHack != null && studioHack.IsValid())
            {
                studioHack.DeleteAllModels();
            }

            foreach (var modelHack in modelHackMap.Values)
            {
                if (modelHack.IsValid())
                {
                    modelHack.DeleteAllModels();
                }
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

        public void SetModelVisible(StudioModelStat model, bool visible)
        {
            var modelHack = GetOrDefault(model.pluginName);
            if (modelHack != null)
            {
                modelHack.SetModelVisible(model, visible);
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

        /// <summary>
        /// groupの修正
        /// </summary>
        /// <param name="models"></param>
        private void FixGroup(List<StudioModelStat> models)
        {
            _modelGroupMap.Clear();

            foreach (var model in models)
            {
                int group = 0;

                if (_modelGroupMap.TryGetValue(model.info.fileName, out group))
                {
                    group++;
                    if (group == 1) group++; // 1は使わない
                }

                model.SetGroup(group);
                _modelGroupMap[model.info.fileName] = group;
            }
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            foreach (var modelHack in modelHackMap.Values)
            {
                modelHack.OnChangedSceneLevel(scene, sceneMode);
            }
        }
    }
}