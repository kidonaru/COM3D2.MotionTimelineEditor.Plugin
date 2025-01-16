using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioLightManager : ManagerBase
    {
        private Dictionary<string, StudioLightStat> lightMap = new Dictionary<string, StudioLightStat>();

        public List<StudioLightStat> lights = new List<StudioLightStat>();
        public List<string> lightNames = new List<string>();

        public static event UnityAction<StudioLightStat> onLightAdded;
        public static event UnityAction<StudioLightStat> onLightRemoved;
        public static event UnityAction<StudioLightStat> onLightUpdated;

        private static StudioLightManager _instance = null;
        public static StudioLightManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StudioLightManager();
                }
                return _instance;
            }
        }

        private StudioLightManager()
        {
        }

        public StudioLightStat GetLight(string name)
        {
            StudioLightStat light;
            if (lightMap.TryGetValue(name, out light))
            {
                return light;
            }
            return null;
        }

        public StudioLightStat GetLight(int index)
        {
            if (index < 0 || index >= lightNames.Count)
            {
                return null;
            }
            return GetLight(lightNames[index]);
        }

        private int _prevUpdateFrame = -1;

        public override void LateUpdate()
        {
            LateUpdate(false);
        }

        public void LateUpdate(bool force)
        {
            if (!force)
            {
                if (Time.frameCount < _prevUpdateFrame + 30 || currentLayer.isAnmPlaying)
                {
                    return;
                }
            }
            _prevUpdateFrame = Time.frameCount;

            var lightList = lightHackManager.lightList;

            var addedLights = new List<StudioLightStat>();
            var removedLights = new List<StudioLightStat>();
            var updatedLights = new List<StudioLightStat>();
            var refresh = false;

            foreach (var stat in lightList)
            {
                if (stat.light == null)
                {
                    MTEUtils.LogWarning("StudioLightManager: lightがありません: name={0}", stat.name);
                    continue;
                }

                StudioLightStat cachedLight;
                if (stat.index >= lights.Count)
                {
                    cachedLight = new StudioLightStat();
                    cachedLight.FromStat(stat);
                    lights.Add(cachedLight);
                    addedLights.Add(cachedLight);
                    refresh = true;
                    continue;
                }

                cachedLight = lights[stat.index];

                if (cachedLight.light != stat.light ||
                    cachedLight.transform != stat.transform ||
                    cachedLight.obj != stat.obj ||
                    cachedLight.type != stat.type)
                {
                    cachedLight.FromStat(stat);
                    updatedLights.Add(cachedLight);
                    refresh = true;
                    continue;
                }
            }

            while (lights.Count > lightList.Count)
            {
                var stat = lights[lights.Count - 1];
                lights.RemoveAt(lights.Count - 1);
                removedLights.Add(stat);
                refresh = true;
            }

            if (refresh)
            {
                lightMap.Clear();
                lightNames.Clear();

                foreach (var light in lights)
                {
                    lightMap.Add(light.name, light);
                    lightNames.Add(light.name);
                }

                MTEUtils.LogDebug("StudioLightManager: Light list updated");

                foreach (var light in lights)
                {
                    MTEUtils.LogDebug("light: type={0} displayName={1} name={2}",
                        light.type, light.displayName, light.name);
                }

                UpdateTimelineLights();
            }

            foreach (var light in addedLights)
            {
                onLightAdded?.Invoke(light);
            }

            foreach (var light in removedLights)
            {
                onLightRemoved?.Invoke(light);
            }

             foreach (var light in updatedLights)
            {
                onLightUpdated?.Invoke(light);
            }
        }

        private void UpdateTimelineLights()
        {
            if (timeline == null)
            {
                return;
            }

            var lights = this.lights;
            var timelineLights = timeline.lights;

            if (lights.Count != timelineLights.Count)
            {
                timelineLights.Clear();
                foreach (var light in lights)
                {
                    var timelineLight = new TimelineLightData(light);
                    timelineLights.Add(timelineLight);
                }
            }
            else
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    var light = lights[i];
                    var timelineModel = timelineLights[i];
                    timelineModel.FromStat(light);
                }
            }
        }

        public void SetupLights(List<TimelineLightData> lightDataList)
        {
            MTEUtils.LogDebug("SetupLights: count={0}", lightDataList.Count);

            var lightList = lightHackManager.lightList.ToList();

            for (var i = 0; i < lightDataList.Count; ++i)
            {
                var lightData = lightDataList[i];
                if (i >= lightList.Count)
                {
                    var stat = CreateLightStat(lightData.type, i);
                    lightHackManager.CreateLight(stat);

                    MTEUtils.Log("Create light: type={0} displayName={1} name={2}",
                        stat.type, stat.displayName, stat.name);
                }
                else
                {
                    var stat = lightList[i];
                    var newStat = stat.Clone();
                    newStat.type = lightData.type;
                    newStat.index = i;
                    lightHackManager.ChangeLight(newStat);
                }
            }

            foreach (var stat in lights)
            {
                if (stat.index >= lightDataList.Count)
                {
                    lightHackManager.DeleteLight(stat);

                    MTEUtils.Log("Remove light: type={0} displayName={1} name={2}",
                        stat.type, stat.displayName, stat.name);
                }
            }

            lightHackManager.SetLightCompatibilityMode(timeline.isLightCompatibilityMode);

            LateUpdate(true);
        }

        public bool CanCreateLight()
        {
            return lightHackManager.CanCreateLight();
        }

        public override void OnLoad()
        {
            SetupLights(timeline.lights);
        }

        public override void OnPluginDisable()
        {
            Reset();
        }

        public void Reset()
        {
            lightMap.Clear();
            lights.Clear();
            lightNames.Clear();
            _prevUpdateFrame = -1;

            lightHackManager.DeleteAllLights();
            lightHackManager.SetLightCompatibilityMode(false);
        }

        public StudioLightStat CreateLightStat(
            LightType lightType,
            int index)
        {
            return new StudioLightStat(lightType, index);
        }

        public void DeleteLight(StudioLightStat stat)
        {
            lightHackManager.DeleteLight(stat);
            LateUpdate(true);

            timelineManager.RequestHistory("ライトの削除: " + stat.displayName);
        }

        public void CreateLight(StudioLightStat stat)
        {
            lightHackManager.CreateLight(stat);
            LateUpdate(true);

            timelineManager.RequestHistory("ライトの追加: " + stat.displayName);
        }

        public void ChangeLight(StudioLightStat stat)
        {
            lightHackManager.ChangeLight(stat);
            //LateUpdate(true);
        }

        public void ApplyLight(StudioLightStat stat)
        {
            lightHackManager.ApplyLight(stat);
            //LateUpdate(true);
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            Reset();
        }
    }
}