using Common.Maths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace Common.WinNative
{
    /// <summary>
    /// Internal class to interact with Native Message
    /// </summary>
    public static class WinApi
    {
        /*
        [NativeCppClass]
        [StructLayout(LayoutKind.Sequential, Size = 8)]
        public struct POINT
        {
            public int X, Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        */


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern int WaitForSingleObjectEx(IntPtr hHandle, [MarshalAs(UnmanagedType.U4)] int dwMilliseconds,bool bAlertable);


        #region Message
        /// <summary>
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "PeekMessage"), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);
        /// <summary>
        /// Retrieves a message from the calling thread's message queue.
        /// The function dispatches incoming sent messages until a posted message is available for retrieval.
        /// </summary>
        /// <returns>
        /// -1: If there is an error, the return value is -1.
        /// 0: If the function retrieves the WM_QUIT message, the return value is zero.
        /// non zero: 
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// ClientRectangle are relative to windows location
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);

        #endregion


        #region Keyboard-Mouse

        /// <summary>
        /// </summary>
        /// <remarks>
        /// GetKeyState reports the state of the keyboard based on the messages you have retrieved from your input queue.
        /// This is not the same as the physical keyboard state:
        /// 
        /// * GetKeyState doesn’t report those changes until you use the PeekMessage function or the GetMessage function
        ///   to retrieve the message from your input queue.
        ///   
        /// * If the user has switched to another program, then the GetKeyState function will not see the input that the user
        ///   typed into that other program, since that input was not sent to your input queue.
        ///   
        /// This mean that to read next keyState you have to call Application.DoEvents or can be insert in the Application.OnIdle event
        /// </remarks>
        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32")]
        public static extern short GetKeyState(int vKey);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        #endregion

        #region Keyboard

        public enum MAPVK : uint 
        {
            /// <summary>
            /// The uCode parameter is a virtual-key code and is translated into a scan code.
            /// If it is a virtual-key code that does not distinguish between left- and right-hand keys, 
            /// the left-hand scan code is returned.If there is no translation, the function returns 0.
            /// </summary>
            VK_TO_VSC = 0,
            /// <summary>
            /// The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish
            /// between left- and right-hand keys. If there is no translation, the function returns 0.
            /// </summary>
            VSC_TO_VK = 1,
            /// <summary>
            /// The uCode parameter is a virtual-key code and is translated into an unshifted character value in the
            /// low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value.
            /// If there is no translation, the function returns 0.
            /// </summary>
            VK_TO_CHAR = 2,
            /// <summary>
            /// The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys.
            /// If there is no translation, the function returns 0.
            /// </summary>
            VSC_TO_VK_EX = 3,
            /// <summary>
            /// <b>Windows Vista and later:</b> The uCode parameter is a virtual-key code and is translated into a scan code.
            /// If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned.
            /// If the scan code is an extended scan code, the high byte of the uCode value can contain either 0xe0 or 0xe1 to specify the extended scan code.
            /// If there is no translation, the function returns 0.
            /// </summary>
            VK_TO_VSC_EX = 4
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uCode"><see cref="Keys.KeyCode"/></param>
        /// <param name="uMapType"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(uint uCode, MAPVK uMapType = MAPVK.VK_TO_CHAR);
        

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetKeyboardState(byte[] data);

        #endregion

        #region Mouse
        
        


        /// <summary>
        /// The MOUSEHOOKSTRUCT structure contains information about a mouse event passed 
        /// to a WH_MOUSE hook procedure, MouseProc. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT
        {
            /// <summary>
            /// The x- and y-coordinates of the cursor, in screen coordinates.
            /// </summary>
            public Vector2i pt;
            /// <summary>
            /// A handle to the window that will receive the mouse message corresponding to the mouse event.
            /// </summary>
            public int hwnd;
            /// <summary>
            /// The hit-test value. For a list of hit-test values, see the description of the WM_NCHITTEST message.
            /// </summary>
            public int wHitTestCode;

            public int dwExtraInfo;

            public static Type TypeOf = typeof(MOUSEHOOKSTRUCT);
        }

        /// <summary>
        /// The MSLLHOOKSTRUCT structure contains information about a low-level keyboard 
        /// input event. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            /// <summary>
            /// The x- and y-coordinates of the cursor, in screen coordinates.
            /// </summary>
            public Vector2i pt;
            /// <summary>
            /// The mouse wheel and button info.
            /// </summary>
            public int mouseData;

            public int flags;
            /// <summary>
            /// Specifies the time stamp for this message. 
            /// </summary>
            public int time;

            public IntPtr dwExtraInfo;

            public static Type TypeOf = typeof(MSLLHOOKSTRUCT);
        }




        /// <summary>
        /// The KeyboardHookStruct structure contains information about a low-level keyboard input event. 
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/cwpstruct.asp
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            /// </summary>
            public int VirtualKeyCode;
            /// <summary>
            /// Specifies a hardware scan code for the key. 
            /// </summary>
            public int ScanCode;
            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int Flags;
            /// <summary>
            /// Specifies the Time stamp for this message.
            /// </summary>
            public int Time;
            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            public int ExtraInfo;

            public static Type TypeOf = typeof(KBDLLHOOKSTRUCT);
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Vector2i lpPoint);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern bool SetScreenCursorPos(int X, int Y);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Vector2i lpPoint);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Vector2i lpPoint);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);
        #endregion

    }
}
