// by johnwhile
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Geometry
{
    public enum Cull
    {
        None,
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// Menage a continue list of vertices as 2d polygon
    /// </summary>
    public class Polygon
    {
        public List<Vector3us> triangles;
        public List<Vector2f> vertices;

        public Polygon()
        {
            vertices = new List<Vector2f>();
            triangles = new List<Vector3us>();
        }

        public Polygon(IEnumerable<Vector2f> point_chain)
        {
            vertices = new List<Vector2f>(point_chain);
            triangles = new List<Vector3us>();
        }

        /// <summary>
        /// Return the orientation of points list. IsConvex is a useful information.
        /// calculate the sum of area
        /// </summary>
        public static Cull GetVerticesOrder(IList<Vector2f> vertices, out bool isPolygonConvex)
        {
            int convexcount = 0;
            int numverts = vertices.Count;
            isPolygonConvex = false;

            if (numverts < 3) return Cull.None;

            float sum = 0;

            for (int i = 0; i < numverts; i++)
            {
                Vector2f p0 = vertices[(i - 1) % numverts];
                Vector2f p1 = vertices[i];
                Vector2f p2 = vertices[(i + 1) % numverts];

                float area = GetTriangleArea(p0, p1, p2);
                sum += area;

                if (area > 0)
                    convexcount++;
                else
                    convexcount--;
            }

            if (convexcount > 0)
                isPolygonConvex = convexcount == numverts;
            else if (convexcount < 0)
                isPolygonConvex = convexcount == -numverts;

            return sum > 0 ? Cull.Clockwise : Cull.CounterClockwise;
        }

        /// <summary>
        /// determines area of triangle formed by three points
        /// </summary>
        public static float GetTriangleArea(Vector2f a, Vector2f b, Vector2f c)
        {
            return (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y)) * 0.5f;
        }
        /// <summary>
        /// returns true if point 'b' is convex
        /// </summary>
        public static bool IsConvex(Vector2f a, Vector2f b, Vector2f c)
        {
            return GetTriangleArea(a, b, c) > 0;
        }
    }
}
