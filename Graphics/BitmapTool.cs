using Common.Maths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tools
{
    public struct Vector<T> where T : unmanaged
    {
        public T x;
        public T y;
    }



    public class BitmapGraphics : IDisposable
    {
        Bitmap bmp;
        Graphics gfx;


        private BitmapGraphics(Bitmap image)
        { 
            bmp = image;
            gfx = Graphics.FromImage(bmp);


            var v = new Vector<float>();

        }



        public static BitmapGraphics Create(Bitmap image)
        {
            BitmapGraphics graphics = new BitmapGraphics(image);

            return graphics;
        }


        public void Draw(Bitmap source, Matrix3x3f transform)
        {





        }



        public void Dispose()
        {
            gfx?.Dispose();
            bmp = null;
        }
    }
}
