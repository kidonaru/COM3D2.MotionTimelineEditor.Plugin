using System;
using System.Diagnostics;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public static class PluginUtils
    {
        public const string PluginName = "MotionTimelineEditor_DCM";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "2.3.0.0";
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

        public static void ShowDialog(string message)
        {
            GameMain.Instance.SysDlg.Show(
                message, SystemDialog.TYPE.OK, null, null);
        }

        public static float GetRotationZ(this Camera camera)
        {
            return camera.transform.eulerAngles.z;
        }

        public static void SetRotationZ(this Camera camera, float z)
        {
            Vector3 eulerAngles = camera.transform.eulerAngles;
            eulerAngles.z = z;
            camera.transform.eulerAngles = eulerAngles;
        }
    }
}