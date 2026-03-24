using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Maths
{
    public static class Utilities2d
    {
        /// <summary>
        /// Get triangle area
        /// </summary>
        public static float TriangleArea(in Vector2f a, in Vector2f b, in Vector2f c) =>
            Mathelp.ABS((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) * .5f);

        public static float TriangleArea(in Vector2f a, Vector2f b, Vector2f c)
        {
            b.x -= a.x;
            b.y -= a.y;
            c.x -= a.x;
            c.y -= a.y;
            return Mathelp.ABS(Vector2f.Cross(in b, in c));
        }

        /// <summary>
        /// Is point inside triangle
        /// </summary>
        public static bool IsPointInsideTriangle(in Vector2f p, in Vector2f a, in Vector2f b, in Vector2f c)
        {
            /* Calculate area of triangle ABC */
            var A = TriangleArea(in a, in b, in c);
            /* Calculate area of triangle PBC */
            var A1 = TriangleArea(in p, in b, in c);
            /* Calculate area of triangle PAC */
            var A2 = TriangleArea(in a, in p, in c);
            /* Calculate area of triangle PAB */
            var A3 = TriangleArea(in a, in b, in p);
            /* Check if sum of A1, A2 and A3 is same as A */
            return (A == A1 + A2 + A3);
        }

        /// <summary>
        /// in screen coordinate where (0,0) is top-left.<br/>
        /// <b>TriangleList</b>
        /// <code>
        /// 0-------1
        /// |       |
        /// 3-------2
        /// </code>
        /// </summary>
        public static void Create(out ushort[] indices, out Vector2f[] texcoord)
        {
            indices = new ushort[]
            {
                0,2,1,
                0,3,2
            };
            texcoord = new Vector2f[]
            {
                new Vector2f(0,0),
                new Vector2f(1,0),
                new Vector2f(1,1),
                new Vector2f(0,1)
            };
        }
        /// <summary>
        /// in screen coordinate where (0,0) is top-left.<br/>
        /// <b>TriangleList</b>
        /// vertices are simply texcoord*radius
        /// center are in (0.5,0.5)
        /// </summary>
        public static void Create(int numofpoints, out ushort[] indices, out Vector2f[] texcoord)
        {
            if (numofpoints < 3) throw new ArgumentException("requires at least three points");
            texcoord = new Vector2f[numofpoints];
            indices = new ushort[3 * (numofpoints - 2)];

            for (int i = 1; i <= numofpoints - 2; i++)
            {
                indices[(i - 1) * 3 + 0] = 0;
                indices[(i - 1) * 3 + 1] = (ushort)(i + 1);
                indices[(i - 1) * 3 + 2] = (ushort)i;
            }

            for (int i = 0; i < numofpoints; i++)
            {
                double rad = System.Math.PI * 2 / numofpoints * i;
                texcoord[i] = new Vector2f((System.Math.Cos(rad) + 1) * 0.5, (System.Math.Sin(rad) + 1) * 0.5);
            }
        }

    }
}
