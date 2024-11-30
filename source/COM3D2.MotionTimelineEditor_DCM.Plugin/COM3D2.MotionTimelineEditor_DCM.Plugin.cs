using System;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using TransformType = MotionTimelineEditor.Plugin.TransformType;

    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginInfo.PluginFullName),
        PluginVersion(PluginInfo.PluginVersion)
    ]
    public class MotionTimelineEditor_DCM : PluginBase
    {
        private static TimelineManager timelineManager => TimelineManager.instance;

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
                typeof(SeTimelineLayer), SeTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(TextTimelineLayer), TextTimelineLayer.Create
            );

            timelineManager.RegisterTransform(
                TransformType.Morph, TimelineManager.CreateTransform<TransformDataMorph>
            );
            timelineManager.RegisterTransform(
                TransformType.Se, TimelineManager.CreateTransform<TransformDataSe>
            );
            timelineManager.RegisterTransform(
                TransformType.Text, TimelineManager.CreateTransform<TransformDataText>
            );
        }

        public static float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }
    }
}