using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Common.Tools
{
    /// <summary>
    /// Use high precise timer. Is a "reader" of computer clock but i can't work if high-performance counter isn't supported
    /// from : C# Game Programming: For Serious Game Creation
    /// 
    /// but StopWatch can do the same thing.
    /// </summary>
    public static class PreciseTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        static long ticks;
        static long begin;

        /// <summary>
        /// How many ticks for one seconds
        /// </summary>
        public static readonly long freq;
        /// <summary>
        /// How many ticks for one seconds (cast to double)
        /// </summary>
        public static readonly double freqS;
        /// <summary>
        /// How many ticks for one millisecond (cast to double)
        /// </summary>
        public static readonly double freqMS;

        public static readonly bool HighPerformanceSupported;
        
        /// <summary>
        /// </summary>
        static PreciseTimer()
        {
            if (QueryPerformanceFrequency(out freq) && freq > 0)
            {
                freqS = freq;
                freqMS = freqS / 1000.0;

                // These value are relative to your device
                long t0, t1;

                //SpinWait measure, is about 0,3 ms
                QueryPerformanceCounter(out t0);
                Thread.SpinWait(100000);
                QueryPerformanceCounter(out t1);
                MS_For100000SpinIterations = (t1 - t0) / freqMS;

                //Sleep measure, is about 15ms
                QueryPerformanceCounter(out t0);
                for (int i = 0; i < 10; i++) Thread.Sleep(1);
                QueryPerformanceCounter(out t1);
                MS_SleepCost = (t1 - t0) / freqMS / 10;

                HighPerformanceSupported = true;
            }
            else
            {
                Debug.Fail("high-performance counter not supported");
                freq = Stopwatch.Frequency;
                freqS = freq;
                freqMS = freqS / 1000.0;

                HighPerformanceSupported = false;
            }

            if (!QueryPerformanceCounter(out begin)) begin = 0;
        }

        /// <summary>
        /// Return Ticks Per Seconds of your device
        /// </summary>
        public static long Frequence
        {
            get { return freq; }
        }

        /// <summary>
        /// Get current absolute ticks counter (precise)
        /// </summary>
        public static long Ticks
        {
            get
            {
                if (QueryPerformanceCounter(out ticks)) return ticks;
                else return -1;
            }
        }
        /// <summary>
        /// Get absolute milliseconds counter
        /// </summary>
        public static double Milliseconds
        {
            get
            {
                if (QueryPerformanceCounter(out ticks)) 
                {
                    return ticks / freqMS;
                }
                else return -1;
            }
        }
        /// <summary>
        /// Conversion to milliseconds
        /// </summary>
        public static double GetMilliseconds(long ticks)
        {
            return ticks / freqMS;
        }
        /// <summary>
        /// Conversion to ticks
        /// </summary>
        public static long GetTicks(double milliseconds)
        {
            return (long)(milliseconds * freqMS);
        }
        /// <summary>
        /// Thread.SpinWait() iterations doesn't have relations with time, depend by your machine
        /// </summary>
        public static double MS_For100000SpinIterations { get; private set; }
        /// <summary>
        /// Thread.Sleep() require about 15ms on my pc, so don't work for smaller values
        /// </summary>
        public static double MS_SleepCost { get; private set; }


        /// <summary>
        /// print time with format HH:mm:ss.fff
        /// </summary>
        public static string ReadableTime(double ms)
        {
            return DateTime.FromBinary(0).AddMilliseconds(ms).ToString("HH:mm:ss.fff");
        }


        public static string GetString()
        {
            return string.Format("Time: {0} freq: {1}", new TimeSpan(ticks), freq);
        }
    }
}
