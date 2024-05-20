using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MeidoPhotoStudio.Plugin;

namespace COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin
{
    public static class PluginUtils
    {
        public const string PluginName = "MotionTimelineEditor_MeidoPhotoStudio";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "2.1.1.0";
        public const string WindowName = PluginName + " " + PluginVersion;

        [Conditional("DEBUG")]
        public static void LogDebug(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.Log("[Debug] " + PluginName + ": " + message);
        }

        public static void Log(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.Log(PluginName + ": " + message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.LogWarning(PluginName + ": " + message);
        }
        
        public static void LogError(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.LogError(PluginName + ": " + message);
        }

        public static void AssertNull(bool condition, string message)
        {
            if (!condition)
            {
                StackFrame stackFrame = new StackFrame(1, true);
                string fileName = stackFrame.GetFileName();
                int fileLineNumber = stackFrame.GetFileLineNumber();
                string f_strMsg = fileName + "(" + fileLineNumber + ") \nNullPointerException：" + message;
                LogError(f_strMsg);
            }
        }

        public static void LogException(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        private static FieldInfo fieldToggleValue = null;

        public static void SetValueOnly(
            this Toggle toggle,
            bool value
        )
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