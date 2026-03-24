
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Common.WinNative;
using Common.Maths;
using System.Drawing;

namespace Common.Inputs
{
    /// <summary>
    /// This class monitors all mouse activities and provides appropriate events.
    /// </summary>
    public class MouseHook : WinHook, IMouse
    {
        internal MyMouseState current = MyMouseState.Empty;
        internal MyMouseState previous = MyMouseState.Empty;
        System.Drawing.Rectangle screen = Screen.PrimaryScreen.Bounds;

        MyMouseArg[] mouseArgs = new MyMouseArg[]{
            new MyMouseArg(MouseKey.Left),
            new MyMouseArg(MouseKey.Right),
            new MyMouseArg(MouseKey.Middle),
            new MyMouseArg(MouseKey.XBtn1),
            new MyMouseArg(MouseKey.XBtn2)};

        MyMouseArg mouseArgsEmpty = new MyMouseArg();


        //public event MyMouseEventHandler OnMouseStateChanged;
        public event MyMouseEventHandler OnMouseDoubleClick;
        public event MyMouseEventHandler OnMouseDown;
        public event MyMouseEventHandler OnMouseUp;
        public event MyMouseEventHandler OnMouseMove;
        public event MyMouseEventHandler OnMouseWheel;


        public MouseHook(HookLevel level = HookLevel.GlobalLowLevel) : base(level)
        {

        }

        /// <summary>
        /// Get current mouse state, will be update by computer
        /// </summary>
        public MyMouseState GetMouseState
        {
            get { return current; }
        }

        /// <summary>
        /// Returns the correct hook id to be used for <see cref="HookNativeMethods.SetWindowsHookEx"/> call.
        /// </summary>
        protected override WH HookId
        {
            get { return isGlobalLowLevel ? WH.MOUSE_LL : WH.MOUSE; }
        }


        /// <summary>
        /// This method processes the data from the hook and initiates event firing for
        /// LowLevelMouseProc input
        /// https://msdn.microsoft.com/en-us/library/ms644986(v=vs.85).aspx
        /// </summary>
        protected override void ProcessLowLevelCallback(IntPtr wParam, IntPtr lParam)
        {
            WinApi.MSLLHOOKSTRUCT mouseHookStruct = (WinApi.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, WinApi.MSLLHOOKSTRUCT.TypeOf);
            process((WM)wParam.ToInt32(), mouseHookStruct.pt, NativeUtils.HIWORD(mouseHookStruct.mouseData));
        }

        /// <summary>
        /// This method processes the data from the hook and initiates event firing for
        /// MessageProc input
        /// https://msdn.microsoft.com/en-us/library/ms644987(v=vs.85).aspx
        /// </summary>
        protected override void ProcessAppLevelCallback(IntPtr wParam, IntPtr lParam)
        {
            WinApi.MOUSEHOOKSTRUCT mouseHookStruct = (WinApi.MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, WinApi.MOUSEHOOKSTRUCT.TypeOf);
            process((WM)wParam.ToInt32(), mouseHookStruct.pt, NativeUtils.HIWORD(wParam));
        }

        /// <summary>
        /// Update mouse state and/or do all events
        /// </summary>
        void process(WM msg, Vector2i Position, int delta)
        {
            // reset these values to avoid a continuous movement until
            // new windows message are called
            current.Move = 0;
            current.Delta = 0;

            // fix position when mouse go out screen, like in DirectInput
            current.Position.x = Mathelp.CLAMP(Position.x, screen.Left, screen.Right);
            current.Position.y = Mathelp.CLAMP(Position.y, screen.Top, screen.Bottom);

            switch (msg)
            {
                #region Left
                case WM.LBUTTONDOWN:
                    current.Left = KeyState.Down;
                    current.DownButtons |= MouseEnum.Left;
                    if (OnMouseDown != null) { mouseArgs[0].state = current; OnMouseDown(msg, mouseArgs[0]); }
                    break;

                case WM.LBUTTONDBLCLK:
                    current.Left = KeyState.DoubleClick;
                    current.DownButtons |= MouseEnum.Left;
                    if (OnMouseDoubleClick != null) { mouseArgs[0].state = current; OnMouseDoubleClick(msg, mouseArgs[0]); }
                    break;

                case WM.LBUTTONUP:
                    current.Left = KeyState.Up;
                    current.DownButtons &= MouseEnum.Not_Left;
                    if (OnMouseUp != null) { mouseArgs[0].state = current; OnMouseUp(msg, mouseArgs[0]); }
                    break;
                #endregion

                #region Right
                case WM.RBUTTONDOWN:
                    current.Right = KeyState.Down;
                    current.DownButtons |= MouseEnum.Right;
                    if (OnMouseDown != null) { mouseArgs[1].state = current; OnMouseDown(msg, mouseArgs[1]); }
                    break;

                case WM.RBUTTONDBLCLK:
                    current.Right = KeyState.DoubleClick;
                    current.DownButtons |= MouseEnum.Right;
                    if (OnMouseDoubleClick != null) { mouseArgs[1].state = current; OnMouseDoubleClick(msg, mouseArgs[1]); }
                    break;

                case WM.RBUTTONUP:
                    current.Right = KeyState.Up;
                    current.DownButtons &= MouseEnum.Not_Right;
                    if (OnMouseUp != null) { mouseArgs[1].state = current; OnMouseUp(msg, mouseArgs[1]); }
                    break;
                #endregion

                #region Middle
                case WM.MBUTTONDOWN:
                    current.Middle = KeyState.Down;
                    current.DownButtons |= MouseEnum.Middle;
                    if (OnMouseDown != null) { mouseArgs[2].state = current; OnMouseDown(msg, mouseArgs[2]); }
                    break;

                case WM.MBUTTONDBLCLK:
                    current.Middle = KeyState.DoubleClick;
                    current.DownButtons |= MouseEnum.Middle;
                    if (OnMouseDoubleClick != null) { mouseArgs[2].state = current; OnMouseDoubleClick(msg, mouseArgs[2]); }
                    break;

                case WM.MBUTTONUP:
                    current.Middle = KeyState.Up;
                    current.DownButtons &= MouseEnum.Not_Middle;
                    if (OnMouseUp != null) { mouseArgs[2].state = current; OnMouseUp(msg, mouseArgs[2]); }
                    break;

                #endregion

                case WM.MOUSEWHEEL:
                    // update delta only when call the event
                    current.Delta = delta;
                    if (OnMouseWheel != null) { mouseArgs[2].state = current; OnMouseWheel(msg, mouseArgs[2]); }
                    break;

                case WM.MOUSEMOVE:
                    // calculate movement with unfixed position to work also when mouse go out screen
                    current.Move = Position - previous.Position;
                    if (OnMouseMove != null) { mouseArgsEmpty.state = current; OnMouseMove(msg, mouseArgsEmpty); }
                    break;

                //default: Console.WriteLine("UNKNOW MESSAGE : " + msg.ToString()); break;
            }


            previous = current;
        }
    }

}
