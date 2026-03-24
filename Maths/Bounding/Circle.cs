using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Maths
{
    /// <summary>
    /// Bounding Circle
    /// </summary>
    public struct Circle
    {
        public Vector2f center;
        public float radius;

        public Circle(Vector2f Center, float Radius)
        {
            center = Center;
            radius = Radius;
        }

        public Circle(float cx, float cy, float Radius)
        {
            center = new Vector2f(cx, cy);
            radius = Radius;
        }

        public static Circle Empty=> new Circle(Vector2f.Zero, -1);
        public bool IsEmpty => radius < 0;
        public float Area => Mathelp.PI * radius * radius;

        public static Circle FromPoints(in Vector2f p1, in Vector2f p2, in Vector2f p3)
        {
            Vector2f p12 = p1 - p2;
            Vector2f p13 = p1 - p3;
            Vector2f p23 = p2 - p3;

            float pow1 = p1.x * p1.x + p1.y * p1.y;
            float pow2 = p2.x * p2.x + p2.y * p2.y;
            float pow3 = p3.x * p3.x + p3.y * p3.y;

            float cross32 = Vector2f.Cross(in p3, in p2);
            //float cross13 = Vector2f.Cross(in p1, in p3);
            //float cross21 = Vector2f.Cross(in p2, in p1);

            float a = Vector2f.Cross(in p1, in p23) - cross32;
            float b = -pow1 * p23.y + pow2 * p13.y - pow3 * p12.y;
            float c = pow1 * p23.x - pow2 * p13.x + pow3 * p12.x;
            //float d = pow1 * cross32 + pow2 * cross13 + pow3 * cross21;

            float x = -b / (2 * a);
            float y = -c / (2 * a);
            float r = (float)Math.Sqrt((x - p1.x) * (x - p1.x) + (y - p1.y) * (y - p1.y));

            return new Circle(x, y, r);
        }

        public bool Contains(in Vector2f p) => (p - center).LengthSq <= radius * radius;
            

        /// <summary>
        /// Convert a 2d rectangle to a screen rectangle where X axis is same and Y inverted
        /// </summary>
        /// <param name="Client">the screen bound in pixel size</param>
        /// <param name="World">the screen bound in 2d coordinates</param>
        public Rectangle4i ConvertToScreen(Rectangle4i Client, AABRminmax World)
        {
            Vector2f min = center - radius;
            Vector2f max = center + radius;

            int x0 = (int)Mathelp.Interpolate(min.x, Client.Left, Client.Right, World.min.x, World.max.x);
            int x1 = (int)Mathelp.Interpolate(max.x, Client.Left, Client.Right, World.min.x, World.max.x);
            int y0 = (int)Mathelp.Interpolate(min.y, Client.Bottom, Client.Top, World.min.y, World.max.y);
            int y1 = (int)Mathelp.Interpolate(max.y, Client.Bottom, Client.Top, World.min.y, World.max.y);

            if (x0 > x1) Mathelp.SWAP(ref x0, ref x1);
            if (y0 > y1) Mathelp.SWAP(ref y0, ref y1);

            return Rectangle4i.FromPoints(x0, y0, x1, y1);
        }

        unsafe public static implicit operator Vector3f(Circle c) => *(Vector3f*)&c;
        unsafe public static implicit operator Circle(Vector3f v) => *(Circle*)&v;


        public override string ToString()
        {
            if (IsEmpty) return "NULL_SPHERE";
            return string.Format("Center : {0,4} {1,4} Radius : {2,4}\n", center.x, center.y, radius);
        }
    }
}
