using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public static class PluginUtils
    {
        public const string PluginName = "MotionTimelineEditor";
        public const string PluginFullName = "COM3D2." + PluginName + ".Plugin";
        public const string PluginVersion = "1.1.2.0";
        public const string WindowName = PluginName + " " + PluginVersion;

        public static readonly string UserDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config");

        public static string ConfigPath
        {
            get
            {
                return CombinePaths(UserDataPath, PluginName + ".xml");
            }
        }
        
        public static string TimelineDirPath
        {
            get
            {
                string text = PhotoWindowManager.path_photo_folder + "_Timeline";
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }

                return text;
            }
        }

        public static CameraMain MainCamera
        {
            get
            {
                return GameMain.Instance.MainCamera;
            }
        }

        public static string Asciify(this string s)
        {
            return Regex.Replace(s, "[^0-9A-Za-z]", "_");
        }

        public static string CombinePaths(params string[] parts)
        {
            return parts.Aggregate(Path.Combine);
        }

        public static void ForEach<T>(this IEnumerable<T> tees, Action<T> action)
        {
            foreach (T tee in tees)
            {
                action(tee);
            }
        }

        public static void ShowDialog(string message)
        {
            GameMain.Instance.SysDlg.Show(
                message, SystemDialog.TYPE.OK, null, null);
        }

        public static void ShowConfirmDialog(string message, SystemDialog.OnClick onYes, SystemDialog.OnClick onNo)
        {
            GameMain.Instance.SysDlg.Show(
                message, SystemDialog.TYPE.YES_NO, onYes, onNo);
        }

        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(array);
            return texture2D;
        }

        public static string ConvertThumPath(string path)
        {
            return Path.ChangeExtension(path, ".png");
        }

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

        public static void AssertNull(bool condition)
        {
            if (!condition)
            {
                StackFrame stackFrame = new StackFrame(1, true);
                string fileName = stackFrame.GetFileName();
                int fileLineNumber = stackFrame.GetFileLineNumber();
                string f_strMsg = fileName + "(" + fileLineNumber + ") \nNullPointerException";
                LogError(f_strMsg);
            }
        }

        public static void LogException(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        public static void AdjustWindowPosition(ref Rect rect)
        {
            if (rect.x < 0) rect.x = 0;
            if (rect.y < 0) rect.y = 0;

            if (rect.x + rect.width > Screen.width)
            {
                rect.x = Screen.width - rect.width;
            }
            if (rect.y + rect.height > Screen.height)
            {
                rect.y = Screen.height - rect.height;
            }
        }

        public static string GetKeyName(this KeyCode key)
        {
            if (key == KeyCode.Return)
            {
                return "Enter";
            }
            if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
            {
                return key.ToString().Substring(5);
            }

            return key.ToString();
        }

        public static void UIHide()
        {
            var methodInfo = typeof(CameraMain).GetMethod("UIHide", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(MainCamera, null);
        }

        public static void UIResume()
        {
            var methodInfo = typeof(CameraMain).GetMethod("UIResume", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(MainCamera, null);
        }

        private static string[] _saveBonePaths = null;
        public static string[] saveBonePaths
        {
            get
            {
                if (_saveBonePaths == null)
                {
                    var methodInfo = typeof(CacheBoneDataArray).GetMethod("GetSaveBonePathArray", BindingFlags.NonPublic | BindingFlags.Static);
                    _saveBonePaths = (string[]) methodInfo.Invoke(null, null);
                    PluginUtils.AssertNull(saveBonePaths != null);
                }
                return _saveBonePaths;
            }
        }

        public static byte[] icon = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAACXBIWXMAABYlAAAWJQFJUiTwAAAD" +
                "q0lEQVRIibVWv0/qUBQ+95a2FBAMEaLRRQcMOhgNk3EgBgETJk0cDYks/gMOujO4OpEwdNME4uDk" +
                "LwwDA5ODizEYYhw0KjFBi62ltG+4pvRd0Ie+976B9B7O/b5z7rk/DjIMA/4nbNT47/UQQl0EdF1/" +
                "e3u7u7ur1Wqqqn5XzCS12+0DAwODg4MOhwNj3BaQZfn8/LxSqTAM02q1PmPBGOu63lXYMAyEEFEK" +
                "BoNTU1MOh6MtcHNzc319PT8/Pzw8TJyoTBuNxsnJSaPRcLvdkUjE5XJ1CpDfarVaKpW8Xu/4+DgA" +
                "YGJ9eHiw2+0jIyMcx7Esy7KszQJFUU5PTxuNBlEqFAqKoth+B5nFcdzo6CjG+OnpiUhiMwSPx0NW" +
                "jYIkSUdHR5IkfWGxgmXZ/v5+cwFsJANZlhmGoVxzudzKygpl3N7e3tjYSCaTABCLxTrXCiHEMIyi" +
                "KKQqHyE3m00ytrouLi7m8/lMJgMAiURCFEVRFHvJwzAMTdPId5c1+eP8b/l0F+hlZo+e3at6eHjY" +
                "C3svGrQAYSc7snd8oWGj/I6Pj3thtxY8k8nwPE80YrGY0+m0eiLDMHRdPzg4IAfqu7FTcLlc0Wi0" +
                "UCgIgrCwsIAxbmfw8vLSbDb/hh0AJEkqlUpWS7sGbrebyu4HcDqdc3Nz3QUYholGo50n81vs8Xic" +
                "Ymgv0f39vcfjsf6XyWS2trZqtRoZhsPh2dnZQCBQLBbNCofD4WKxSCml0+mZmRk6A4JsNvv6+prL" +
                "5URR5HkeAEKhkCiK5MJIp9O3t7fhcHhzcxMA1tfXk8mkKIr5fH55eTkQCJAtEwwGTcIuB83lcsXj" +
                "caoePM8vLS0BwNnZWad/LBZjWbaTqotAKpVCCPX19e3t7XVqAAB1mgj7F5WjH/1sNru2tka+JUmy" +
                "vhDv7+8AMDY2ZlrsdvvX7F0yoKJzu902m42w7+/vA0AoFDIdpqenP2M3320bACCEOI4jj0EqlUql" +
                "UtYMyuVyuVwGAFJbv99v7qLV1VVVVc2MTRDCtgAACILg8/k0TaPetaurK+i4AROJxM7ODhX77u6u" +
                "GbumaWbE2LTW6/XPGhbrvvpjVVVVrdfr5hCTjIaGhlRVrVariqJomqZpWut3kMvL7/dHIhFBEFod" +
                "ILNkWa5UKgghv9//0f6QakiSdHFxcXl5Sfx+1kBijBmG4ThucnJyYmKCZPkhoOu6LMuPj4/Pz89k" +
                "O8KPWkee571er8/nEwSBlBNRLP+8+aUF/jl+Ae7kHr5CRcT/AAAAAElFTkSuQmCC"
        );
    }

}