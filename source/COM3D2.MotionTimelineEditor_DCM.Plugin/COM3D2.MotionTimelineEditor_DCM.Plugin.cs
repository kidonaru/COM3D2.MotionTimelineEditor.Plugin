using System;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginInfo.PluginFullName),
        PluginVersion(PluginInfo.PluginVersion)
    ]
    public class MotionTimelineEditor_DCM : PluginBase
    {
        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
        }

        public void Start()
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        private void Initialize()
        {
            TimelineLayerBase.EasingFunction = CalcEasingValue;

            timelineManager.RegisterLayer(
                typeof(MorphTimelineLayer), MorphTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(UndressTimelineLayer), UndressTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(SeTimelineLayer), SeTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(TextTimelineLayer), TextTimelineLayer.Create
            );
        }

        public static float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }
    }
}