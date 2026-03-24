using System;
using System.Collections.Generic;
using System.Text;
using Common.Maths;

namespace Common.Tools
{
    /*

    /// <summary>
    /// Not work for self-intersected polygons
    /// http://cgm.cs.mcgill.ca/~godfried/teaching/cg-projects/97/Ian/applets/BruteForceEarCut.java
    /// </summary>
    public static class PolygonEarCuter
    {
        /// <summary>
        /// Circular linked list is useful and easy to remove node
        /// </summary>
        static CircularLinkedList2<Point> updatedpoints;

        public static List<Vector3us> GetPolygon(IList<Vector2f> perimeter)
        {
            if (perimeter.Count < 3) return null;

            updatedpoints = new CircularLinkedList2<Point>();
            int npoints = 0;
            bool isConvex;

            for (int i = 0; i < perimeter.Count; i++)
            {
                Point node = new Point { value = perimeter[i], ID = npoints++ };
                updatedpoints.AddLast(node);
            }

            if (classify(out isConvex) != Cull.Clockwise)
            {
                //throw new Exception("the area of polygon is outside perimeter, the list of vertices are in opposite direction and no polygons can be created");
                updatedpoints.Invert();
                classify(out isConvex);
            }
            doCutEar();
            return indices;
        }

        static List<Vector3us> indices = new List<Vector3us>();

        /// <summary>
        /// Return the orientation of points list. IsConvex is a usefull information.
        /// The algorithm require a Clockwise order
        /// </summary>
        static Cull classify(out bool isPolygonConvex)
        {
            int convexcount = 0;

            isPolygonConvex = false;

            if (updatedpoints.Count < 3) return Cull.None;

            float sum = 0;

            foreach (Point p in updatedpoints)
            {
                float area = triangleArea(p.Prev, p, p.Next);
                sum += area;

                if (area>0)
                {
                    p.isConvex = true;
                    convexcount++;
                }
                else
                {
                    p.isConvex = false;
                    convexcount--;
                }
            }

            if (convexcount > 0)
            {
                isPolygonConvex = convexcount == updatedpoints.Count;
            }
            else if (convexcount < 0)
            {
                isPolygonConvex = convexcount == -updatedpoints.Count;
            }

            Console.WriteLine(sum);

            return sum > 0 ? Cull.Clockwise : Cull.CounterClockwise;
        }

        /// <summary>
        /// returns true if point 'b' is convex
        /// </summary>
        static bool isConvex(Point a, Point b, Point c)
        {
            return triangleArea(a, b, c) > 0;
        }
        
        /// <summary>
        /// determines area of triangle formed by three points
        /// </summary>
        static float triangleArea(Point a, Point b, Point c)
        {
            float areaSum = 0;

            areaSum += a.value.x * (c.value.y - b.value.y);
            areaSum += b.value.x * (a.value.y - c.value.y);
            areaSum += c.value.x * (b.value.y - a.value.y);

            // for actual area, we need to multiple areaSum * 0.5,
            // but we are only interested in the sign of the area (+/-)
            return areaSum;
        }

        /// <summary>
        /// returns the contains point of triangle formed by three points, if not found return null
        /// </summary>
        static Point triangleContainsPoint(Point a, Point b, Point c)
        {
            float area1, area2, area3;

            foreach (Point p in updatedpoints)
            {
                if (!p.isConvex && p != a && p != b && p != c)
                {
                    area1 = triangleArea(a, b, p);
                    area2 = triangleArea(b, c, p);
                    area3 = triangleArea(c, a, p);

                    if (area1 > 0)
                        if ((area2 > 0) && (area3 > 0))
                            return p;
                    if (area1 < 0)
                        if ((area2 < 0) && (area3 < 0))
                            return p;
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if the point 'b' is an ear
        /// </summary>
        static bool isEar(Point a, Point b, Point c)
        {
            if (!b.isConvex) return false;

            Point inside = triangleContainsPoint(a, b, c);

            if (inside == null) return true;

            Console.WriteLine(string.Format("{0} inside [{1} {2} {3}]", inside.ID, a.ID, b.ID, c.ID));
            return false;
        }

        static bool MoveNext()
        {
            bool findTriangle = false;

            if (updatedpoints.Count > 2)
            {
                Console.WriteLine("head is " + updatedpoints.Head.ID.ToString());

                foreach (Point curr in updatedpoints)
                {
                    Point prev = curr.Prev;
                    Point next = curr.Next;

                    Console.WriteLine(string.Format("check [{0} {1} {2}]", prev.ID, curr.ID, next.ID));

                    if (isEar(prev, curr, next))
                    {
                        Console.WriteLine("is ear");

                        indices.Add(new Face16(prev.ID, curr.ID, next.ID));
                        findTriangle = true;

                        updatedpoints.Remove(curr);

                        // update prev and next classification
                        prev.isConvex = isConvex(prev.Prev, prev, prev.Next);
                        next.isConvex = isConvex(next.Prev, next, next.Next);

                        Console.WriteLine("set head " + next.ID.ToString());

                        updatedpoints.Head = next;
                        break;
                    }
                }
            }

            return findTriangle;
        }

        /// <summary>
        /// Performs all the functions needed to find and cut an ear.
        /// </summary>
        static void doCutEar()
        {
            indices = new List<Face16>();

            while (MoveNext()) ;

            foreach (Point p in updatedpoints)
            {
                Console.WriteLine(string.Format("P{0} convex: {1}", p.ID, p.isConvex));
            }
        }

        /// <summary>
        /// The nested point class
        /// </summary>
        private class Point : ILink<Point>
        {
            public Vector2f value;
            public int ID;
            /// <summary>
            /// Convex point are not used to generate a triangle
            /// </summary>
            public bool isConvex;

            public override string ToString()
            {
                return string.Format("ID: {0} Convex: {1}", ID, isConvex);
            }

            public Point Next { get; set; }
            public Point Prev { get; set; }
            public bool marked2remove { get { return false; } }
        }
    }
    */
}
