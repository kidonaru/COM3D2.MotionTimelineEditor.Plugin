using System;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StopwatchDebug
    {
        private readonly Stopwatch stopwatch;

#if COM3D2
        private static Config config => ConfigManager.instance.config;

        private bool isEnabled
        {
            get
            {
                return config.outputElapsedTime;
            }
        }
#else
        private bool isEnabled = true;
#endif

        public StopwatchDebug()
        {
            if (isEnabled)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
        }

        public void ProcessStart()
        {
            if (isEnabled)
            {
                stopwatch.Reset();
                stopwatch.Start();
            }
        }

        public void ProcessEnd(string processName)
        {
            if (isEnabled)
            {
                TimeSpan elapsed = stopwatch.Elapsed;
#if COM3D2
                PluginUtils.Log(string.Format("{0}: {1:F3}ms", processName, elapsed.TotalMilliseconds));
#else
                UnityEngine.Debug.Log(string.Format("{0}: {1:F3}ms", processName, elapsed.TotalMilliseconds));
#endif

                stopwatch.Reset();
                stopwatch.Start();
            }
        }
    }
}