using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StageLightManager : MonoBehaviour, IManager
    {
        public List<StageLightController> controllers = new List<StageLightController>();
        public Dictionary<string, StageLightController> controllerMap = new Dictionary<string, StageLightController>();
        public List<string> controllerNames = new List<string>();
        public List<StageLight> lights = new List<StageLight>();
        public Dictionary<string, StageLight> lightMap = new Dictionary<string, StageLight>();
        public List<string> lightNames = new List<string>();

        public static event UnityAction<string> onControllerAdded;
        public static event UnityAction<string> onControllerRemoved;
        public static event UnityAction<string> onLightAdded;
        public static event UnityAction<string> onLightRemoved;

        private static StageLightManager _instance = null;
        public static StageLightManager instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("StageLightManager");
                    _instance = go.AddComponent<StageLightManager>();
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

        public StageLightController GetController(string name)
        {
            StageLightController controller;
            if (controllerMap.TryGetValue(name, out controller))
            {
                return controller;
            }
            return null;
        }

        public StageLight GetLight(string name)
        {
            StageLight light;
            if (lightMap.TryGetValue(name, out light))
            {
                return light;
            }
            return null;
        }

        public void UpdateLights()
        {
            controllerMap.Clear();
            controllerNames.Clear();
            lights.Clear();
            lightMap.Clear();
            lightNames.Clear();

            foreach (var controller in controllers)
            {
                controllerMap.Add(controller.name, controller);
                controllerNames.Add(controller.name);

                foreach (var light in controller.lights)
                {
                    lights.Add(light);
                    lightMap.Add(light.name, light);
                    lightNames.Add(light.name);
                }
            }

            PluginUtils.LogDebug("StageLightManager: Light list updated");

            foreach (var light in lights)
            {
                PluginUtils.LogDebug("light: displayName={0} name={1}",
                    light.displayName, light.name);
            }

            UpdateLightCount();
        }

        private void UpdateLightCount()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.stageLightCountList.Clear();

            foreach (var controller in controllers)
            {
                timeline.stageLightCountList.Add(controller.lights.Count);
            }
        }

        public void SetupLights(List<int> lightCounts)
        {
            lightCounts = new List<int>(lightCounts);

            for (var i = 0; i < lightCounts.Count; i++)
            {
                PluginUtils.LogDebug("StageLight.SetupLights: [{0}]={1}", i, lightCounts[i]);
            }

            while (controllers.Count < lightCounts.Count)
            {
                AddController(false);
            }

            while (controllers.Count > lightCounts.Count)
            {
                RemoveController(false);
            }

            for (int i = 0; i < lightCounts.Count; i++)
            {
                var controller = controllers[i];
                var lightCount = lightCounts[i];

                while (controller.lights.Count < lightCount)
                {
                    AddLight(i, false);
                }

                while (controller.lights.Count > lightCount)
                {
                    RemoveLight(i, false);
                }
            }

            UpdateLights();
        }

        public void OnLoad()
        {
            SetupLights(timeline.stageLightCountList);
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
            controllerMap.Clear();
            controllerNames.Clear();
            lights.Clear();
            lightMap.Clear();
            lightNames.Clear();
        }

        public void AddController(bool notify)
        {
            var groupIndex = controllers.Count;
            var go = new GameObject("StageLightController");
            go.transform.parent = transform;
            var controller = go.AddComponent<StageLightController>();
            controller.groupIndex = groupIndex;
            controller.isManualUpdate = true;
            controllers.Add(controller);
            
            if (notify)
            {
                UpdateLights();

                if (onControllerAdded != null)
                {
                    onControllerAdded.Invoke(controller.name);
                }
            }
            
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
                UpdateLights();

                if (onControllerRemoved != null)
                {
                    onControllerRemoved.Invoke(controllerName);
                }
            }
        }

        public void AddLight(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            var lightName = controller.AddLight();
            
            if (notify)
            {
                UpdateLights();

                if (onLightAdded != null)
                {
                    onLightAdded.Invoke(lightName);
                }
            }
        }

        public void RemoveLight(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            if (controller.lights.Count == 0)
            {
                return;
            }

            var lightName = controller.RemoveLight();

            if (notify)
            {
                UpdateLights();

                if (onLightRemoved != null)
                {
                    onLightRemoved.Invoke(lightName);
                }
            }
        }

        public void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            Reset();
        }
    }
}