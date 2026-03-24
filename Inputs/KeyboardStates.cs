using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Inputs
{
    /// <summary>
    /// Store here all keys state (keyboard + mouse)
    /// </summary>
    public class MyKeyboardState
    {
        public List<KeyState> AllStates;

        public MyKeyboardState()
        {
            AllStates = new List<KeyState>(256);
            for (int i = 0; i < 255; i++) AllStates.Add(KeyState.Up);
        }

        public bool isKeyDown(vKey k)
        {
            return AllStates[(int)k] == KeyState.Down;
        }

        public bool isKeyUp(vKey k)
        {
            return AllStates[(int)k] == KeyState.Up;
        }

        public KeyState this[vKey k]
        {
            get { return AllStates[(int)k]; }
        }

        public override string ToString()
        {
            string pressed = "PRESSED : ";
            for (int i = 0; i < 255; i++) if (AllStates[i] == KeyState.Down) pressed += string.Format("{0} ", (vKey)i);
            pressed+= "\r\nHOLD : ";
            for (int i = 0; i < 255; i++) if (AllStates[i] == KeyState.Hold) pressed += string.Format("{0} ", (vKey)i);
            
            return pressed;
        }

    }
}
