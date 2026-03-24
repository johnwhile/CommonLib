using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Common.Tools;
using System.IO;

namespace Common.Maths
{
    /// <summary>
    /// Axis Aligned Bounding Rectangle (both version)
    /// </summary>
    /// <remarks>
    /// <code>
    /// 2-----3
    /// |     |
    /// 0-----1
    /// </code>
    /// </remarks>
    public interface IRectangleAA
    {
        Vector2f Max { get; }
        Vector2f Min { get; }
        Vector2f Center { get; }
        Vector2f Size { get; }
        Vector2f HalfSize { get; }
    }

    /// <summary>
    /// Axis Aligned Bounding Rectangle (Min-Max version)
    /// </summary>
    public struct AABRminmax : IRectangleAA
    {
        public Vector2f max, min;

        public AABRminmax(Vector2f Min, Vector2f Max)
        {
            max = Max;
            min = Min;
            safetyCheck();
        }

        public AABRminmax(float minx, float miny, float maxx, float maxy) :
            this(new Vector2f(minx, miny), new Vector2f(maxx, maxy))
        { }

        public AABRminmax(Segment2D Seg)
            : this(Seg.orig, Seg.orig + Seg.dir * Seg.length)
        { }

        public AABRminmax(BinaryReader reader)
        {
            min = new Vector2f(reader);
            max = new Vector2f(reader);
        }

        public void Write(BinaryWriter writer)
        {
            min.Write(writer);
            max.Write(writer);
        }

        public Vector2f Max { get { return max; } }
        public Vector2f Min { get { return min; } }
        public Vector2f Center { get { return (max + min) * 0.5f; } }
        public Vector2f HalfSize { get { return (max - min) * 0.5f; } }
        public Vector2f Size { get { return max - min; } }
        public Vector2f Corner0 { get { return min; } }
        public Vector2f Corner1 { get { return new Vector2f(max.x, min.y); } }
        public Vector2f Corner2 { get { return new Vector2f(min.x, max.y); } }
        public Vector2f Corner3 { get { return max; } }

        public float Sizex { get { return max.x - min.x; } }
        public float Sizey { get { return max.y - min.y; } }


        /// <summary>
        /// static constructor to avoid same parameters of struct contructor
        /// </summary>
        public static AABRminmax FromHalfSize(Vector2f center, Vector2f halfsize)
        {
            return new AABRminmax(center - halfsize, center + halfsize);
        }
        /// <summary>
        /// </summary>
        public static AABRminmax FromOriginSize(Vector2f min, Vector2f size)
        {
            return new AABRminmax(min.x, min.y, min.x + size.x, min.y + size.y);
        }

        void safetyCheck()
        {
            if (max.x < min.x) Mathelp.SWAP(ref max.x, ref min.x);
            if (max.y < min.y) Mathelp.SWAP(ref max.y, ref min.y);
        }

        /// <summary>
        /// Return a rectangle with invalid area (negative inf).
        /// When you merge if with a second valid rectangle, the sum will be always equal to valid rectangle
        /// </summary>
        public static AABRminmax Empty
        {
            get
            {
                AABRminmax empty = new AABRminmax();
                // without safety check
                empty.max = new Vector2f(float.NegativeInfinity, float.NegativeInfinity);
                empty.min = new Vector2f(float.PositiveInfinity, float.PositiveInfinity);
                return empty;
            }
        }
        /// <summary>
        /// Return the default rectangle with bound (0,0) (1,1)
        /// </summary>
        public static AABRminmax UnitXY
        {
            get { return new AABRminmax(0, 0, 1, 1); }
        }
        /// <summary>
        /// Return the default rectangle with bound (0,0)
        /// </summary>
        public static AABRminmax Zero
        {
            get { return new AABRminmax(0, 0, 0, 0); }
        }

        /// <summary>
        /// set to zero if negative, negative inf, NaN.
        /// Positive inf is allowed
        /// </summary>
        public float Area
        {
            get 
            {
                float area = (max.x - min.x) * (max.y - min.y);
                return (float.IsNaN(area) || float.IsNegativeInfinity(area)) ? 0 : area;
            }
        }
        /// <summary>
        /// Return if rectangle is not valid
        /// </summary>
        public bool IsEmpty
        {
            get { return max.x <= min.x || max.y <= min.y || Area <= 0.0f; }
        }

        public void SetEmpty()
        {
            min.x = min.y = float.PositiveInfinity;
            max.x = max.y = float.NegativeInfinity;
        }

        public bool isPointInside(Vector2f p)
        {
            return isPointInside(p.x, p.y);
        }

        public bool isPointInside(float x, float y)
        {
            return x <= max.x &&
                   y <= max.y &&
                   x >= min.x &&
                   y >= min.y;
        }

        /// <summary>
        /// Update rectangle to include this point 
        /// </summary>
        public void MergeX(float x)
        {
            if (x > max.x) max.x = x;
            if (x < min.x) min.x = x;
        }
        /// <summary>
        /// Update rectangle to include this point
        /// </summary>
        public void MergeY(float y)
        {
            if (y > max.y) max.y = y;
            if (y < min.y) min.y = y;
        }
        /// <summary>
        /// Update rectangle to include this point, more efficent than AABBox2 
        /// </summary>
        public void Merge(float x, float y)
        {
            MergeX(x);
            MergeY(y);
        }

        public void Merge(ref Vector2f point)
        {
            if (point.x > max.x) max.x = point.x;
            if (point.y > max.y) max.y = point.y;
            if (point.x < min.x) min.x = point.x;
            if (point.y < min.y) min.y = point.y;
        }

        /// <summary>
        /// Update rectangle to include this box
        /// </summary>
        public void Merge(IRectangleAA rect)
        {
            // if a == "NaN" will not be added because a.max.X are always < than b.max.X
            // if a and b == "NaN" the result is a Empty box
            max.x = Mathelp.MAX(max.x, rect.Max.x);
            max.y = Mathelp.MAX(max.y, rect.Max.y);
            min.x = Mathelp.MIN(min.x, rect.Min.x);
            min.y = Mathelp.MIN(min.y, rect.Min.y);
        }
        /// <summary>
        /// Convert a 2d rectangle to a screen rectangle where X axis is same and Y inverted
        /// </summary>
        /// <param name="Client">the screen bound in pixel size</param>
        /// <param name="World">the screen bound in 2d coordinates</param>
        public Rectangle4i ConvertToScreen(Rectangle4i Client, AABRminmax World)
        {
            int x0 = (int)Mathelp.Interpolate(this.min.x, Client.Left, Client.Right, World.min.x, World.max.x);
            int x1 = (int)Mathelp.Interpolate(this.max.x, Client.Left, Client.Right, World.min.x, World.max.x);
            int y0 = (int)Mathelp.Interpolate(this.min.y, Client.Bottom, Client.Top, World.min.y, World.max.y);
            int y1 = (int)Mathelp.Interpolate(this.max.y, Client.Bottom, Client.Top, World.min.y, World.max.y);

            if (x0 > x1) Mathelp.SWAP(ref x0, ref x1);
            if (y0 > y1) Mathelp.SWAP(ref y0, ref y1);

            return Rectangle4i.FromPoints(x0, y0, x1, y1);
        }

        /// <summary>
        /// Return a circle inscribed in this rectangle
        /// </summary>
        public Circle Inscribed
        {
            get
            {
                Vector2f c = Center;
                Vector2f d = max - c;
                return new Circle(c, Mathelp.MIN(d.x, d.y));
            }
        }

        /// <summary>
        /// Return a circle circumscribed of this rectangle
        /// </summary>
        public Circle Circumscribed
        {
            get
            {
                Vector2f c = Center;
                Vector2f d = max - c;
                return new Circle(c, d.Length);
            }
        }
        /// <summary>
        /// implicit conversion example :
        /// <code>
        /// RectangleAA2 box1 = new RectangleAA2();
        /// RectangleAA box2 = box1;
        /// </code>
        /// </summary>
        public static implicit operator AABRminmax(RectangleAA2 rect)
        {
            return AABRminmax.FromHalfSize(rect.center, rect.halfsize);
        }

        public override string ToString()
        {
            if (this.IsEmpty) return "NULL_AABB";
            StringBuilder str = new StringBuilder();
            str.Append(string.Format("Min : {0,4} {1,4} \n", min.x, min.y));
            str.Append(string.Format("Max : {0,4} {1,4} \n", max.x, max.y));
            return str.ToString();
        }
    }

    /// <summary>
    /// Axis Aligned Bounding Rectangle (Center-Size version)
    /// </summary>
    public struct RectangleAA2 : IRectangleAA
    {
        public Vector2f center;
        public Vector2f halfsize;

        /// <summary>
        /// default constructor, notice parameters match with current version
        /// </summary>
        public RectangleAA2(Vector2f Center, Vector2f HalfSize)
        {
            this.center = Center;
            this.halfsize = HalfSize;
        }

        public RectangleAA2(float cx, float cy, float hx, float hy)
            : this(new Vector2f(cx, cy), new Vector2f(hx, hy))
        { }

        public RectangleAA2(Segment2D Seg)
            : this(Seg.orig, Seg.orig + Seg.dir * Seg.length)
        { }

        public Vector2f Max { get { return center + halfsize; ;} }
        public Vector2f Min { get { return center - halfsize; } }
        public Vector2f Center { get { return center; } }
        public Vector2f HalfSize { get { return halfsize; } }
        public Vector2f Size { get { return halfsize * 2.0f; } }
        public Vector2f Corner0 { get { return Min; } }
        public Vector2f Corner1 { get { return new Vector2f(center.x + halfsize.x, center.y - halfsize.y); } }
        public Vector2f Corner2 { get { return new Vector2f(center.x - halfsize.x, center.y + halfsize.y); } }
        public Vector2f Corner3 { get { return Max; } }
        public float Sizex { get { return halfsize.x * 2.0f ; } }
        public float Sizey { get { return halfsize.y * 2.0f; } }

        /// <summary>
        /// different contructor using min max
        /// </summary>
        public static RectangleAA2 FromMinMax(Vector2f Min, Vector2f Max)
        {
            return new RectangleAA2((Max + Min) * 0.5f, (Max - Min) * 0.5f);
        }
        /// <summary>
        /// </summary>
        public static RectangleAA2 FromOriginSize(Vector2f min, Vector2f size)
        {
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            return new RectangleAA2(min.x + hx, min.y + hy, hx, hy);
        }


        /// <summary>
        /// Return a not valid rectangle
        /// </summary>
        public static RectangleAA2 Empty
        {
            get
            {
                RectangleAA2 empty = new RectangleAA2();
                // without safety check
                empty.center = Vector2f.Zero;
                empty.halfsize = new Vector2f(-1, -1);
                return empty;
            }
        }

        /// <summary>
        /// Return if rectangle is valid but with zero area
        /// </summary>
        public bool IsEmpty
        {
            get { return halfsize.x < 0 || halfsize.y < 0; }
        }

        public bool isPointInside(Vector2f p)
        {
            return isPointInside(p.x, p.y);
        }
        public bool isPointInside(float x, float y)
        {
            return x <= center.x + halfsize.x &&
                   y <= center.y + halfsize.y &&
                   x >= center.x - halfsize.x &&
                   y >= center.y - halfsize.y;
        }

        /// <summary>
        /// Update rectangle to include this point
        /// </summary>
        public void Merge(Vector2f point)
        {
            Vector2f max = Max;
            Vector2f min = Min;
            if (point.x > max.x) max.x = point.x;
            if (point.y > max.y) max.y = point.y;
            if (point.x < min.x) min.x = point.x;
            if (point.y < min.y) min.y = point.y;
            center.x = (max.x + min.x) * 0.5f;
            center.y = (max.y + min.y) * 0.5f;
            halfsize.x = (max.x - min.x) * 0.5f;
            halfsize.y = (max.y - min.y) * 0.5f;
        }

        /// <summary>
        /// Update rectangle to include this box
        /// </summary>
        public void Merge(IRectangleAA rect)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// implicit conversion example :
        /// <code>
        /// AABRectangle box1 = new AABRectangle();
        /// AABRectangle2 box2 = box1;
        /// </code>
        /// </summary>
        public static implicit operator RectangleAA2(AABRminmax rect)
        {
            return RectangleAA2.FromMinMax(rect.min, rect.max);
        }

        /// <summary>
        /// Just to remember how split a rectangle using quadtree annotation
        /// </summary>
        public void QuadSplitter(
            out RectangleAA2 child0,
            out RectangleAA2 child1,
            out RectangleAA2 child2,
            out RectangleAA2 child3)
        {
            float hx = halfsize.x * 0.5f;
            float hy = halfsize.y * 0.5f;
            child0 = new RectangleAA2(center.x - hx, center.y - hy, hx, hy);
            child1 = new RectangleAA2(center.x + hx, center.y - hy, hx, hy);
            child2 = new RectangleAA2(center.x - hx, center.y + hy, hx, hy);
            child3 = new RectangleAA2(center.x + hx, center.y + hy, hx, hy);
        }

        public RectangleAA2 QuadChild0
        {
            get
            {
                float hx = halfsize.x * 0.5f;
                float hy = halfsize.y * 0.5f;
                return new RectangleAA2(center.x - hx, center.y - hy, hx, hy);
            }
        }
        public RectangleAA2 QuadChild1
        {
            get
            {
                float hx = halfsize.x * 0.5f;
                float hy = halfsize.y * 0.5f;
                return new RectangleAA2(center.x + hx, center.y - hy, hx, hy);
            }
        }
        public RectangleAA2 QuadChild2
        {
            get
            {
                float hx = halfsize.x * 0.5f;
                float hy = halfsize.y * 0.5f;
                return new RectangleAA2(center.x - hx, center.y + hy, hx, hy);
            }
        }
        public RectangleAA2 QuadChild3
        {
            get
            {
                float hx = halfsize.x * 0.5f;
                float hy = halfsize.y * 0.5f;
                return new RectangleAA2(center.x + hx, center.y + hy, hx, hy);
            }
        }


        public override string ToString()
        {
            if (this.IsEmpty) return "NULL_AABB";
            StringBuilder str = new StringBuilder();
            str.Append(string.Format("Min : {0,4} {1,4} \n", Min.x, Min.y));
            str.Append(string.Format("Max : {0,4} {1,4} \n", Max.x, Max.y));
            return str.ToString();
        }
    }
}
