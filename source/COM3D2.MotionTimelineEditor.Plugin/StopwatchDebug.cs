using System;
using System.Diagnostics;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StopwatchDebug
    {
        private readonly Stopwatch stopwatch;

        public StopwatchDebug()
        {
#if DEBUG
            stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
        }

        [Conditional("DEBUG")]
        public void ProcessStart()
        {
            stopwatch.Stop();
        }

        [Conditional("DEBUG")]
        public void ProcessEnd(string processName)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            PluginUtils.LogDebug(processName + "：" + (int) elapsed.TotalMilliseconds + "ms");

            stopwatch.Reset();
            stopwatch.Start();
        }
    }
}