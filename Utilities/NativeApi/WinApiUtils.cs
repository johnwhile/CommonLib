using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

using Common.Inputs;
using System.ComponentModel;

namespace Common.WinNative
{
    public static class NativeUtils
    {
        const int WHEEL_DELTA = 120;
        const int WH_MOUSE_LL = 14;

        /// <summary>
        /// CTRL and SHIT are not used, see System.Window.Form.MouseButtons
        /// </summary>
        static MouseEnum ConvertToMyMouseEnum(MK keys)
        {
            MouseEnum flag = MouseEnum.None;
            if ((keys & MK.LBUTTON) != 0) flag |= MouseEnum.Left;
            if ((keys & MK.RBUTTON) != 0) flag |= MouseEnum.Right;
            if ((keys & MK.MBUTTON) != 0) flag |= MouseEnum.Middle;
            if ((keys & MK.XBUTTON1) != 0) flag |= MouseEnum.XButton1;
            if ((keys & MK.XBUTTON2) != 0) flag |= MouseEnum.XButton2;
            return flag;
        }

        /// <summary>
        /// http://winapi.freetechsecrets.com/win32/WIN32WMSIZE.htm
        /// </summary>
        /// <param name="width">width of client area</param>
        /// <param name="height">height of client area</param>
        public static void ResolveMsgSIZE(IntPtr wparam, IntPtr lparam, out SIZE size, out int width, out int height)
        {
            size = (SIZE)wparam.ToInt64();
            width = LOWORD(lparam);
            height = HIWORD(lparam);
        }

        /// <summary>
        /// http://winapi.freetechsecrets.com/win32/WIN32WMMOUSEMOVE.htm
        /// </summary>
        public static void ResolveMsgMOUSEMOVE(IntPtr wparam, IntPtr lparam, ref MyMouseState state)
        {
            state.DownButtons = ConvertToMyMouseEnum((MK)LOWORD(wparam));
            state.Delta = HIWORD(wparam);
            state.Position.x = LOWORD(lparam);
            state.Position.y = HIWORD(lparam);
        }
        public static void ResolveMsgMOUSEMOVE(IntPtr lparam, out int x, out int y)
        {
            x = LOWORD(lparam);
            y = HIWORD(lparam);
        }
        /// <summary>
        /// http://winapi.freetechsecrets.com/win32/WIN32WMMOUSEWHEEL_New__Windows_NT.htm
        /// </summary>
        /// <param name="fKeys"> Indicates whether various virtual keys are down. ATTENTION: when wheeling and buttons are up, return zero. </param>
        /// <param name="zDelta"> Indicates the distance that the wheel is rotated, expressed in multiples or divisions of WHEEL_DELTA, which is 120.</param>
        /// <param name="xPos"> Specifies the x-coordinate of the pointer, relative to the upper-left corner of the screen.</param>
        /// <param name="yPos"> Specifies the y-coordinate of the pointer, relative to the upper-left corner of the screen.</param>
        /// <remarks>
        /// IMPORTANT : Do not use the LOWORD or HIWORD macros to extract the x- and y- coordinates of the cursor position
        /// because these macros return incorrect results on systems with multiple monitors.
        /// Systems with multiple monitors can have negative x- and y- coordinates, and LOWORD and HIWORD treat the coordinates as unsigned quantities.
        /// </remarks>
        public static void ResolveMsgMOUSEWHEEL(IntPtr wparam, IntPtr lparam, ref MyMouseState state)
        {
            state.DownButtons = ConvertToMyMouseEnum((MK)LOWORD(wparam));
            state.Delta = HIWORD(wparam);
            state.Position.x = LOWORD(lparam);
            state.Position.y = HIWORD(lparam);
        }

        public static short LOWORD(IntPtr param)
        {
            return (short)(((uint)param) & 0xffff);
        }

        public static short HIWORD(IntPtr param)
        {
            return (short)((((uint)param) >> 16) & 0xffff);
        }
        public static short HIWORD(int param)
        {
            return (short)((param >> 16) & 0xffff);
        }
        /// <summary>
        /// http://1code.codeplex.com/SourceControl/changeset/view/39074#842775
        /// The function determines whether the current operating system is a 
        /// 64-bit operating system.
        /// </summary>
        /// <returns>
        /// The function returns true if the operating system is 64-bit; 
        /// otherwise, it returns false.
        /// </returns>
        public static bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                return true;
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows
            {
                // Detect whether the current process is a 32-bit process 
                // running on a 64-bit system.
                bool flag;
                return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                    WinApi.IsWow64Process(WinApi.GetCurrentProcess(), out flag)) && flag);
            }
        }

        /// <summary>
        /// The function determines whether a method exists in the export 
        /// table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName 
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = WinApi.GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (WinApi.GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        /// <summary>
        /// Throw the error called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
        /// </summary>
        public static void ThrowLastUnmanagedErrorAsException()
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
