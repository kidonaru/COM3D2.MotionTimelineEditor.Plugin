using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StageLaserManager : MonoBehaviour, IManager
    {
        public List<StageLaserController> controllers = new List<StageLaserController>();
        public Dictionary<string, StageLaserController> controllerMap = new Dictionary<string, StageLaserController>();
        public List<string> controllerNames = new List<string>();
        public List<StageLaser> lasers = new List<StageLaser>();
        public Dictionary<string, StageLaser> laserMap = new Dictionary<string, StageLaser>();
        public List<string> laserNames = new List<string>();

        public static event UnityAction<string> onControllerAdded;
        public static event UnityAction<string> onControllerRemoved;
        public static event UnityAction<string> onLaserAdded;
        public static event UnityAction<string> onLaserRemoved;

        private static StageLaserManager _instance = null;
        public static StageLaserManager instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("StageLaserManager");
                    _instance = go.AddComponent<StageLaserManager>();
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

        public StageLaserController GetController(string name)
        {
            StageLaserController controller;
            if (controllerMap.TryGetValue(name, out controller))
            {
                return controller;
            }
            return null;
        }

        public StageLaser GetLaser(string name)
        {
            StageLaser laser;
            if (laserMap.TryGetValue(name, out laser))
            {
                return laser;
            }
            return null;
        }

        public void UpdateLasers()
        {
            controllerMap.Clear();
            controllerNames.Clear();
            lasers.Clear();
            laserMap.Clear();
            laserNames.Clear();

            foreach (var controller in controllers)
            {
                controllerMap.Add(controller.name, controller);
                controllerNames.Add(controller.name);

                foreach (var laser in controller.lasers)
                {
                    lasers.Add(laser);
                    laserMap.Add(laser.name, laser);
                    laserNames.Add(laser.name);
                }
            }

            MTEUtils.LogDebug("StageLaserManager: Laser list updated");

            foreach (var laser in lasers)
            {
                MTEUtils.LogDebug("laser: displayName={0} name={1}",
                    laser.displayName, laser.name);
            }

            UpdateTimelineLaser();
        }

        private void UpdateTimelineLaser()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.stageLaserCountList.Clear();

            foreach (var controller in controllers)
            {
                timeline.stageLaserCountList.Add(controller.lasers.Count);
            }
        }

        public void SetupLasers(List<int> laserCounts)
        {
            laserCounts = new List<int>(laserCounts);

            for (var i = 0; i < laserCounts.Count; i++)
            {
                MTEUtils.LogDebug("StageLaser.SetupLasers: [{0}]={1}", i, laserCounts[i]);
            }

            while (controllers.Count < laserCounts.Count)
            {
                AddController(false);
            }

            while (controllers.Count > laserCounts.Count)
            {
                RemoveController(false);
            }

            for (int i = 0; i < laserCounts.Count; i++)
            {
                var controller = controllers[i];
                var laserCount = laserCounts[i];

                while (controller.lasers.Count < laserCount)
                {
                    AddLaser(i, false);
                }

                while (controller.lasers.Count > laserCount)
                {
                    RemoveLaser(i, false);
                }
            }

            UpdateLasers();
        }

        public void OnLoad()
        {
            SetupLasers(timeline.stageLaserCountList);
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
            lasers.Clear();
            laserMap.Clear();
            laserNames.Clear();
        }

        public void AddController(bool notify)
        {
            var groupIndex = controllers.Count;
            var go = new GameObject("StageLaserController");
            go.transform.SetParent(transform, false);

            var controller = go.AddComponent<StageLaserController>();
            controller.groupIndex = groupIndex;
            controller.isManualUpdate = true;
            controllers.Add(controller);

            if (notify)
            {
                UpdateLasers();
                onControllerAdded?.Invoke(controller.name);
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
                UpdateLasers();
                onControllerRemoved?.Invoke(controllerName);
            }
        }

        public void AddLaser(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            var laserName = controller.AddLaser();
            
            if (notify)
            {
                UpdateLasers();
                onLaserAdded?.Invoke(laserName);
            }
        }

        public void RemoveLaser(int groupIndex, bool notify)
        {
            var controller = controllers[groupIndex];
            if (controller.lasers.Count == 0)
            {
                return;
            }

            var laserName = controller.RemoveLaser();

            if (notify)
            {
                UpdateLasers();
                onLaserRemoved?.Invoke(laserName);
            }
        }

        public void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            Reset();
        }
    }
}