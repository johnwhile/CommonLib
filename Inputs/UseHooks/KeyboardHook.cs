
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Common.WinNative;

namespace Common.Inputs
{
    /// <summary>
    /// This class monitors all keyboard activities and provides appropriate events.
    /// </summary>
    public class KeyboardHook : WinHook, IKeyboard
    {
        public event MyKeyEventHandler OnKeyStateChanged;
        /// <summary>
        /// MyKeyEventHandler(WM extra, MyKeyboardArg arg)
        /// </summary>
        public event MyKeyEventHandler OnKeyDown;
        public event MyKeyEventHandler OnKeyUp;

        MyKeyboardState current;
        MyKeyboardArg argument;

        public KeyboardHook(HookLevel level) : base(level)
        {
            current = new MyKeyboardState();
            argument = new MyKeyboardArg();
            argument.state = current;
        }


        /// <summary>
        /// Get current mouse state, will be update by computer
        /// </summary>
        public MyKeyboardState GetKyboardState
        {
            get { return current; }
        }

        /// <summary>
        /// Returns the correct hook id to be used for <see cref="HookNativeMethods.SetWindowsHookEx"/> call.
        /// </summary>
        protected override WH HookId
        {
            get { return isGlobalLowLevel ? WH.KEYBOARD_LL : WH.KEYBOARD; }
        }

        /// <summary>
        /// This method processes the data from the hook and initiates event firing for
        /// LowLevelKeyboardProc input
        /// https://msdn.microsoft.com/en-us/library/ms644985(v=vs.85).aspx
        /// </summary>
        protected override void ProcessLowLevelCallback(IntPtr wParam, IntPtr lParam)
        {
            WinApi.KBDLLHOOKSTRUCT keyboardHookStruct = (WinApi.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, WinApi.KBDLLHOOKSTRUCT.TypeOf);
            
            WM msg = (WM)wParam.ToInt32();
            int vkey = keyboardHookStruct.VirtualKeyCode;
            argument.key = (vKey)vkey;

            switch (msg)
            {
                case WM.KEYDOWN:
                    current.AllStates[vkey] = KeyState.Down;
                    if (OnKeyDown != null) OnKeyDown(msg, argument);
                    break;

                case WM.KEYUP:
                    current.AllStates[vkey] = KeyState.Up;
                    if (OnKeyUp != null) OnKeyUp(msg, argument);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine("ProcessLowLevelCallback UNKNOW MESSAGE : " + msg.ToString()); break;
            }

            if (OnKeyStateChanged != null) OnKeyStateChanged(msg, argument);
        }

        /// <summary>
        /// This method processes the data from the hook and initiates event firing for
        /// KeyboardProc input
        /// https://msdn.microsoft.com/en-us/library/ms644984(v=vs.85).aspx
        /// </summary>
        protected override void ProcessAppLevelCallback(IntPtr wParam, IntPtr lParam)
        {
            Int32 w = lParam.ToInt32();
            int vkey = wParam.ToInt32();

            argument.key = (vKey)vkey;
            WM msg = WM.NULL;

            if ((w & 0x80000000) != 0)
            {
                current.AllStates[vkey] = KeyState.Up;
                msg = WM.KEYUP;
                if (OnKeyUp != null) OnKeyUp(msg, argument);
            }
            else
            {
                current.AllStates[vkey] = KeyState.Down;
                msg = WM.KEYDOWN;
                if (OnKeyDown != null) OnKeyDown(msg, argument);
            }

            if (OnKeyStateChanged != null) OnKeyStateChanged(msg, argument);
        }
    }
}
