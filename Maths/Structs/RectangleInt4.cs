using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common.Maths
{
    /// <summary>
    /// Rectangle with integer.<br/>
    /// (x,y) is origin
    /// (z,w) or (width,height) is the size
    /// the values <b><see cref="int.MinValue"/></b> and <b><see cref="int.MaxValue"/></b> are reserved to simulate positive and negative infinite values
    /// </summary>
    /// <remarks>
    /// For texture usage:<br/>
    /// <code>
    /// +---> U,Width<br/>
    /// |<br/>
    /// V,Height<br/></code>
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToString()}")]
    public struct Rectangle4i
    {
        /*
        /// <summary>
        /// for arithmetic consistency of <see cref="x2"/> and <see cref="y2"/>
        /// </summary>
        const int MIN_INT = -MAX_INT;
        const int MAX_INT = int.MaxValue / 2;
        */

        [FieldOffset(0)]
        public int x;
        [FieldOffset(4)]
        public int y;
        [FieldOffset(8)]
        public int width;
        [FieldOffset(8)]
        public int z;
        [FieldOffset(12)]
        public int height;
        [FieldOffset(12)]
        public int w;

        /// <summary>
        /// according from https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Mathematics/Rectangle.cs
        /// </summary>
        public int Left => x;
        /// <summary><inheritdoc cref="Left"/> </summary>
        public int Top => y;
        /// <summary><inheritdoc cref="Left"/> </summary>
        public int Right => x + width;
        /// <summary><inheritdoc cref="Left"/> </summary>
        public int Bottom => y + height;
        /// <summary><code> =>(float)width</code></summary>
        public float widthF => width;
        /// <summary><code> =>(float)height</code></summary>
        public float heightF => height;

        /// <summary>union to match the struct of <see cref="System.Drawing.Rectangle"/></summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(0)]
        public Rectangle rectangle;
        /// <summary>union vector of x,y</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(0)]
        public Vector2i position;
        /// <summary>union vector of width,height</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(8)]
        public Vector2i size;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int x2 { get => width < int.MaxValue ? x + width : width; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int y2 { get => height < int.MaxValue ? y + height : height; }

        public Rectangle4i(Vector2i size) : this(size.x, size.y) { }
        public Rectangle4i(SizeF size) : this((int)size.Width, (int)size.Height) { }
        public Rectangle4i(Size size) : this(size.Width, size.Height) { }
        public Rectangle4i(int W, int H) : this(0, 0, W, H) { }
        public Rectangle4i(int X, int Y, int W, int H) : this()
        {
            x = X;
            y = Y;
            width = W;
            height = H;
            /*
            x = Mathelp.CLAMP(X, MIN_INT, MAX_INT);
            y = Mathelp.CLAMP(Y, MIN_INT, MAX_INT);
            width = Mathelp.CLAMP(W, MIN_INT, MAX_INT);
            height = Mathelp.CLAMP(H, MIN_INT, MAX_INT);
            */
        }



        /// <summary>
        /// Try parse a string with format "x, y, z, w"
        /// </summary>
        /// <param name="toparse"></param>
        public static bool TryParse(string toparse, out Rectangle4i value)
        {
            return MathParsers.TryParse(toparse, out value);
        }
        /// <summary>
        /// Return the width/height ratio
        /// </summary>
        public float Aspect => width / (float)height;
        /// <summary>
        /// Area is negative or zero
        /// </summary>
        public bool IsEmpty => width <= 0 || height <= 0;
        /// <summary>
        /// Area is negative and position are equals to <see cref="int.MinValue"/>
        /// </summary>
        public bool IsUndefined => IsEmpty && (x == int.MinValue || y == int.MinValue);
        /// <summary>
        /// if <see cref="width"/> or <see cref="height"/> are equals to <see cref="int.MaxValue"/>, it's considered infinite
        /// </summary>
        public bool IsInfinite => width == int.MaxValue || height == int.MaxValue;


        public static Rectangle4i? Null => null;
        /// <summary>
        /// All value set to zero, default constructor.
        /// </summary>
        public static Rectangle4i Zero = default;
        /// <summary>
        /// It's bigger possible. Adding any rectangle results in the same rectangle.
        /// </summary>
        public static Rectangle4i Infinite { get => new Rectangle4i(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue); }
        /// <summary>
        /// In order that X and Y are considered undefined, they are set to the minimum value and negative size.
        /// Adding any rectangle returns the rectangle passed
        /// </summary>
        public static Rectangle4i Undefined { get => new Rectangle4i(int.MinValue, int.MinValue, int.MinValue, int.MinValue); }

        public static Rectangle4i FromPoints(Vector2i p0, Vector2i p1)
        {
            return FromPoints(p0.x, p0.y, p1.x, p1.y);
        }
        public static Rectangle4i FromPoints(int x0, int y0, int x1, int y1)
        {
            return new Rectangle4i(Math.Min(x0, x1), Math.Min(y0, y1), Math.Abs(x1 - x0), Math.Abs(y1 - y0));
        }


        /// <summary>
        /// </summary>
        /// <typeparam name="T">can be any 2 int vector</typeparam>
        static unsafe Rectangle4i makeBoundFromPoints<T>(IEnumerable<T> list) where T:unmanaged
        {
            var r = new Rectangle4i(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

            foreach (var p in list)
            {
                var pi = *(Vector2i*)&p;
                r.x = Mathelp.MIN(r.x, pi.x);
                r.y = Mathelp.MIN(r.y, pi.y);
                r.z = Mathelp.MAX(r.z, pi.x);
                r.w = Mathelp.MAX(r.w, pi.y);
            }
            r.z -= r.x;
            r.w -= r.y;

            return r;
        }
        public static Rectangle4i MakeBoundFromPoints(IEnumerable<Vector2i> list) => makeBoundFromPoints(list);
        public static Rectangle4i MakeBoundFromPoints(IEnumerable<Point> list) => makeBoundFromPoints(list);

        public bool Contain(Vector2i p) { return Contain(p.x, p.y); }
        public bool Contain(int X, int Y) => X >= x && Y >= y && X <= x2 && Y <= y2;

        public static bool Contain(ref Vector2i p, ref Rectangle4i r)
            => p.x >= r.x && p.y >= r.y && p.x <= r.x2 && p.y <= r.y2;

        public static bool Contain(ref Vector2i p, int x, int y, int w, int h)
            => p.x >= x && p.y >= y && p.x <= (x + w) && p.y <= (y + h);

        public static bool Contain(int px, int py, int x, int y, int w, int h)
            => px >= x && py >= y && px <= (x + w) && py <= (y + h);


        public void Add(int X, int Y)
        {
            if (IsUndefined)
            {
                x = X;
                y = Y;
                width = 0;
                height = 0;
            }
            else
            {
                width = Mathelp.MAX(Mathelp.ABS(x - X), width);
                height = Mathelp.MAX(Mathelp.ABS(y - Y), height);
                x = Mathelp.MIN(x, X);
                y = Mathelp.MIN(y, Y);
            }
        }
        public void Add(Vector2i p) => Add(p.x, p.y);

      

        /// <summary>
        /// Enlarges the rectangle to contain both.<br/>
        /// If area is zero, It's like adding a point.<br/>
        /// <code>
        ///  A---+...C....,
        ///  |   |--------B
        ///  +---+        |
        ///  :  |         |
        ///  :..+---------+
        /// </code>
        /// </summary>
        public static Rectangle4i Add(Rectangle4i A, Rectangle4i B)
        {
            A.Add(ref B);
            return A;
        }
        /// <summary>
        /// <inheritdoc cref="Add(Rectangle4i, Rectangle4i)"/>
        /// </summary>
        public void Add(ref Rectangle4i r)
        {
            if (r.IsUndefined) return;

            if (IsUndefined)
            {
                CopyFrom(ref r);
                return;
            }
            width = Mathelp.MAX(x2, r.x2);
            height = Mathelp.MAX(y2, r.y2);
            x = Mathelp.MIN(x, r.x);
            y = Mathelp.MIN(y, r.y);
            if (width < int.MaxValue) width -= x;
            if (height < int.MaxValue) height -= y;
        }
        /// <summary>
        /// <inheritdoc cref="Add(Rectangle4i, Rectangle4i)"/>
        /// </summary>
        public void Add(Rectangle4i r) { Add(ref r); }


        /// <summary>
        /// returns the common area
        /// <code>
        ///  A------+
        ///  |      |
        ///  |  +...+-----B
        ///  |  : C :     |
        ///  +--+...:     |
        ///     +---------+
        /// </code>
        /// </summary>
        /// <remarks>return true if the intersection exist. If false the <b><paramref name="C"/></b> is zero</remarks>
        public static bool Intersect(Rectangle4i A, Rectangle4i B, out Rectangle4i C)
        {
            C = default;

            if (A.IsEmpty || B.IsEmpty) return false;

            C.x = Math.Max(A.x, B.x);
            C.y = Math.Max(A.y, B.y);
            C.width = Math.Min(A.x2, B.x2); //C.x2
            C.height = Math.Min(A.y2, B.y2);

            if (C.width >= C.x && C.height >= C.y)
            {
                C.width -= C.x; //C.x2 -> C.width
                C.height -= C.y;
                return true;
            }
            else C = Zero;
            return false;
        }
        /// <summary>
        /// fast intersection test, return true only if point are inside rectangle and not above perimeter
        /// </summary>
        public bool Intersecting(Rectangle4i rect)
        {
            return (rect.x < x2) && (x < rect.x2) && (rect.y < y2) && (y < rect.y2);
        }


        public static Rectangle4i operator *(float scalar, Rectangle4i right)
        {
            right.position *= scalar;
            right.size *= scalar;
            return right;
        }
        public static Rectangle4i operator *(Rectangle4i left, float scalar)
        {
            left.position *= scalar;
            left.size *= scalar;
            return left;
        }
        public static bool operator ==(Rectangle4i left, Rectangle4i right) => Equals(ref left, ref right);

        public static bool operator !=(Rectangle4i left, Rectangle4i right) => !Equals(ref left, ref right);

        public static implicit operator RectangleF(Rectangle4i rect) => new RectangleF(rect.x, rect.y, rect.width, rect.height);

        public static implicit operator Rectangle(Rectangle4i rect) => rect.rectangle;

        public static implicit operator Rectangle4f(Rectangle4i rect) => new Rectangle4f(rect.x, rect.y, rect.width, rect.height);

        public static implicit operator Rectangle4i(Rectangle rect) => new Rectangle4i() { rectangle = rect };

        public static unsafe implicit operator Rectangle4i(Vector4i vect) => *(Rectangle4i*)&vect;

        public static implicit operator Rectangle4i(Size size) => new Rectangle4i(size);

        static bool Equals(ref Rectangle4i a, ref Rectangle4i b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
        public override bool Equals(object obj) => obj is Rectangle4i rect ? Equals(ref this, ref rect) : false;


        void CopyFrom(ref Rectangle4i r)
        {
            x = r.x;
            y = r.y;
            width = r.width;
            height = r.height;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + width.GetHashCode();
                hash = hash * 23 + height.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// format "x,y,z,w"
        /// </summary>
        public override string ToString()
        {
            if (IsUndefined) return "Undefined";
            if (IsInfinite) return "Infinite";
            return $"{x}, {y}, {width}, {height}";
        }
    }
}
