using System;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    internal static class PluginInfo
    {
        public const string PluginName = "MotionTimelineEditor_MeidoPhotoStudio";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "2.7.0.1";
        public const string WindowName = PluginName + " " + PluginVersion;

        private static FieldInfo fieldToggleValue = null;

        public static void SetValueOnly(
            this Toggle toggle,
            bool value)
        {
            if (fieldToggleValue == null)
            {
                fieldToggleValue = typeof(Toggle).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldToggleValue != null, "fieldToggleValue is null");
            }
            fieldToggleValue.SetValue(toggle, value);
        }

        private static FieldInfo fieldLight = null;

        public static Light GetLight(
            this DragPointLight dragPointLight)
        {
            if (fieldLight == null)
            {
                fieldLight = typeof(DragPointLight).GetField("light", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldLight != null, "fieldLight is null");
            }
            return (Light) fieldLight.GetValue(dragPointLight);
        }
    }
}