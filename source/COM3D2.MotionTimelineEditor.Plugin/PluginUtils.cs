using System;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using System.Collections;

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

        public readonly static byte[] Icon = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAABYlAAAWJQFJUiTwAAAD" +
                "E0lEQVRYhcWXvUvrUBjGn7apxVRaSlXwY/BjaECdi0ugbtpZREHUxU3Foa7eP0EzCAYHt24mOolL" +
                "/QcERcQhk4OCYBux6aARnjvc20BvU5ukV3zgQMjJOc8vJyfv+x7wrzRNoyzLFEWRAL6liaJIWZap" +
                "aVrdliDJQqFASZKo6zqr1Sq/S9VqlbquU5IkFgqFPwCaplGSJJqm+W3G/8o0TUqSRE3TCFmWqet6" +
                "oIleX1/58fERaOzp6SllWWZIFEU+Pz+jp6cHfmSaJsrlMgRBwPDwMARB8DW+Vquhv78fIQAkGci8" +
                "rmg0iqGhId8QoVAIYV8jXMwBwLZtPD4+4vPz0+90/gDczDuF8AzwlXknEJ4AvJgHhWgL4Mc8KARJ" +
                "cnNzkwAYi8X49PREkqxUKjQMgwcHB044VVWV8/PzbcPuzc0NM5lMy/5yuUwAbPpv3t/foSgKdnZ2" +
                "nDc/OjpqeGZsbAzZbBYAUKlUYBgG4vE4JicnnWcikYhzPTExgb6+voY5otGo+woAYCKR4NXVFQ3D" +
                "YLFYbCBXVZWGYThNURQC4NTUVMN9wzCcFTg8PKRt203REACb9kAul8Pb2xuKxSIAQFVV9Pb2IpPJ" +
                "tPqEbXV/f4+TkxOUSiVcXl7i+vra6XMFGB0dxfHxMW5vb1EqlbCysoKurq7AAHt7e1hYWMDMzAxy" +
                "uRy2t7edvqY9EA6Hsbq6it3dXayvryMej2NxcREXFxeBAfL5PMbHxyEIAhKJBEZGRloDxGIxzM7O" +
                "Yn9/Hy8vL1heXkYymQxsDgBzc3PI5/Ou+aLpEySTSQwODmJrawvZbBZra2sdmQOAIAgtk5Vr+kql" +
                "UtjY2MDS0lLH5gCgKIqzqes6OztrDVCHAOA7Crrp7u6u6Z5t2wDQvh4IEooBbzWCp3oglUohnU7/" +
                "d/O6PGVDPxB+qyPP9YAXiCClma+K6CuIoHWh75rQDSKoOQCERVGEZVmBIYKa12o1iKKIyPT09K+B" +
                "gQHf2a67uxuRSATpdDrQm5+fn+Ph4QE/fzQjGw+nlmV9m7FlWc2H03rnTx3PfwOkkRgyh6JzmQAA" +
                "AABJRU5ErkJggg==");

        public readonly static byte[] LockIcon = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAC4jAAAuIwF4pT92AAAB" +
                "pElEQVRYhe2WP08UQRjGf8txwRMSIIGYiD0hFBY211kbOjsqY+j5BH4BCmobWij4CFgSCyltIEJQ" +
                "YmFM+GPhHwq4H8VOwgq7eztk785EnmSzszs7z/PM7PvOO4nKIDE0UPV7A8DwHcc1gXngKTANfAU+" +
                "Ap+AyygmNfaaVTfV7/6NI3VNnYnhixWfUw+D4Bf1tdpWl9WT8H5HfdILA+PqXhBZUVtqI/QNq2Pq" +
                "eujfCv21GlgK5O8ywjevlnqg/lQXqvBWzYIG8Cq035YE2jmwCowCL6mQZVUNtAIppJFeBIHd0J6i" +
                "QpZVNTCUIfvR5dvjzJikG3GVfeAh6ewb4XkiiCSkM+aG0GS4j4SxDeB3EXlicTGaAlaAR8ADoB2M" +
                "vAfOyJ9dB3gMPCNdqQ/ABbAPvAF+3RpREJ2Jumi9eB6TBQkwVrQ0d0QuX1kQdmo2kJu6/ayGuRlR" +
                "ZqAvR6UyA11zuNcG6kbuig78RPTPBmEvAjDqFyRc7/11IWoFOsBn4E9N4qfAt7yOsmLUBF4ACxFC" +
                "2QqZxQawTc7uWmagL/iv0vDeQC6uAKyOO781btv8AAAAAElFTkSuQmCC");

        public readonly static byte[] DefaultBgmData = Convert.FromBase64String(
                "T2dnUwACAAAAAAAAAAAD8iFxAAAAAHbAeUEBHgF2b3JiaXMAAAAAAkSsAAAAAAAAgLUBAAAAAAC4" +
                "AU9nZ1MAAAAAAAAAAAAAA/IhcQEAAADDMXE0ET7///////////////////8HA3ZvcmJpcwwAAABM" +
                "YXZmNjAuNC4xMDEBAAAAHgAAAGVuY29kZXI9TGF2YzYwLjkuMTAwIGxpYnZvcmJpcwEFdm9yYmlz" +
                "JUJDVgEAQAAAJHMYKkalcxaEEBpCUBnjHELOa+wZQkwRghwyTFvLJXOQIaSgQohbKIHQkFUAAEAA" +
                "AIdBeBSEikEIIYQlPViSgyc9CCGEiDl4FIRpQQghhBBCCCGEEEIIIYRFOWiSgydBCB2E4zA4DIPl" +
                "OPgchEU5WBCDJ0HoIIQPQriag6w5CCGEJDVIUIMGOegchMIsKIqCxDC4FoQENSiMguQwyNSDC0KI" +
                "moNJNfgahGdBeBaEaUEIIYQkQUiQgwZByBiERkFYkoMGObgUhMtBqBqEKjkIH4QgNGQVAJAAAKCi" +
                "KIqiKAoQGrIKAMgAABBAURTHcRzJkRzJsRwLCA1ZBQAAAQAIAACgSIqkSI7kSJIkWZIlWZIlWZLm" +
                "iaosy7Isy7IsyzIQGrIKAEgAAFBRDEVxFAcIDVkFAGQAAAigOIqlWIqlaIrniI4IhIasAgCAAAAE" +
                "AAAQNENTPEeURM9UVde2bdu2bdu2bdu2bdu2bVuWZRkIDVkFAEAAABDSaWapBogwAxkGQkNWAQAI" +
                "AACAEYowxIDQkFUAAEAAAIAYSg6iCa0535zjoFkOmkqxOR2cSLV5kpuKuTnnnHPOyeacMc4555yi" +
                "nFkMmgmtOeecxKBZCpoJrTnnnCexedCaKq0555xxzulgnBHGOeecJq15kJqNtTnnnAWtaY6aS7E5" +
                "55xIuXlSm0u1Oeecc84555xzzjnnnOrF6RycE84555yovbmWm9DFOeecT8bp3pwQzjnnnHPOOeec" +
                "c84555wgNGQVAAAEAEAQho1h3CkI0udoIEYRYhoy6UH36DAJGoOcQurR6GiklDoIJZVxUkonCA1Z" +
                "BQAAAgBACCGFFFJIIYUUUkghhRRiiCGGGHLKKaeggkoqqaiijDLLLLPMMssss8w67KyzDjsMMcQQ" +
                "QyutxFJTbTXWWGvuOeeag7RWWmuttVJKKaWUUgpCQ1YBACAAAARCBhlkkFFIIYUUYogpp5xyCiqo" +
                "gNCQVQAAIACAAAAAAE/yHNERHdERHdERHdERHdHxHM8RJVESJVESLdMyNdNTRVV1ZdeWdVm3fVvY" +
                "hV33fd33fd34dWFYlmVZlmVZlmVZlmVZlmVZliA0ZBUAAAIAACCEEEJIIYUUUkgpxhhzzDnoJJQQ" +
                "CA1ZBQAAAgAIAAAAcBRHcRzJkRxJsiRL0iTN0ixP8zRPEz1RFEXTNFXRFV1RN21RNmXTNV1TNl1V" +
                "Vm1Xlm1btnXbl2Xb933f933f933f933f931dB0JDVgEAEgAAOpIjKZIiKZLjOI4kSUBoyCoAQAYA" +
                "QAAAiuIojuM4kiRJkiVpkmd5lqiZmumZniqqQGjIKgAAEABAAAAAAAAAiqZ4iql4iqh4juiIkmiZ" +
                "lqipmivKpuy6ruu6ruu6ruu6ruu6ruu6ruu6ruu6ruu6ruu6ruu6ruu6rguEhqwCACQAAHQkR3Ik" +
                "R1IkRVIkR3KA0JBVAIAMAIAAABzDMSRFcizL0jRP8zRPEz3REz3TU0VXdIHQkFUAACAAgAAAAAAA" +
                "AAzJsBTL0RxNEiXVUi1VUy3VUkXVU1VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVU3T" +
                "NE0TCA1ZCQCQAQCQEFMtLcaaCYskYtJqq6BjDFLspbFIKme1t8oxhRi1XhqHlFEQe6kkY4pBzC2k" +
                "0CkmrdZUQoUUpJhjKhVSDlIgNGSFABCaAeBwHECyLECyLAAAAAAAAACQNA3QPA+wNA8AAAAAAAAA" +
                "JE0DLE8DNM8DAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAEDSNEDzPEDzPAAAAAAAAADQPA/wPBHwRBEAAAAAAAAALM8DNNED" +
                "PFEEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAEDSNEDzPEDzPAAAAAAAAACwPA/wRBHQPBEAAAAAAAAALM8DPFEEPNEDAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAQ4AAAEGAhFBqyIgCI" +
                "EwBwSBIkCZIEzQNIlgVNg6bBNAGSZUHToGkwTQAAAAAAAAAAAAAkTYOmQdMgigBJ06Bp0DSIIgAA" +
                "AAAAAAAAAACSpkHToGkQRYCkadA0aBpEEQAAAAAAAAAAAADPNCGKEEWYJsAzTYgiRBGmCQAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAIAAAYcAAACDChDBQasiIAiBMAcDiKZQEAgOM4lgUAAI7jWBYAAFiW" +
                "JYoAAGBZmigCAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAgAABhwAAAIMKEMFBqyEgCIAgBwKIplAcexLOA4lgUkybIAlgXQPICm" +
                "AUQRAAgAAChwAAAIsEFTYnGAQkNWAgBRAAAGxbEsTRNFkqRpmieKJEnTPE8UaZrneZ5pwvM8zzQh" +
                "iqJomhBFUTRNmKZpqiowTVUVAABQ4AAAEGCDpsTiAIWGrAQAQgIAHIpiWZrmeZ4niqapmiRJ0zxP" +
                "FEXRNE1TVUmSpnmeKIqiaZqmqrIsTfM8URRF01RVVYWmeZ4oiqJpqqrqwvM8TxRF0TRV1XXheZ4n" +
                "iqJomqrquhBFUTRN01RNVXVdIIqmaZqqqqquC0RPFE1TVV3XdYHniaJpqqqrui4QTdNUVVV1XVkG" +
                "mKZpqqrryjJAVVXVdV1XlgGqqqqu67qyDFBV13VdWZZlAK7rurIsywIAAA4cAAACjKCTjCqLsNGE" +
                "Cw9AoSErAoAoAADAGKYUU8owJiGkEBrGJIQUQiYlpdJSqiCkUlIpFYRUSiolo5RSailVEFIpqZQK" +
                "QiollVIAANiBAwDYgYVQaMhKACAPAIAwRinGGHNOIqQUY845JxFSijHnnJNKMeacc85JKRlzzDnn" +
                "pJTOOeecc1JK5pxzzjkppXPOOeeclFJK55xzTkopJYTOQSellNI555wTAABU4AAAEGCjyOYEI0GF" +
                "hqwEAFIBAAyOY1ma5nmiaJqWJGma53meKJqmJkma5nmeJ4qqyfM8TxRF0TRVled5niiKommqKtcV" +
                "RdM0TVVVXbIsiqZpmqrqujBN01RV13VdmKZpqqrrui5sW1VV1XVlGbatqqrqurIMXNd1ZdmWgSy7" +
                "ruzasgAA8AQHAKACG1ZHOCkaCyw0ZCUAkAEAQBiDkEIIIWUQQgohhJRSCAkAABhwAAAIMKEMFBqy" +
                "EgBIBQAAjLHWWmuttdZAZ6211lprrYDMWmuttdZaa6211lprrbXWUmuttdZaa6211lprrbXWWmut" +
                "tdZaa6211lprrbXWWmuttdZaa6211lprrbXWWmuttdZaay2llFJKKaWUUkoppZRSSimllFJKBQD6" +
                "VTgA+D/YsDrCSdFYYKEhKwGAcAAAwBilGHMMQimlVAgx5px0VFqLsUKIMeckpNRabMVzzkEoIZXW" +
                "Yiyecw5CKSnFVmNRKYRSUkottliLSqGjklJKrdVYjDGppNZai63GYoxJKbTUWosxFiNsTam12Gqr" +
                "sRhjayottBhjjMUIX2RsLabaag3GCCNbLC3VWmswxhjdW4ultpqLMT742lIsMdZcAAB3gwMARIKN" +
                "M6wknRWOBhcashIACAkAIBBSijHGGHPOOeekUow55pxzDkIIoVSKMcaccw5CCCGUjDHmnHMQQggh" +
                "hFJKxpxzEEIIIYSQUuqccxBCCCGEEEopnXMOQgghhBBCKaWDEEIIIYQQSiilpBRCCCGEEEIIqaSU" +
                "QgghhFJCKCGVlFIIIYQQQiklpJRSCiGEUkIIoYSUUkophRBCCKWUklJKKaUSSgklhBJSKSmlFEoI" +
                "IZRSSkoppVRKCaGEEkopJaWUUkohhBBKKQUAABw4AAAEGEEnGVUWYaMJFx6AQkNWAgBkAACQopRS" +
                "KS1FgiKlGKQYS0YVc1BaiqhyDFLNqVLOIOYklogxhJSTVDLmFEIMQuocdUwpBi2VGELGGKTYckuh" +
                "cw4AAABBAICAkAAAAwQFMwDA4ADhcxB0AgRHGwCAIERmiETDQnB4UAkQEVMBQGKCQi4AVFhcpF1c" +
                "QJcBLujirgMhBCEIQSwOoIAEHJxwwxNveMINTtApKnUgAAAAAAANAPAAAJBcABER0cxhZGhscHR4" +
                "fICEiIyQCAAAAAAAGQB8AAAkJUBERDRzGBkaGxwdHh8gISIjJAEAgAACAAAAACCAAAQEBAAAAAAA" +
                "AgAAAAQET2dnUwAAQK4AAAAAAAAD8iFxAgAAAG7Wn+ktAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAAoODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4OT2dnUwAAQF4BAAAAAAAD8iFxAwAAANOv740sAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODk9nZ1MAAEAOAgAAAAAAA/IhcQQAAABJ+TcMLAEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg5PZ2dTAABAvgIAAAAAAAPyIXEFAAAACD5+cSwBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4OT2dnUwAAQG4DAAAAAAAD8iFxBgAAAKaU5yMsAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODk9nZ1MAAEAeBAAAAAAAA/IhcQcAAABRyxRKLAEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg5PZ2dTAABAzgQAAAAAAAPyIXEIAAAAjmPTyywBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4OT2dnUwAAQH4FAAAAAAAD8iFxCQAAAGTj31ksAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEAuBgAAAAAAA/IhcQoAAABIiQ7MLAEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABA3gYAAAAAAAPyIXELAAAA42mmPSwBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQI4HAAAAAAAD8iFxDAAAAJitNWYsAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEA+CAAAAAAAA/IhcQ0AAAC76kn6LAEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABA7ggAAAAAAAPyIXEOAAAAvgeVRywBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQJ4JAAAAAAAD8iFxDwAAAN3ye0QsAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEBOCgAAAAAAA/IhcRAAAACQfZagLAEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABA/goAAAAAAAPyIXERAAAA0brf3SwBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQK4LAAAAAAAD8iFxEgAAABxCRZIsAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEBeDAAAAAAAA/IhcRMAAACIT7XmLAEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABADg0AAAAAAAPyIXEUAAAA84smvSwBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQL4NAAAAAAAD8iFxFQAAALJMb8AsAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEBuDgAAAAAAA/IhcRYAAAD9dL1ILAEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAHg8AAAAAAAPyIXEXAAAAnoFTSywB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQM4PAAAAAAAD8iFxGAAAAEEplMos" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEB+EAAAAAAAA/IhcRkAAADYtDKe" +
                "LAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABALhEAAAAAAAPyIXEaAAAAFUyo" +
                "0SwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQN4RAAAAAAAD8iFxGwAAAL6s" +
                "ACAsAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAECOEgAAAAAAA/IhcRwAAAAk" +
                "+tihLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAPhMAAAAAAAPyIXEdAAAA" +
                "znrUMywBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQO4TAAAAAAAD8iFxHgAA" +
                "AMuXCI4sAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAECeFAAAAAAAA/IhcR8A" +
                "AAA8yPvnLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAThUAAAAAAAPyIXEg" +
                "AAAAIDAUeSwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQP4VAAAAAAAD8iFx" +
                "IQAAAGH3XQQsAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEODg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAECuFgAAAAAAA/Ih" +
                "cSIAAABNnYyRLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAXhcAAAAAAAPy" +
                "IXEjAAAATTphjywBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4O" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQA4YAAAAAAAD" +
                "8iFxJAAAAP85gtosAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEO" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEC+GAAAAAAA" +
                "A/IhcSUAAAC+/sunLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "Dg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAbhkAAAAA" +
                "AAPyIXEmAAAAEFRS9SwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQB4aAAAA" +
                "AAAD8iFxJwAAAJIz9ywsAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEDOGgAA" +
                "AAAAA/IhcSgAAABNmzCtLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABAfhsA" +
                "AAAAAAPyIXEpAAAApxs8PywBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAAQC4c" +
                "AAAAAAAD8iFxKgAAAP5JuxosAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MAAEDe" +
                "HAAAAAAAA/IhcSsAAABVqRPrLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dTAABA" +
                "jh0AAAAAAAPyIXEsAAAALm2AsCwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dnUwAA" +
                "QD4eAAAAAAAD8iFxLQAAACV/x/gsAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9nZ1MA" +
                "AEDuHgAAAAAAA/IhcS4AAAAgkhtFLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5PZ2dT" +
                "AABAnh8AAAAAAAPyIXEvAAAAQ2f1RiwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4OT2dn" +
                "UwAAQE4gAAAAAAAD8iFxMAAAAF/PjSssAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODk9n" +
                "Z1MAAED+IAAAAAAAA/IhcTEAAAAeCMRWLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg5P" +
                "Z2dTAABAriEAAAAAAAPyIXEyAAAA0/BeGSwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "T2dnUwAAQF4iAAAAAAAD8iFxMwAAADLF+N0sAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dk9nZ1MAAEAOIwAAAAAAA/IhcTQAAABJAWuGLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg5PZ2dTAABAviMAAAAAAAPyIXE1AAAACMYi+ywBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4OT2dnUwAAQG4kAAAAAAAD8iFxNgAAADLGpsMsAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODk9nZ1MAAEAeJQAAAAAAA/IhcTcAAABRM0jALAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg5PZ2dTAABAziUAAAAAAAPyIXE4AAAAjpuPQSwBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4OT2dnUwAAQH4mAAAAAAAD8iFxOQAAAIWJyAksAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODk9nZ1MAAEAuJwAAAAAAA/IhcToAAABIcVJGLAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg5PZ2dTAABA3icAAAAAAAPyIXE7AAAA45H6tywBAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQ4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O" +
                "Dg4ODg4ODg4OT2dnUwAE8F8oAAAAAAAD8iFxPAAAAP/YXdwhAQEBAQEBAQEBAQEBAQEBAQEBAQEB" +
                "AQEBAQEBAQEBAQEBDg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4ODg4O");
    }
}