using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PsylliumManager : MonoBehaviour
    {
        public List<PsylliumController> controllers = new List<PsylliumController>();
        public Dictionary<string, PsylliumController> controllerMap = new Dictionary<string, PsylliumController>();
        public List<string> controllerNames = new List<string>();

        public List<PsylliumArea> areas = new List<PsylliumArea>();
        public Dictionary<string, PsylliumArea> areaMap = new Dictionary<string, PsylliumArea>();
        public List<string> areaNames = new List<string>();

        public List<PsylliumBarConfig> barConfigs = new List<PsylliumBarConfig>();
        public Dictionary<string, PsylliumBarConfig> barConfigMap = new Dictionary<string, PsylliumBarConfig>();
        public List<string> barConfigNames = new List<string>();

        public List<PsylliumHandConfig> handConfigs = new List<PsylliumHandConfig>();
        public Dictionary<string, PsylliumHandConfig> handConfigMap = new Dictionary<string, PsylliumHandConfig>();
        public List<string> handConfigNames = new List<string>();

        public List<PsylliumAnimationConfig> animationConfigs = new List<PsylliumAnimationConfig>();
        public Dictionary<string, PsylliumAnimationConfig> animationConfigMap = new Dictionary<string, PsylliumAnimationConfig>();
        public List<string> animationConfigNames = new List<string>();

        public static event UnityAction onSetup;
        public static event UnityAction<string> onControllerAdded;
        public static event UnityAction<string> onControllerRemoved;
        public static event UnityAction<string> onAreaAdded;
        public static event UnityAction<string> onAreaRemoved;

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

        private static StudioHackBase studioHack => StudioHackManager.studioHack;
        private static TimelineData timeline => TimelineManager.instance.timeline;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            SceneManager.sceneLoaded += OnChangedSceneLevel;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnChangedSceneLevel;

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

        public PsylliumBarConfig GetBarConfig(string name)
        {
            return barConfigMap.GetOrNull(name);
        }

        public PsylliumHandConfig GetHandConfig(string name)
        {
            return handConfigMap.GetOrNull(name);
        }

        public PsylliumAnimationConfig GetAnimationConfig(string name)
        {
            return animationConfigMap.GetOrNull(name);
        }

        public void Update()
        {
            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }
        }

        public void ClearCache()
        {
            controllerMap.Clear();
            controllerNames.Clear();

            areas.Clear();
            areaMap.Clear();
            areaNames.Clear();

            barConfigs.Clear();
            barConfigMap.Clear();
            barConfigNames.Clear();

            handConfigs.Clear();
            handConfigMap.Clear();
            handConfigNames.Clear();

            animationConfigs.Clear();
            animationConfigMap.Clear();
            animationConfigNames.Clear();
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

                var animationConfig = controller.animationConfig;
                animationConfigs.Add(animationConfig);
                animationConfigMap.Add(animationConfig.name, animationConfig);
                animationConfigNames.Add(animationConfig.name);

                foreach (var area in controller.areas)
                {
                    areas.Add(area);
                    areaMap.Add(area.name, area);
                    areaNames.Add(area.name);
                }
            }

            foreach (var area in areas)
            {
                PluginUtils.LogDebug(" Area: displayName={0} name={1}",
                    area.displayName, area.name);
            }

            UpdateAreaCount();
        }

        private void UpdateAreaCount()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.psylliumAreaCountList.Clear();

            foreach (var controller in controllers)
            {
                timeline.psylliumAreaCountList.Add(controller.areas.Count);
            }
        }

        public void SetupAreas(List<int> areaCounts)
        {
            areaCounts = new List<int>(areaCounts);

            for (var i = 0; i < areaCounts.Count; i++)
            {
                PluginUtils.LogDebug("Psyllium.SetupAreas: [{0}]={1}", i, areaCounts[i]);
            }

            while (controllers.Count < areaCounts.Count)
            {
                AddController(false);
            }

            while (controllers.Count > areaCounts.Count)
            {
                RemoveController(false);
            }

            for (int i = 0; i < areaCounts.Count; i++)
            {
                var controller = controllers[i];
                var areaCount = areaCounts[i];

                while (controller.areas.Count < areaCount)
                {
                    AddArea(i, false);
                }

                while (controller.areas.Count > areaCount)
                {
                    RemoveArea(i, false);
                }
            }

            UpdateCache();

            if (onSetup != null)
            {
                onSetup.Invoke();
            }
        }

        public void OnPluginDisable()
        {
            Reset();
        }

        public void OnPluginEnable()
        {
            // SetupAreasが呼ばれるので不要
        }

        public void Reset()
        {
            foreach (var controller in controllers)
            {
                GameObject.Destroy(controller.gameObject);
            }
            controllers.Clear();

            UpdateCache();
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

                if (onControllerAdded != null)
                {
                    onControllerAdded.Invoke(controller.name);
                }
            }

            AddArea(groupIndex, notify);
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

                if (onControllerRemoved != null)
                {
                    onControllerRemoved.Invoke(controllerName);
                }
            }
        }

        public void AddArea(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            var area = controller.AddArea();
            
            if (notify)
            {
                UpdateCache();

                if (onAreaAdded != null)
                {
                    onAreaAdded.Invoke(area.name);
                }
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

                if (onAreaRemoved != null)
                {
                    onAreaRemoved.Invoke(areaName);
                }
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}