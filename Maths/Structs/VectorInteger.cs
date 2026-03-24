using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

#pragma warning disable

namespace Common.Maths
{
    /// <summary>
    /// 2x signed 32bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Vector2i
    {
        [FieldOffset(0)]
        public int x;
        [FieldOffset(4)]
        public int y;

        ///<summary>x</summary>
        [FieldOffset(0)]
        public int width;
        /// <summary>y</summary>
        [FieldOffset(4)]
        public int height;

        public Vector2i(int i) : this()
        {
            this.x = i;
            this.y = i;
        }
        public Vector2i(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
        }
        public Vector2i(float x, float y) : this()
        {
            this.x = (int)x;
            this.y = (int)y;
        }

        public bool IsZero { get => x == 0 && y == 0; }

        public static Vector2i Zero { get => new Vector2i(); }

        public int this[int index]
        {
            get { return index == 0 ? x : y; }
            set { if (index == 0) x = value; else y = value; }
        }


        /// <summary>
        /// Convert a rectangle to a 2d rectangle where X axis is same and Y inverted
        /// </summary>
        /// <param name="Client">the screen bound in pixel size</param>
        /// <param name="World">the screen bound in 2d coordinates</param>
        public Vector2f ConvertToWorld(Rectangle4i Client, AABRminmax World)
        {
            float xf = Mathelp.Interpolate(x, World.min.x, World.max.x, Client.Left, Client.Right);
            float yf = Mathelp.Interpolate(y, World.min.y, World.max.y, Client.Bottom, Client.Top);
            return new Vector2f(xf, yf);
        }
        /// <summary>
        /// Get squared length"
        /// </summary>
        public float LengthSq => x * x + y * y;
        
        /// <summary>
        /// Sum
        /// </summary>
        public void Sum(int x, int y)
        {
            this.x += x;
            this.y += y;
        }
        public void Sum(int i)
        {
            this.x += i;
            this.y += i;
        }
        /// <summary>
        /// Subtraction
        /// </summary>
        public void Sub(int x, int y)
        {
            this.x -= x;
            this.y -= y;
        }
        public void Sub(int i)
        {
            this.x -= i;
            this.y -= i;
        }
        public static bool operator >(Vector2i left, int i) => left.x > i && left.y > i;
        public static bool operator <(Vector2i left, int i) => left.x < i && left.y < i;
        public static bool operator >=(Vector2i left, int i) => left.x >= i && left.y >= i;
        public static bool operator <=(Vector2i left, int i) => left.x <= i && left.y <= i;
        public static bool operator ==(Vector2i left, int i) => left.x == i && left.y == i;
        public static bool operator !=(Vector2i left, int i) => left.x != i || left.y != i;
        public static Vector2i operator +(Vector2i left, int i)
        {
            left.x += i;
            left.y += i;
            return left;
        }
        public static Vector2i operator -(Vector2i left, int i)
        {
            left.x -= i;
            left.y -= i;
            return left;
        }
        public static Vector2i operator +(Vector2i left, Vector2i right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2i operator -(Vector2i left, Vector2i right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }
        /// <summary>
        /// the result is truncate to integer
        /// </summary>
        public static Vector2i operator *(Vector2i left, float scalar)
        {
            left.x = (int)(left.x * scalar);
            left.y = (int)(left.y * scalar);
            return left;
        }
        /// <summary>
        /// the result is truncate to integer
        /// </summary>
        public static Vector2i operator *(float scalar, Vector2i right)
        {
            right.x = (int)(right.x * scalar);
            right.y = (int)(right.y * scalar);
            return right;
        }
        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.x == right.x && left.y == right.y;
        }
        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !(left == right);
        }
        public static bool operator >(Vector2i left, Vector2i right)
        {
            return left.x * left.y > right.x * right.y;
        }
        public static bool operator <(Vector2i left, Vector2i right)
        {
            return left.x * left.y < right.x * right.y;
        }

        public static implicit operator Vector2i(int i) { return new Vector2i(i); }

        unsafe public static implicit operator Vector2i(Point p) => *(Vector2i*)&p;
        unsafe public static implicit operator Point(Vector2i v) => *(Point*)&v;
        unsafe public static implicit operator Size(Vector2i v) => *(Size*)&v;
        unsafe public static implicit operator Vector2i(Size s) => *(Vector2i*)&s;

        public static implicit operator PointF(Vector2i v) =>new PointF(v.x, v.y);
        public static implicit operator Vector2i(PointF p) => new Vector2i(p.X, p.Y);
        public static implicit operator SizeF(Vector2i v) => new SizeF(v.x, v.y);
        public static implicit operator Vector2i(SizeF s) => new Vector2i(s.Width, s.Height);


        public static implicit operator Vector2f(Vector2i v) => new Vector2f(v.x, v.y);

        private string DebugString { get => ToString(); }
        public override int GetHashCode()=> (x * 397) ^ y;
        public override string ToString() => string.Format("{0},{1}", x, y);
        
        /// <summary>
        /// It can be correctly parsed a vector with four or more items 
        /// </summary>
        public static bool TryParse(string s, out Vector2i result)
        {
            result = default(Vector2i);
            var split = s.Split(',');
            if (split.Length < 2) return false;
            if (!int.TryParse(split[0], out result.x)) return false;
            if (!int.TryParse(split[1], out result.y)) return false;
            return true;
        }
    }

    /// <summary>
    /// 3x signed 32bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Vector3i
    {
        [FieldOffset(0)]
        public int x;
        [FieldOffset(4)]
        public int y;
        [FieldOffset(8)]
        public int z;

        public Vector3i(int i) : this(i, i, i)
        { }

        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3i(float x, float y, float z) :
            this((int)x, (int)y, (int)z)
        { }


        public static implicit operator Vector3i(Vector3ui vector)
        {
            return new Vector3i(vector.x, vector.y, vector.z);
        }
        public static Vector3i operator +(Vector3i left, Vector3i right)
        {
            unchecked { return new Vector3i(left.x + right.x, left.y + right.y, left.z + right.z); }
        }
        public static Vector3i operator /(Vector3i left, float scalar)
        {
            unchecked { return new Vector3i(left.x / scalar, left.y / scalar, left.z / scalar); }
        }
        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3i sv)
                return sv == this;
            if (obj is Vector3ui uv)
                return uv.x == x && uv.y == y && uv.z == z;
            return false;
        }

        private string DebugString { get => ToString(); }
        public override string ToString() => string.Format("{0},{1},{2}", x, y, z);
        public string ToHexString(string format = "X2") => string.Format("{0},{1},{2}", x.ToString(format), y.ToString(format), z.ToString(format));

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x;
                hash = hash * 23 + y;
                hash = hash * 23 + z;
                return hash;
            }
        }

        /// <summary>
        /// It can be correctly parsed a vector with tree or more items 
        /// </summary>
        public static bool TryParse(string s, out Vector3i result)
        {
            result = default(Vector3i);
            var split = s.Split(',');
            if (split.Length < 3) return false;
            if (!int.TryParse(split[0], out result.x)) return false;
            if (!int.TryParse(split[1], out result.y)) return false;
            if (!int.TryParse(split[2], out result.z)) return false;
            return true;
        }
    }
    /// <summary>
    /// 4x signed 32bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Vector4i
    {
        [FieldOffset(0)]
        public int x;
        [FieldOffset(4)]
        public int y;
        [FieldOffset(8)]
        public int z;
        [FieldOffset(12)]
        public int w;
        /// <summary>
        /// </summary>
        public Vector4i(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public static bool operator ==(Vector4i a, Vector4i b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }
        public static bool operator !=(Vector4i a, Vector4i b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector4i sv) return sv == this;
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + z.GetHashCode();
                hash = hash * 23 + w.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// It can be correctly parsed a vector with four or more items 
        /// </summary>
        public static bool TryParse(string s, out Vector4i result)
        {
            result = default(Vector4i);
            var split = s.Split(',');
            if (split.Length < 4) return false;
            if (!int.TryParse(split[0], out result.x)) return false;
            if (!int.TryParse(split[1], out result.y)) return false;
            if (!int.TryParse(split[2], out result.z)) return false;
            if (!int.TryParse(split[3], out result.w)) return false;
            return true;
        }

        private string DebugString { get => ToString(); }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", x, y, z, w);
        }
        public string ToHexString(string format = "X2")
        {
            return string.Format("{0},{1},{2},{3}", x.ToString(format), y.ToString(format), z.ToString(format), w.ToString(format));
        }

    }

    /// <summary>
    /// 2x unsigned 32bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Vector2ui
    {
        [FieldOffset(0)]
        public uint x;
        [FieldOffset(4)]
        public uint y;

        /// <summary>
        /// </summary>
        public Vector2ui(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }

        public uint this[int index]
        {
            get { return index == 0 ? x : y; }
            set { if (index == 0) x = value; else y = value; }
        }

        private string DebugString { get => ToString(); }
        public override string ToString()
        {
            return string.Format("{0},{1}", x, y);
        }
    }

    /// <summary>
    /// 3x unsigned 32bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Vector3ui
    {
        [FieldOffset(0)]
        public uint x;
        [FieldOffset(4)]
        public uint y;
        [FieldOffset(8)]
        public uint z;

        /// <summary>
        /// </summary>
        public Vector3ui(uint x, uint y, uint z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        /// <summary>
        /// </summary>
        public Vector3ui(int x, int y, int z)
        {
            this.x = (uint)x;
            this.y = (uint)y;
            this.z = (uint)z;
        }

        public static implicit operator Vector3ui(Vector3i vector)
        {
            return new Vector3ui(vector.x, vector.y, vector.z);
        }
        public static Vector3ui operator +(Vector3ui left, Vector3ui right)
        {
            unchecked { return new Vector3ui((uint)(left.x + right.x), (uint)(left.y + right.y), left.z + right.z); }
        }
        public static Vector3ui operator /(Vector3ui left, uint scalar)
        {
            unchecked { return new Vector3ui((uint)(left.x / scalar), (uint)(left.y / scalar), left.z / scalar); }
        }

        public static bool operator ==(Vector3ui a, Vector3ui b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3ui a, Vector3ui b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3ui uv)
                return uv == this;
            if (obj is Vector3i sv)
                return sv.x == x && sv.y == y && sv.z == z;

            return false;
        }
        private string DebugString { get => ToString(); }
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", x, y, z);
        }
    }

}


#pragma warning restore
