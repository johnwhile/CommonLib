#region LGPL License

/*
Axiom Graphics Engine Library
Copyright © 2003-2011 Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

The math library included in this project, in addition to being a derivative of
the works of Ogre, also include derivative work of the free portion of the 
Wild Magic mathematics source code that is distributed with the excellent
book Game Engine Design.
http://www.wild-magic.com/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Common.Maths
{
    /// <summary>
    /// 4D homogeneous vector float4
    /// </summary>
    /// <remarks>
    /// A 3d point can be converted in vector4 with w=1, but if is a Normal need to set w=0 to avoid traslation operations
    /// A 4 channel color use X=R , Y=G , Z=B , W=A , in 0.0f-1.0f range format to respect shader language
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Vector4f : IEquatable<Vector4f>
    {
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;
        [FieldOffset(12)]
        public float w;
        [FieldOffset(0)]
        public unsafe fixed float field[4];


        static float clamp01(float value)
        {
            if (value > 1) return 1;
            else if (value < 0) return 0;
            else return value;
        }

        /// <summary>
        /// 1 1 1 1
        /// </summary>
        public static readonly Vector4f White = new Vector4f(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Vector4f Red = new Vector4f(1.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Vector4f Green = new Vector4f(0.0f, 1.0f, 0.0f, 1.0f);
        public static readonly Vector4f Blue = new Vector4f(0.0f, 0.0f, 1.0f, 1.0f);
        /// <summary>
        /// 0 0 0 1
        /// </summary>
        public static readonly Vector4f Black = new Vector4f(0.0f, 0.0f, 0.0f, 1.0f);
        /// <summary>
        /// 0 0 0 0
        /// </summary>
        public static readonly Vector4f Zero = new Vector4f(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// 1 1 1 1
        /// </summary>
        public static readonly Vector4f One = White;

        /// <summary>
        /// Use vector4 as 32bit color, the value are scaled to 0-1
        /// </summary>
        public Vector4f(byte r, byte g, byte b, byte a)
        {
            float inv = 1.0f / 255.0f;
            x = r * inv;
            y = g * inv;
            z = b * inv;
            w = a * inv;
        }
        public Vector4f(float all)
        {
            x = y = z = w = all;
        }
        public Vector4f(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public Vector4f(double x, double y, double z, double w)
        {
            this.x = (float)x;
            this.y = (float)y;
            this.z = (float)z;
            this.w = (float)w;
        }
        public Vector4f(Vector3f v, float w)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.w = w;
        }

        public Vector4f(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            w = reader.ReadSingle();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(w);
        }

        /// <summary>
        /// Try parse a string with format "x, y, z"
        /// </summary>
        /// <param name="toparse"></param>
        public static bool TryParse(string toparse, out Vector4f value) => MathParsers.TryParse(toparse, out value);

        /// <summary>
        /// float value is infinite or nan
        /// </summary>
        public bool IsNaN =>
            float.IsInfinity(x) || float.IsNaN(x) ||
            float.IsInfinity(y) || float.IsNaN(y) ||
            float.IsInfinity(z) || float.IsNaN(z) ||
            float.IsInfinity(w) || float.IsNaN(w);

        #region operators
        /// <summary>
        /// <code>
        /// 1  0  0  Tx    x
        /// 0  1  0  Ty *  y
        /// 0  0  1  Tz    z
        /// 0  0  0  1     w
        /// </code>
        /// </summary>
        public static Vector4f operator *(Matrix4x4f matrix, Vector4f vector)
        {
            Vector4f result = new Vector4f();

            result.x = vector.x * matrix.m00 + vector.y * matrix.m01 + vector.z * matrix.m02 + vector.w * matrix.m03;
            result.y = vector.x * matrix.m10 + vector.y * matrix.m11 + vector.z * matrix.m12 + vector.w * matrix.m13;
            result.z = vector.x * matrix.m20 + vector.y * matrix.m21 + vector.z * matrix.m22 + vector.w * matrix.m23;
            result.w = vector.x * matrix.m30 + vector.y * matrix.m31 + vector.z * matrix.m32 + vector.w * matrix.m33;

            return result;
        }
        public static Vector4f operator *(Vector4f vector, Matrix4x4f matrix)
        {
            Vector4f result = new Vector4f();

            result.x = vector.x * matrix.m00 + vector.y * matrix.m10 + vector.z * matrix.m20 + vector.w * matrix.m30;
            result.y = vector.x * matrix.m01 + vector.y * matrix.m11 + vector.z * matrix.m21 + vector.w * matrix.m31;
            result.z = vector.x * matrix.m02 + vector.y * matrix.m12 + vector.z * matrix.m22 + vector.w * matrix.m32;
            result.w = vector.x * matrix.m03 + vector.y * matrix.m13 + vector.z * matrix.m23 + vector.w * matrix.m33;

            return result;
        }
        public static Vector4f operator *(Vector4f vector, float scalar)
        {
            vector.Mul(scalar);
            return vector;
        }
        public static Vector4f operator /(Vector4f vector, float scalar)
        {
            if (Mathelp.isZero(scalar)) throw new ArithmeticException("Scalar can't be zero");
            vector.Mul(1.0f / scalar);
            return vector;
        }
        /// <summary>
        /// Attention, it's not dot product but x = x0*x1
        /// </summary>
        public static Vector4f operator *(Vector4f left, Vector4f right)
        {
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            left.w *= right.w;
            return left;
        }
        public static Vector4f operator /(Vector4f left, Vector4f right)
        {
            if (Mathelp.isZero(right.x * right.y * right.z * right.w)) throw new ArithmeticException("Scalar can't be zero");
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            left.w /= right.w;
            return left;
        }
        public static Vector4f operator +(Vector4f left, Vector4f right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            left.w += right.w;
            return left;
        }
        public static Vector4f operator -(Vector4f left, Vector4f right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            left.w -= right.w;
            return left;
        }
        public static Vector4f operator -(Vector4f left)
        {
            return left * -1;
        }
        public static bool operator ==(Vector4f left, Vector4f right) => left.Equals(ref right);
        public static bool operator !=(Vector4f left, Vector4f right) => !left.Equals(ref right);

        unsafe public static implicit operator Quaternion4f(Vector4f v) => *(Quaternion4f*)&v;
        #endregion

        public override bool Equals(object obj) => obj is Vector4f v && Equals(ref v);
        public bool Equals(Vector4f v) => Equals(ref v);
        public bool Equals(ref Vector4f v) => v.x == x && v.y == y && v.z == z && v.w == w;


        /// <summary>
        /// Add 1 to w component
        /// </summary>
        public static implicit operator Vector4f(Vector3f vector)
            => new Vector4f(vector.x, vector.y, vector.z, 1);

        public static implicit operator Vector4f(Color color)
            => new Vector4f(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        public static implicit operator Color4b(Vector4f color)
            => new Color4b(
                clamp01(color.x),
                clamp01(color.y),
                clamp01(color.z),
                clamp01(color.w));

        /// <summary>
        ///		Used to access a Vector by index 0 = this.x, 1 = this.y, 2 = this.z, 3 = this.w.  
        /// </summary>
        /// <remarks>
        ///		Uses unsafe pointer arithmetic to reduce the code required.
        ///	</remarks>
        public float this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < 4, "Indexer boundaries overrun in Vector4.");
                // using pointer arithmetic here for less code.  Otherwise, we'd have a big switch statement.
#if !UNSAFE
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                }
                return 0;
#else
				unsafe
				{
					fixed ( float* pX = &this.x )
					{
						return *( pX + index );
					}
				}
#endif
            }
            set
            {
                Debug.Assert(index >= 0 && index < 4, "Indexer boundaries overrun in Vector4.");

                // using pointer arithmetic here for less code.  Otherwise, we'd have a big switch statement.
#if !UNSAFE
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                }
#else
				unsafe
				{
                    fixed (float* pX = &this.x)
					{
						*( pX + index ) = value;
					}
				}
#endif
            }
        }

        /// <summary>
        /// Get the squared legth
        /// </summary>
        public float LengthSq => x * x + y * y + z * z + w * w;

        /// <summary>
        /// Get the length
        /// </summary>
        public float Length => (float)Math.Sqrt(LengthSq);

        /// <summary>
        /// Normalize the vector and get the calculated length. 
        /// </summary>
        public float Normalize() => Normalize(ref x, ref y, ref z, ref w);


        public static float Normalize(ref float x, ref float y, ref float z, ref float w)
        {
            float length = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            if (length > 1e-6f)
            {
                x /= length;
                y /= length;
                z /= length;
                w /= length;
            }
            else
            {
#if DEBUG
                Debugg.Message($"normalizing value {x} {y} {z} {w} return zero length, please check the code");
#endif
                x = y = z = w = 0;
                length = 0;
            }
            return length;
        }

        /// <summary>
        /// Get a new normalized vector3 copy
        /// </summary>
        public Vector4f Normal
        {
            get
            {
                Vector4f vect = this;
                vect.Normalize();
                return vect;
            }
        }
        /// <summary>
        /// Multiplies values by a scalar
        /// </summary>
        public void Mul(float scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
            w *= scalar;
        }
        public void Div(float scalar)
        {
            x /= scalar;
            y /= scalar;
            z /= scalar;
            w /= scalar;
        }
        public void Sum(Vector4f vect)
        {
            x += vect.x;
            y += vect.y;
            z += vect.z;
            w += vect.w;
        }
        public void Sub(Vector4f vect)
        {
            x -= vect.x;
            y -= vect.y;
            z -= vect.z;
            w -= vect.w;
        }

        /// <summary>
        /// x0*x1 + y0*y1 + ...
        /// </summary>
        public float Dot(Vector4f vector)
            => x * vector.x + y * vector.y + z * vector.z + w * vector.w;

        /// <inheritdoc cref="Dot(Vector4f)"/>
        public static float Dot(Vector4f left, Vector4f right)
            => left.Dot(right); //left passed as copy

        public static Vector4f Read(BinaryReader br) => new Vector4f(
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle());


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                hash = (hash * 16777619) ^ w.GetHashCode();
                return hash;
            }
        }
        /// <summary>
        /// format for normalized vector where first digit is always in 0.0-1.0 range and decimals is not very important
        /// </summary>
        public string ToStringRounded()
            => string.Format("{0:0.000},{1:0.000},{2:0.000},{3:0.000} ", x, y, z, w);

        public override string ToString()
            => IsNaN ? "NaN" : string.Format(Mathelp.DotCulture, "{0} {1} {2} {3}", x, y, z, w);

    }
}