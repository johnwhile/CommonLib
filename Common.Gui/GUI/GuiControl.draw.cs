using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Common.Gui.SystemGraphic;
using Common.Inputs;
using Common.Maths;

namespace Common.Gui
{
    public abstract partial class GuiControl
    {
        /// <summary>
        /// A list of Drawable elements
        /// </summary>
        public GuiElementManager<GuiElement> Elements_;

        /// <summary>
        /// A list of Drawable elements
        /// </summary>
        public List<GuiElement> Elements;

        /// <summary>
        /// Pre-calculated clip rectangle, using infinite rectangle mean not clipping
        /// </summary>
        /// <remarks>
        /// <i>The cliprectangle can't simply be the parent destination's rectangle, must be cut recursively with <see cref="Rectangle4i.Intersecting(Rectangle4i)"/>"
        /// foreach parents.</i>
        /// </remarks>
        internal Rectangle4i m_cliprectangle = Rectangle4i.Infinite;

        /// <summary>
        /// Default mouse cursor
        /// </summary>
        protected MouseCursor cursor;

        /// <summary>
        /// Optional, initialize custom shape
        /// </summary>
        public abstract void InitDefaultComponents();
        
        
        internal override void DrawDebugName(GraphicsRenderer renderer)
        {
            renderer.ClipRectangle = null;
            renderer.DrawString($"{Name}{instance} focus:{IsFocused} depth:{depth}", m_globalrect.x+2, m_globalrect.y + 2 , Color4b.Black);
        }

        /// <summary>
        /// The cursor type used then mouse is over this control.
        /// The returned value can be different.
        /// </summary>
        public virtual MouseCursor GuiCursor
        {
            get => cursor;
            set => cursor = value;
        }
    }
}
