using System;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using CM3D2.PngPlacement.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    internal static class PluginInfo
    {
        public const string PluginName = "MotionTimelineEditor_PngPlacement";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = MotionTimelineEditor.Plugin.PluginUtils.PluginVersion;
        public const string WindowName = PluginName + " " + PluginVersion;
    }
}