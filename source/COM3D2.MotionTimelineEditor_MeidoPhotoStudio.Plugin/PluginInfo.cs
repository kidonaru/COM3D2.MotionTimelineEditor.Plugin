using System;
using System.Reflection;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using MeidoPhotoStudio.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    internal static class PluginInfo
    {
        public const string PluginName = "MotionTimelineEditor_MeidoPhotoStudio";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = MotionTimelineEditor.Plugin.PluginUtils.PluginVersion;
        public const string WindowName = PluginName + " " + PluginVersion;

        private static FieldInfo fieldToggleValue = null;

        public static void SetValueOnly(
            this Toggle toggle,
            bool value)
        {
            if (fieldToggleValue == null)
            {
                fieldToggleValue = typeof(Toggle).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
                MTEUtils.AssertNull(fieldToggleValue != null, "fieldToggleValue is null");
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
                MTEUtils.AssertNull(fieldLight != null, "fieldLight is null");
            }
            return (Light) fieldLight.GetValue(dragPointLight);
        }

        public class DragDetector : MonoBehaviour
        {
            public bool isDragging = false;

            void OnMouseDown()
            {
                isDragging = true;
                MTEUtils.Log("OnMouseDown");
            }

            void OnMouseUp()
            {
                isDragging = false;
                MTEUtils.Log("OnMouseUp");
            }
        }

        public static bool IsDragging(this DragPointMeido dragPoint)
        {
            var detector = dragPoint.GetOrAddComponent<DragDetector>();
            if (detector == null)
            {
                MTEUtils.LogError("Detector is null");
                return false;
            }

            return detector.isDragging;
        }
    }
}