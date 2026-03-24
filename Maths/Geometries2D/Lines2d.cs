using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Maths
{
    /// <summary>
    /// Infinite line (parametric implicit layout)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ToString()}")]
    public struct Line2d
    {
        public float a, b, c;
        public float Slope => -a / b;

        /// <summary>
        /// </summary>
        public Line2d(float a, float b, float c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        /// <summary>
        /// Line from two points
        /// </summary>
        public static Line2d FromPoints(Vector2f p1, Vector2f p2)
        {
            Line2d line = new Line2d();
            line.a = p1.y - p2.y;
            line.b = p2.x - p1.x;
            line.c = p1.x * p2.y - p2.x * p1.y;
            return line;
        }

        /// <summary>
        /// Perperdicular line with origin in the middle of two point
        /// </summary>
        public static Line2d Bisector(Vector2f p1, Vector2f p2)
        {
            Line2d line = new Line2d();
            line.a = p2.x - p1.x;
            line.b = p2.y - p1.y;
            line.c = -line.a * (p1.x + p2.x) / 2 - line.b * (p1.y + p2.y) / 2;
            return line;
        }

        public override string ToString()
        {
            return string.Format("{0} x {1}{2} y {3}{4} = 0", a, b > 0 ? "+" : "", b, c > 0 ? "+" : "", c);
        }
    }

    /// <summary>
    /// A infinite line
    /// </summary>
    [Obsolete("Use Line2d instead to reduce by one float")]
    public struct Line2D
    {
        internal Vector2f orig, dir;

        public Vector2f Origin
        {
            get { return orig; }
            set { orig = value; }
        }
        public Vector2f Direction
        {
            get { return dir; }
            set
            {
                Debug.Assert(value.Normalize() > 1e-8f, "invalid direction value");
                dir = value;
            }
        }

        /// <summary>
        /// Direction will be normalized for safety. Only in DEBUG mode i made a direction check
        /// </summary>
        public Line2D(Vector2f point, Vector2f dir)
        {
            float length = dir.Normalize();
#if DEBUG
            Debug.Assert(length > 1e-8f, "invalid direction value");
#endif
            float t = Vector2f.Dot(point, dir);

            this.dir = dir;
            orig = point - t * dir;
        }

        /// <summary>
        /// Return a ray using segment value, direction is oriented from start to end
        /// </summary>
        public static Line2D FromStartEnd(Vector2f start, Vector2f end)
        {
            return new Line2D(start, end - start);
        }

        /// <summary>
        /// A ray can be generalized to line but not viceversa
        /// </summary>
        public static implicit operator Line2D(Ray2D ray)
        {
            return new Line2D(ray.orig, ray.dir);
        }
        /// <summary>
        /// A segment can be generalized to line but not viceversa
        /// </summary>
        public static implicit operator Line2D(Segment2D seg)
        {
            return new Line2D(seg.orig, seg.dir);
        }

        public bool IsNaN
        {
            get { return orig.IsNaN || dir.IsNaN; }
        }

        public static readonly Line2D NaN = new Line2D { orig = Vector2f.NaN, dir = Vector2f.NaN };
    }

    /// <summary>
    /// A ray 
    /// </summary>
    public struct Ray2D
    {
        internal Vector2f orig, dir;

        public Vector2f Origin
        {
            get { return orig; }
            set { orig = value; }
        }

        /// <summary>
        /// When set and u are in DEBUG mode, i check if direction isn't collapsed
        /// </summary>
        public Vector2f Direction
        {
            get { return dir; }
            set
            {
#if DEBUG
                Debug.Assert(value.Normalize() > 1e-8f, "invalid direction value");
#endif
                dir = value;
            }
        }

        /// <summary>
        /// Direction will be normalized for safety
        /// </summary>
        public Ray2D(Vector2f Origin, Vector2f Direction)
        {
            orig = Origin;
            dir = Direction;
            Debug.Assert(dir.Normalize() > 1e-6f, "invalid direction value");
        }

        /// <summary>
        /// Return a ray using segment value, direction is oriented from start to end
        /// </summary>
        public static Ray2D FromStartEnd(Vector2f start, Vector2f end)
        {
            return new Ray2D(start, end - start);
        }


        public Vector2f this[float t] { get { return orig + dir * t; } }


        public eAxis IsParallel
        {
            get
            {
                // work only if dir is normalized
                if (dir.x > 0.999999 || dir.x < -0.999999) return eAxis.X;
                else if (dir.y > 0.999999 || dir.y < -0.999999) return eAxis.Y;
                else return eAxis.None;
            }
        }

        public bool IsNaN
        {
            get { return orig.IsNaN || dir.IsNaN; }
        }
        public static readonly Ray2D NaN = new Ray2D { orig = Vector2f.NaN, dir = Vector2f.NaN };
    }

    /// <summary>
    /// A segment, instead store only 2 point, i store origin direction and length because is more usefull
    /// </summary>
    public struct Segment2D
    {
        public Vector2f orig, dir;
        public float length;
        /// <summary>
        /// Direction will be normalized for safety
        /// </summary>
        public Segment2D(Vector2f P0, Vector2f P1)
        {
            orig = P0;
            dir = P1 - P0;
            length = dir.Normalize();
            Debug.Assert(length > 1e-6f, "invalid direction value");
        }

        public bool IsNaN
        {
            get { return dir.IsNaN; }
        }

        public static readonly Segment2D NaN = new Segment2D { dir = Vector2f.NaN };
    }
}
