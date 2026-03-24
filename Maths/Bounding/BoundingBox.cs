using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Common.Tools;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.Maths
{
    /// <summary>
    /// Axis Aligned Bounding Box
    /// </summary>
    /// <remarks>
    /// <code>
    ///    6-----7
    ///   /     /|
    ///  2-----3 5
    ///  |     | /
    ///  0-----1
    /// </code>
    /// </remarks>
    public interface IAABBox
    {
        Vector3f Max { get; }
        Vector3f Min { get; }
        Vector3f Center { get; }
        Vector3f HalfSize { get; }
        Vector3f Size { get; }
        
    }

    /// <summary>
    /// Axis Aligned Bounding Box (Min-Max version)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxMinMax : IAABBox
    {
        public Vector3f min;
        public Vector3f max;

        public BoundingBoxMinMax(Vector3f Min, Vector3f Max)
        {
            max = Max;
            min = Min;
            minmaxfix();
        }
        public BoundingBoxMinMax(float minx, float miny, float minz, float maxx, float maxy, float maxz)
        {
            min = new Vector3f(minx, miny, minz);
            max = new Vector3f(maxx, maxy, maxz);
        }

        public BoundingBoxMinMax(BinaryReader reader)
        {
            min = new Vector3f(reader);
            max = new Vector3f(reader);
        }

        public void Write(BinaryWriter writer)
        {
            min.Write(writer);
            max.Write(writer);
        }

        /// <summary>
        /// Max - Min
        /// </summary>
        public Vector3f Size => max - min;

        public Vector3f Max
        {
            get => max;
            set => max = value;
        }
        public Vector3f Min
        {
            get => min;
            set => min = value;
        }

        public Vector3f Center
        {
            get { return (max + min) * 0.5f; }
            set
            {
                Vector3f halfsize = (max - min) * 0.5f;
                min = value - halfsize;
                max = value + halfsize;
            }
        }
        public Vector3f HalfSize { get { return (max - min) * 0.5f; } }
        public Vector3f Corner0 { get { return new Vector3f(min.x, min.y, min.z); } }
        public Vector3f Corner1 { get { return new Vector3f(min.x, min.y, max.z); } }
        public Vector3f Corner2 { get { return new Vector3f(min.x, max.y, min.z); } }
        public Vector3f Corner3 { get { return new Vector3f(min.x, max.y, max.z); } }
        public Vector3f Corner4 { get { return new Vector3f(max.x, min.y, min.z); } }
        public Vector3f Corner5 { get { return new Vector3f(max.x, min.y, max.z); } }
        public Vector3f Corner6 { get { return new Vector3f(max.x, max.y, min.z); } }
        public Vector3f Corner7 { get { return new Vector3f(max.x, max.y, max.z); } }


        public static BoundingBoxMinMax FromHalfSize(Vector3f center, Vector3f halfsize)
        {
            return new BoundingBoxMinMax(center - halfsize, center + halfsize);
        }

        /// <summary>
        /// Only traslation and scale component, rotation are not allowed
        /// </summary>
        public BoundingBoxMinMax(Matrix4x4f transform)
        {
            Vector3f scale = transform.GetScale();
            Vector3f position = transform.Position;

            max = new Vector3f(position.x + scale.x / 2.0f, position.y + scale.y / 2.0f, position.z + scale.z / 2.0f);
            min = new Vector3f(position.x - scale.x / 2.0f, position.y - scale.y / 2.0f, position.z - scale.z / 2.0f);

            minmaxfix();
        }

        void minmaxfix()
        {
            if (max.x < min.x) Mathelp.SWAP(ref max.x, ref min.x);
            if (max.y < min.y) Mathelp.SWAP(ref max.y, ref min.y);
            if (max.z < min.z) Mathelp.SWAP(ref max.z, ref min.z);
        }

        public bool isPointInside(Vector3f p, float epsilon = float.Epsilon)
        {
            return isPointInside(p.x, p.y, p.z, epsilon);
        }

        public static bool IsPointInside(Vector3f min, Vector3f max, Vector3f point, float epsilon = float.Epsilon)
        {
            return point.x - epsilon <= max.x &&
                   point.y - epsilon <= max.y &&
                   point.z - epsilon <= max.z &&
                   point.x + epsilon >= min.x &&
                   point.y + epsilon >= min.y &&
                   point.z + epsilon >= min.z;
        }

        public bool isPointInside(float x, float y, float z, float epsilon = float.Epsilon)
        {
            return x - epsilon <= max.x &&
                   y - epsilon <= max.y &&
                   z - epsilon <= max.z &&
                   x + epsilon >= min.x &&
                   y + epsilon >= min.y &&
                   z + epsilon >= min.z;
        }

        /// <summary>
        /// Return an empty box, where max = -INF , min = +INF so when merging or adding
        /// a box or a point the result is always the added element
        /// </summary>
        public static BoundingBoxMinMax NaN
        {
            get
            {
                BoundingBoxMinMax empty = new BoundingBoxMinMax();
                // without safety check
                empty.max = new Vector3f(float.MinValue, float.MinValue, float.MinValue);
                empty.min = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);
                return empty;
            }
        }
        /// <summary>
        /// zero or negative size, or undefined floats
        /// </summary>
        public bool isNaN
        {
            get
            {
                return
                    max.x < min.x ||
                    max.y < min.y ||
                    max.z < min.z ||
                    max.IsNaN ||
                    min.IsNaN;
            }
        }
        /// <summary>
        /// Get the Axis aligned boundary from a vertices array
        /// </summary>
        public static BoundingBoxMinMax FromData(IList<Vector3f> Points)
        {
            BoundingBoxMinMax aabb = NaN;
            int count = Points.Count;
            if (Points == null || count == 0) return aabb;
            for (int i = 0; i < count; i++) aabb.Merge(Points[i]);
            return aabb;
        }

        /// <summary>
        /// Get the Axis aligned boundary from a sphere
        /// </summary>
        public static BoundingBoxMinMax FromSphere(Sphere bsphere)
        {
            BoundingBoxMinMax aabb = NaN;
            aabb.max.x = bsphere.center.x + bsphere.radius;
            aabb.max.y = bsphere.center.y + bsphere.radius;
            aabb.max.z = bsphere.center.z + bsphere.radius;
            aabb.min.x = bsphere.center.x - bsphere.radius;
            aabb.min.y = bsphere.center.y - bsphere.radius;
            aabb.min.z = bsphere.center.z - bsphere.radius;
            aabb.minmaxfix();
            return aabb;
        }

        /// <summary>
        /// Update box to include this point, more efficent than AABBox2 
        /// </summary>
        public void Merge(Vector3f point)
        {
            if (point.x > max.x) max.x = point.x;
            if (point.y > max.y) max.y = point.y;
            if (point.z > max.z) max.z = point.z;
            if (point.x < min.x) min.x = point.x;
            if (point.y < min.y) min.y = point.y;
            if (point.z < min.z) min.z = point.z;
        }

        /// <summary>
        /// Update box to include this box
        /// </summary>
        public void Merge(IAABBox box)
        {
            // if a == "NaN" will not be added because a.max.X are always < than b.max.X
            // if a and b == "NaN" the result is a Empty box
            max.x = Math.Max(max.x, box.Max.x);
            max.y = Math.Max(max.y, box.Max.y);
            max.z = Math.Max(max.z, box.Max.z);

            min.x = Math.Min(min.x, box.Min.x);
            min.y = Math.Min(min.y, box.Min.y);
            min.z = Math.Min(min.z, box.Min.z);
        }

        /// <summary>
        /// Merge two Bounding Box, 
        /// </summary>
        public static BoundingBoxMinMax operator +(BoundingBoxMinMax a, BoundingBoxMinMax b)
        {
            // if a == "NaN" will not be added because a.max.X are always < than b.max.X
            // if a and b == "NaN" the result is a Empty box
            BoundingBoxMinMax sum = a;
            sum.Merge(b);
            return sum;
        }

        /// <summary>
        /// implicit conversion example :
        /// <code>
        /// AABBox box1 = new AABBox();
        /// AABBox2 box2 = box1;
        /// </code>
        /// </summary>
        public static implicit operator BoundingBoxMinMax(BoundingBoxCenter box)
        {
            return FromHalfSize(box.center, box.extend);
        }


        public override string ToString()
        {
            if (isNaN) return "NaN";
            return $"Max {max} , Min {min}";
        }
    }

    /// <summary>
    /// Axis Aligned Bounding Box (centered version)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxCenter : IAABBox
    {
        public Vector3f center;
        public Vector3f extend;

        public BoundingBoxCenter(Vector3f center, Vector3f extend)
        {
            this.center = center;
            this.extend = extend;
        }
        public BoundingBoxCenter(float cx, float cy, float cz, float hx, float hy, float hz)
        {
            center = new Vector3f(cx, cy, cz);
            extend = new Vector3f(hx, hy, hz);
        }

        public BoundingBoxCenter(BinaryReader reader)
        {
            center = new Vector3f(reader);
            extend = new Vector3f(reader);
        }

        public void Write(BinaryWriter writer)
        {
            center.Write(writer);
            extend.Write(writer);
        }

        public Vector3f Size => extend * 2;

        public Vector3f Max { get { return center + extend; } }
        public Vector3f Min { get { return center - extend; } }
        public Vector3f Center
        {
            get { return center; }
            set { center = value; }
        }
        public Vector3f HalfSize
        {
            get { return extend; }
            set { extend = value; }
        }

        public Vector3f Corner0 { get { return new Vector3f(center.x - extend.x, center.y - extend.y, center.z - extend.z); } }
        public Vector3f Corner1 { get { return new Vector3f(center.x - extend.x, center.y - extend.y, center.z + extend.z); } }
        public Vector3f Corner2 { get { return new Vector3f(center.x - extend.x, center.y + extend.y, center.z - extend.z); } }
        public Vector3f Corner3 { get { return new Vector3f(center.x - extend.x, center.y + extend.y, center.z + extend.z); } }
        public Vector3f Corner4 { get { return new Vector3f(center.x + extend.x, center.y - extend.y, center.z - extend.z); } }
        public Vector3f Corner5 { get { return new Vector3f(center.x + extend.x, center.y - extend.y, center.z + extend.z); } }
        public Vector3f Corner6 { get { return new Vector3f(center.x + extend.x, center.y + extend.y, center.z - extend.z); } }
        public Vector3f Corner7 { get { return new Vector3f(center.x + extend.x, center.y + extend.y, center.z + extend.z); } }

        public static BoundingBoxCenter FromMinMax(Vector3f Min, Vector3f Max)
        {
            return new BoundingBoxCenter((Max + Min) * 0.5f, (Max - Min) * 0.5f);
        }

        /// <summary>
        /// Only traslation and scale component, rotation are not allowed
        /// </summary>
        public BoundingBoxCenter(Matrix4x4f transform)
        {
            Vector3f scale = transform.GetScale();
            Vector3f position = transform.Position;

            this.center = position;
            this.extend = scale * 0.5f;
        }

        public bool isPointInside(Vector3f p)
        {
            return isPointInside(p.x, p.y, p.z);
        }
        public bool isPointInside(float x, float y, float z)
        {
            return x <= center.x + extend.x &&
                   y <= center.y + extend.y &&
                   z <= center.z + extend.z &&
                   x >= center.x - extend.x &&
                   y >= center.y - extend.y &&
                   z >= center.z - extend.z;
        }

        /// <summary>
        /// Update box to include this point, less efficent that AABBox
        /// </summary>
        public void Merge(Vector3f point)
        {
            Vector3f max = Max;
            Vector3f min = Min;

            if (point.x > max.x) max.x = point.x;
            if (point.y > max.y) max.y = point.y;
            if (point.z > max.z) max.z = point.z;
            if (point.x < min.x) min.x = point.x;
            if (point.y < min.y) min.y = point.y;
            if (point.z < min.z) min.z = point.z;

            center = (max + min) * 0.5f;
            extend = (max - min) * 0.5f;
        }

        /// <summary>
        /// Update box to include this box
        /// </summary>
        public void Merge(IAABBox box)
        {
            // precompute to avoid continuos get{} function
            Vector3f amax = Max;
            Vector3f amin = Min;
            Vector3f bmax = box.Max;
            Vector3f bmin = box.Min;

            // if a == "NaN" will not be added because a.max.X are always < than b.max.X
            // if a and b == "NaN" the result is a Empty box
            amax.x = System.Math.Max(amax.x, bmax.x);
            amax.y = System.Math.Max(amax.y, bmax.y);
            amax.z = System.Math.Max(amax.z, bmax.z);

            amin.x = System.Math.Min(amin.x, bmin.x);
            amin.y = System.Math.Min(amin.y, bmin.y);
            amin.z = System.Math.Min(amin.z, bmin.z);

            center = (amax + amin) * 0.5f;
            extend = (amax - amin) * 0.5f;
        }


        public static BoundingBoxCenter NaN
        {
            get
            {
                BoundingBoxCenter empty = new BoundingBoxCenter();
                // without safety check
                empty.extend = new Vector3f(float.MinValue, float.MinValue, float.MinValue);
                empty.center = Vector3f.Zero;
                return empty;
            }
        }
        public bool isNaN
        {
            get
            {
                return
                    extend.x < 0 ||
                    extend.y < 0 ||
                    extend.z < 0 ||
                    extend.IsNaN ||
                    center.IsNaN;
            }
        }
        /// <summary>
        /// Get the Axis aligned boundary from a vertices array
        /// </summary>
        public static BoundingBoxCenter FromData(IList<Vector3f> Points)
        {
            BoundingBoxCenter aabb = BoundingBoxCenter.NaN;
            int count = Points.Count;

            if (Points == null || count == 0)
                return aabb;

            return (BoundingBoxCenter)(BoundingBoxMinMax.FromData(Points));
        }

        /// <summary>
        /// Get the Axis aligned boundary from a sphere
        /// </summary>
        public static BoundingBoxCenter FromSphere(Sphere bsphere)
        {
            return new BoundingBoxCenter(bsphere.center, new Vector3f(bsphere.radius, bsphere.radius, bsphere.radius));
        }

        /// <summary>
        /// Merge two Bounding Box, 
        /// </summary>
        public static BoundingBoxCenter operator +(BoundingBoxCenter a, BoundingBoxCenter b)
        {
            // if a == "Empty" will not be added because a.max.X are always < than b.max.X
            // if a and b == "Empty" the result is a Empty box

            Vector3f maxa = a.Max;
            Vector3f maxb = b.Max;
            Vector3f mina = a.Min;
            Vector3f minb = b.Min;

            Vector3f max = new Vector3f(System.Math.Max(maxa.x, maxb.x), System.Math.Max(maxa.y, maxb.y), System.Math.Max(maxa.z, maxb.z));
            Vector3f min = new Vector3f(System.Math.Min(mina.x, minb.x), System.Math.Min(mina.y, minb.y), System.Math.Min(mina.z, minb.z));

            return BoundingBoxCenter.FromMinMax(min, max);
        }

        /// <summary>
        /// implicit conversion example :
        /// <code>
        /// AABBox box1 = new AABBox();
        /// AABBox2 box2 = box1;
        /// </code>
        /// </summary>
        public static implicit operator BoundingBoxCenter(BoundingBoxMinMax box)
        {
            return BoundingBoxCenter.FromMinMax(box.min, box.max);
        }


        public override string ToString()
        {
            if (isNaN) return "NaN";
            return $"Center {center} , Hsize {extend}";
        }
    }


}