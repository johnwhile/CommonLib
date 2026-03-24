
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Common.WinNative;
using Common.Maths;

namespace Common.Inputs
{
    /// <summary>
    /// This class monitors all mouse message from application messages queue, work only local to client area.
    /// </summary>
    public class MouseMsg : WinMessagesFilter, IMouse
    {
        MouseFilter filter;

        public MouseMsg(Form app)
            : base(app)
        {
            filter = new MouseFilter(this);
        }

        protected override IMessageFilter MyFilter
        {
            get { return filter; }
        }

        /// <summary>
        /// Current MouseState
        /// </summary>
        public MyMouseState GetMouseState
        {
            get { return filter.current; }
        }

        public override bool Update()
        {
            return true;
        }

        public event MyMouseEventHandler OnMouseStateChanged;
        public event MyMouseEventHandler OnMouseDoubleClick;
        public event MyMouseEventHandler OnMouseDown;
        public event MyMouseEventHandler OnMouseUp;
        public event MyMouseEventHandler OnMouseMove;
        public event MyMouseEventHandler OnMouseWheel;


        /// <summary>
        /// The Filter implementation
        /// </summary>
        class MouseFilter : IMessageFilter
        {
            public MouseMsg owner;
            public MyMouseState previous;
            public MyMouseState current;

            MyMouseArg[] mouseArgs = new MyMouseArg[]{
                new MyMouseArg(MouseKey.Left),
                new MyMouseArg(MouseKey.Right),
                new MyMouseArg(MouseKey.Middle),
                new MyMouseArg(MouseKey.XBtn1),
                new MyMouseArg(MouseKey.XBtn2)};

            MyMouseArg mouseArgsEmpty = new MyMouseArg();

            System.Drawing.Rectangle screen = Screen.PrimaryScreen.Bounds;

            public MouseFilter(MouseMsg owner)
            {
                this.owner = owner;          
                current = new MyMouseState();
            }



            public bool PreFilterMessage(ref Message m)
            {
                WM msg = (WM)m.Msg;

                previous = current;
                current.Move = 0;
                current.Delta = 0;

                switch (msg)
                {
                    #region Left
                    case WM.LBUTTONDOWN:
                        current.Left = KeyState.Down;
                        current.DownButtons |= MouseEnum.Left;
                        if (owner.OnMouseDown != null) { mouseArgs[0].state = current; owner.OnMouseDown(msg, mouseArgs[0]); }
                        break;

                    case WM.LBUTTONDBLCLK:
                        current.Left = KeyState.DoubleClick;
                        current.DownButtons |= MouseEnum.Left;
                        if (owner.OnMouseDoubleClick != null) { mouseArgs[0].state = current; owner.OnMouseDoubleClick(msg, mouseArgs[0]); }
                        break;

                    case WM.LBUTTONUP:
                        current.Left = KeyState.Up;
                        current.DownButtons &= MouseEnum.Not_Left;
                        if (owner.OnMouseUp != null) { mouseArgs[0].state = current; owner.OnMouseUp(msg, mouseArgs[0]); }
                        break;
                    #endregion

                    #region Right
                    case WM.RBUTTONDOWN:
                        current.Right = KeyState.Down;
                        current.DownButtons |= MouseEnum.Right;
                        if (owner.OnMouseDown != null) { mouseArgs[1].state = current; owner.OnMouseDown(msg, mouseArgs[1]); }
                        break;

                    case WM.RBUTTONDBLCLK:
                        current.Right = KeyState.DoubleClick;
                        current.DownButtons |= MouseEnum.Right;
                        if (owner.OnMouseDoubleClick != null) { mouseArgs[1].state = current; owner.OnMouseDoubleClick(msg, mouseArgs[1]); }
                        break;

                    case WM.RBUTTONUP:
                        current.Right = KeyState.Up;
                        current.DownButtons &= MouseEnum.Not_Right;
                        if (owner.OnMouseUp != null) { mouseArgs[1].state = current; owner.OnMouseUp(msg, mouseArgs[1]); }
                        break;
                    #endregion

                    #region Middle
                    case WM.MBUTTONDOWN:
                        current.Middle = KeyState.Down;
                        current.DownButtons |= MouseEnum.Middle;
                        if (owner.OnMouseDown != null) { mouseArgs[2].state = current; owner.OnMouseDown(msg, mouseArgs[2]); }
                        break;

                    case WM.MBUTTONDBLCLK:
                        current.Middle = KeyState.DoubleClick;
                        current.DownButtons |= MouseEnum.Middle;
                        if (owner.OnMouseDoubleClick != null) { mouseArgs[2].state = current; owner.OnMouseDoubleClick(msg, mouseArgs[2]); }
                        break;

                    case WM.MBUTTONUP:
                        current.Middle = KeyState.Up;
                        current.DownButtons &= MouseEnum.Not_Middle;
                        if (owner.OnMouseUp != null) { mouseArgs[2].state = current; owner.OnMouseUp(msg, mouseArgs[2]); }
                        break;

                    #endregion

                    case WM.MOUSEWHEEL:
                        current.Delta = NativeUtils.HIWORD(m.WParam);
                        if (owner.OnMouseWheel != null) { mouseArgs[2].state = current; owner.OnMouseWheel(msg, mouseArgs[2]); }
                        break;

                    case WM.MOUSEMOVE:
                        current.Position.x = NativeUtils.LOWORD(m.LParam);
                        current.Position.y = NativeUtils.HIWORD(m.LParam);
                        current.Move = current.Position - previous.Position;
                        current.Position.x = Maths.Mathelp.CLAMP(current.Position.x, screen.Left, screen.Right);
                        current.Position.y = Maths.Mathelp.CLAMP(current.Position.y, screen.Top, screen.Bottom);

                        if (owner.OnMouseMove != null) { mouseArgsEmpty.state = current; owner.OnMouseMove(msg, mouseArgsEmpty); }
                        break;

                    // exit if isn't a mouse's message
                    default: return false;
                }

                if (owner.OnMouseStateChanged != null) throw new NotImplementedException();

                return false;
            }
        }




    }
}
