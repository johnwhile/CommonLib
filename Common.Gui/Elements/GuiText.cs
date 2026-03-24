


using Common.Gui.SystemGraphic;
using Common.Maths;

using System.Drawing;
using System.Drawing.Imaging;

namespace Common.Gui
{
    /// <summary>
    /// </summary>
    public class GuiText : GuiElement
    {
        public Font Font;
        public string Text;
        public Color4b Color;
        public GuiRectangle Background;

        /// <summary>
        /// </summary>
        /// <param name="source">the portion of image to draw</param>
        /// <param name="rectangle">local rectangle</param>
        public GuiText(GuiControl control, string text, Font font, Color4b color, Rectangle4i rectangle, string name = "GuiText") : base(control, rectangle, name)
        {
            Font = font;
            Text = text;
            Color = color;
            Background = new GuiRectangle(control, rectangle, "GuiTextBackground")
            {
                Border = Color4b.Black,
                Background = Color4b.White,
                Thickness = 0,
                Radius = 2
            };
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            var destination = Destination;

            if (IsVisible)
            {
                renderer.ClipRectangle = UseClipping ? ClipParent.m_cliprectangle : Rectangle4i.Null;
                var dest = Destination;
                dest.size = renderer.MeasureString(Font, Text);
                Background.Destination = dest;
                Background.Draw(renderer, debug);
                renderer.DrawString(Text, dest, Color, Font);
            }

            //if (debug) DrawDebugName(renderer);
        }



        public override void Draw()
        {

        }
    }
}
