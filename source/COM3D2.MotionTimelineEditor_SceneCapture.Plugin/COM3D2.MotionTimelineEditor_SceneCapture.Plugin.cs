using System;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_SceneCapture.Plugin
{
    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginUtils.PluginFullName),
        PluginVersion(PluginUtils.PluginVersion)
    ]
    public class MotionTimelineEditor_DCM : PluginBase
    {
        private static ModelHackManager modelHackManager
        {
            get
            {
                return ModelHackManager.instance;
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
            modelHackManager.Register(new SceneCaptureHack());
        }
    }
}