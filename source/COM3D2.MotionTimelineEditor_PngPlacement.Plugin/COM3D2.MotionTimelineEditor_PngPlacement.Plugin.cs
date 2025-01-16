using System;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    using MTE = MotionTimelineEditor.Plugin.MotionTimelineEditor;

    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginInfo.PluginFullName),
        PluginVersion(PluginInfo.PluginVersion)
    ]
    public class MotionTimelineEditor_PngPlacement : PluginBase
    {
        private static ManagerRegistry managerRegistry => ManagerRegistry.instance;
        private static TimelineManager timelineManager => TimelineManager.instance;
        PngPlacementWrapper wrapper = new PngPlacementWrapper();

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
            if (!wrapper.Init())
            {
                return;
            }

            timelineManager.RegisterLayer(
                typeof(PngPlacementTimelineLayer), PngPlacementTimelineLayer.Create
            );
            timelineManager.RegisterTransform(
                TransformType.PngObject, TimelineManager.CreateTransform<TransformDataPngObject>
            );

            managerRegistry.RegisterManager(PngPlacementManager.instance);
        }
    }
}