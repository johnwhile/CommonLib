using System;

using Common.Tools;

namespace Common
{

    /// <summary>
    /// This provides timing information similar to <see cref="Stopwatch"/> but an update occurring only on a <see cref="Update"/> method.
    /// </summary>
    public class TimerTick
    {
        long start;
        long last;
        bool pause;
        long pauseStartTime;
        long timePaused;
        readonly static Func<long> GetCurrentTick;

        /// <summary>
        /// Gets the total time elapsed since the last reset or when this timer was created.
        /// </summary>
        public TimeSpan TotalTime { get; private set; }
        /// <summary>
        /// Gets the elapsed time since the previous call to <see cref="Update"/>.
        /// </summary>
        public TimeSpan ElapsedFrameTime { get; private set; }
        /// <summary>
        /// Same of <b><see cref="ElapsedFrameTime"/></b> taking into account <see cref="Pause"/> time.
        /// </summary>
        public TimeSpan ElapsedAdjustedTime { get; private set; }

        
        static TimerTick()
        {
            if (PreciseTimer.HighPerformanceSupported)
                GetCurrentTick = GetTickByPreciseTimer;
            else
                GetCurrentTick = GetTickByStopWatch;
        }

        public TimerTick()
        {
            Reset();
        }

        public bool IsPaused => pause;

        /// <summary>
        ///  Resets this instance. <see cref="TotalTime"/> is set to zero or ticks (when specified).
        /// </summary>
        /// <param name="ticks"></param>
        public void Reset(long ticks = 0)
        {
            TotalTime = TimeSpan.FromTicks(ticks);
            ElapsedFrameTime = TimeSpan.Zero;
            ElapsedAdjustedTime = TimeSpan.Zero;

            start = GetCurrentTick() - ticks;
            last = start;
        }

        /// <summary>
        /// Update counters, it must be called on a regular basis at every *tick*.
        /// </summary>
        public void Update()
        {
            if (IsPaused) return;
            long current = GetCurrentTick();
            TotalTime = TimeSpan.FromTicks(current - start);
            ElapsedFrameTime = TimeSpan.FromTicks(current - last);
            ElapsedAdjustedTime = TimeSpan.FromTicks(current - (last + timePaused));

            if (ElapsedAdjustedTime < TimeSpan.Zero) { ElapsedAdjustedTime = TimeSpan.Zero; }

            timePaused = 0;
            last = current;
        }
        /// <summary>
        /// the purpose is to measure time intervals for debugging
        /// </summary>
        public TimeSpan GetCurrentTime => TimeSpan.FromTicks(GetCurrentTick());

        /// <summary>
        /// Get current absolute ticks counter
        /// </summary>
        public static long Ticks => GetCurrentTick();
        /// <summary>
        /// Get MilliSeconds from Ticks
        /// </summary>
        public static double GetMSFromTick(long ticks) => PreciseTimer.GetMilliseconds(ticks);

        /// <summary>
        /// Get Ticks from MilliSeconds
        /// </summary>
        public static long GetTicksFromMS(double ms) => PreciseTimer.GetTicks(ms);
        
        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public void Pause()
        {
            if (IsPaused) return;
            pauseStartTime = GetCurrentTick();
            pause = true;
        }
        /// <summary>
        /// Resumes this instance, only if a call to <see cref="Pause"/> has been already issued.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused) return;
            timePaused += GetCurrentTick() - pauseStartTime;
            pauseStartTime = 0;
        }


        static long GetTickByPreciseTimer()
        {
            return PreciseTimer.Ticks;
        }

        static long GetTickByStopWatch()
        {
            return System.Diagnostics.Stopwatch.GetTimestamp();
        }
    }
}
