using System;
using System.Drawing;
using System.Windows.Forms;

using Common;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// An element is any drawable object related to the control. It can be an image, a text, etc...<br/>
    /// By default
    /// It's a fixed object that not accepts interactions by user but it depends by its <see cref="GuiControl"/>
    /// </summary>
    public abstract class GuiElement : GuiResource
    {
        /// <summary>
        /// </summary>
        /// <remarks>
        /// <i>For <see cref="GuiElement"/> class I preferred the local destination instead of the global because it's doesn't has children</i>
        /// </remarks>
        Rectangle4i m_localRect;

        /// <summary>
        /// <inheritdoc/><br/>
        /// </summary>
        /// <remarks><inheritdoc cref="m_localRect"/></remarks>
        public override Rectangle4i Destination
        {
            get
            {
                var dest = m_localRect;
                dest.position += Parent.Destination.position;
                return dest;
            }
            internal set
            {
                m_localRect = value;
                m_localRect.position = Parent.Destination.position - m_localRect.position;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Rectangle4i Local
        {
            get => m_localRect;
            internal set => m_localRect = value;
        }

        public GuiElement(GuiControl control, Rectangle4i destination, string name = "GuiElement") : base(control, destination, name)
        {

        }

        public abstract void Draw();

        internal override void DrawDebugName(GraphicsRenderer renderer)
        {
            renderer.ClipRectangle = null;
            renderer.DrawString($"{Name}{instance}\noffset:{m_localRect.position}", Destination.position + 10, Color4b.Black);
        }

        public override void Update()
        {
            if (Dock== GuiEdge.Fill)
            {
                Destination = Parent.Destination;
            }
        }

        public override string ToString() => $"{Name}{instance} ctr:{Parent.Name}";
    }
}
