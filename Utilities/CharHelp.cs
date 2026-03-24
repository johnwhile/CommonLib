using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Common.Tools
{
    public static class CharHelp
    {
        [DllImport("user32.dll")]
        static extern short VkKeyScanEx(char chr, IntPtr dwhkl);

        public static Keys GetVirtualKeyFromKey(char chr)
        {
            return (Keys)(VkKeyScanEx(chr, InputLanguage.CurrentInputLanguage.Handle) & 0xff);
        }


        [DllImport("user32.dll")]
        static extern int ToUnicode(
            uint virtualKeyCode, 
            uint scanCode, 
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer,
            int bufferSize, 
            uint flags);

        static StringBuilder buf = new StringBuilder(256);
        static byte[] keyboardState = new byte[256];

        const byte OxFF = byte.MaxValue;
        const byte OxOO = byte.MinValue;


        static void ToUnicode(Keys keys, bool shift, bool altGr)
        {
            //bool cap = Control.IsKeyLocked(Keys.CapsLock);

            keyboardState[(int)Keys.ShiftKey] = shift ? OxFF : OxOO;
            keyboardState[(int)Keys.ControlKey] = altGr ? OxFF : OxOO;
            keyboardState[(int)Keys.Menu] = altGr ? OxFF : OxOO;
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
        }

        public static string GetUnicodeFromKeys(Keys keys, bool shift, bool altGr)
        {
            ToUnicode(keys, shift, altGr);
            return buf.ToString();
        }
        public static char GetAsciiFromKeys(Keys keys, bool shift, bool altGr)
        {
            ToUnicode(keys, shift, altGr);
            return buf.Length > 0 ? buf[0] : char.MinValue;
        }
        public static char GetAsciiFromKeys(KeyEventArgs arg)
        {
            ToUnicode(arg.KeyCode, arg.Shift, arg.Control);
            return buf.Length > 0 ? buf[0] : char.MinValue;
        }
    }
}
