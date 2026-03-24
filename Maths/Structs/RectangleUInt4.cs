using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Common.Maths
{
    /// <summary>
    /// Rectangle with origin (0,0) on topleft corner
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
    public struct Rectangle4ui
    {
        [FieldOffset(0)]
        public uint x;
        [FieldOffset(4)]
        public uint y;
        [FieldOffset(8)]
        public uint width;
        [FieldOffset(8)]
        public uint z;
        [FieldOffset(12)]
        public uint height;
        [FieldOffset(12)]
        public uint w;
        /// <summary><code> =>(float)width</code></summary>
        public float widthF => width;
        /// <summary><code> =>(float)height</code></summary>
        public float heightF => height;

        /// <summary>union vector of x,y</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(0)]
        public Vector2i position;
        /// <summary>union vector of width,height</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [FieldOffset(8)]
        public Vector2i size;

        public Rectangle4ui(Vector2ui size) : this(size.x, size.y) { }
        public Rectangle4ui(uint W, uint H) : this(0, 0, W, H) { }
        public Rectangle4ui(uint X, uint Y, uint W, uint H) : this()
        {
            x = X;
            y = Y;
            width = W;
            height = H;
        }

        public static Rectangle4ui Infinite { get => new Rectangle4ui(0, 0, uint.MaxValue, uint.MaxValue); }

        /// <summary>
        /// Try parse a string with format "x, y, z"
        /// </summary>
        /// <param name="toparse"></param>
        public static bool TryParse(string toparse, out Rectangle4i value)
        {
            return MathParsers.TryParse(toparse, out value);
        }

        public static bool operator ==(Rectangle4ui left, Rectangle4ui right)
        {
            return left.x == right.x && left.y == right.y && left.width == right.width && left.height == right.height;
        }
        public static bool operator !=(Rectangle4ui left, Rectangle4ui right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            if (obj is Rectangle4ui rect) return rect == this;
            return false;
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
            return $"{x}, {y}, {width}, {height}";
        }
    }
}
