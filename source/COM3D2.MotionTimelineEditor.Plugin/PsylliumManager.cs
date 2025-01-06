using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PsylliumManager : MonoBehaviour, IManager
    {
        public List<PsylliumController> controllers = new List<PsylliumController>();
        public Dictionary<string, PsylliumController> controllerMap = new Dictionary<string, PsylliumController>();
        public List<string> controllerNames = new List<string>();

        public List<PsylliumArea> areas = new List<PsylliumArea>();
        public Dictionary<string, PsylliumArea> areaMap = new Dictionary<string, PsylliumArea>();
        public List<string> areaNames = new List<string>();

        public List<PsylliumPattern> patterns = new List<PsylliumPattern>();
        public Dictionary<string, PsylliumPatternConfig> patternConfigMap = new Dictionary<string, PsylliumPatternConfig>();
        public List<string> patternConfigNames = new List<string>();
        public Dictionary<string, PsylliumTransformConfig> transformConfigMap = new Dictionary<string, PsylliumTransformConfig>();
        public List<string> transformConfigNames = new List<string>();

        public List<PsylliumBarConfig> barConfigs = new List<PsylliumBarConfig>();
        public Dictionary<string, PsylliumBarConfig> barConfigMap = new Dictionary<string, PsylliumBarConfig>();
        public List<string> barConfigNames = new List<string>();

        public List<PsylliumHandConfig> handConfigs = new List<PsylliumHandConfig>();
        public Dictionary<string, PsylliumHandConfig> handConfigMap = new Dictionary<string, PsylliumHandConfig>();
        public List<string> handConfigNames = new List<string>();

        public static event UnityAction<string> onControllerAdded;
        public static event UnityAction<string> onControllerRemoved;
        public static event UnityAction<string> onAreaAdded;
        public static event UnityAction<string> onAreaRemoved;
        public static event UnityAction<string> onPatternAdded;
        public static event UnityAction<string> onPatternRemoved;

        private static PsylliumManager _instance = null;
        public static PsylliumManager instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PsylliumManager");
                    _instance = go.AddComponent<PsylliumManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        private static TimelineData timeline => TimelineManager.instance.timeline;

        public void Init()
        {
        }

        public void PreUpdate()
        {
        }

        public void Update()
        {
        }

        public void LateUpdate()
        {
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public PsylliumController GetController(string name)
        {
            return controllerMap.GetOrNull(name);
        }

        public PsylliumController GetController(int groupIndex)
        {
            foreach (var controller in controllers)
            {
                if (controller.groupIndex == groupIndex)
                {
                    return controller;
                }
            }

            return null;
        }

        public PsylliumArea GetArea(string name)
        {
            return areaMap.GetOrNull(name);
        }

        public PsylliumPattern GetPattern(int groupIndex, int patternIndex)
        {
            var controller = GetController(groupIndex);
            if (controller == null)
            {
                return null;
            }

            return controller.GetPattern(patternIndex);
        }

        public PsylliumPatternConfig GetPatternConfig(string name)
        {
            return patternConfigMap.GetOrNull(name);
        }

        public PsylliumTransformConfig GetTransformConfig(string name)
        {
            return transformConfigMap.GetOrNull(name);
        }

        public PsylliumBarConfig GetBarConfig(string name)
        {
            return barConfigMap.GetOrNull(name);
        }

        public PsylliumHandConfig GetHandConfig(string name)
        {
            return handConfigMap.GetOrNull(name);
        }

        public void ClearCache()
        {
            controllerMap.Clear();
            controllerNames.Clear();

            areas.Clear();
            areaMap.Clear();
            areaNames.Clear();

            patterns.Clear();
            patternConfigMap.Clear();
            patternConfigNames.Clear();
            transformConfigMap.Clear();
            transformConfigNames.Clear();

            barConfigs.Clear();
            barConfigMap.Clear();
            barConfigNames.Clear();

            handConfigs.Clear();
            handConfigMap.Clear();
            handConfigNames.Clear();
        }

        public void UpdateCache()
        {
            ClearCache();

            PluginUtils.Log("PsylliumManager: List updated");

            foreach (var controller in controllers)
            {
                PluginUtils.LogDebug("  Controller: displayName={0} name={1}",
                    controller.displayName, controller.name);

                controllerMap.Add(controller.name, controller);
                controllerNames.Add(controller.name);

                var barConfig = controller.barConfig;
                barConfigs.Add(barConfig);
                barConfigMap.Add(barConfig.name, barConfig);
                barConfigNames.Add(barConfig.name);

                var handConfig = controller.handConfig;
                handConfigs.Add(handConfig);
                handConfigMap.Add(handConfig.name, handConfig);
                handConfigNames.Add(handConfig.name);

                foreach (var area in controller.areas)
                {
                    areas.Add(area);
                    areaMap.Add(area.name, area);
                    areaNames.Add(area.name);
                }

                foreach (var pattern in controller.patterns)
                {
                    patterns.Add(pattern);
                    patternConfigMap.Add(pattern.patternConfig.name, pattern.patternConfig);
                    patternConfigNames.Add(pattern.patternConfig.name);
                    transformConfigMap.Add(pattern.transformConfig.name, pattern.transformConfig);
                    transformConfigNames.Add(pattern.transformConfig.name);
                }
            }

            foreach (var area in areas)
            {
                PluginUtils.LogDebug(" Area: displayName={0} name={1}",
                    area.displayName, area.name);
            }

            foreach (var pattern in patterns)
            {
                PluginUtils.LogDebug(" Pattern: displayName={0} name={1}",
                    pattern.patternConfig.displayName, pattern.patternConfig.name);
            }

            UpdateTimelineData();
        }

        private void UpdateTimelineData()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.psylliums.Clear();

            foreach (var controller in controllers)
            {
                var psylliumData = new TimelinePsylliumData();
                psylliumData.areaCount = controller.areas.Count;
                psylliumData.patternCount = controller.patterns.Count;
                timeline.psylliums.Add(psylliumData);
            }
        }

        public void Setup(List<TimelinePsylliumData> psylliumDatas)
        {
            for (var i = 0; i < psylliumDatas.Count; i++)
            {
                PluginUtils.LogDebug("Psyllium.Setup: [{0}] areaCount={1} patternCount={2}",
                        i, psylliumDatas[i].areaCount, psylliumDatas[i].patternCount);
            }

            while (controllers.Count < psylliumDatas.Count)
            {
                AddController(false);
            }

            while (controllers.Count > psylliumDatas.Count)
            {
                RemoveController(false);
            }

            for (int i = 0; i < psylliumDatas.Count; i++)
            {
                var controller = controllers[i];
                var psylliumData = psylliumDatas[i];
                var areaCount = psylliumData.areaCount;
                var patternCount = psylliumData.patternCount;

                while (controller.areas.Count < areaCount)
                {
                    AddArea(i, false);
                }
                while (controller.areas.Count > areaCount)
                {
                    RemoveArea(i, false);
                }

                while (controller.patterns.Count < patternCount)
                {
                    AddPattern(i, false);
                }
                while (controller.patterns.Count > patternCount)
                {
                    RemovePattern(i, false);
                }
            }

            UpdateCache();
        }

        public void OnLoad()
        {
            Setup(timeline.psylliums);
        }

        public void OnPluginDisable()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (var controller in controllers)
            {
                GameObject.Destroy(controller.gameObject);
            }
            controllers.Clear();

            ClearCache();
        }

        public void AddController(bool notify)
        {
            var groupIndex = controllers.Count;
            var go = new GameObject("PsylliumController");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = PsylliumController.DefaultPosition;
            go.transform.localEulerAngles = PsylliumController.DefaultEulerAngles;

            var controller = go.AddComponent<PsylliumController>();
            controller.groupIndex = groupIndex;
            controllers.Add(controller);

            if (notify)
            {
                UpdateCache();
                onControllerAdded?.Invoke(controller.name);
            }

            AddArea(groupIndex, notify);
            AddPattern(groupIndex, notify);
        }

        public void RemoveController(bool notify)
        {
            if (controllers.Count == 0)
            {
                return;
            }

            var controller = controllers[controllers.Count - 1];
            var controllerName = controller.name;
            controllers.Remove(controller);
            GameObject.Destroy(controller.gameObject);
            
            if (notify)
            {
                UpdateCache();
                onControllerRemoved?.Invoke(controllerName);
            }
        }

        public void AddArea(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            var area = controller.AddArea();
            
            if (notify)
            {
                UpdateCache();
                onAreaAdded?.Invoke(area.name);
            }
        }

        public void RemoveArea(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            if (controller.areas.Count == 0)
            {
                return;
            }

            var areaName = controller.RemoveAreaLast();

            if (notify)
            {
                UpdateCache();
                onAreaRemoved?.Invoke(areaName);
            }
        }

        public void AddPattern(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            var pattern = controller.AddPattern();
            
            if (notify)
            {
                UpdateCache();
                onPatternAdded?.Invoke(pattern.patternConfig.name);
            }
        }

        public void RemovePattern(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            if (controller.patterns.Count == 0)
            {
                return;
            }

            var patternName = controller.RemovePatternLast();

            if (notify)
            {
                UpdateCache();
                onPatternRemoved?.Invoke(patternName);
            }
        }

        public void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            Reset();
        }
    }
}