using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Common.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Vector2d : IEquatable<Vector2d>
    {
        [FieldOffset(0)]
        public double x;
        [FieldOffset(8)]
        public double y;
        [FieldOffset(0)]
        unsafe fixed double field[2];

        public Vector2d(double x, double y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public Vector2d(BinaryReader reader) : this()
        {
            x = reader.ReadDouble();
            y = reader.ReadDouble();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
        }
        public static Vector2d Read(BinaryReader reader) => new Vector2d(reader);

        public unsafe double this[int i]
        {
            get => field[i];
            set => field[i] = value;
        }

        public bool IsNaN =>
            double.IsInfinity(x) || double.IsNaN(x) ||
            double.IsInfinity(y) || double.IsNaN(y);

        public static readonly Vector2d PosInf = new Vector2d(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector2d NegInf = new Vector2d(double.NegativeInfinity, double.NegativeInfinity);
        public static readonly Vector2d NaN = new Vector2d(double.NaN, double.NaN);

        #region operator overload
        public static Vector2d operator /(Vector2d left, double scalar)
        {
            if (scalar <= double.Epsilon) throw new ArgumentException("Cannot divide a Vector2 by zero");
            var inverse = 1.0f / scalar;
            left.x *= inverse;
            left.y *= inverse;
            return left;
        }
        public static Vector2d operator +(Vector2d left, Vector2d right)
        {
            left.x += right.x;
            left.y += right.y;
            return left;
        }
        public static Vector2d operator -(Vector2d left, Vector2d right)
        {
            left.x -= right.x;
            left.y -= right.y;
            return left;
        }
        public static Vector2d operator +(Vector2d left, double scalar)
        {
            left.x += scalar;
            left.y += scalar;
            return left;
        }
        public static Vector2d operator *(Vector2d left, double scalar)
        {
            left.x *= scalar;
            left.y *= scalar;
            return left;
        }
        public static Vector2d operator *(Vector2d left, Vector2d right)
        {
            left.x *= right.x;
            left.y *= right.y;
            return left;
        }
        public static Vector2d operator /(Vector2d left, Vector2d right)
        {
            left.x /= right.x;
            left.y /= right.y;
            return left;
        }


        public static Vector2d operator -(Vector2d v, double scalar) => v + (-scalar);
        public static Vector2d operator *(double scalar, Vector2d right) => right * scalar;
        public static Vector2d operator -(Vector2d left) => left * (-1);
        public static bool operator ==(Vector2d left, Vector2d right) => left.Equals(in right);
        public static bool operator !=(Vector2d left, Vector2d right) => !left.Equals(in right);
        public override bool Equals(object obj) => obj is Vector2d vector && Equals(in vector);
        public bool Equals(Vector2d other) => Equals(in other);
        public bool Equals(in Vector2d other) => x == other.x && y == other.y;

        #endregion
        /// <summary>
        /// <seealso cref="GetLengthSquared"/>
        /// </summary>
        public double LengthSq => x * x + y * y;
        /// <summary>
        /// <seealso cref="GetLength"/>
        /// </summary>
        public double Length => Math.Sqrt(LengthSq);
        /// <summary>
        /// sqrt(x*x + y*y)
        /// </summary>
        public static double GetLength(Vector2d vector) => vector.Length;
        /// <summary>
        /// x*x + y*y
        /// </summary>
        public static double GetLengthSquared(Vector2d vector) => vector.LengthSq;
        /// <summary>
        /// |x| + |y| + |z|
        /// </summary>
        public static double GetManhattanLength(Vector2d vector) => Mathelp.ABS(vector.x) + Mathelp.ABS(vector.y);

        /// <summary>
        /// Normalize the vector and get the calculated length. 
        /// </summary>
        public double Normalize()
        {
            double length = Length;

            if (length > double.Epsilon)
            {
                x /= length;
                y /= length;
            }
            return length;
        }
        /// <summary>
        /// <inheritdoc cref="Vector2f.Dot(Vector2f, Vector2f)"/>
        /// </summary>
        public static double Dot(Vector2d a, Vector2d b) => a.x * b.x + a.y * b.y;

        /// <summary>
        /// <inheritdoc cref="Vector2f.Cross(Vector2f, Vector2f)"/>
        /// </summary>
        public static double Cross(Vector2d a, Vector2d b) => a.x * b.y - a.y * b.x;

        /// <summary>
        /// <seealso cref="Normalize"/>
        /// </summary>
        public Vector2d Normal
        {
            get
            {

                Vector2d vect = new Vector2d(x, y);
                vect.Normalize();
                return vect;
            }
        }
        /// <summary>
        /// <seealso cref="Normal"/>
        /// </summary>
        public static Vector2d GetNormal(Vector2d vector) => vector.Normal;

        /// <summary>
        /// x XOR y
        /// </summary>
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();

        /// <summary>
        /// format for normalized vector where first digit is always in 0.0-1.0 range and decimals is not very important
        /// </summary>
        public string ToStringRounded() => string.Format(Mathelp.DotCulture, "{0:0.000000} {1:0.000000}", x, y);
        public override string ToString() => IsNaN ? "NaN" : string.Format(Mathelp.DotCulture, "{0} {1}", x, y);

    }
}