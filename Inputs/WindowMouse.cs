using System;
using System.Windows.Forms;
using Common.Maths;
using Common.WinNative;

namespace Common.Inputs
{
    /// <summary>
    /// NO WHEEL DATA
    /// </summary>
    public static class WindowMouse
    {
        public static MouseEnum GetCurrentState()
        {
            var Down = MouseEnum.None;
            if (WindowKeysTool.IsKeyPressedAsync(Keys.LButton)) Down |= MouseEnum.Left;
            if (WindowKeysTool.IsKeyPressedAsync(Keys.RButton)) Down |= MouseEnum.Right;
            if (WindowKeysTool.IsKeyPressedAsync(Keys.MButton)) Down |= MouseEnum.Middle;
            if (WindowKeysTool.IsKeyPressedAsync(Keys.XButton1)) Down |= MouseEnum.XButton1;
            if (WindowKeysTool.IsKeyPressedAsync(Keys.XButton2)) Down |= MouseEnum.XButton2;
            return Down;
        }

        /// <summary>
        /// Return the Mouse State, wheel delta and cursor movement are calculate comparing two MouseStates
        /// </summary>
        public static bool GetCurrentState(ref MyMouseState MouseState)
        {
            MouseState.Reset();

            if (WindowKeysTool.IsKeyPressedAsync(Keys.LButton))
            {
                MouseState.DownButtons |= MouseEnum.Left;
                MouseState.Left = KeyState.Down;
            }

            if (WindowKeysTool.IsKeyPressedAsync(Keys.RButton))
            {
                MouseState.DownButtons |= MouseEnum.Right;
                MouseState.Right = KeyState.Down;
            }
            if (WindowKeysTool.IsKeyPressedAsync(Keys.MButton))
            {
                MouseState.DownButtons |= MouseEnum.Middle;
                MouseState.Middle = KeyState.Down;
            }
            if (WindowKeysTool.IsKeyPressedAsync(Keys.XButton1))
            {
                MouseState.DownButtons |= MouseEnum.XButton1;
                MouseState.XButton1 = KeyState.Down;
            }
            if (WindowKeysTool.IsKeyPressedAsync(Keys.XButton2))
            {
                MouseState.DownButtons |= MouseEnum.XButton2;
                MouseState.XButton2 = KeyState.Down;
            }

            if (!WinApi.GetCursorPos(out MouseState.Position)) return false;

            return true;
        }
        /// <summary>
        /// Set mouse position to screen coordinates
        /// </summary>
        /// <returns>return true if success</returns>
        public static bool SetPosition(int x, int y) => WinApi.SetScreenCursorPos(x, y);
        /// <summary>
        /// Get mouse position from screen coordinates
        /// </summary>
        /// <returns>return true if success</returns>
        public static bool GetPosition(out Vector2i pos)=> WinApi.GetCursorPos(out pos);

        /// <summary>
        /// Get mouse position from control coordinates
        /// </summary>
        /// <returns>return true if success</returns>
        /// <param name="window">Relative Control</param>
        public static bool GetPosition(out Vector2i pos, IntPtr window)
        {
            bool success = WinApi.GetCursorPos(out pos);
            success &= WinApi.ScreenToClient(window, ref pos);
            return success;
        }
        public new static string ToString()
        {
            MyMouseState state = new MyMouseState();
            if (GetCurrentState(ref state))
                return state.ToString();
            else 
                return "GetCurrentState ERROR";
        }
    }
}
