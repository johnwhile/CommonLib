using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.ComponentModel;

using Common.WinNative;
using System.Windows.Forms;
using Common.Tools;

namespace Common.Inputs
{
    /// <summary>
    /// </summary>
    public static class WindowKeysTool
    {
        /// <summary>
        /// More significant bit for current pressed state
        /// </summary>
        const byte CURRMARK = 0x80; // 1000 0000
        /// <summary>
        /// Less significant bit for previous pressed state
        /// </summary>
        const byte PREVMASK = 0x01; // 0000 0001
        /// <summary>
        /// KeyBuffer
        /// </summary>
        static byte[] windowsKeys = new byte[256];

        static int pressedCount;

        /// <summary>
        /// Get current "true" keys states
        /// </summary>
        public static bool UpdateAsyncKeyState()
        {
            pressedCount = 0;
            for (int i = 0; i < 256; i++)
            {
                windowsKeys[i] = (byte)(WinApi.GetAsyncKeyState(i)>>15);
                if (pressed(i)) pressedCount++;
            }
            return true;
        }
        /// <summary>
        /// Get keys states, next state only available after end of message queue, tipically for each
        /// GameLoop Frame. This is correct for me because doesn't exit other cases.
        /// </summary>
        public static bool UpdateCurrentKeyState()
        {
            pressedCount = 0;
            //Get pressed keys
            if (!WinApi.GetKeyboardState(windowsKeys))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            for (int i = 0; i < 256; i++)
            {
                if (pressed(i)) pressedCount++;
            }
            return true;
        }

        /// <summary>
        /// All keys that contain CTRL, ALT , SHIFT
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsModifierKey(Keys key)
        {
            return (key & Keys.Modifiers) != 0;
        }
        /// <summary>
        /// All Windows keys (keyboard + mouse)
        /// </summary>
        public static bool IsKeyPressed(vKey key)
        {
            return pressed((int)key);
        }

        /// <summary>
        /// Return key state directly reading current state, not require to call UpdateCurrentState()
        /// </summary>
        public static bool IsKeyPressedAsync(Keys key)
        {
            byte keystate = (byte)(WinApi.GetAsyncKeyState((int)key) >> 15);
            return (keystate & CURRMARK) != 0;
        }

        /// <summary>
        /// All Windows keys (keyboard + mouse)
        /// </summary>
        public static bool IsMouseKey(vKey key)
        {
            //None = 0
            //LButton = 1
            //RButton = 2
            //Cancel = 3
            //MButton = 4
            //XButton1 = 5
            //XButton2 = 6
            return key > vKey.None && key <= vKey.XButton2 && key != vKey.Cancel;
        }

        /// <summary>
        /// All Windows keys (keyboard + mouse)
        /// </summary>
        public static bool IsKeyPressed(Keys key)
        {
            if (IsModifierKey(key)) throw new NotSupportedException("can't use Shift, Control and Alt key with other key, example ALT+A");
            
            int i = (int)key & 0xFF;
            return pressed(i);
        }

        /// <summary>
        /// Windows keys contain also mouse buttons as key
        /// </summary>
        public static bool IsKeyPressed(MouseEnum key)
        {
            switch (key)
            {
                case MouseEnum.Left: return pressed((int)Keys.LButton);
                case MouseEnum.Right: return pressed((int)Keys.RButton);
                case MouseEnum.Middle: return pressed((int)Keys.MButton);
                case MouseEnum.XButton1: return pressed((int)Keys.XButton1);
                case MouseEnum.XButton2: return pressed((int)Keys.XButton2);
                default: throw new ArgumentException("Key value not valid");
            }
        }

        /// <summary>
        /// Number of keyboard's keys pressed
        /// </summary>
        public static int KeyPressedCount
        {
            get { return pressedCount; }
        }

        /// <summary>
        /// Number of mouse's keys pressed
        /// </summary>
        public static int MousePressedCount
        {
            get { return pressedCount; }
        }


        /// <summary>
        /// Key.Unknow or Key.None doesn't exit or reserved for unknown uses, so return always false 
        /// </summary>
        internal static bool pressed(int key)
        {
            return key > 0 && (windowsKeys[key] & CURRMARK) != 0;
        }


        public static new string ToString()
        {
            StringBuilder builder = new StringBuilder();
           
            for (int i = 0; i < 256; i++)
            {
                if (pressed(i)) builder.Append(string.Format("{0} ", (vKey)i)) ;
            }
            return builder.ToString();
        }
    }
}
