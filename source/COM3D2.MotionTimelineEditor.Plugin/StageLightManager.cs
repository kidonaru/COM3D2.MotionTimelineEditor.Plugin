using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StageLightManager : MonoBehaviour
    {
        public List<StageLightController> controllers = new List<StageLightController>();
        public Dictionary<string, StageLightController> controllerMap = new Dictionary<string, StageLightController>();
        public List<string> controllerNames = new List<string>();
        public List<StageLight> lights = new List<StageLight>();
        public Dictionary<string, StageLight> lightMap = new Dictionary<string, StageLight>();
        public List<string> lightNames = new List<string>();

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

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

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

        public void Update()
        {
            if (studioHack == null || !studioHack.IsValid())
            {
                return;
            }
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

            PluginUtils.Log("StageLightManager: Light list updated");

            foreach (var light in lights)
            {
                PluginUtils.LogDebug("light: displayName={0} name={1}",
                    light.displayName, light.name);
            }
        }

        public void SetupLights(List<int> lightCounts)
        {
            lightCounts = new List<int>(lightCounts);

            while (controllers.Count < lightCounts.Count)
            {
                AddController();
            }

            while (controllers.Count > lightCounts.Count)
            {
                RemoveController();
            }

            for (int i = 0; i < lightCounts.Count; i++)
            {
                var controller = controllers[i];
                var lightCount = lightCounts[i];

                while (controller.lights.Count < lightCount)
                {
                    AddLight(i);
                }

                while (controller.lights.Count > lightCount)
                {
                    RemoveLight(i);
                }
            }

            UpdateLights();
        }

        public void OnPluginDisable()
        {
            Reset();
        }

        public void OnPluginEnable()
        {
            // SetupLightsが呼ばれるので不要
        }

        public void Reset()
        {
            foreach (var controller in controllers)
            {
                GameObject.Destroy(controller.gameObject);
            }
            controllers.Clear();

            UpdateLights();
        }

        public void AddController()
        {
            var groupIndex = controllers.Count;
            var go = new GameObject("StageLightController");
            go.transform.parent = transform;
            var controller = go.AddComponent<StageLightController>();
            controller.groupIndex = groupIndex;
            controllers.Add(controller);

            UpdateLights();
        }

        public void RemoveController()
        {
            if (controllers.Count > 0)
            {
                var controller = controllers[controllers.Count - 1];
                controllers.Remove(controller);
                GameObject.Destroy(controller.gameObject);

                UpdateLights();
            }
        }

        public void AddLight(int groupIndex)
        {
            var controller = controllers[groupIndex];
            var lightName = controller.AddLight();
            UpdateLights();

            if (onLightAdded != null)
            {
                onLightAdded.Invoke(lightName);
            }
        }

        public void RemoveLight(int groupIndex)
        {
            var controller = controllers[groupIndex];
            var lightName = controller.RemoveLight();
            UpdateLights();

            if (onLightRemoved != null)
            {
                onLightRemoved.Invoke(lightName);
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}