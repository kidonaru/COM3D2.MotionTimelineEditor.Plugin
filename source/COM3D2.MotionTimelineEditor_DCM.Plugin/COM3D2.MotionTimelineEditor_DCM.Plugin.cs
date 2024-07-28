using System;
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
            timelineManager.RegisterLayer(
                typeof(MorphTimelineLayer), MorphTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(MoveTimelineLayer), MoveTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(EyesTimelineLayer), EyesTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(ShapeKeyTimelineLayer), ShapeKeyTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(VoiceTimelineLayer), VoiceTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(CameraTimelineLayer), CameraTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(ModelTimelineLayer), ModelTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(ModelBoneTimelineLayer), ModelBoneTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(ModelShapeKeyTimelineLayer), ModelShapeKeyTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(BGTimelineLayer), BGTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(BGColorTimelineLayer), BGColorTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(LightTimelineLayer), LightTimelineLayer.Create
            );
            timelineManager.RegisterLayer(
                typeof(SeTimelineLayer), SeTimelineLayer.Create
            );
        }
    }
}