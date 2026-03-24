using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace Common.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Vector2f : IEquatable<Vector2f>
    {
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        /// <summary>x</summary>
        [FieldOffset(0)]
        public float u;
        /// <summary>y</summary>
        [FieldOffset(4)]
        public float v;
        [FieldOffset(0)]
        public float width;
        [FieldOffset(4)]
        public float height;
        [FieldOffset(0)]
        unsafe fixed float field[2];

        public Vector2f(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }
        public Vector2f(double x, double y)
            : this((float)x, (float)y) { }

        public Vector2f(BinaryReader reader) : this()
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
        }
        public static Vector2f Read(BinaryReader br)
        {
            return new Vector2f(
                br.ReadSingle(),
                br.ReadSingle());
        }
        public unsafe float this[int i]
        {
            get => field[i];
            set => field[i] = value;
        }

        public bool IsNaN =>
            float.IsInfinity(x) || float.IsNaN(x) ||
            float.IsInfinity(y) || float.IsNaN(y);

        public static readonly Vector2f PosInf = new Vector2f(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector2f NegInf = new Vector2f(float.NegativeInfinity, float.NegativeInfinity);
        public static readonly Vector2f NaN = new Vector2f(float.NaN, float.NaN);
        public static readonly Vector2f Zero = new Vector2f(0, 0);
        public static readonly Vector2f One = new Vector2f(1, 1);
        public static readonly Vector2f UnitX = new Vector2f(1, 0);
        public static readonly Vector2f UnitY = new Vector2f(0, 1);

        #region operator overload
        public static Vector2f operator /(Vector2f left, float scalar)
        {
            if (scalar <= float.Epsilon) throw new ArgumentException("Cannot divide a Vector2 by zero");
            var inverse = 1.0f / scalar;
            left.x *= inverse;
            left.y *= inverse;
            return left;
        }
        public static Vector2f operator +(Vector2i left, Vector2f right)
        {
            right.x += left.x;
            right.y += left.y;
            return left;
        }
        public static Vector2f operator +(Vector2f left, Vector2i right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2f operator +(Vector2f left, Vector2f right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2f operator -(Vector2f left, Vector2i right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }
        public static Vector2f operator -(Vector2f left, Vector2f right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }

        public static Vector2f operator +(Vector2f v, float scalar)
        {
            v.x += scalar;
            v.y += scalar;
            return v;
        }
        public static Vector2f operator -(Vector2f v, float scalar)
        {
            v.x -= scalar;
            v.y -= scalar;
            return v;
        }
        public static Vector2f operator *(Vector2f left, float scalar)
        {
            left.x *= scalar;
            left.y *= scalar;
            return left;
        }
        public static Vector2f operator *(float scalar, Vector2f right)
        {
            right.x *= scalar;
            right.y *= scalar;
            return right;
        }

        public static Vector2f operator -(Vector2f left)
        {
            left.x *= -1;
            left.y *= -1;
            return left;
        }
        /// <summary>
        /// is not a dot product, simple x0*x1, y0*y1
        /// </summary>
        public static Vector2f operator *(Vector2f left, Vector2f right)
        {
            left.x *= right.x;
            left.y *= right.y;
            return left;
        }
        public static Vector2f operator /(Vector2f left, Vector2f right)
        {
            left.x /= right.x;
            left.y /= right.y;
            return left;
        }

        public static bool operator ==(Vector2f left, Vector2f right) => left.Equals(ref right);
        public static bool operator !=(Vector2f left, Vector2f right) => !left.Equals(ref right);
        public override bool Equals(object obj) => obj is Vector2f vector && Equals(ref vector);
        public bool Equals(Vector2f other) => Equals(ref other);
        public bool Equals(ref Vector2f other) => x == other.x && y == other.y;


        #endregion

        /// <summary>
        /// x*x + y*y
        /// </summary>
        public float LengthSq => x * x + y * y;

        /// <summary>
        /// <seealso cref="GetLength"/>
        /// </summary>
        public float Length => (float)Math.Sqrt(LengthSq);

        /// <summary>
        /// sqrt(x*x + y*y)
        /// </summary>
        public static float GetLength(Vector2f vector) => vector.Length;

        /// <summary>
        /// <seealso cref="LengthSq"/>
        /// </summary>
        public static float GetLengthSquared(Vector2f vector) => vector.LengthSq;

        /// <summary>
        /// |x| + |y| + |z|
        /// </summary>
        public static float GetManhattanLength(Vector2f vector) => Mathelp.ABS(vector.x) + Mathelp.ABS(vector.y);

        /// <summary>
        /// Normalize the vector and get the calculated length. 
        /// </summary>
        public float Normalize()
        {
            float length = Length;

            if (length > float.Epsilon)
            {
                x /= length;
                y /= length;
            }
            return length;
        }
        /// <summary>
        /// <code>A·B =|A|·|B|·cos(angle AOB)</code>
        /// ax * bx + ay * by
        /// </summary>
        public static float Dot(in Vector2f a, in Vector2f b) => a.x * b.x + a.y * b.y;

        /// <summary>
        /// <code>A*B =|A|·|B|·sin(angle AOB)</code>
        /// a.x * b.y - a.y * b.x, is the magnitude of vector c perpendicular to plane that lies in a b.
        /// </summary>
        /// <remarks>
        /// the formula was derived from 3d:
        /// a = Vector3(a.x,a.y,0) 
        /// b = Vector3(b.x,b.y,0)
        /// c = Vector3(0,0, a.x*b.y - a.y*b.x)
        /// 
        /// notice that is used to find the orientation of a vector:
        /// where a = p1-p0 and b = p2-p1 , the cross is positive on top, negative on bottom
        /// <code>
        ///         +cross
        ///     p0-------->p1
        ///        -cross    \
        ///                   \p2
        /// </code>
        /// </remarks>
        public static float Cross(in Vector2f a, in Vector2f b) => a.x * b.y - a.y * b.x;

        /// <summary>
        /// <seealso cref="Normalize"/>
        /// </summary>
        public Vector2f Normal
        {
            get
            {

                Vector2f vect = new Vector2f(x, y);
                vect.Normalize();
                return vect;
            }
        }
        /// <summary>
        /// <seealso cref="Normal"/>
        /// </summary>
        public static Vector2f GetNormal(Vector2f vector) => vector.Normal;

        /// <summary>
        /// x XOR y
        /// </summary>
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();

        /// <summary>
        /// Convert a 2d rectangle to a screen rectangle where X axis is same and Y inverted
        /// </summary>
        /// <param name="Client">the screen bound in pixel size</param>
        /// <param name="World">the screen bound in 2d coordinates</param>
        public Vector2i ConvertToScreen(Rectangle4i Client, AABRminmax World)
        {
            int xi = (int)Mathelp.Interpolate(x, Client.Left, Client.Right, World.min.x, World.max.x);
            int yi = (int)Mathelp.Interpolate(y, Client.Bottom, Client.Top, World.min.y, World.max.y);
            return new Vector2i(xi, yi);
        }

        public static implicit operator Vector2i(Vector2f vector) => new Vector2i(vector.x, vector.y);

        public static implicit operator Point(Vector2f vector) => new Point((int)vector.x, (int)vector.y);
        public static implicit operator Vector2f(Point point) => new Vector2f(point.X, point.Y);
        public static implicit operator Size(Vector2f vector) => new Size((int)vector.x, (int)vector.y);
        public static implicit operator Vector2f(Size size) => new Vector2f(size.Width, size.Height);

        unsafe public static implicit operator PointF(Vector2f v) => *(PointF*)&v;
        unsafe public static implicit operator Vector2f(PointF p) => *(Vector2f*)&p;
        unsafe public static implicit operator SizeF(Vector2f v) => *(SizeF*)&v;
        unsafe public static implicit operator Vector2f(SizeF s) => *(Vector2f*)&s;

        public static explicit operator Vector3f(Vector2f v) => new Vector3f(v.x, v.y, 0);

        /// <summary>
        /// format for normalized vector where first digit is always in 0.0-1.0 range and decimals is not very important
        /// </summary>
        public string ToStringRounded() => string.Format(Mathelp.DotCulture, "{0:0.000} {1:0.000}", x, y);
        public override string ToString() => IsNaN ? "NaN" : string.Format(Mathelp.DotCulture, "{0} {1}", x, y);

    }
}