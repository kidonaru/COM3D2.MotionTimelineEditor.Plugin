using System;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    public static class PluginUtils
    {
        public const string PluginName = "MotionTimelineEditor_MultipleMaids";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "1.3.0.0";
        public const string WindowName = PluginName + " " + PluginVersion;

        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            UnityEngine.Debug.Log("[Debug] " + PluginName + "：" + message);
        }

        public static void Log(string message)
        {
            UnityEngine.Debug.Log(PluginName + "：" + message);
        }

        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(PluginName + "：" + message);
        }
        
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(PluginName + "：" + message);
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
    }
}