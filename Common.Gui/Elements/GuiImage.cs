


using Common.Gui.SystemGraphic;
using Common.Maths;

using System.Drawing;
using System.Drawing.Imaging;

namespace Common.Gui
{
    /// <summary>
    /// It's a simple image
    /// </summary>
    public class GuiImage : GuiElement
    {
        Image image;
        Rectangle4i source;
        ImageAttributes attributes;

        /// <summary>
        /// </summary>
        /// <param name="source">the portion of image to draw</param>
        /// <param name="rectangle">local rectangle</param>
        public GuiImage(GuiControl control, Image image, Rectangle source, Rectangle4i rectangle, string name = "GuiImage") : base(control, rectangle, name)
        {
            this.image = image;
            this.source = source;
        }

        public static ImageAttributes GetRemapColorAttribute(Color4b oldcolor, Color4b newcolor)
        {
            var attribute = new ImageAttributes();
            ColorMap colormap = new ColorMap()
            {
                OldColor = oldcolor,
                NewColor = newcolor
            };
            attribute.SetRemapTable(new ColorMap[] { colormap }, ColorAdjustType.Bitmap);
            return attribute;
        }

        public ImageAttributes ImageAttributes
        {
            get => attributes;
            set => attributes = value;
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            var destination = Destination;

            if (IsVisible)
            {
                renderer.ClipRectangle = UseClipping ? ClipParent.m_cliprectangle : Rectangle4i.Null;
                var dest = Destination;
                renderer.DrawImage(image, source, dest, attributes);
            }

            //if (debug) DrawDebugName(renderer);
        }



        public override void Draw()
        {

        }
    }
}
