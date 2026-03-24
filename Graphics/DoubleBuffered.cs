using System;
using System.Drawing;


namespace Common.Windows
{
    /// <summary>
    /// Deprecated for OptimizedBuffer of Net2.0
    /// </summary>
    public class DoubleBuffered : IDisposable
    {
        public Graphics BufferGraphics;
        internal Bitmap BackBuffer;

        public DoubleBuffered(Size size)
            : this(size.Width, size.Height)
        {
        }
        public DoubleBuffered(int Width,int Height)
        {
            BackBuffer = new Bitmap(Width, Height);
            BufferGraphics = Graphics.FromImage(BackBuffer);
        }

        public bool Resize(Size size)
        {
            return Resize(size.Width, size.Height);
        }
        public bool Resize(int Width, int Height)
        {
            if (Width * Height == 0)
            {
                Width = Height = 1;
            }

            if (Width != BackBuffer.Width || Height != BackBuffer.Height)
            {
                BackBuffer = new Bitmap(Width, Height);
                BufferGraphics = Graphics.FromImage(BackBuffer);
                return true;
            }
            return false;
        }

        public void Render(Graphics g)
        {
            if (BackBuffer != null)
            {
                g.DrawImageUnscaled(BackBuffer, 0, 0);
            }
        }
        
        public void Dispose()
        {
            if (BackBuffer != null)
            {
                BackBuffer.Dispose();
                BackBuffer = null;
            }

            if (BufferGraphics != null)
            {
                BufferGraphics.Dispose();
                BufferGraphics = null;
            }
        }
    }
}
