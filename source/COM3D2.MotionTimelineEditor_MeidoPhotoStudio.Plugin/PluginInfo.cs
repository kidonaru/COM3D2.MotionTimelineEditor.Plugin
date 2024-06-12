using System;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    internal static class PluginInfo
    {
        public const string PluginName = "MotionTimelineEditor_MeidoPhotoStudio";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "2.4.0.2";
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
    }
}