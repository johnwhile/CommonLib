using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Maths;

namespace Common.Gui.SystemGraphic
{
    /// <summary>
    /// A sample textbox rendered by <see cref="System.Drawing.Graphics"/> renderer.
    /// The <see cref="renderer"/> value must be assigned
    /// </summary>
    public class SysGuiText : GuiEditText
    {
        GraphicsRenderer renderer;

        public SysGuiText(GuiContainer parent, GraphicsRenderer renderer, string initialText = "") : base(parent, initialText)
        {
            this.renderer = renderer;
        }

        public override void InitDefaultComponents()
        {

        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            if (IsVisible && Elements != null && Elements.Count > 1)
            {
                if (IsFocused)
                {
                    Elements[0].Draw(renderer, debug);
                }
                else
                {
                    Elements[1].Draw(renderer, debug);
                }
            }
            if (debug) DrawDebugName(renderer);
        }
    }
}
