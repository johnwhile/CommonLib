using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using Common;
using Common.Maths;

namespace Common.Gui
{
    public abstract class GuiRenderer : Disposable
    {
        public GuiManager Manager { get; internal set; }
        /// <summary>
        /// Manager one or more texture atlas
        /// </summary>
        public ImageAtlasLayout Layout
        {
            get; private set;
        }

        public GuiRenderer(ImageAtlasLayout layout)
        {
            Layout = layout;
        }

        public bool UseClipRegion = false;

        public abstract Rectangle4i ClipRegion { get;  set; }

        /// <summary>
        /// Draw a debug rectangle with 1pixel border
        /// </summary>
        public abstract void DrawRectangle(Rectangle4i destination, Vector4b bordercolor, int thickness = 1);
        /// <summary>
        /// Fill a debug rectangle
        /// </summary>
        public abstract void FillRectangle(Rectangle4i destination, Vector4b fillcolor);

        public abstract void DrawImage(Rectangle4i destination, int layoutimage);
        public abstract void DrawImage(Rectangle4i destination, Rectangle4i source, int imageindex);
        public abstract void DrawString(string text, int x, int y, Vector4b color);
        public abstract Vector2i GetCharSize(char c);
        /// <summary>
        /// return size in pixel of string
        /// </summary>
        public abstract Vector2i MeasureString(string text);
    }
}
