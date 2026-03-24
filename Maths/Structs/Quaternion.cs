/*
Axiom Graphics Engine Library
Copyright � 2003-2011 Axiom Project Team

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

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Common.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebugString}")]
    public struct Quaternion4f : IEquatable<Quaternion4f>
    {
        const float EPSILON = 1e-03f;
        private static readonly int[] next = new int[3] { 1, 2, 0 };
        [FieldOffset(0)]
        Vector4f vector;
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;
        [FieldOffset(12)]
        public float w;
        [FieldOffset(0)]
        public unsafe fixed float Dim[4];

        public static readonly Quaternion4f Identity = new Quaternion4f(0, 0, 0, 1);
        public static readonly Quaternion4f Zero = new Quaternion4f(0, 0, 0, 0);

        public Quaternion4f(Vector4f vector) : this(vector.x, vector.y, vector.z, vector.w) { }
        public Quaternion4f(Vector3f axe, float w = 1) : this(axe.x, axe.y, axe.z, w) { }
        public Quaternion4f(float x, float y, float z, float w)
        {
            vector = default(Vector4f);
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static bool TryParse(string toparse, out Quaternion4f value)
        {
            if (!MathParsers.TryParse(toparse, out Vector4f vector))
            {
                value = Zero;
                return false;
            }
            value = vector;
            return true;
        }


        #region operators
        /// <summary>
        /// Used to multiply 2 Quaternions together.
        /// </summary>
        /// <remarks>
        ///		Quaternion multiplication is not communative in most cases.
        ///		i.e. p*q != q*p
        /// </remarks>
        public static Quaternion4f operator *(Quaternion4f left, Quaternion4f right)
        {
            var q = new Quaternion4f();
            q.w = left.w * right.w - left.x * right.x - left.y * right.y - left.z * right.z;
            q.x = left.w * right.x + left.x * right.w + left.y * right.z - left.z * right.y;
            q.y = left.w * right.y + left.y * right.w + left.z * right.x - left.x * right.z;
            q.z = left.w * right.z + left.z * right.w + left.x * right.y - left.y * right.x;
            return q;
        }

        public static Vector3f operator *(Quaternion4f quat, Vector3f vector)
        {
            // nVidia SDK implementation
            Vector3f uv, uuv;
            var qvec = new Vector3f(quat.x, quat.y, quat.z);

            uv = Vector3f.Cross(qvec, vector);
            uuv = Vector3f.Cross(qvec, uv);
            uv *= 2.0f * quat.w;
            uuv *= 2.0f;

            return vector + uv + uuv;

            // get the rotation matrix of the Quaternion and multiply it times the vector
            //return quat.ToRotationMatrix() * vector;
        }

        /// <summary>
        /// Used when a Real value is multiplied by a Quaternion.
        /// </summary>
        public static Quaternion4f operator *(float scalar, Quaternion4f right)
        {
            right.vector.Mul(scalar); //(right passed as copy)
            return right;
        }
        /// <summary>
        /// Used when a Quaternion is multiplied by a Real value.
        /// </summary>
        public static Quaternion4f operator *(Quaternion4f left, float scalar)
        {
            left.vector.Mul(scalar);//(left passed as copy)
            return left;
        }
        public static Quaternion4f operator +(Quaternion4f left, Quaternion4f right)
        {
            left.vector.Sum(right.vector);
            return left;
        }
        public static Quaternion4f operator -(Quaternion4f left, Quaternion4f right)
        {
            left.vector.Sub(right.vector);
            //left.w = 1.0f;
            return left;
        }
        public static Quaternion4f operator -(Quaternion4f right)
        {
            right.vector.Mul(-1);
            return right;
        }
        public static bool operator ==(Quaternion4f left, Quaternion4f right)
            => left.Equals(ref right);

        public static bool operator !=(Quaternion4f left, Quaternion4f right)
         => !left.Equals(ref right);

        public static implicit operator Vector4f(Quaternion4f q) => q.vector;
        #endregion

        /// <summary>
        ///    Local X-axis portion of this rotation.
        /// </summary>
        public Vector3f XAxis
        {
            get
            {
                var fTx = 2.0f * x;
                var fTy = 2.0f * y;
                var fTz = 2.0f * z;
                var fTwy = fTy * w;
                var fTwz = fTz * w;
                var fTxy = fTy * x;
                var fTxz = fTz * x;
                var fTyy = fTy * y;
                var fTzz = fTz * z;

                return new Vector3f(1.0f - (fTyy + fTzz), fTxy + fTwz, fTxz - fTwy);
            }
        }

        /// <summary>
        ///    Local Y-axis portion of this rotation.
        /// </summary>
        public Vector3f YAxis
        {
            get
            {
                var fTx = 2.0f * x;
                var fTy = 2.0f * y;
                var fTz = 2.0f * z;
                var fTwx = fTx * w;
                var fTwz = fTz * w;
                var fTxx = fTx * x;
                var fTxy = fTy * x;
                var fTyz = fTz * y;
                var fTzz = fTz * z;
                return new Vector3f(fTxy - fTwz, 1.0f - (fTxx + fTzz), fTyz + fTwx);
            }
        }

        /// <summary>
        ///    Local Z-axis portion of this rotation.
        /// </summary>
        public Vector3f ZAxis
        {
            get
            {
                var fTx = 2.0f * x;
                var fTy = 2.0f * y;
                var fTz = 2.0f * z;
                var fTwx = fTx * w;
                var fTwy = fTy * w;
                var fTxx = fTx * x;
                var fTxz = fTz * x;
                var fTyy = fTy * y;
                var fTyz = fTz * y;

                return new Vector3f(fTxz + fTwy, fTyz - fTwx, 1.0f - (fTxx + fTyy));
            }
        }
        /// <summary>
        /// Creates a Quaternion from a supplied angle and axis.
        /// </summary>
        /// <param name="angle">Value of an angle in radians.</param>
        /// <param name="axis">Arbitrary axis vector.</param>
        public static Quaternion4f FromAngleAxis(Vector3f axis, float angle)
        {
            float halfAngle = 0.5f * angle;
            float sin = (float)Math.Sin(halfAngle);

            return new Quaternion4f(
                sin * axis.x,
                sin * axis.y,
                sin * axis.z,
                (float)Math.Cos(halfAngle));
        }

        /// <summary>
        /// return pitch yaw, roll
        /// </summary>
        public Vector3f ToEulerAngles()
        {
            float halfPi = (float)Math.PI / 2;
            float test = x * y + z * w;
            float pitch, yaw, roll;

            if (test > 0.499f)
            {
                // singularity at north pole
                yaw = 2 * (float)Math.Atan2(this.x, this.w);
                roll = halfPi;
                pitch = 0;
            }
            else if (test < -0.499f)
            {
                // singularity at south pole
                yaw = -2 * (float)Math.Atan2(this.x, this.w);
                roll = -halfPi;
                pitch = 0;
            }
            else
            {
                float sqx = x * x;
                float sqy = y * y;
                float sqz = z * z;
                yaw = (float)Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * sqy - 2 * sqz);
                roll = (float)Math.Asin(2 * test);
                pitch = (float)Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * sqx - 2 * sqz);
            }

            if (pitch <= float.Epsilon)
            {
                pitch = 0f;
            }
            if (yaw <= float.Epsilon)
            {
                yaw = 0f;
            }
            if (roll <= float.Epsilon)
            {
                roll = 0f;
            }
            return new Vector3f(pitch, yaw, roll);
        }

        public static Quaternion4f FromEulerAngles(Vector3f euler)
        {
            return FromEulerAngles(euler.x, euler.y, euler.z);
        }
        /// <summary>
        /// Combines the euler angles in the order yaw, pitch, roll to create a rotation quaternion
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <param name="roll"></param>
        public static Quaternion4f FromEulerAngles(float pitch, float yaw, float roll)
        {
            return FromAngleAxis(Vector3f.UnitY, yaw) *
                   FromAngleAxis(Vector3f.UnitX, pitch) *
                   FromAngleAxis(Vector3f.UnitZ, roll);

            /*TODO: Debug
            //Equation from http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
            //heading
			
            Real c1 = (Real)Math.Cos(yaw/2);
            Real s1 = (Real)Math.Sin(yaw/2);
            //attitude
            Real c2 = (Real)Math.Cos(roll/2);
            Real s2 = (Real)Math.Sin(roll/2);
            //bank
            Real c3 = (Real)Math.Cos(pitch/2);
            Real s3 = (Real)Math.Sin(pitch/2);
            Real c1c2 = c1*c2;
            Real s1s2 = s1*s2;

            Real w =c1c2*c3 - s1s2*s3;
            Real x =c1c2*s3 + s1s2*c3;
            Real y =s1*c2*c3 + c1*s2*s3;
            Real z =c1*s2*c3 - s1*c2*s3;
            return new Quaternion(w,x,y,z);*/
        }

        /// <inheritdoc cref="Vector4f.Dot(Vector4f)"/>
        public float Dot(Quaternion4f quat)
        {
            return vector.Dot(quat);
        }

        /// <summary>
        /// Gets a 3x3 rotation matrix from this Quaternion.
        /// </summary>
        /// <returns></returns>
        public Matrix3x3f ToRotationMatrix()
        {
            Matrix3x3f rotation = new Matrix3x3f();

            var tx = 2.0f * x;
            var ty = 2.0f * y;
            var tz = 2.0f * z;
            var twx = tx * w;
            var twy = ty * w;
            var twz = tz * w;
            var txx = tx * x;
            var txy = ty * x;
            var txz = tz * x;
            var tyy = ty * y;
            var tyz = tz * y;
            var tzz = tz * z;

            rotation.m00 = 1.0f - (tyy + tzz);
            rotation.m01 = txy - twz;
            rotation.m02 = txz + twy;
            rotation.m10 = txy + twz;
            rotation.m11 = 1.0f - (txx + tzz);
            rotation.m12 = tyz - twx;
            rotation.m20 = txz - twy;
            rotation.m21 = tyz + twx;
            rotation.m22 = 1.0f - (txx + tyy);

            return rotation;
        }

        public static Quaternion4f FromRotationMatrix(Matrix3x3f matrix)
        {
            // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
            // article "Quaternion Calculus and Fast Animation".

            Quaternion4f result = Zero;

            float trace = matrix.m00 + matrix.m11 + matrix.m22;

            float root = 0.0f;

            if (trace > 0.0f)
            {
                // |this.w| > 1/2, may as well choose this.w > 1/2
                root = (float)Math.Sqrt(trace + 1.0f); // 2w
                result.w = 0.5f * root;

                root = 0.5f / root; // 1/(4w)

                result.x = (matrix.m21 - matrix.m12) * root;
                result.y = (matrix.m02 - matrix.m20) * root;
                result.z = (matrix.m10 - matrix.m01) * root;
            }
            else
            {
                // |result.w| <= 1/2

                int i = 0;
                if (matrix.m11 > matrix.m00)
                {
                    i = 1;
                }
                if (matrix.m22 > matrix[i, i])
                {
                    i = 2;
                }

                int j = next[i];
                int k = next[j];

                root = (float)Math.Sqrt(matrix[i, i] - matrix[j, j] - matrix[k, k] + 1.0f);

#if !UNSAFE
                float pi = 0.5f * root;
                root = 0.5f / root;
                float pw = (matrix[k, j] - matrix[j, k]) * root;
                float pj = (matrix[j, i] + matrix[i, j]) * root;
                float pk = (matrix[k, i] + matrix[i, k]) * root;
                result = i == 0
                             ? new Quaternion4f(pw, pi, pj, pk)
                             : i == 1
                                   ? new Quaternion4f(pw, pk, pi, pj)
                                   : new Quaternion4f(pw, pj, pk, pi);
#else
                unsafe
                {
                    float* apkQuat = &result.x;

                    apkQuat[i] = 0.5f * root;
                    root = 0.5f / root;

                    result.w = (matrix[k, j] - matrix[j, k]) * root;

                    apkQuat[j] = (matrix[j, i] + matrix[i, j]) * root;
                    apkQuat[k] = (matrix[k, i] + matrix[i, k]) * root;
                }
#endif
            }

            return result;
        }

        /// <summary>
        /// Computes the inverse of a Quaternion.
        /// </summary>
        public Quaternion4f Inverse()
        {
            var norm = LengthSq;
            return norm > 0.0f ? new Quaternion4f(w / norm, -x / norm, -y / norm, -z / norm) : Zero;
        }

        public void Write(BinaryWriter bw) => vector.Write(bw);
        public static Quaternion4f Read(BinaryReader reader) => new Vector4f(reader);
        

        public override bool Equals(object obj) => obj is Quaternion4f quat && Equals(ref quat);
        public bool Equals(Quaternion4f other) => Equals(ref other);
        public bool Equals(ref Quaternion4f quat) =>
            Mathelp.AlmostEqual(x, quat.x) &&
            Mathelp.AlmostEqual(y, quat.y) &&
            Mathelp.AlmostEqual(z, quat.z) &&
            Mathelp.AlmostEqual(w, quat.w);




        #region Vector4f inheritage
        /// <summary>
        /// Normalizes elements of this quaterion to the range [0,1].<br/>
        /// <inheritdoc cref="Vector4f.Normalize"/>
        /// </summary>
        public float Normalize() => vector.Normalize();
        /// <inheritdoc cref="Vector4f.IsNaN"/>
        public bool IsNaN => vector.IsNaN;
        /// <inheritdoc cref="Vector4f.LengthSq"/>
        public float LengthSq => vector.LengthSq;
        /// <inheritdoc cref="Vector4f.GetHashCode"/>
        public override int GetHashCode() => vector.GetHashCode();
        /// <inheritdoc cref="Vector4f.ToString"/>
        public override string ToString() => vector.ToString();
        #endregion

    }
}