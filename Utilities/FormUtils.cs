using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

using Common.Maths;
using Common.Inputs;
using Common.WinNative;

using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Common.Windows
{
    public class WinProcArgs : EventArgs
    {
        string info;

        public WinProcArgs(string info)
        {
            this.info = info;
        }

        public static new WinProcArgs Empty
        {
            get { return new WinProcArgs(string.Empty); }
        }

        public override string ToString()
        {
            return info;
        }
    }


    public static class FormUtils
    {
        static NativeMessage msg;

        /// <summary>
        /// From Book:
        /// "IsAppStillIdle uses a new PeekMessage function to check if the application has any important events that need
        /// to be dealt with. If there are important messages for the application then these need to be handled before 
        /// returning to the game loop."
        /// </summary>
        public static bool IsAppStillIdle => !WinApi.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        

        /// <summary>
        /// http://stackoverflow.com/questions/336817/how-can-i-detect-whether-a-user-control-is-running-in-the-ide-in-debug-mode-or
        /// </summary>
        public static bool IsInDesignMode(IComponent component)
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            /*
            bool ret = false;
            if (null != component)
            {
                ISite site = component.Site;
                if (null != site)
                {
                    ret = site.DesignMode;
                }
                else if (component is System.Windows.Forms.Control)
                {
                    IComponent parent = ((System.Windows.Forms.Control)component).Parent;
                    ret = IsInDesignMode(parent);
                }
            }
            return ret;
            */
        }

        /// <summary>
        /// Simple conversion from long flag to my byte flag
        /// </summary>
        public static MouseEnum Convert(MouseButtons winButton)
        {
            //    Windows's MouseButton definition
            //    MouseButtons.Left     = 1048576 =      0001 0000 0000 0000 0000 0000
            //    MouseButtons.Right    = 2097152 =      0010 0000 0000 0000 0000 0000
            //    MouseButtons.Middle   = 4194304 =      0100 0000 0000 0000 0000 0000
            //    MouseButtons.XButton1 = 8388608 =      1000 0000 0000 0000 0000 0000
            //    MouseButtons.XButton2 = 16777216= 0001 0000 0000 0000 0000 0000 0000
            return (MouseEnum)((long)winButton >> 20);
        }


    }

    /// <summary>
    /// Utilities for System.Drawing
    /// </summary>
    public static class FormDrawingUtils
    {
        /// <summary>
        /// Transforms your [-1,1] directx coordinate point to screen coordinate, exactly how directx convert
        /// the rasterized coordinate to screen, openGL use [0,1] range
        /// </summary>
        /// <param name="point">The point in "bound" coordinate</param>
        /// <remarks>
        /// <para>[-1, 1]-----------[1, 1] </para>
        /// <para>  |                 |   </para>
        /// <para>[-1,-1]-----------[1,-1]</para>
        /// </remarks>
        public static void TransformDirectxCoordToScreen(Vector2f vector, Rectangle screen, ref Point coord)
        {
            coord.X = (int)(screen.X + (vector.x + 1) / 2 * screen.Width);
            coord.Y = (int)(screen.Y + (1 - vector.y) / 2 * screen.Height);
        }


        /// <summary>
        /// Transforms your coordinate point to screen coordinate.
        /// </summary>
        /// <param name="point">The point in "bound" coordinate</param>
        /// <param name="screen">The screen area.</param>
        /// <param name="bound">The bound rectangle that fill the screen area, it use Min and Max so best performance for RectangleAA</param>
        /// <remarks>
        /// Rectangle use TopLeft origin, bound use BottomLeft origin
        /// </remarks>
        /// <returns> non additional operation if point exit from screen</returns>
        public static void TransformCoordinateToScreen(Vector2f vector, Rectangle screen, IRectangleAA bound, ref Point coord)
        {
            Vector2f min = bound.Min;
            Vector2f max = bound.Max;
            coord.X = (int)(screen.X + (vector.x - min.x) / (max.x - min.x) * screen.Width);
            coord.Y = (int)(screen.Y + (max.y - vector.y) / (max.y - min.y) * screen.Height);
        }
        /// <summary>
        /// Transforms the screen coordinate to your coordinate system
        /// </summary>
        /// <param name="point">The point in "screen" coordinate</param>
        /// <param name="screen">The screen area.</param>
        /// <param name="bound">The bound rectangle that fill the screen area, it use Min and Max so best performance for RectangleAA</param>
        /// <remarks>
        /// Rectangle use TopLeft origin, bound use BottomLeft origin
        /// </remarks>
        /// <returns> non additional operation if point exit from screen</returns>
        public static void TransformScreenToCoordinate(Point point, Rectangle screen, IRectangleAA bound , ref Vector2f coord)
        {
            Vector2f min = bound.Min;
            Vector2f max = bound.Max;
            coord.x = min.x + (point.X - screen.X) * (max.x - min.x) / screen.Width;
            coord.y = max.y + (screen.Y - point.Y) * (max.y - min.y) / screen.Height;
        }

        /// <summary>
        /// Build the useful 2D transformation matrix to draw with Graphics.
        /// For semplicity is not rotated and not sheared.
        /// http://source.winehq.org/source/dlls/gdiplus/matrix.c
        /// </summary>
        /// <param name="MinCoord">min corner of your coordinate system</param>
        /// <param name="MaxCoord">max corner of your coordinate system</param>
        /// <param name="GraphicRect">Graphic rectangle where your coordinate fill</param>
        /// <param name="CoordToScreen">The result passed as reference to improve performace, is a TRS matrix so Scale-Rotation-Traslation order</param>
        /// <param name="ScaleMatrix">The invers of CoordToScreen scale part, is necessary to draw correctly text or line thickness</param>
        public static void MakeScreenTransform(ref Vector2f MinCoord, ref Vector2f MaxCoord, ref Rectangle GraphicRect,
            Matrix CoordToScreen, 
            Matrix ScaleMatrix)
        {
            // the custom graph coordinate bound, where Y will be inverted to match with windows coordinate system
            float CoordX = MinCoord.x;
            float CoordY = MaxCoord.y;
            float Width = MaxCoord.x - MinCoord.x;
            float Height = MinCoord.y - MaxCoord.y;

            // the three screen coorners, notice that matrix will be not Sheared because is a rectangle
            float TopLeftX = GraphicRect.X;
            float TopLeftY = GraphicRect.Y;

            float TopRightX = GraphicRect.X + GraphicRect.Width;
            float TopRightY = GraphicRect.Y;

            float BottomLeftX = GraphicRect.X;
            float BottomLeftY = GraphicRect.Y + GraphicRect.Height;

            // the exatly math used to build NET matrix
            float m11 = (TopRightX - TopLeftX) / Width;
            float m21 = (BottomLeftX - TopLeftX) / Height;
            float dx = TopLeftX - m11 * CoordX - m21 * CoordY;
            float m12 = (TopRightY - TopLeftY) / Width;
            float m22 = (BottomLeftY - TopLeftY) / Height;
            float dy = TopLeftY - m12 * CoordX - m22 * CoordY;

            // force use of Append order because i'm not sure are the default parameter.
            CoordToScreen.Reset();
            CoordToScreen.Shear(m12, m21, MatrixOrder.Append);
            CoordToScreen.Scale(m11, m22, MatrixOrder.Append);
            CoordToScreen.Translate(dx, dy, MatrixOrder.Append);

            ScaleMatrix.Reset();
            ScaleMatrix.Scale(1.0f / m11, 1.0f/ m22, MatrixOrder.Append);
        }

        /// <summary>
        /// Matrix x Vector multiplication
        /// </summary>
        public static void TransformPoint(Matrix matrix, float x, float y, out float X, out float Y)
        {
            X = x * matrix.Elements[0] + y * matrix.Elements[2] + matrix.Elements[4];
            Y = x * matrix.Elements[1] + y * matrix.Elements[3] + matrix.Elements[5];
        }

        /// <summary>
        /// Draw a string inside a rectangle, try the best fit
        /// </summary>
        public static void DrawStringInRectangle(Graphics g, Font font, Brush fontbrush, string text, RectangleF bound)
        {
            SizeF textsize = g.MeasureString(text, font);
            float width = Maths.Mathelp.MIN(bound.Width, textsize.Width);
            
            // avoid empty string
            if (Maths.Mathelp.isZero(width)) return;
            
            int numlines = (int)System.Math.Ceiling(textsize.Width / width);
            
            float height = Maths.Mathelp.MIN(bound.Height, textsize.Height * numlines);

            float centerx = bound.X + bound.Width / 2;
            float centery = bound.Y + bound.Height / 2;

            bound.X = centerx - width / 2.0f;
            bound.Y = centery - height / 2.0f;
            bound.Width = width;
            bound.Height = height;

            //g.DrawString(text, font, fontbrush, (bound.Width - textsize.Width) / 2, (bound.Height - textsize.Height) / 2);
            g.DrawString(text, font, fontbrush, bound);
        }
    }
}
