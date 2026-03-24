using System;
using Common.Maths;

namespace Common.Gui
{
    [Flags]
    public enum GuiControlFlags : byte
    {
        Enable = 1,
        Visible = 2,
        NeedChildSort = 3,
        Translatable = 4,
        Scalable = 8,
    }
    /*
    public struct GuiControlFlagsByte
    {
        byte flag;

        public bool enable { get => get(1); set => set(value, 1); }
        public bool visible { get => get(1 << 1); set => set(value, 1 << 1); }
        public bool focused { get => get(1 << 2); set => set(value, 1 << 2); }
        public bool needchildsorting { get => get(1 << 3); set => set(value, 1 << 3); }
        public bool translatable { get => get(1 << 4); set => set(value, 1 << 4); }
        public bool scalable { get => get(1 << 5); set => set(value, 1 << 5); }
        public bool focusparent { get => get(1 << 6); set => set(value, 1 << 6); }


        bool get(byte bit) => (flag & bit) > 0;
        void set(bool value, byte bit) => flag = (byte)(value ? flag | bit : flag & ~bit);
    }
    */


    [Flags]
    enum GuiUpdate : byte
    {
        None = 0,
        /// <summary>
        /// tells the manager that require to re-assign the depth value
        /// </summary>
        Depth = 1,
        /// <summary>
        /// tells the manager that require to update clip rectangle
        /// </summary>
        Clip = 2,

        All = Clip | Depth
    }

    /// <summary>
    /// defines the action to do during mouse movement
    /// </summary>
    enum GuiMovement : byte
    {
        nothing = 0,
        translation = 1,
        scaling = 2
    }
    [Flags]
    public enum GuiEdge : byte
    {
        Outside = 0,
        Inside = 1,
        Left = 2,
        Right = 4,
        Top = 8,
        Bottom = 16,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        Fill = Left | Right | Top | Bottom,
    }


    [Flags]
    public enum GuiState
    {
        Disabled = 0,
        Focused = 1,
        Pressed = 2,
        MouseOver = 4,
        Checked = 8
    }


    public static class GuiUtils
    {
        public static void NineSplit(Rectangle4i rect, int border, Rectangle4i[] splitted)
        {
            int b = border;
            int w2b = rect.width - 2 * border;
            int h2b = rect.height - 2 * border;

            int ax = rect.x;
            int bx = rect.x + b;
            int cx = rect.x + rect.width - b;

            int ay = rect.y;
            int by = rect.y + b;
            int cy = rect.y + rect.height - b;

            //top left corner
            splitted[0] = new Rectangle4i(ax, ay, b, b);
            //top border
            splitted[1] = new Rectangle4i(bx, ay, w2b, b);
            //top right corner
            splitted[2] = new Rectangle4i(cx, ay, b, b);
            //right border
            splitted[3] = new Rectangle4i(cx, by, b, h2b);
            //bottom right corner
            splitted[4] = new Rectangle4i(cx, cy, b, b);
            //bottom border
            splitted[5] = new Rectangle4i(bx, cy, w2b, b);
            //bottom left corner
            splitted[6] = new Rectangle4i(ax, cy, b, b);
            //left border
            splitted[7] = new Rectangle4i(ax, by, b, h2b);
            //middle
            splitted[8] = new Rectangle4i(bx, by, w2b, h2b);
        }
    }

}
