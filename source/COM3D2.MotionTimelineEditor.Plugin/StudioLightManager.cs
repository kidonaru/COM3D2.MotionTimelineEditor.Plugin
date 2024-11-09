using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioLightManager
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

        private static LightHackManager lightHackManager
        {
            get
            {
                return LightHackManager.instance;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private StudioLightManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
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

        public void LateUpdate(bool force)
        {
            if (!force && _prevUpdateFrame > Time.frameCount - 60)
            {
                return;
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
                    PluginUtils.LogWarning("StudioLightManager: lightがありません: name={0}", stat.name);
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
                    cachedLight.type != stat.type ||
                    cachedLight.visible != stat.visible)
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

                PluginUtils.Log("StudioLightManager: Light list updated");

                foreach (var light in lights)
                {
                    PluginUtils.LogDebug("light: type={0} displayName={1} name={2}",
                        light.type, light.displayName, light.name);
                }
            }

            foreach (var light in addedLights)
            {
                if (onLightAdded != null)
                {
                    onLightAdded.Invoke(light);
                }
            }

            foreach (var light in removedLights)
            {
                if (onLightRemoved != null)
                {
                    onLightRemoved.Invoke(light);
                }
            }

             foreach (var light in updatedLights)
            {
                if (onLightUpdated != null)
                {
                    onLightUpdated.Invoke(light);
                }
            }
        }

        public void SetupLights(List<TimelineLightData> lightDataList)
        {
            lightDataList = lightDataList.ToList(); // 更新される可能性があるので複製
            PluginUtils.LogDebug("SetupLights: count={0}", lightDataList.Count);
            LateUpdate(true);

            int index = 0;
            foreach (var lightData in lightDataList)
            {
                var stat = GetLight(index);
                if (stat == null)
                {
                    stat = CreateLightStat(
                        lightData.type,
                        lightData.visible,
                        index++);
                    lightHackManager.CreateLight(stat);

                    PluginUtils.Log("Create light: type={0} displayName={1} name={2}",
                        stat.type, stat.displayName, stat.name);
                }
                else
                {
                    var newStat = stat.Clone();
                    newStat.type = lightData.type;
                    newStat.visible = lightData.visible;
                    newStat.index = index++;
                    lightHackManager.ApplyLight(newStat);
                }
            }

            foreach (var stat in lights)
            {
                if (stat.index >= lightDataList.Count)
                {
                    lightHackManager.DeleteLight(stat);

                    PluginUtils.Log("Remove light: type={0} displayName={1} name={2}",
                        stat.type, stat.displayName, stat.name);
                }
            }

            LateUpdate(true);
        }

        public bool CanCreateLight()
        {
            return lightHackManager.CanCreateLight();
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
            lightMap.Clear();
            lights.Clear();
            lightNames.Clear();
            _prevUpdateFrame = -1;

            lightHackManager.DeleteAllLights();
        }

        public StudioLightStat CreateLightStat(
            LightType lightType,
            bool visible,
            int index)
        {
            return new StudioLightStat(lightType, visible, index);
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

        public void ApplyLight(StudioLightStat stat)
        {
            lightHackManager.ApplyLight(stat);
            //LateUpdate(true);
        }

        private Dictionary<LightType, int> _lightGroupMap = new Dictionary<LightType, int>();

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}