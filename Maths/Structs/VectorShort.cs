using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

#pragma warning disable

namespace Common.Maths
{
    /// <summary>
    /// 2x signed 16bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector2s
    {
        [FieldOffset(0)]
        public short x;
        [FieldOffset(2)]
        public short y;
        [FieldOffset(0)]
        Vector2us union;

        /// <summary>
        /// </summary>
        public Vector2s(int n) : this()
        {
            this.x = (short)n;
            this.y = (short)(n >> 16);
        }

        /// <summary>
        /// </summary>
        public Vector2s(short x, short y) : this()
        {
            this.x = x;
            this.y = y;
        }
        /// <summary>
        /// Exeption if value is greater than short.maxvalue
        /// </summary>
        public Vector2s(int x, int y) : this()
        {
            this.x = Convert.ToInt16(x);
            this.y = Convert.ToInt16(y);
        }

        public Vector2s(Point point) :
            this(point.X, point.Y)
        { }

        public short this[int index]
        {
            get { return index == 0 ? x : y; }
            set { if (index == 0) x = value; else y = value; }
        }

        public static implicit operator Vector2s(Int32 packed)
        {
            return new Vector2s(packed);
        }

        public static implicit operator Vector2s(Point point)
        {
            return new Vector2s(point.X, point.Y);
        }

        public static implicit operator Point(Vector2s vector)
        {
            return new Point(vector.x, vector.y);
        }

        public static Vector2s operator +(Vector2s left, Vector2s right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2s operator -(Vector2s left, Vector2s right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }
        public static Vector2s operator /(Vector2s left, Vector2s right)
        {
            if (right.x == 0 || right.y == 0) throw new ArithmeticException("i can't divide by zero");
            left.x /= right.x;
            left.y /= right.y;
            return left;
        }
        public static Vector2s operator *(Vector2s left, Vector2s right)
        {
            left.x *= right.x;
            left.y *= right.y;
            return left;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", x, y);
        }
    }

    /// <summary>
    /// 2x unsigned 16bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector2us
    {
        [FieldOffset(0)]
        public ushort x;
        [FieldOffset(2)]
        public ushort y;
        [FieldOffset(0)]
        public int packed;

        /// <summary>
        /// </summary>
        public Vector2us(ushort x, ushort y) : this()
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Exeption if value is greater than ushort.maxvalue
        /// </summary>
        public Vector2us(int x, int y) : this()
        {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
        }

        public ushort this[int index]
        {
            get { return index == 0 ? x : y; }
            set { if (index == 0) x = value; else y = value; }
        }

        public static implicit operator Point(Vector2us vector)
        {
            return new Point(vector.x, vector.y);
        }

        public static Vector2us operator +(Vector2us left, Vector2us right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2us operator -(Vector2us left, Vector2us right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }
        public static Vector2us operator /(Vector2us left, Vector2us right)
        {
            if (right.x == 0 || right.y == 0) throw new ArithmeticException("i can't divide by zero");
            left.x /= right.x;
            left.y /= right.y;
            return left;
        }
        public static Vector2us operator *(Vector2us left, Vector2us right)
        {
            left.x *= right.x;
            left.y *= right.y;
            return left;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", x, y);
        }
    }

    /// <summary>
    /// 3x unsigned 16bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector3us
    {
        [FieldOffset(0)]
        public ushort x;
        [FieldOffset(2)]
        public ushort y;
        [FieldOffset(4)]
        public ushort z;
        [FieldOffset(0)]
        unsafe fixed ushort field[3];

        /// <summary>
        /// </summary>
        public Vector3us(ushort x, ushort y,ushort z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Exeption if value is greater than ushort.maxvalue
        /// </summary>
        public Vector3us(int x, int y, int z)
        {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
            this.z = Convert.ToUInt16(z);
        }

        public unsafe ushort this[int index] => field[index];

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", x, y, z);
        }
    }

}


#pragma warning restore
