
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Common.Tools;
using Common.WinNative;

namespace Common.Inputs
{
    /// <summary>
    /// from: https://support.microsoft.com/en-us/kb/318804
    /// from: http://globalmousekeyhook.codeplex.com/
    /// from: https://code.msdn.microsoft.com/windowsapps/CSWindowsHook-a578ed12
    /// 
    /// Base class used to implement mouse or keyboard hook listeners.
    /// It provides base methods to subscribe and unsubscribe to hooks.
    /// Common processing, error handling and cleanup logic.
    /// </summary>
    public abstract class WinHook : Disposable
    {
        protected bool acquired;
        protected bool is64bitOS;
        protected bool isGlobalLowLevel;
        protected HookLevel level;

        /// <summary>
        /// Need to call Install() to activate hook
        /// </summary>
        protected WinHook(HookLevel level)
        {
            acquired = false;
            is64bitOS = NativeUtils.Is64BitOperatingSystem();
            this.level = level;
        }

        /// <summary>
        /// Update isn't do by you
        /// </summary>
        public virtual bool Update()
        {
            return true;
        }

        /// <summary>
        /// Keeps the reference to prevent garbage collection of delegate. See: CallbackOnCollectedDelegate
        /// http://msdn.microsoft.com/en-us/library/43yky316(v=VS.100).aspx
        /// </summary>
        private NativeMethods.HookProc HookCallbackReferenceKeeper { get; set; }

        /// <summary>
        /// Stores the handle to the Keyboard or Mouse hook procedure.
        /// </summary>
        protected int HookHandle { get; set; }
        /// <summary>
        /// Override to deliver correct id to be used for <see cref="HookNativeMethods.SetWindowsHookEx"/> call.
        /// </summary>
        protected abstract WH HookId { get; }

        /// <summary>
        /// Override this method to modify logic of firing events.
        /// </summary>
        protected abstract void ProcessLowLevelCallback(IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// Override this method to modify logic of firing events.
        /// </summary>
        protected abstract void ProcessAppLevelCallback(IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// A callback function which will be called every time a keyboard or mouse activity detected.
        /// </summary>
        protected IntPtr winHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // for MouseProc and LowLevelMouseProc callback function:
            // https://msdn.microsoft.com/en-us/library/ms644988(v=vs.85).aspx

            if (isGlobalLowLevel)
            {
                if (nCode >= 0) 
                    ProcessLowLevelCallback(wParam, lParam);
            }
            else
            {
                if ( nCode == (int)HC.ACTION)
                    ProcessAppLevelCallback(wParam, lParam);
            }
            
            // if event handle = false can continue process the same event to other applications
            //if (!continueProcessing) return new IntPtr(-1);

            return NativeMethods.CallNextHookEx(HookHandle, nCode, wParam, lParam);
        }


        public bool IsAcquired
        {
            get { return acquired; }
        }

        public void EnableAcquiring()
        {
            Install(level);
        }
        /// <summary>
        /// Subscribes to the hook and starts firing events.
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public void Install(HookLevel level)
        {
            Debugg.Message($"WinHook start acquiring with level : {level}");

            isGlobalLowLevel = (level == HookLevel.GlobalLowLevel);

            if (HookHandle != 0)
                throw new InvalidOperationException("Hook is already started.");

            HookCallbackReferenceKeeper = new NativeMethods.HookProc(winHookCallback);

            try
            {
                HookHandle = isGlobalLowLevel ?
                   NativeMethods.SetWindowsHookEx((int)HookId, HookCallbackReferenceKeeper, Process.GetCurrentProcess().MainModule.BaseAddress, 0) :
                   NativeMethods.SetWindowsHookEx((int)HookId, HookCallbackReferenceKeeper, IntPtr.Zero, NativeMethods.GetCurrentThreadId());

                if (HookHandle == 0) NativeUtils.ThrowLastUnmanagedErrorAsException();
            }
            catch (Exception)
            {
                HookCallbackReferenceKeeper = null;
                HookHandle = 0;
                throw;
            }
            acquired = true;
        }

        /// <summary>
        /// Unsubscribes from the hook and stops firing events.
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public void StopAcquiring()
        {
            Debugg.Message("WinHook stop acquiring");
            try
            {
                if (HookHandle == 0)
                    Debugg.Message("Hook is already removed.");
                else
                {
                    if (NativeMethods.UnhookWindowsHookEx(HookHandle) == 0)
                        NativeUtils.ThrowLastUnmanagedErrorAsException();
                }
            }
            finally
            {
                HookCallbackReferenceKeeper = null;
                HookHandle = 0;
            }
            acquired = false;
        }
        /// <summary>
        /// Release delegates, unsubscribes from hooks without throw exception
        /// </summary>
        protected override void Dispose(bool isDisposing)
        {
            if (!IsDisposed)
            {
                StopAcquiring();
            }
            else
            {
                if (HookHandle != 0) NativeMethods.UnhookWindowsHookEx(HookHandle);
            }
        }
    }

    /// <summary>
    /// The Hook Codes
    /// </summary>
    public enum HC : int
    {
        ACTION = 0,
        GETNEXT = 1,
        SKIP = 2,
        NOREMOVE = 3,
        SYSMODALON = 4,
        SYSMODALOFF = 5
    }

    /// <summary>
    /// Windows Hook Type
    /// </summary>
    public enum WH : int
    {
        /// <summary>
        /// Installs a hook procedure that monitors messages before the system sends them to the destination window procedure.
        /// </summary>
        CALLWNDPROC = 4,
        /// <summary>
        /// Installs a hook procedure that monitors messages after they have been processed by the destination window procedure.
        /// </summary>
        CALLWNDPROCRET = 12,
        /// <summary>
        /// Installs a hook procedure that receives notifications useful to a CBT application
        /// </summary>
        CBT = 5,
        /// <summary>
        /// Installs a hook procedure useful for debugging other hook procedures.
        /// </summary>
        DEBUG = 9,
        /// <summary>
        /// Installs a hook procedure that will be called when the application's foreground thread is about to become idle.
        /// This hook is useful for performing low priority tasks during idle time.
        /// </summary>
        FOREGROUNDIDLE = 11,
        /// <summary>
        /// Installs a hook procedure that monitors messages posted to a message queue.
        /// </summary>
        GETMESSAGE = 3,
        /// <summary>
        /// Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook procedure.
        /// </summary>
        JOURNALPLAYBACK = 1,
        /// <summary>
        /// Installs a hook procedure that records input messages posted to the system message queue. This hook is useful for recording macros.
        /// </summary>
        JOURNALRECORD = 0,
        /// <summary>
        /// Installs a hook procedure that monitors keystroke messages.
        /// </summary>
        KEYBOARD = 2,
        /// <summary>
        /// Installs a hook procedure that monitors low-level keyboard input events.
        /// </summary>
        KEYBOARD_LL = 13,
        /// <summary>
        /// Installs a hook procedure that monitors mouse messages.
        /// </summary>
        MOUSE = 7,
        /// <summary>
        /// Installs a hook procedure that monitors low-level mouse input events.
        /// </summary>
        MOUSE_LL = 14,
        /// <summary>
        ///Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box, message box, menu, or scroll bar. 
        /// </summary>
        MSGFILTER = -1,
        /// <summary>
        /// Installs a hook procedure that receives notifications useful to shell applications.
        /// </summary>
        SHELL = 10,
        /// <summary>
        /// Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box, message box, menu, or scroll bar.
        /// The hook procedure monitors these messages for all applications in the same desktop as the calling thread.
        /// </summary>
        SYSMSGFILTER = 6
    }

    /// <summary>
    /// 
    /// </summary>
    public enum HookLevel
    {
        /// <summary>
        /// Enables you to monitor messages about to be returned by the GetMessage or PeekMessage function.
        /// This mean that work only inside ClientRectangle and for focused application but isn't the purpose
        /// of my hook system...
        /// </summary>
        Application,
        /// <summary>
        /// Except for the WH_KEYBOARD_LL low-level hook and the WH_MOUSE_LL low-level hook,
        /// you cannot implement global hooks in the Microsoft .NET Framework.
        /// This mean that work independently from application, can substitute DirectInput for mouse and keyboard
        /// </summary>
        GlobalLowLevel
    }


    static class NativeMethods
    {
        #region Native
        /// <summary>
        /// The system calls this function before calling the window procedure to process a message sent to the thread.
        /// https://msdn.microsoft.com/it-it/library/windows/desktop/ms644975(v=vs.85).aspx
        /// </summary>
        internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain. 
        /// A hook procedure can call this function either before or after processing the hook information.
        /// https://msdn.microsoft.com/it-it/library/windows/desktop/ms644974(v=vs.85).aspx
        /// </summary>
        /// <param name="idHook">Ignored.</param>
        /// <param name="nCode">The next hook procedure uses this code to determine how to process the hook information.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain. 
        /// You would install a hook procedure to monitor the system for certain types of events. These events 
        /// are associated either with a specific thread or with all threads in the same desktop as the calling thread. 
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644990(v=vs.85).aspx
        /// </summary>
        /// <param name="idHook"> Specifies the type of hook procedure to be installed. (WH flag)</param>
        /// <param name="lpfn"> Pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a 
        /// thread created by a different process, the lpfn parameter must point to a hook procedure in a dynamic-link 
        /// library (DLL). Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
        /// </param>
        /// <param name="hMod">
        /// Handle to the DLL containing the hook procedure pointed to by the lpfn parameter. 
        /// The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by 
        /// the current process and if the hook procedure is within the code associated with the current process. 
        /// </param>
        /// <param name="dwThreadId">
        /// Specifies the identifier of the thread with which the hook procedure is to be associated. 
        /// If this parameter is zero, the hook procedure is associated with all existing threads running in the 
        /// same desktop as the calling thread. 
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to the hook procedure.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// https://msdn.microsoft.com/it-it/library/windows/desktop/ms644993(v=vs.85).aspx
        /// </summary>
        /// <param name="idHook"> Handle to the hook to be removed.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int UnhookWindowsHookEx(int idHook);
        /// <summary>
        /// Retrieves the unmanaged thread identifier of the calling thread.
        /// </summary>
        [DllImport("kernel32")]
        internal static extern int GetCurrentThreadId();

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working). 
        /// The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that 
        /// created the window. 
        /// </summary>
        /// <param name="handle">A handle to the window. </param>
        /// <param name="processId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, 
        /// GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not. </param>
        /// <returns>The return value is the identifier of the thread that created the window. </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// The Point structure defines the X- and Y- coordinates of a point. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Point
        {
            public int X;
            public int Y;
        }


        /// <summary>
        /// The <see cref="MouseStruct"/> structure contains information about a mouse input event.
        /// See full documentation at http://globalmousekeyhook.codeplex.com/wikipage?title=MouseStruct
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MouseStruct
        {
            /// <summary>
            /// Specifies a Point structure that contains the X- and Y-coordinates of the cursor, in screen coordinates. 
            /// </summary>
            [FieldOffset(0)]
            public Point Point;

            /// <summary>
            /// Specifies information associated with the message.
            /// The possible values are:
            /// <list type="bullet">
            /// <item><description>0 - No Information</description></item>
            /// <item><description>1 - X-Button1 Click</description></item>
            /// <item><description>2 - X-Button2 Click</description></item>
            /// <item><description>120 - Mouse Scroll Away from User</description></item>
            /// <item><description>-120 - Mouse Scroll Toward User</description></item>
            /// </list>
            /// </summary>
            [FieldOffset(10)]
            public Int16 MouseData;

            /// <summary>
            /// Returns a Timestamp associated with the input, in System Ticks.
            /// </summary>
            [FieldOffset(16)]
            public Int32 Timestamp;
        }

        /// <summary>
        /// The AppMouseStruct structure contains information about a application-level mouse input event.
        /// </summary>
        /// <remarks>
        /// See full documentation at http://globalmousekeyhook.codeplex.com/wikipage?title=MouseStruct
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        internal struct AppMouseStruct32bit
        {
            [FieldOffset(0)]
            public Point Point;

            [FieldOffset(22)]
            public Int16 MouseData;

            /// <summary>
            /// Converts the current <see cref="AppMouseStruct"/> into a <see cref="MouseStruct"/>.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// The AppMouseStruct does not have a timestamp, thus one is generated at the time of this call.
            /// </remarks>
            public MouseStruct ToMouseStruct()
            {
                MouseStruct tmp = new MouseStruct();
                tmp.Point = this.Point;
                tmp.MouseData = this.MouseData;
                tmp.Timestamp = Environment.TickCount;
                return tmp;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct AppMouseStruct64bit
        {
            /// <summary>
            /// Specifies a Point structure that contains the X- and Y-coordinates of the cursor, in screen coordinates. 
            /// </summary>
            [FieldOffset(0)]
            public Point Point;

            [FieldOffset(34)]
            public Int16 MouseData;

            public MouseStruct ToMouseStruct()
            {
                MouseStruct tmp = new MouseStruct();
                tmp.Point = this.Point;
                tmp.MouseData = this.MouseData;
                tmp.Timestamp = Environment.TickCount;
                return tmp;
            }
        }

        #endregion
    }
}
