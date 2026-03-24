using System.Drawing;
using System.Drawing.Imaging;

using Common.Maths;

namespace Common.Gui.SystemGraphic
{
    public class SysGuiImage
    {
        public Image texture;
        public ImageAttributes attributes;
        public Rectangle4i source;

        public SysGuiImage(Image texture, Rectangle4i source)
        {
            this.texture = texture;
            this.source = source;
            attributes = null;
        }
        public void SetRemapColor(Color4b oldcolor, Color4b newcolor)
        {
            attributes = new ImageAttributes();
            ColorMap colormap = new ColorMap()
            {
                OldColor = oldcolor,
                NewColor = newcolor
            };
            attributes.SetRemapTable(new ColorMap[] { colormap }, ColorAdjustType.Bitmap);
        }

        public void Draw(GraphicsRenderer renderer, Rectangle4i destination)
        {
            renderer.DrawImage(texture, source, destination, attributes);
        }
    }
}
