using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Common
{
    public static class TimerHelper
    {
        /// <summary>
        /// <b>MinimumResolution</b>: Highest possible delay (in 100-ns units) between timer events<br/>
        /// <b>MaximumResolution</b>: Lowest possible delay(in 100-ns units) between timer events.<br/>
        /// <b>CurrentResolution</b>: Current timer resolution, in 100-ns unit<br/>
        /// </summary>
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);

        /// <summary>
        /// Milliseconds of lowest resolution (for me is 15ms)
        /// </summary>
        public static readonly double LowestSleepThreshold;

        static TimerHelper()
        {
            uint min, max, current;
            NtQueryTimerResolution(out min, out max, out current);
            LowestSleepThreshold = 1.0 + (max / 10000.0);
        }

        /// <summary>
        /// Returns the current timer resolution in milliseconds
        /// </summary>
        public static double GetCurrentResolution()
        {
            uint min, max, current;
            NtQueryTimerResolution(out min, out max, out current);
            return current / 10000.0;
        }

        /// <summary>
        /// Sleeps as long as possible without exceeding the specified period
        /// </summary>
        public static void SleepForNoMoreThan(double milliseconds)
        {

            // Assumption is that Thread.Sleep(t) will sleep for at least (t), and at most (t + timerResolution)
            if (milliseconds < LowestSleepThreshold) return;

            var sleepTime = (int)(milliseconds - GetCurrentResolution());
            
            if (sleepTime > 0) Thread.Sleep(sleepTime);
        }
    }
}
