using System;
using Common.Maths;

namespace Common.Gui.MyStyle
{
    public class MyGuiShapePanel9
    {
        Rectangle4i[] sub_sources;
        Rectangle4i[] sub_destination;

        public byte ImageBackground;

        public MyGuiShapePanel9(byte imageBackground) : base()
        {
            ImageBackground = imageBackground;
            sub_destination = new Rectangle4i[9];
            sub_sources = new Rectangle4i[9];
        }

        public void Draw(GuiRenderer renderer, Rectangle4i destination)
        {
            //source image not found or not defined, return the default drawer
            if (ImageBackground > 0 && renderer.Layout.TryGetSource(ImageBackground, out var source, out var index, out var _))
            {
                //divide source and destination in 9 parts for a correct scaling shape for fixed corner size
                GuiUtils.NineSplit(destination, 20, sub_destination);
                GuiUtils.NineSplit(source, 12, sub_sources);
                for (int i = 0; i < 9; i++)
                    renderer.DrawImage(sub_destination[i], sub_sources[i], index);
            }
        }
    }

}
