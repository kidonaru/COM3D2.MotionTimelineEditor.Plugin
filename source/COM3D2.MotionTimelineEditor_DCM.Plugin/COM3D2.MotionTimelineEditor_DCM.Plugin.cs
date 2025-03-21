using System;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using TransformType = MotionTimelineEditor.Plugin.TransformType;
    using MTE = MotionTimelineEditor.Plugin.MotionTimelineEditor;

    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginInfo.PluginFullName),
        PluginVersion(PluginInfo.PluginVersion)
    ]
    public class MotionTimelineEditor_DCM : PluginBase
    {
        private static ManagerRegistry managerRegistry => ManagerRegistry.instance;
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
                MTEUtils.LogException(e);
            }
        }

        private void Initialize()
        {
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

            managerRegistry.RegisterManager(MTETextManager.instance);
        }
    }
}