using System;
using System.Collections.Generic;
using System.Text;

using Common.Tools;

namespace Common.Inputs
{
    public class MyMouseArg : EventArgs
    {
        /// <summary>
        /// </summary>
        public MyMouseState state;
        /// <summary>
        /// The key that generate event
        /// </summary>
        public readonly MouseKey key;

        public MyMouseArg(MouseKey key = MouseKey.None)
        {
            this.key = MouseKey.None;
            state = MyMouseState.Empty;
        }

        public override string ToString()
        {
            return $"{key} {state}";
        }
    }

    public class MyKeyboardArg : EventArgs
    {
        public MyKeyboardState state;
        public vKey key;

        public MyKeyboardArg(vKey key = vKey.None)
        {
            this.key = key;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", key, state[key]);
        }
    }


    /// <summary>
    /// </summary>
    /// <param name="extra">usualy Window Message (WM)</param>
    /// <param name="key">the key that generate event</param>
    public delegate void MyKeyEventHandler(object extra, MyKeyboardArg mouseArg);

    /// <summary>
    /// </summary>
    /// <param name="extra">usualy Window Message (WM)</param>
    /// <param name="button">the button that generate event</param>
    public delegate void MyMouseEventHandler(object extra, MyMouseArg mouseArg);
}
