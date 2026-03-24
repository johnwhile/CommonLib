using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Common.Maths
{
    /// <summary>
    /// Vector3D used when need accurate math precision, not used to store data
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Vector3d : IEquatable<Vector3d>
    {
        [FieldOffset(0)]
        public double x;
        [FieldOffset(8)]
        public double y;
        [FieldOffset(16)]
        public double z;
        [FieldOffset(0)]
        unsafe fixed double field[3];

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool IsNaN
        {
            get
            {
                return double.IsInfinity(x) || double.IsNaN(x) ||
                       double.IsInfinity(y) || double.IsNaN(y) ||
                       double.IsInfinity(z) || double.IsNaN(z);
            }
        }

        public unsafe double this[int i]
        {
            get => field[i];
            set => field[i] = value;
        }

        /// <summary>
        /// A "null" vector, not zero, used example when you want considerate the value not processed or divided by 0
        /// </summary>
        public static readonly Vector3d NaN = new Vector3d(double.NaN, double.NaN, double.NaN);
        public static readonly Vector3d PosInf = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector3d NegInf = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        public static readonly Vector3d Zero = new Vector3d(0, 0, 0);
        public static readonly Vector3d One = new Vector3d(1, 1, 1);
        public static readonly Vector3d UnitX = new Vector3d(1, 0, 0);
        public static readonly Vector3d UnitY = new Vector3d(0, 1, 0);
        public static readonly Vector3d UnitZ = new Vector3d(0, 0, 1);
        public static readonly Vector3d NormalOne = new Vector3d(1.0 / Mathelp.Sqrt3, 1.0 / Mathelp.Sqrt3, 1.0 / Mathelp.Sqrt3);


        #region operator overload
        public static Vector3d operator +(Vector3d left, Vector3d right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            return left;
        }
        public static Vector3d operator +(Vector3d left, double scalar)
        {
            left.x += scalar;
            left.y += scalar;
            left.z += scalar;
            return left;
        }
        public static Vector3d operator -(Vector3d left, Vector3d right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            return left;
        }
        public static Vector3d operator *(Vector3d left, Vector3d right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            return left;
        }
        public static Vector3d operator *(Vector3d left, double scalar)
        {
            left.x *= scalar;
            left.y *= scalar;
            left.z *= scalar;
            return left;
        }
        public static Vector3d operator /(Vector3d left, Vector3d right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            return left;
        }
        public static Vector3d operator /(Vector3d left, double scalar)
        {
            if (Mathelp.isZero(scalar)) throw new DivideByZeroException("Cannot divide by zero");
            return left * (1 / scalar);
        }
        public static Vector3d operator -(Vector3d v, double scalar) => v + (-scalar);
        public static Vector3d operator -(Vector3d left) => left * -1;
        public static Vector3d operator *(double scalar, Vector3d right) => right * scalar;
        public static Vector3d operator /(double scalar, Vector3d right) => right / scalar;


        public static bool operator ==(Vector3d left, Vector3d right)
        {
            if (Mathelp.isZero(left.x - right.x)) return false;
            if (Mathelp.isZero(left.y - right.y)) return false;
            if (Mathelp.isZero(left.z - right.z)) return false;
            return true;
        }
        public static bool operator !=(Vector3d left, Vector3d right) => !(left == right);

        public override bool Equals(object obj) => obj is Vector3d vector && Equals(in vector);
        public bool Equals(Vector3d vector) => Equals(in vector);
        public bool Equals(in Vector3d vector) => vector == this;

        #endregion

        #region Math

        /// <summary>
        /// Get the squared Length
        /// </summary>
        public double LengthSq => x * x + y * y + z * z;

        /// <summary>
        /// Get the length
        /// </summary>
        public double Length => Math.Sqrt(LengthSq);

        /// <summary>
        /// sqrt(x*x + y*y + z*z)
        /// </summary>
        public static double GetLength(Vector3d vector) => vector.Length;

        /// <summary>
        /// x*x + y*y + z*z
        /// </summary>
        public static double GetLengthSquared(Vector3d vector) => vector.LengthSq;

        /// <summary>
        /// |x| + |y| + |z|
        /// </summary>
        public static double GetManhattanLength(Vector3d vector) => Mathelp.ABS(vector.x) + Mathelp.ABS(vector.y) + Mathelp.ABS(vector.z);

        /// <summary>
        /// Update minimum values
        /// </summary>
        public void Min(double x, double y, double z)
        {
            if (this.x > x) this.x = x;
            if (this.y > y) this.y = y;
            if (this.z > z) this.z = z;
        }

        /// <summary>
        /// Update maximum values
        /// </summary>
        public void Max(double x, double y, double z)
        {
            if (this.x < x) this.x = x;
            if (this.y < y) this.y = y;
            if (this.z < z) this.z = z;
        }
        /// <summary>
        /// Sum
        /// </summary>
        public void Sum(ref Vector3d vect)
        {
            x += vect.x;
            y += vect.y;
            z += vect.z;
        }
        /// <summary>
        /// Subtraction
        /// </summary>
        public void Sub(ref Vector3d vect)
        {
            x -= vect.x;
            y -= vect.y;
            z -= vect.z;
        }
        /// <summary>
        /// multiple x,y,z * scalar
        /// </summary>
        public void Multiply(double scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
        }

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
                z /= length;
            }
            return length;
        }
        /// <summary>
        /// Get a new normalized VectorDouble3 copy
        /// </summary>
        public Vector3d Normal
        {
            get
            {
                Vector3d vect = new Vector3d(x, y, z);
                vect.Normalize();
                return vect;
            }
        }
        public static Vector3d GetNormal(Vector3d vector) => vector.Normal;

        public static double Distance(Vector3d a, Vector3d b)
        {
            a.Sub(ref b);
            return a.Length;
        }
        /// <summary>
        /// <inheritdoc cref="Vector3f.Dot(Vector3f, Vector3f)"/>
        /// </summary>
        public static double Dot(Vector3d a, Vector3d b) => a.x * b.x + a.y * b.y + a.z * b.z;

        /// <summary>
        /// remember that b × a = −(a × b) and for a LH coord system :
        /// <para> X = Y × Z </para>
        /// <para> Y = Z × X </para>
        /// <para> Z = X × Y </para>
        /// </summary>
        /// <remarks>
        /// A = direction of thumb, B = index finger, Cross = middle finger
        /// using Left-Hand rule for a Left Hand coordinate system
        /// </remarks>
        public static Vector3d Cross(Vector3d left, Vector3d right) => new Vector3d(
                (left.y * right.z) - (left.z * right.y),
                (left.z * right.x) - (left.x * right.z),
                (left.x * right.y) - (left.y * right.x));

        #endregion



        /// <summary>
        /// Optimized version when you call it intensively 
        /// </summary>
        public static void TransformCoordinate(in Vector3d vector, in Matrix4x4f transform, ref Vector3d result)
        {
            double iw = transform.m30 * vector.x + transform.m31 * vector.y + transform.m32 * vector.z + transform.m33 * 1.0f; //w = 1;
            if (iw * iw < 1e-14) iw = 1.0f;
            iw = 1.0f / iw;
            result.x = (vector.x * transform.m00 + vector.y * transform.m01 + vector.z * transform.m02 + transform.m03) * iw;
            result.y = (vector.x * transform.m10 + vector.y * transform.m11 + vector.z * transform.m12 + transform.m13) * iw;
            result.z = (vector.x * transform.m20 + vector.y * transform.m21 + vector.z * transform.m22 + transform.m23) * iw;
        }

        /// <summary>
        /// Transform the Point vector using matrix, then result is transform * vector
        /// </summary>
        /// <remarks>
        /// VectorDouble3 are calculated as Vector4.w = 1 , 
        /// </remarks>
        public static Vector3d TransformCoordinate(Vector3d vector, Matrix4x4f transform)
        {
            Vector3d result = default;
            TransformCoordinate(in vector, in transform, ref result);
            return result;
        }

        public static implicit operator Vector3d(Vector3f vector) => new Vector3d(vector.x, vector.y, vector.z);

        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        /// <summary>
        /// format for normalized vector where first digit is always in 0.0-1.0 range and decimals is not very important
        /// </summary>
        public string ToStringRounded() => string.Format(Mathelp.DotCulture, "{0:0.000000} {1:0.000000} {2:0.000000}", x, y, z);

        public override string ToString() => IsNaN ? "NaN" : string.Format(Mathelp.DotCulture, "{0} {1} {2}", x, y, z);

    }
}