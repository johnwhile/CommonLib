using System;
using System.Drawing;

using Common.Maths;

namespace Common.Gui
{
    public class ImageAtlasInfo
    {
        public Image image;   
        public Rectangle4i source;

        public Rectangle4i[] srcSplitted;
        public Rectangle4i[] dstSplitted;

        public void SplitSource(int srcBorder, int dstBorder, Rectangle4i destination )
        {
            srcSplitted = new Rectangle4i[9];
            dstSplitted = new Rectangle4i[9];
            GuiUtils.NineSplit(source, srcBorder, srcSplitted);
            GuiUtils.NineSplit(destination, dstBorder, dstSplitted);
        }
    }
}
