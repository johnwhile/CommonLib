using Common.Tools;
using System;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Common
{

    /// <summary>
    /// look the visual studio extension: VSColorOutput64
    /// </summary>
    public enum DebugInfo
    {
        Message = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Success = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Debugg
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static bool to_console = false;

        public static bool ToConsole
        {
            get => to_console;
            set
            {
                to_console = value;

                //check if application is a Console or Windows Application
                if (GetConsoleWindow() == IntPtr.Zero)
                {
                    to_console = false;
                }
            }
        }
        public static bool ToOutput = true;

        static ConsoleColor[] ColorMap = new ConsoleColor[]
        {
            ConsoleColor.White,
            ConsoleColor.Blue,
            ConsoleColor.DarkYellow,
            ConsoleColor.Red
        };

        /// <summary>
        /// regex sintax = ^\s*€:\s
        /// </summary>
        static string[] MessageMap = new string[]
        {
            ">: ",
            "i: ",
            "w: ",
            "e: ",
            "s: "
        };

        static int currentIndent = 0;

        /// <summary>
        /// add some space in the message
        /// </summary>
        public static int Indent
        {
            get => currentIndent;
            set
            {
                int move = value - currentIndent;
                if (move > 0)
                {
                    while (move-- > 0) Debug.Indent();
                }
                else
                {
                    while (move++ < 0) Debug.Unindent();
                }
                currentIndent = value;
            }
        }

        static Debugg()
        {

        }

        public static void AddListener(TraceListener listener)
        {
            Debug.Listeners.Add(listener);
            Debug.AutoFlush = true;
        }


        /// <summary>
        /// Write a message to the DebugOutput. Function disabled in Release mode
        /// </summary>
        /// <param name="info">make a prefix for VSColorOutput64</param>
        [Conditional("DEBUG")]
        private static void Print(object message, DebugInfo info = DebugInfo.Message, int addSpace = 0)
        {
            if (ToConsole)
            {
                ConsoleColor prev = Console.ForegroundColor;
                Console.ForegroundColor = ColorMap[(int)info];
                Console.CursorLeft = (Indent + addSpace) * 2;
                Console.WriteLine(message);
                Console.CursorLeft = 0;
                Console.ForegroundColor = prev;
            }
            if (ToOutput)
            {
                Debug.Write(MessageMap[(int)info]);
                if (addSpace > 0) Indent += addSpace;
                Debug.WriteLine(message);
                if (addSpace > 0) Indent -= addSpace;
            }
        }
        [Conditional("DEBUG")]
        public static void Message(object message, int addSpace = 0) => Print(message, DebugInfo.Message, addSpace);
        [Conditional("DEBUG")]
        public static void Error(object message, int addSpace = 0) => Print(message, DebugInfo.Error, addSpace);
        [Conditional("DEBUG")]
        public static void Warning(object message, int addSpace = 0) => Print(message, DebugInfo.Warning, addSpace);
        [Conditional("DEBUG")]
        public static void Info(object message, int addSpace = 0) => Print(message, DebugInfo.Info, addSpace);
        [Conditional("DEBUG")]
        public static void Success(object message, int addSpace = 0) => Print(message, DebugInfo.Success, addSpace);

    }


    public class TextBoxDebugListener : TraceListener
    {
        private delegate void StringSendDelegate(string message);

        TextBox m_box;
        StringSendDelegate m_invoke_write;

        public TextBoxDebugListener(TextBox box)
        {
            m_box = box;
            m_invoke_write = new StringSendDelegate(SendString);
        }

        public override void Write(string message)
        {
            m_box.Invoke(m_invoke_write, new object[] { message });
        }

        public override void WriteLine(string message)
        {
            m_box.Invoke(m_invoke_write, new object[]{ message + Environment.NewLine });
        }

        private void SendString(string message)
        {
            // No need to lock text box as this function will only 
            // ever be executed from the UI thread
            m_box.AppendText(message);
        }

       
    }
}
