using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Inputs
{
    /// <summary>
    /// Contain all necessary informations about mouse state
    /// </summary>
    public struct MyMouseState
    {
        /// <summary>
        /// from DirectInput
        /// </summary>
        public const int MAXBUTTONCOUNT = 8;

        /// <summary>
        /// Wheel value
        /// </summary>
        public int Delta;
        /// <summary>
        /// Screen Absolute position
        /// </summary>
        public Vector2i Position;
        /// <summary>
        /// Movement from last state, using hook is equal to pixels
        /// </summary>
        public Vector2i Move;
        /// <summary>
        /// fast access for all buttons currently down
        /// </summary>
        public MouseEnum DownButtons;

        /// <summary>
        /// specified state of all mouse's buttons
        /// </summary>
        public KeyState Left, Right, Middle, XButton1, XButton2;


        public static MyMouseState Empty
        {
            get
            {
                MyMouseState state = new MyMouseState();
                state.Reset();
                return state;
            }
        }

        public void Reset()
        {
            DownButtons = MouseEnum.None;
            Left = Right = Middle = XButton1 = XButton2 = KeyState.Up;
            Delta = 0;
            Position = 0;
            Move = 0;
        }


        public override string ToString()
        {
            return $"pos:{Position} move:{Move} delta:{Delta} D_{DownButtons} L_{Left} R_{Right} M_{Middle} X1_{XButton1} X2_{XButton2}";
        }
    }
}
