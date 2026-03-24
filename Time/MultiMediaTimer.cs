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
    /// Hi-resolution wait timer, internally uses multimedia timer, is Thread Safe
    /// from: https://www.codeproject.com/Articles/5501/The-Multimedia-Timer-for-the-NET-Framework
    /// from: http://www.pinvoke.net/default.aspx/winmm/timesetevent.html?diff=y
    /// </summary>
    public sealed class MultiMediaTimer : IDisposable
    {
        bool isRunning;
        int timerID = 0;
        int resolution = 1;

        long startTick = 0;

        /// <summary>
        /// Multimedia timer capabilities.
        /// </summary>
        static TIMECAPS caps;

        /// <summary>
        /// Called by Windows when a timer event occurs.
        /// Must be here to prevent GC collection of delegate
        /// </summary>
        TimeProc timeProcPeriodic;
        TimeProc timeProcOneShot;

        public Thread mythread;
        

        /// <summary>
        /// Represents methods that raise events.
        /// </summary>
        /// <param name="elapsedTicks">return the time elapsed from start event, theoretically match with value set on start function, the difference is the cost time of all process</param>
        public delegate void TimeHandler(long elapsedTicks);

        /// <summary>
        /// Occurs when the time period has elapsed, insert here your action
        /// </summary>
        public event TimeHandler OnTick;


        static MultiMediaTimer()
        {
            timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
        }


        private MultiMediaTimer()
        {
            isRunning = false;
            timeProcPeriodic = new TimeProc(TimerPeriodicCallback);
            timeProcOneShot = new TimeProc(TimerOneShotCallback);
        }


        static MultiMediaTimer singletoninstance;

        public static MultiMediaTimer GetSingleton()
        {
            if (singletoninstance==null)
            {
                singletoninstance = new MultiMediaTimer();
            }
            return singletoninstance;
        }

        /// <summary>
        /// Starts one shot periodic timer.
        /// (from space engineers source code: Handler must be STORED somewhere to prevent GC collection until it's called!)
        /// </summary>
        public void StartOneShot(int intervalMS)
        {
            if (isRunning)
            {
                return;
            }

            //Debug.WriteLine("TimerID " + timerID.ToString());

            if (timerID != 0)
            {
                string current = Thread.CurrentThread.Name;

                throw new Exception("TimerTick not disposed/stop before starting again! timerID = " + timerID.ToString());

            }

            startTick = PreciseTimer.Ticks;
            timeBeginPeriod(1);
            // Create and start timer.
            timerID = timeSetEvent(intervalMS, resolution, timeProcOneShot, IntPtr.Zero, ONESHOT);

            //Debug.WriteLine("TimerID " + timerID.ToString());

            if (timerID != 0)
                isRunning = true;
            else
                throw new Exception("multimedia 'timeSetEvent' error");
        }

        /// <summary>
        /// Starts periodic timer.
        /// </summary>
        public void StartPeriodic(int intervalMS)
        {
            if (isRunning)
            {
                return;
            }

            if (timerID != 0) throw new Exception("TimerTick not disposed/stop before starting again!");

            startTick = PreciseTimer.Ticks;
            timeBeginPeriod(1);
            // Create and start timer.
            timerID = timeSetEvent(intervalMS, resolution, timeProcPeriodic, IntPtr.Zero, PERIODIC);

            if (timerID != 0)
                isRunning = true;
            else
                throw new Exception("multimedia 'timeSetEvent' error");
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            if (timerID != 0)
            {
                int result = timeKillEvent(timerID);

                if (result != NOERROR)
                {
                    Debug.WriteLine("multimedia 'timeKillEvent' error: " + result.ToString());
                    //throw new Exception("multimedia 'timeKillEvent' error: " + result.ToString());
                }
                isRunning = false;

                timeEndPeriod(1);
                timerID = 0;
            }
        }

        /// <summary>
        /// Callback method called by the Win32 multimedia timer when a timer periodic event occurs.
        /// </summary>
        void TimerPeriodicCallback(int id, int msg, IntPtr user, int param1, int param2)
        {
            OnTick(PreciseTimer.Ticks - startTick);
            startTick = PreciseTimer.Ticks;
        }

        /// <summary>
        /// Callback method called by the Win32 multimedia timer when a timer one shot event occurs.
        /// </summary>
        void TimerOneShotCallback(int id, int msg, IntPtr user, int param1, int param2)
        { 
            Stop();
            OnTick(PreciseTimer.Ticks - startTick);
           
        }

        /// <summary>
        /// releases unmanaged resources
        /// </summary>
        public void Dispose()
        {
            OnTick = null;
            Stop();
            GC.SuppressFinalize(this);
        }

        ~MultiMediaTimer()
        {
            Dispose();
            //Debug.Fail("Timer not disposed!");
            //Stop(); // Valid, 
        }


        const int ONESHOT = 0;
        const int PERIODIC = 1;
        const int NOERROR = 0;

        /// <summary>
        /// Represents the method that is called by Windows when a timer event occurs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        private delegate void TimeProc(int id, int msg, IntPtr user, int param1, int param2);


        [StructLayout(LayoutKind.Sequential)]
        private struct TIMECAPS
        {
            /// <summary>
            /// Minimum supported period in milliseconds.
            /// </summary>
            internal int wPeriodMin;
            /// <summary>
            /// Maximum supported period in milliseconds.
            /// </summary>
            internal int wPeriodMax;
        }

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TIMECAPS caps, int sizeOfTimerCaps);
        
        
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc handler, IntPtr user, int eventType);
        
        
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);
        /// <summary>
        /// inform the operating system that you need high timing precision
        /// </summary>
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        /// <summary>
        /// inform the OS that you no longer need high timing precision
        /// </summary>
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
    }

}
