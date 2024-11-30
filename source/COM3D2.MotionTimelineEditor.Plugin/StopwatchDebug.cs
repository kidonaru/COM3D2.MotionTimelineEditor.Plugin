using System;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StopwatchDebug
    {
        private readonly Stopwatch stopwatch;

        private static Config config => ConfigManager.config;

        private bool isEnabled
        {
            get
            {
                return config.outputElapsedTime;
            }
        }

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
                PluginUtils.Log(string.Format("{0}: {1:F3}ms", processName, elapsed.TotalMilliseconds));

                stopwatch.Reset();
                stopwatch.Start();
            }
        }
    }
}