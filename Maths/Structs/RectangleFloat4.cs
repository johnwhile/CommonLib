using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace Common.Maths
{
    /// <summary>
    /// Rectangle with TopLeft corner and Width Height size data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Rectangle4f
    {
        [FieldOffset(0)]
        Vector4f vector;
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float width;
        [FieldOffset(8)]
        public float z;
        [FieldOffset(12)]
        public float height;
        [FieldOffset(12)]
        public float w;

        //Unions
        [FieldOffset(0)]
        RectangleF rectangle;
        [FieldOffset(0)]
        public Vector2f position;
        [FieldOffset(8)]
        public Vector2f size;

        public Vector2f center
        {
            get { return new Vector2f(x + width / 2, y + height / 2); }
            set { x = value.x - width / 2; y = value.y - height / 2; }
        }

        public Rectangle4f(SizeF size) : this(0, 0, size.Width, size.Height) { }
        public Rectangle4f(float w, float h) : this(0, 0, w, h) { }
        public Rectangle4f(float x, float y, float w, float h) : this()
        {
            this.x = x;
            this.y = y;
            width = w;
            height = h;
        }

        public static Rectangle4f Infinite => new Rectangle4f(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static Rectangle4f NaN => new Rectangle4f(float.NaN, float.NaN, float.NaN, float.NaN);
        public static Rectangle4f UVfull => new Rectangle4f(0, 0, 1, 1);

        public bool Contain(int x, int y) =>
            x >= this.x &&
            y >= this.y &&
            x <= this.x + width &&
            y <= this.y + height;

        public void Enlarge(Rectangle4f bound)
        {
            x = Mathelp.MIN(x, bound.x);
            y = Mathelp.MIN(y, bound.y);
            if (bound.x + bound.width > x + width) width = bound.x + bound.width - x;
            if (bound.y + bound.height > y + height) height = bound.y + bound.height - y;
        }

        /// <summary>
        /// usefull for texture coordinate normalization
        /// </summary>
        public static Rectangle4f ScalarDivision(Rectangle4i integerRect, float scalex, float scaley)
        {
            var result = new Rectangle4f()
            {
                x = integerRect.x / scalex,
                y = integerRect.y / scaley,
                width = integerRect.width / scalex,
                height = integerRect.height / scaley
            };

            if (Mathelp.isZero(scalex)) { result.x = result.width = float.NaN; }
            if (Mathelp.isZero(scaley)) { result.y = result.height = float.NaN; }

            return result;
        }
        public static Rectangle4f ScalarMultiplication(Rectangle4i integerRect, float scalex, float scaley) =>
            new Rectangle4f()
            {
                x = integerRect.x * scalex,
                y = integerRect.y * scaley,
                width = integerRect.x * scalex,
                height = integerRect.y * scaley
            };

        public static unsafe Rectangle4f MakeBoundFromPoints(IEnumerable<Vector2f> list)
        {
            var r = new Rectangle4f(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            foreach (var p in list)
            {
                //var pi = *(Vector2i*)&p;
                r.x = Mathelp.MIN(r.x, p.x);
                r.y = Mathelp.MIN(r.y, p.y);
                r.z = Mathelp.MAX(r.z, p.x);
                r.w = Mathelp.MAX(r.w, p.y);
            }
            r.z -= r.x;
            r.w -= r.y;

            return r;
        }

        /// <summary>
        /// </summary>
        public static Rectangle4f operator *(float scalar, Rectangle4f right)
        {
            right.position *= scalar;
            right.size *= scalar;
            return right;
        }
        public static Rectangle4f operator *(Rectangle4f left, float scalar)
        {
            left.position *= scalar;
            left.size *= scalar;
            return left;
        }
        /// <summary>
        /// perform a scale operation where
        /// <code>
        /// x = left.x*scalar.x
        /// y = left.y*scalar.y
        /// w = left.w*scalar.x
        /// h = left.h*scalar.y</code>
        /// </summary>
        public static Rectangle4f operator *(Rectangle4f left, Vector2f scalar)
        {
            left.position *= scalar;
            left.size *= scalar;
            return left;
        }
        /// <summary>
        /// perform a scale operation where
        /// <code>
        /// x = left.x/scalar.x
        /// y = left.y/scalar.y
        /// w = left.w/scalar.x
        /// h = left.h/scalar.y</code>
        /// </summary>
        public static Rectangle4f operator /(Rectangle4f left, Vector2f scalar)
        {
            left.position /= scalar;
            left.size /= scalar;
            return left;
        }

        public static Rectangle4f operator +(Rectangle4f left, Rectangle4f right)
        {
            left.x += right.x;
            left.y += right.y;
            left.width += right.width;
            left.height += right.height;
            return left;
        }
        public static bool IsEqual(ref Rectangle4f left, ref Rectangle4f right) =>
            left.x == right.x &&
            left.y == right.y &&
            left.width == right.width
            && left.height == right.height;


        public static implicit operator RectangleF(Rectangle4f obj) => obj.rectangle;
        public static implicit operator Vector4f(Rectangle4f obj) => obj.vector;
        public static implicit operator Rectangle4i(Rectangle4f obj) => new Rectangle4i((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
        public static implicit operator Rectangle(Rectangle4f obj) => new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
        public static implicit operator Rectangle4f(Size size) => new Rectangle4f(size);
        public static implicit operator Rectangle4f(RectangleF rectF) => new Rectangle4f() { rectangle = rectF };

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ width.GetHashCode();
                hash = (hash * 16777619) ^ height.GetHashCode();
                return hash;
            }
        }
        public bool IsNaN => position.IsNaN || size.IsNaN;

        private string DebugString { get => ToStringNormalized(); }

        public string ToStringNormalized() =>
            string.Format("{0:0.000} {1:0.000} {2:0.000} {3:0.000}", x, y, width, height);

        public override string ToString() =>
            IsNaN ? "NaN" : string.Format("{0},{1},{2},{3}", x, y);

    }
}