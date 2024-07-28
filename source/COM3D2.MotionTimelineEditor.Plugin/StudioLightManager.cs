using System;
using System.Collections.Generic;
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

            foreach (var light in lightList)
            {
                if (string.IsNullOrEmpty(light.name))
                {
                    continue;
                }

                if (light.light == null)
                {
                    PluginUtils.LogWarning("StudioLightManager: lightがありません: name={0}", light.name);
                    continue;
                }

                StudioLightStat cachedLight;
                if (!lightMap.TryGetValue(light.name, out cachedLight))
                {
                    cachedLight = new StudioLightStat();
                    lightMap[light.name] = cachedLight;
                    addedLights.Add(cachedLight);
                }

                if (cachedLight.light != light.light ||
                    cachedLight.transform != light.transform ||
                    cachedLight.obj != light.obj ||
                    cachedLight.type != light.type)
                {
                    cachedLight.FromStat(light);
                    updatedLights.Add(cachedLight);
                    refresh = true;
                    continue;
                }

                if (cachedLight.visible != light.visible)
                {
                    cachedLight.FromStat(light);
                    updatedLights.Add(cachedLight);
                    continue;
                }
            }

            if (lightList.Count < lightMap.Count)
            {
                var names = new List<string>(lightMap.Keys);
                foreach (var name in names)
                {
                    if (!lightList.Exists(light => light.name == name))
                    {
                        removedLights.Add(lightMap[name]);
                        lightMap.Remove(name);
                        refresh = true;
                    }
                }
            }

            if (refresh)
            {
                lights.Clear();
                lightNames.Clear();

                lights.AddRange(lightMap.Values);

                lights.Sort((light1, light2) =>
                {
                    return light1.index - light2.index;
                });

                foreach (var light in lights)
                {
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
            LateUpdate(true);

            var index = 0;
            foreach (var lightData in lightDataList)
            {
                var light = GetLight(index);
                if (light == null)
                {
                    light = CreateLightStat(
                        index,
                        lightData.lightType,
                        lightData.visible);
                    lightHackManager.CreateLight(light);

                    PluginUtils.Log("Create light: type={0} displayName={1} name={2}",
                        light.type, light.displayName, light.name);
                }
                else
                {
                    light.visible = lightData.visible;
                    lightHackManager.ApplyLight(light);
                }
                index++;
            }

            foreach (var light in lights)
            {
                if (lightDataList.FindIndex(data => index == light.index) < 0)
                {
                    lightHackManager.DeleteLight(light);

                    PluginUtils.Log("Remove light: type={0} displayName={1} name={2}",
                        light.type, light.displayName, light.name);
                }
            }

            LateUpdate(true);
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

            studioHack.DeleteAllLights();
        }

        public StudioLightStat CreateLightStat(
            int index,
            LightType lightType,
            bool visible)
        {
            return new StudioLightStat
            {
                type = lightType,
                index = index,
                visible = visible,
            };
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

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Reset();
        }
    }
}