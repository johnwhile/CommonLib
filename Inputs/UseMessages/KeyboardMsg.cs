
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
    /// This class monitors all keys message from application messages queue, work only local to client area.
    /// </summary>
    public class KeyboardMsg : WinMessagesFilter, IKeyboard
    {
        private KeyboardFilter filter;

        public event MyKeyEventHandler OnKeyStateChanged;
        public event MyKeyEventHandler OnKeyDown;
        public event MyKeyEventHandler OnKeyUp;


        public override bool Update()
        {
            return false;
        }

        public KeyboardMsg(Form app)
            : base(app)
        {
            filter = new KeyboardFilter(this);
        }

        protected override IMessageFilter MyFilter
        {
            get { return filter; }
        }

        /// <summary>
        /// Current KyboardState
        /// </summary>
        public MyKeyboardState GetKyboardState
        {
            get { return filter.current; }
        }

        /// <summary>
        /// event that contain all other
        /// </summary>


        /// <summary>
        /// The Filter implementation
        /// </summary>
        class KeyboardFilter : IMessageFilter
        {
            public KeyboardMsg owner;
            public MyKeyboardState current;

            MyKeyboardArg argument;

            public KeyboardFilter(KeyboardMsg owner)
            {
                this.current = new MyKeyboardState();
                this.owner = owner;

                argument = new MyKeyboardArg();
                argument.state = current;
            }

            public bool PreFilterMessage(ref Message m)
            {
                WM msg = (WM)m.Msg;

                int vk = m.WParam.ToInt32();
                argument.key = (vKey)vk;

                switch (msg)
                {
                    case WM.KEYDOWN:
                        current.AllStates[vk] = KeyState.Down;
                        if (owner.OnKeyDown != null) owner.OnKeyDown(msg,argument);
                        break;

                    case WM.KEYUP:
                        current.AllStates[vk] = KeyState.Up;
                        if (owner.OnKeyUp != null) owner.OnKeyUp(msg, argument);
                        break;

                    // exit if isn't a key's message
                    default: return false;
                }

                if (owner.OnKeyStateChanged != null) owner.OnKeyStateChanged(msg, argument);

                return false;
            }
        }

    }
}
