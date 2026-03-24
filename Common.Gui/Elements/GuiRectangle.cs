


using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// It's a simple rectangle
    /// </summary>
    public class GuiRectangle : GuiElement
    {
        public Color4b Background;
        public Color4b Border;
        /// <summary>
        /// Thickness of border in pixels, set zero for no border
        /// </summary>
        public int Thickness;
        /// <summary>
        /// Corner radius in pixels, set zero for squared corner
        /// </summary>
        public int Radius;

        public GuiRectangle(GuiRectangle copyfrom) : this(copyfrom.Parent, copyfrom.Local, copyfrom.Name)
        {
            Background = copyfrom.Background;
            Border = copyfrom.Border;
            Thickness = copyfrom.Thickness;
            Radius = copyfrom.Radius;
        }
        /// <summary>
        /// </summary>
        /// <param name="rectangle">local rectangle</param>
        public GuiRectangle(GuiControl control, Rectangle4i rectangle, string name = "GuiRectangle") : base(control, rectangle, name)
        {
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            var destination = Destination;

            if (IsVisible)
            {
                if (UseClipping) renderer.ClipRectangle = ClipParent.m_cliprectangle;
                else renderer.ClipRectangle = null;
                var dest = Destination;
                renderer.FillRectangle(dest, Background, Radius);
                renderer.DrawRectangle(dest, Border, Thickness, Radius);
            }

            //if (debug) DrawDebugName(renderer);
        }



        public override void Draw()
        {

        }
    }
}
