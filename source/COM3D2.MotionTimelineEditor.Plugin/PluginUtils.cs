using System;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public static class PluginUtils
    {
        public static readonly string UserDataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config");

        public const string PluginVersion = PluginInfo.PluginVersion;

        public static string ConfigPath
        {
            get
            {
                return CombinePaths(UserDataPath, PluginInfo.PluginName + ".xml");
            }
        }
        
        public static string TimelineDirPath
        {
            get
            {
                string path = PhotoWindowManager.path_photo_folder + "_Timeline";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static string DCMConfigPath
        {
            get
            {
                return CombinePaths(UserDataPath, "DanceCameraMotion");
            }
        }

        public static string ExtraModelCsvPath
        {
            get
            {
                return CombinePaths(UserDataPath, PluginInfo.PluginName + "_ExtraModel.csv");
            }
        }

        public static string PluginConfigDirPath
        {
            get
            {
                var path = CombinePaths(UserDataPath, PluginInfo.PluginName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static CameraMain MainCamera
        {
            get
            {
                return GameMain.Instance.MainCamera;
            }
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.Log("[Debug] " + PluginInfo.PluginName + ": " + message);
        }

        public static void Log(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.Log(PluginInfo.PluginName + ": " + message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.LogWarning(PluginInfo.PluginName + ": " + message);
        }
        
        public static void LogError(string format, params object[] args)
        {
            string message = string.Format(format, args);
#if DEBUG
            UnityEngine.Debug.LogError(PluginInfo.PluginName + ": " + message + "\n" + Environment.StackTrace);
#else
            UnityEngine.Debug.LogError(PluginInfo.PluginName + ": " + message);
#endif
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

        public static string CombinePaths(params string[] parts)
        {
            return parts.Aggregate(Path.Combine);
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

        public static string GetTimelinePath(string anmName, string directoryName)
        {
            return CombinePaths(TimelineDirPath, directoryName, anmName + ".xml");
        }

        public static string ConvertThumPath(string path)
        {
            return Path.ChangeExtension(path, ".png");
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

        public static void ResetInputOnScroll(Rect windowRect)
        {
            var mousePosition = Input.mousePosition;
            if (mousePosition.x > windowRect.x &&
                mousePosition.x < windowRect.x + windowRect.width &&
                Screen.height - mousePosition.y > windowRect.y &&
                Screen.height - mousePosition.y < windowRect.y + windowRect.height &&
                Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                Input.ResetInputAxes();
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

        public static string GetDcmSongDirPath(string songName)
        {
            var path = CombinePaths(DCMConfigPath, "song", songName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool IsExistsDcmSongDirPath(string songName)
        {
            var path = CombinePaths(DCMConfigPath, "song", songName);
            return Directory.Exists(path);
        }

        public static string GetDcmSongListDirPath()
        {
            var path = CombinePaths(DCMConfigPath, "songList");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetDcmSongFilePath(string songName, string fileName)
        {
            return CombinePaths(GetDcmSongDirPath(songName), fileName);
        }
        
        public static string GetDcmSongListFilePath(string songName)
        {
            return CombinePaths(GetDcmSongListDirPath(), songName + ".xml");
        }

        public static void ExecuteNextFrame(Action action)
        {
            GameMain.Instance.StartCoroutine(ExecuteNextFrameInternal(action));
        }

        public static IEnumerator ExecuteNextFrameInternal(Action action)
        {
            yield return null;
            if (action != null)
            {
                action();
            }
        }

        public static string GetVoiceInfoCsvPath(Personality personality)
        {
            return CombinePaths(PluginConfigDirPath, "Voice_" + personality.ToString() + ".csv");
        }

        public static void OpenDirectory(string path)
        {
            path = Path.GetFullPath(path);

            LogDebug("OpenDirectory: {0}", path);

            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", path);
                }
                else
                {
                    LogWarning("指定されたディレクトリが存在しません: {0}", path);
                }
            }
        }

        private static readonly Regex _regexGroup = new Regex(@"\(\d+\)$", RegexOptions.Compiled);
        private static readonly Regex _regexMyRoomId = new Regex(@"MYR_\d+", RegexOptions.Compiled);

        public static int ExtractGroup(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var match = _regexGroup.Match(input);
            if (match.Success)
            {
                return int.Parse(match.Value.Substring(1, match.Value.Length - 2));
            }
            return 0;
        }

        public static string RemoveGroupSuffix(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return _regexGroup.Replace(input, "").Trim();
        }

        public static int ExtractMyRoomId(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var match = _regexMyRoomId.Match(input);
            if (match.Success)
            {
                return int.Parse(match.Value.Substring(4));
            }
            return 0;
        }

        public static string GetGroupSuffix(int group)
        {
            if (group > 0)
            {
                return string.Format(" ({0})", group);
            }
            return "";
        }

        // エルミート曲線の補間計算
        public static float Hermite(
            float t0,
            float t1,
            float v0,
            float v1,
            float outTangent,
            float inTangent,
            float t)
        {
            float dt = t1 - t0;
            if (dt == 0) return v0; // 時間差がない場合は開始値を返す

            float t2 = t * t;
            float t3 = t2 * t;

            // エルミート基底関数
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            // エルミート補間の計算
            float y = h00 * v0 +
                      h10 * dt * outTangent +
                      h01 * v1 +
                      h11 * dt * inTangent;

            return y;
        }

        // エルミート曲線の補間計算（簡略化版）
        public static float HermiteSimplified(
            float outTangent,
            float inTangent,
            float t)
        {
            return Hermite(0, 1, 0, 1, outTangent, inTangent, t);
        }

        // Vector3の補間
        public static Vector3 HermiteVector3(
            float t0,
            float t1,
            Vector3 startPoint,
            Vector3 endPoint,
            Vector3 outTangent,
            Vector3 inTangent,
            float t)
        {
            return new Vector3(
                Hermite(t0, t1, startPoint.x, endPoint.x, outTangent.x, inTangent.x, t),
                Hermite(t0, t1, startPoint.y, endPoint.y, outTangent.y, inTangent.y, t),
                Hermite(t0, t1, startPoint.z, endPoint.z, outTangent.z, inTangent.z, t)
            );
        }

        // Vector3の補間
        public static Vector3 HermiteVector3(
            float t0,
            float t1,
            Vector3 startPoint,
            Vector3 endPoint,
            float[] outTangent,
            float[] inTangent,
            float t)
        {
            return HermiteVector3(
                t0,
                t1,
                startPoint,
                endPoint,
                outTangent.ToVector3(),
                inTangent.ToVector3(),
                t
            );
        }

        // Vector4の補間
        public static Vector4 HermiteVector4(
            float t0,
            float t1,
            Vector4 startPoint,
            Vector4 endPoint,
            Vector4 outTangent,
            Vector4 inTangent,
            float t)
        {
            return new Vector4(
                Hermite(t0, t1, startPoint.x, endPoint.x, outTangent.x, inTangent.x, t),
                Hermite(t0, t1, startPoint.y, endPoint.y, outTangent.y, inTangent.y, t),
                Hermite(t0, t1, startPoint.z, endPoint.z, outTangent.z, inTangent.z, t),
                Hermite(t0, t1, startPoint.w, endPoint.w, outTangent.w, inTangent.w, t)
            );
        }

        // Colorの補間
        public static Color HermiteColor(
            float t0,
            float t1,
            Color startColor,
            Color endColor,
            float[] outTangent,
            float[] inTangent,
            float t)
        {
            if (outTangent.Length == 4 && inTangent.Length == 4)
            {
                return new Color(
                    Hermite(t0, t1, startColor.r, endColor.r, outTangent[0], inTangent[0], t),
                    Hermite(t0, t1, startColor.g, endColor.g, outTangent[1], inTangent[1], t),
                    Hermite(t0, t1, startColor.b, endColor.b, outTangent[2], inTangent[2], t),
                    Hermite(t0, t1, startColor.a, endColor.a, outTangent[3], inTangent[3], t)
                );
            }
            
            return new Color(
                Hermite(t0, t1, startColor.r, endColor.r, outTangent[0], inTangent[0], t),
                Hermite(t0, t1, startColor.g, endColor.g, outTangent[1], inTangent[1], t),
                Hermite(t0, t1, startColor.b, endColor.b, outTangent[2], inTangent[2], t)
            );
        }

        // Quaternionの補間
        public static Quaternion HermiteQuaternion(
            float t0,
            float t1,
            Quaternion startPoint,
            Quaternion endPoint,
            Vector4 outTangent,
            Vector4 inTangent,
            float t)
        {
            return new Quaternion(
                Hermite(t0, t1, startPoint.x, endPoint.x, outTangent.x, inTangent.x, t),
                Hermite(t0, t1, startPoint.y, endPoint.y, outTangent.y, inTangent.y, t),
                Hermite(t0, t1, startPoint.z, endPoint.z, outTangent.z, inTangent.z, t),
                Hermite(t0, t1, startPoint.w, endPoint.w, outTangent.w, inTangent.w, t)
            );
        }

        // Quaternionの補間
        public static Quaternion HermiteQuaternion(
            float t0,
            float t1,
            Quaternion startPoint,
            Quaternion endPoint,
            float[] outTangent,
            float[] inTangent,
            float t)
        {
            return HermiteQuaternion(
                t0,
                t1,
                startPoint,
                endPoint,
                outTangent.ToVector4(),
                inTangent.ToVector4(),
                t
            );
        }
    }
}