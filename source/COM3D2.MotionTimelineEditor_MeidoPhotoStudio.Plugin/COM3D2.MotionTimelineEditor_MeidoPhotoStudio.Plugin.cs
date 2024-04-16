using System;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    using MTE = MotionTimelineEditor.Plugin.MotionTimelineEditor;

    [
        PluginFilter("COM3D2x64"),
        PluginName(PluginUtils.PluginFullName),
        PluginVersion(PluginUtils.PluginVersion)
    ]
    public class MotionTimelineEditor_MeidoPhotoStudio : PluginBase
    {
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
            MTE.instance.AddStudioHack(new MeidoPhotoStudioHack());
        }
    }
}