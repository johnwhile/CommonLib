using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

using Common.Maths;

namespace Common.Tools
{
    /// <summary>
    /// Improve access to bitmap's pixels
    /// </summary>
    public class BitmapLock : IDisposable
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;
        InterpolationMode mode;

        public byte[] pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="mode">Define how i access pixel if you use float parameter in GetPixel(float,float) </param>
        public BitmapLock(Bitmap source , InterpolationMode mode = InterpolationMode.Default)
        {
            this.source = source;
            this.mode = mode;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, pixels, 0, pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                Marshal.Copy(pixels, 0, Iptr, pixels.Length);
                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the interpolated pixels
        /// </summary>
        public Color GetPixel(float x, float y)
        {
            x *= Width - 1;
            y *= Height - 1;

            if (x < Width - 1 && y < Height - 1)
            {
                return GetPixel((int)x, (int)y);
            }
            else
            {
                return GetPixel((int)x, (int)y);
            }

        }


        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        public Color GetPixel(int x, int y)
        {
            if (pixels == null) throw new ArgumentNullException("Pixels m_buffer not created, have you locked it ?");

            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;



            if (i > pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        public Vector4f GetPixelV4(int x, int y)
        {
            if (pixels == null) throw new ArgumentNullException("Pixels m_buffer not created, have you locked it ?");

            Vector4f clr = Vector4f.Zero;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3]; // a
                clr = new Vector4f(r, g, b, a);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                clr = new Vector4f(r, g, b, 255);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = pixels[i];
                clr = clr = new Vector4f(c, c, c, 255);
            }
            return clr;
        }


        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            if (pixels == null) throw new ArgumentNullException("Pixels m_buffer not created, have you locked it ?");

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                pixels[i] = (byte)(255 * color.GetBrightness());
            }
        }
        public void SetPixel(int x, int y, Color4b color)
        {
            if (pixels == null) throw new ArgumentNullException("Pixels m_buffer not created, have you locked it ?");

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                pixels[i] = color.b;
                pixels[i + 1] = color.g;
                pixels[i + 2] = color.r;
                pixels[i + 3] = color.a;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                pixels[i] = color.b;
                pixels[i + 1] = color.g;
                pixels[i + 2] = color.r;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                pixels[i] = color.b;
            }
        }

        private Color linear(Color a, Color b , float fa)
        {
            float fb = 1 - fa;
            int A = (int)(a.A * fa + b.A * fb);
            int R = (int)(a.R * fa + b.R * fb);
            int G = (int)(a.G * fa + b.G * fb);
            int B = (int)(a.B * fa + b.B * fb);
            return Color.FromArgb(A, R, G, B); 
        }

        public void Dispose()
        {
            UnlockBits();
        }

    }
}
