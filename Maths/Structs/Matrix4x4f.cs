using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Common.Maths
{
    /// <summary>
    /// <b>Column-major version</b><br/>
    /// I follow the left-handle directx format and i use the classic mathematical notation :
    /// <code>
    /// 1  0  0  Tx
    /// 0  1  0  Ty
    /// 0  0  1  Tz
    /// 0  0  0  1
    /// </code>
    /// </summary>
    /// <remarks>
    /// DirectXMath uses <b>row-major matrices, row vectors, and pre-multiplication</b> so require to be traspose.<br/>
    /// HLSL shaders default use <b>column-major matrices</b> so not require to be trasposed when passing.<br/>
    /// When i use a common transformation affine matrix SRT i mean that first Scale, second Rotate, last Traslate
    /// and is calculated as T*R*S, notice that text order is inverted.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Matrix4x4f : IEquatable<Matrix4x4f>
    {
        #region Field
        [FieldOffset(0)]
        public float m00;
        [FieldOffset(4)]
        public float m01;
        [FieldOffset(8)]
        public float m02;
        [FieldOffset(12)]
        public float m03;

        [FieldOffset(16)]
        public float m10;
        [FieldOffset(20)]
        public float m11;
        [FieldOffset(24)]
        public float m12;
        [FieldOffset(28)]
        public float m13;

        [FieldOffset(32)]
        public float m20;
        [FieldOffset(36)]
        public float m21;
        [FieldOffset(40)]
        public float m22;
        [FieldOffset(44)]
        public float m23;

        [FieldOffset(48)]
        public float m30;
        [FieldOffset(52)]
        public float m31;
        [FieldOffset(56)]
        public float m32;
        [FieldOffset(60)]
        public float m33;

        [FieldOffset(0)]
        public unsafe fixed float field[16];
        #endregion

        public static readonly Matrix4x4f Identity = new Matrix4x4f(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        public static readonly Matrix4x4f Sequence = new Matrix4x4f(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        public bool IsIdentity => Equals(Identity);

        public unsafe Matrix4x4f(BinaryReader reader) : this()
        {
            for (int i = 0; i < 16; i++) field[i] = reader.ReadSingle();
        }

        public Matrix4x4f(float[,] m) : this(
            m[0, 0], m[0, 1], m[0, 2], m[0, 3],
            m[1, 0], m[1, 1], m[1, 2], m[1, 3],
            m[2, 0], m[2, 1], m[2, 2], m[2, 3],
            m[3, 0], m[3, 1], m[3, 2], m[3, 3])
        { }

        public Matrix4x4f(float[] m) : this(
            m[0], m[1], m[2], m[3],
            m[4], m[5], m[6], m[7],
            m[8], m[9], m[10], m[11],
            m[12], m[13], m[14], m[15])
        { }

        public Matrix4x4f(Vector3f X, Vector3f Y, Vector3f Z) :
            this(X.x, Y.x, Z.x, 0,
                 X.y, Y.y, Z.y, 0,
                 X.z, Y.z, Z.z, 0,
                 0, 0, 0, 1)
        { }

        public Matrix4x4f(
            double m00, double m01, double m02, double m03,
            double m10, double m11, double m12, double m13,
            double m20, double m21, double m22, double m23,
            double m30, double m31, double m32, double m33)
            : this(
            (float)m00, (float)m01, (float)m02, (float)m03,
            (float)m10, (float)m11, (float)m12, (float)m13,
            (float)m20, (float)m21, (float)m22, (float)m23,
            (float)m30, (float)m31, (float)m32, (float)m33)
        { }

        public Matrix4x4f(
        float m00, float m01, float m02, float m03,
        float m10, float m11, float m12, float m13,
        float m20, float m21, float m22, float m23,
        float m30, float m31, float m32, float m33)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
        }

        public static unsafe float[,] Convert(in Matrix4x4f m)
        {
            float[,] tmp = new float[4, 4];
            fixed (float* src = m.field)
            fixed (float* dst = tmp)
                Buffer.MemoryCopy(src, dst, 64, 64);
            return tmp;
        }

        public static unsafe Matrix4x4f Convert(in float[,] m)
        {
            Matrix4x4f matrix = default;
            fixed (float* src = m)
                Buffer.MemoryCopy(src, matrix.field, 64, 64);
            return matrix;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(m00); bw.Write(m01); bw.Write(m02); bw.Write(m03);
            bw.Write(m10); bw.Write(m11); bw.Write(m12); bw.Write(m13);
            bw.Write(m20); bw.Write(m21); bw.Write(m22); bw.Write(m23);
            bw.Write(m30); bw.Write(m31); bw.Write(m32); bw.Write(m33);
        }

        public static bool TryParse(string toparse, out Matrix4x4f value) => MathParsers.TryParse(toparse, out value);

        /// <summary>
        /// Get the i-th Row
        /// </summary>
        public Vector4f getRow(int i)
        {
            switch (i)
            {
                case 0: return new Vector4f(m00, m01, m02, m03);
                case 1: return new Vector4f(m10, m11, m12, m13);
                case 2: return new Vector4f(m20, m21, m22, m23);
                case 3: return new Vector4f(m30, m31, m32, m33);
                default: throw new IndexOutOfRangeException("index not in the range 0-3");
            }
        }
        /// <summary>
        /// Set the i-th Row
        /// </summary>
        public void setRow(int i, float x, float y, float z, float w)
        {
            switch (i)
            {
                case 0: m00 = x; m01 = y; m02 = z; m03 = w; break;
                case 1: m10 = x; m11 = y; m12 = z; m13 = w; break;
                case 2: m20 = x; m21 = y; m22 = z; m23 = w; break;
                case 3: m30 = x; m31 = y; m32 = z; m33 = w; break;
                default: throw new IndexOutOfRangeException("index not in the range 0-3");
            }
        }
        /// <summary>
        /// Get the i-th Column
        /// </summary>
        public Vector4f getCol(int i)
        {
            switch (i)
            {
                case 0: return new Vector4f(m00, m10, m20, m30);
                case 1: return new Vector4f(m01, m11, m21, m31);
                case 2: return new Vector4f(m02, m12, m22, m32);
                case 3: return new Vector4f(m03, m13, m23, m33);
                default: throw new IndexOutOfRangeException("index not in the range 0-3");
            }
        }
        /// <summary>
        /// Set the i-th Column
        /// </summary>
        public void setCol(int i, Vector4f val)
        {
            switch (i)
            {
                case 0: m00 = val.x; m10 = val.y; m20 = val.z; m30 = val.w; break;
                case 1: m01 = val.x; m11 = val.y; m21 = val.z; m31 = val.w; break;
                case 2: m02 = val.x; m12 = val.y; m22 = val.z; m32 = val.w; break;
                case 3: m03 = val.x; m13 = val.y; m23 = val.z; m33 = val.w; break;
                default: throw new IndexOutOfRangeException("index not in the range 0-3");
            }
        }


        /// <summary>
        /// Allows the Matrix to be accessed like a 2d array (i.e. matrix[2,3]). The Unsafe version is little slower...
        /// </summary>
        /// <remarks>
        /// This indexer is only provided as a convenience, and is <b>not</b> recommended for use in intensive applications.  
        /// </remarks>
        public unsafe float this[int row, int col]
        {
            get => this[4 * row + col];
            set => this[4 * row + col] = value;
        }

        /// <summary>
        ///	Allows the Matrix to be accessed linearly (m[0] -> m[15]).  
        /// </summary>
        /// <remarks>
        /// This indexer is only provided as a convenience, and is <b>not</b> recommended for use in intensive applications.  
        /// </remarks>
        public unsafe float this[int index]
        {
            get { fixed (float* ptr_m = &m00) return *(ptr_m + index); }
            set { fixed (float* ptr_m = &m00) { *(ptr_m + index) = value; } }
        }

        /// <summary>
        /// Used to generate the adjoint of this matrix.
        /// </summary>
        Matrix4x4f Adjoint()
        {
            // note: this is an expanded version of the Ogre adjoint() method, to give better performance in C#. Generated using a script
            float i00 = m11 * (m22 * m33 - m32 * m23) -
                        m12 * (m21 * m33 - m31 * m23) +
                        m13 * (m21 * m32 - m31 * m22);

            float i01 = -(m01 * (m22 * m33 - m32 * m23) -
                          m02 * (m21 * m33 - m31 * m23) +
                          m03 * (m21 * m32 - m31 * m22));

            float i02 = m01 * (m12 * m33 - m32 * m13) -
                          m02 * (m11 * m33 - m31 * m13) +
                          m03 * (m11 * m32 - m31 * m12);

            float i03 = -(m01 * (m12 * m23 - m22 * m13) -
                           m02 * (m11 * m23 - m21 * m13) +
                           m03 * (m11 * m22 - m21 * m12));

            float i10 = -(m10 * (m22 * m33 - m32 * m23) -
                           m12 * (m20 * m33 - m30 * m23) +
                           m13 * (m20 * m32 - m30 * m22));

            float i11 = m00 * (m22 * m33 - m32 * m23) -
                          m02 * (m20 * m33 - m30 * m23) +
                          m03 * (m20 * m32 - m30 * m22);

            float val6 = -(m00 * (m12 * m33 - m32 * m13) -
                            m02 * (m10 * m33 - m30 * m13) +
                            m03 * (m10 * m32 - m30 * m12));

            float val7 = m00 * (m12 * m23 - m22 * m13) -
                          m02 * (m10 * m23 - m20 * m13) +
                          m03 * (m10 * m22 - m20 * m12);

            float val8 = m10 * (m21 * m33 - m31 * m23) -
                          m11 * (m20 * m33 - m30 * m23) +
                          m13 * (m20 * m31 - m30 * m21);

            float val9 = -(m00 * (m21 * m33 - m31 * m23) -
                            m01 * (m20 * m33 - m30 * m23) +
                            m03 * (m20 * m31 - m30 * m21));

            float val10 = m00 * (m11 * m33 - m31 * m13) -
                           m01 * (m10 * m33 - m30 * m13) +
                          m03 * (m10 * m31 - m30 * m11);

            float val11 = -(m00 * (m11 * m23 - m21 * m13) -
                             m01 * (m10 * m23 - m20 * m13) +
                             m03 * (m10 * m21 - m20 * m11));

            float val12 = -(m10 * (m21 * m32 - m31 * m22) -
                             m11 * (m20 * m32 - m30 * m22) +
                             m12 * (m20 * m31 - m30 * m21));

            float val13 = m00 * (m21 * m32 - m31 * m22) -
                           m01 * (m20 * m32 - m30 * m22) +
                           m02 * (m20 * m31 - m30 * m21);

            float val14 = -(m00 * (m11 * m32 - m31 * m12) -
                             m01 * (m10 * m32 - m30 * m12) +
                             m02 * (m10 * m31 - m30 * m11));

            float val15 = m00 * (m11 * m22 - m21 * m12) -
                           m01 * (m10 * m22 - m20 * m12) +
                           m02 * (m10 * m21 - m20 * m11);

            return new Matrix4x4f(i00, i01, i02, i03, i10, i11, val6, val7, val8, val9, val10, val11, val12, val13, val14, val15);
        }

        /// <summary>
        ///	 Get or Set the translation value of the matrix using math notation
        ///	 <code>
        ///	┌ 0 0 0 Tx ┐
        ///	│ 0 0 0 Ty │
        ///	│ 0 0 0 Tz │
        ///	└ 0 0 0 1  ┘
        ///	</code>
        /// </summary>
        public Vector3f Position
        {
            get => new Vector3f(m03, m13, m23);
            set { m03 = value.x; m13 = value.y; m23 = value.z; }
        }


        /// <summary>
        ///  Get or Set the scale value of the matrix. Not match when matrix is rotated
        ///	 <code>
        ///	┌ Sx 0  0  0 ┐
        ///	│ 0  Sy 0  0 │
        ///	│ 0  0  Sz 0 │
        ///	└ 0  0  0  1 ┘
        ///</code>
        /// </summary>
        public Vector3f DiagonalComponent
        {
            get => new Vector3f(m00, m11, m22);
            set { m00 = value.x; m11 = value.y; m22 = value.z; }
        }
        /// <summary>
        /// Get the determinant of matrix.
        /// </summary>
        public float Determinant =>
            m00 *
            (m11 * (m22 * m33 - m32 * m23) -
            m12 * (m21 * m33 - m31 * m23) +
            m13 * (m21 * m32 - m31 * m22)) -
            m01 *
            (m10 * (m22 * m33 - m32 * m23) -
            m12 * (m20 * m33 - m30 * m23) +
            m13 * (m20 * m32 - m30 * m22)) +
            m02 *
            (m10 * (m21 * m33 - m31 * m23) -
            m11 * (m20 * m33 - m30 * m23) +
            m13 * (m20 * m31 - m30 * m21)) -
            m03 *
            (m10 * (m21 * m32 - m31 * m22) -
            m11 * (m20 * m32 - m30 * m22) +
            m12 * (m20 * m31 - m30 * m21));

        /// <summary>
        /// Extract the 3x3 matrix representing the current rotation. 
        /// </summary>
        public Matrix3x3f GetRotation()
        {
            var rotation = (Matrix3x3f)this;
            Vector3f.Normalize(ref rotation.m00, ref rotation.m10, ref rotation.m20);
            Vector3f.Normalize(ref rotation.m01, ref rotation.m11, ref rotation.m21);
            Vector3f.Normalize(ref rotation.m02, ref rotation.m12, ref rotation.m22);
            Vector3f.Normalize(ref rotation.m00, ref rotation.m10, ref rotation.m20);
            return rotation;
        }
        /// <summary>
        /// Extract scaling information.
        /// </summary>
        public Vector3f GetScale()
        {
            var scale = new Vector3f(1, 1, 1);
            var axis = new Vector3f(0, 0, 0);

            axis.x = m00;
            axis.y = m10;
            axis.z = m20;
            scale.x = axis.Length;

            axis.x = m01;
            axis.y = m11;
            axis.z = m21;
            scale.y = axis.Length;

            axis.x = m02;
            axis.y = m12;
            axis.z = m22;
            scale.z = axis.Length;

            return scale;
        }

        /// <summary>
        /// http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
        /// </summary>
        public Quaternion4f GetQuaternion()
        {
            Quaternion4f q = new Quaternion4f();
            float trace = m00 + m11 + m22;

            if (trace > 0)
            {
                float s = 0.5f / (float)Math.Sqrt(trace + 1);
                q.w = 0.25f / s;
                q.x = (m21 - m12) * s;
                q.y = (m02 - m20) * s;
                q.z = (m10 - m01) * s;
            }
            else
            {
                if (m00 > m11 && m00 > m22)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + m00 - m11 - m22);
                    q.w = (m21 - m12) / s;
                    q.x = 0.25f * s;
                    q.y = (m01 + m10) / s;
                    q.z = (m02 + m20) / s;
                }
                else if (m11 > m22)
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + m11 - m00 - m22);
                    q.w = (m02 - m20) / s;
                    q.x = (m01 + m10) / s;
                    q.y = 0.25f * s;
                    q.z = (m12 + m21) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + m22 - m00 - m11);
                    q.w = (m10 - m01) / s;
                    q.x = (m02 + m20) / s;
                    q.y = (m12 + m21) / s;
                    q.z = 0.25f * s;
                }
            }
            return q;
        }


        /// <summary>
        /// Create the T*R*S matrix. First scale, second rotate and at last traslate
        /// </summary>
        public static Matrix4x4f ComposeTRS(Vector3f t, Quaternion4f r, Vector3f s) => ComposeTRS(
            t.x, t.y, t.z,
            r.x, r.y, r.z, r.w,
            s.x, s.y, s.z);

        public static Matrix4x4f ComposeTRS(float tx, float ty, float tz, float qx, float qy, float qz, float qw, float sx, float sy, float sz) =>
            Scaling(sx, sy, sz) *
            Rotating(qx, qy, qz, qw) *
            Translating(tx, ty, tz);


        /// <summary>
        /// Decompose the matrix.
        /// </summary>
        public void Decompose(out Vector3f translation, out Quaternion4f orientation, out Vector3f scale)
        {
            scale = new Vector3f(1, 1, 1);
            var rotation = Matrix3x3f.Identity;
            var axis = Vector3f.Zero;

            axis.x = m00;
            axis.y = m10;
            axis.z = m20;
            scale.x = axis.Normalize(); // Normalize() returns the vector's length before it was normalized
            rotation.m00 = axis.x;
            rotation.m10 = axis.y;
            rotation.m20 = axis.z;

            axis.x = m01;
            axis.y = m11;
            axis.z = m21;
            scale.y = axis.Normalize();
            rotation.m01 = axis.x;
            rotation.m11 = axis.y;
            rotation.m21 = axis.z;

            axis.x = m02;
            axis.y = m12;
            axis.z = m22;
            scale.z = axis.Normalize();
            rotation.m02 = axis.x;
            rotation.m12 = axis.y;
            rotation.m22 = axis.z;

            /* http://www.robertblum.com/articles/2005/02/14/decomposing-matrices check to support transforms with negative scaling */
            //thanks sebj for the info
            if (rotation.Determinant < 0)
            {
                rotation.m00 = -rotation.m00;
                rotation.m10 = -rotation.m10;
                rotation.m20 = -rotation.m20;
                scale.x = -scale.x;
            }

            orientation = Quaternion4f.FromRotationMatrix(rotation);
            translation = Position;
        }


        #region Operators
        public static implicit operator Matrix3x3f(Matrix4x4f matrix)
            => new Matrix3x3f(
                matrix.m00, matrix.m01, matrix.m02,
                matrix.m10, matrix.m11, matrix.m12,
                matrix.m20, matrix.m21, matrix.m22);


        /// <summary>
        /// <code>
        /// M[r,c] = Σ A[r,i] * B[i,c]
        /// </code>  
        /// </summary>
        /// <remarks> (A*B)^t = B^t*A^t </remarks>
        public static Matrix4x4f operator *(Matrix4x4f left, Matrix4x4f right)
        {
            left.Multiply(in right);
            return left;
        }

        // function for matrix * vector multiplication
        //
        // | a b c d |   |vx|
        // | 0 0 0 0 |   |vy|
        // | 0 0 0 0 | * |vz| = | x y z w |
        // | 0 0 0 0 |   |vw|
        //
        //
        // function for vector^T * Matrix multiplication
        //
        //                 | a 0 0 0 |    |x|
        //                 | b 0 0 0 |    |y|
        // |vx vy vz vw| * | c 0 0 0 |  = |z| 
        //                 | d 0 0 0 |    |w|


        /// <summary>
        /// <inheritdoc cref="Vector3f.TransformCoordinate(Matrix4x4f)"/><br/>
        ///	Transforms the given 3-D vector by the matrix, projecting the 
        ///	result back into <i>w</i> = 1.
        ///	<p/>
        ///	This means that the initial <i>w</i> is considered to be 1.0,
        ///	and then all the tree elements of the resulting 3-D vector are
        ///	divided by the resulting <i>w</i>.
        /// </summary>
        /// <remarks>
        /// ABij = Σk(Aik,Bkj)
        /// </remarks>
        public static Vector3f operator *(Matrix4x4f matrix, Vector3f right)
        {
            right.TransformCoordinate(matrix);
            return right;
        }

        /// <summary>
        /// Never used a pre-multiply
        /// </summary>
        public static Vector3f operator *(Vector3f left, Matrix4x4f matrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///	Used to multiply a Matrix4 object by a scalar value..
        /// </summary>
        public static Matrix4x4f operator *(Matrix4x4f left, float scalar)
        {
            // left is passed as value
            left.Multiply(scalar);
            return left;
        }
        public static Matrix4x4f operator +(Matrix4x4f left, Matrix4x4f right)
        {
            left.m00 += right.m00;
            left.m01 += right.m01;
            left.m02 += right.m02;
            left.m03 += right.m03;

            left.m10 += right.m10;
            left.m11 += right.m11;
            left.m12 += right.m12;
            left.m13 += right.m13;

            left.m20 += right.m20;
            left.m21 += right.m21;
            left.m22 += right.m22;
            left.m23 += right.m23;

            left.m30 += right.m30;
            left.m31 += right.m31;
            left.m32 += right.m32;
            left.m33 += right.m33;

            return left;
        }
        public static Matrix4x4f operator -(Matrix4x4f left, Matrix4x4f right)
        {
            left.m00 -= right.m00;
            left.m01 -= right.m01;
            left.m02 -= right.m02;
            left.m03 -= right.m03;

            left.m10 -= right.m10;
            left.m11 -= right.m11;
            left.m12 -= right.m12;
            left.m13 -= right.m13;

            left.m20 -= right.m20;
            left.m21 -= right.m21;
            left.m22 -= right.m22;
            left.m23 -= right.m23;

            left.m30 -= right.m30;
            left.m31 -= right.m31;
            left.m32 -= right.m32;
            left.m33 -= right.m33;

            return left;
        }

        public static bool AlmostEqual(ref Matrix4x4f left, ref Matrix4x4f right, int maxdeltabits = 2)
        {
            return Mathelp.AlmostEqual(left.m00, right.m00, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m01, right.m01, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m02, right.m02, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m03, right.m03, maxdeltabits) &&

                    Mathelp.AlmostEqual(left.m10, right.m10, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m11, right.m11, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m12, right.m12, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m13, right.m13, maxdeltabits) &&

                    Mathelp.AlmostEqual(left.m20, right.m20, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m21, right.m21, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m22, right.m22, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m23, right.m23, maxdeltabits) &&

                    Mathelp.AlmostEqual(left.m30, right.m30, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m31, right.m31, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m32, right.m32, maxdeltabits) &&
                    Mathelp.AlmostEqual(left.m33, right.m33, maxdeltabits);
        }

        public static bool operator ==(Matrix4x4f left, Matrix4x4f right) => left.Equals(in right);
        public static bool operator !=(Matrix4x4f left, Matrix4x4f right) => !left.Equals(in right);

        #endregion

        #region Static Constructors

        #region NOT TESTED

        /// <summary>
        /// Semplified inverse for projection matrix
        /// </summary>
        public static Matrix4x4f InverseProjection(Matrix4x4f proj)
        {
            //traslate position -p.r -p.u -p.l
            float m30 = -proj.m03 * proj.m00 - proj.m13 * proj.m10 - proj.m23 * proj.m20;
            float m31 = -proj.m03 * proj.m01 - proj.m13 * proj.m11 - proj.m23 * proj.m21;
            float m32 = -proj.m03 * proj.m02 - proj.m13 * proj.m12 - proj.m23 * proj.m22;

            proj.Traspose();

            // fix col4
            proj.m03 = proj.m13 = proj.m23 = 0; proj.m33 = 1;

            proj.m30 = m30;
            proj.m31 = m31;
            proj.m32 = m32;

            return proj;
        }


        #endregion

        /// <summary>
        /// Make a Left-Handle orthogonal projection matrix.
        /// The left right top bottom value are the corner in world coordinate of the AABB build with
        /// Identity View Matrix ( eye=Zero, look=unitZ  up=unitY) 
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb205347(v=vs.85).aspx
        /// </summary>
        public static Matrix4x4f MakeOrthoLH(float near, float far, float left, float right, float top, float bottom)
        {
            // 2/(r-l)      0            0         0
            // 0            2/(t-b)      0         0
            // 0            0            1/(f-n)   0
            // (l+r)/(l-r)  (t+b)/(b-t)  n/(n-f)   1

            float w = right - left;
            float h = top - bottom;

            float A = 2 / w;
            float B = 2 / h;
            float C = 1.0f / (far - near);
            float D = near < 1e-16 ? -C : -C * near;
            float E = -(left + right) / w;
            float F = -(top + bottom) / h;

            // trasposed
            return new Matrix4x4f(
                A, 0, 0, E,
                0, B, 0, F,
                0, 0, C, D,
                0, 0, 0, 1);
        }


        /// <summary>
        /// Make a Left-Handle orthogonal projection matrix.
        /// The width and height size isn't the viewport value but camera volume size.
        /// For a camera implementation you have to set at the beginning the relation of
        /// Viewport_Width and World_Width of default zoom level (100%)
        /// 
        /// https://msdn.microsoft.com/en-us/library/bb205346(v=vs.85).aspx
        /// </summary>
        /// <param name="volumeheight">sugestion : volumewidth * screen_aspect_ratio</param>
        /// <param name="volumewidth">sugestion : volumeheight / screen_aspect_ratio</param>
        public static Matrix4x4f MakeOrthoLH(float near, float far, float volumewidth, float volumeheight)
        {
            // 2/w  0    0        0
            // 0    2/h  0        0
            // 0    0    1/(f-n)  0
            // 0    0   -n/(f-n)  1

            float A = 2 / volumewidth;
            float B = 2 / volumeheight;
            float C = 1.0f / (far - near);
            float D = near < 1e-16 ? -C : -C * near;

            // trasposed
            return new Matrix4x4f(
                A, 0, 0, 0,
                0, B, 0, 0,
                0, 0, C, D,
                0, 0, 0, 1);
        }

        /// <summary>
        /// NOT TESTED
        /// Make a Left-Handle prospective projection matrix with Frustum Coorner
        /// The volume of Frustum is define by rectangle :
        /// Min(Left,Bottom,Near) Max(Right,Top,Near) and Eye of view at (0,0,0) where near = 0
        /// 
        /// https://msdn.microsoft.com/en-us/library/bb205353(v=vs.85).aspx
        /// </summary>
        public static Matrix4x4f MakeProjectionLH(float near, float far, float left, float right, float top, float bottom)
        {
            // 2*n/(r-l)    0            0          0
            // 0            2*n/(t-b)    0          0
            // (l+r)/(l-r)  (t+b)/(b-t)  f/(f-n)    1
            // 0            0            n*f/(n-f)  0

            float w = right - left;
            float h = top - bottom;
            float A = 2 * near / w;
            float B = 2 * near / h;
            float C = far / (far - near);
            float D = near < 1e-16 ? -C : -C * near;
            float E = -(left + right) / w;
            float F = -(top + bottom) / h;

            // trasposed
            return new Matrix4x4f(
                A, 0, E, 0,
                0, B, F, 0,
                0, 0, C, D,
                0, 0, 1, 0);
        }

        /// <summary>
        /// Make a Left-Handle prospective projection matrix with FOVY , FovX derived from aspect ratio
        /// https://msdn.microsoft.com/en-us/library/bb205350(v=VS.85).aspx
        /// </summary>
        public static Matrix4x4f MakeProjectionLHAFovY(float near, float far, float aspectratio, float fovy = Mathelp.Rad45)
        {
            // xScale     0          0           0
            // 0        yScale       0           0
            // 0          0       f/(f-n)        1
            // 0          0       -n*f/(f-n)     0

            float B = (float)(1 / Math.Tan(fovy / 2));
            // by default Width size have the prioirity than Height so xscale
            float A = B / aspectratio;
            // far positive infinite
            float C = far > 1e16 ? 1 : far / (far - near);
            // near zero ???
            float D = -near * C;

            // notice the D and 1 are trasposed respect standard directx because i use colum_major
            return new Matrix4x4f(
                A, 0, 0, 0,
                0, B, 0, 0,
                0, 0, C, D,
                0, 0, 1, 0);
        }
        /// <summary>
        /// Make a Left-Handle prospective projection matrix with FOVX and FOVY.
        /// FovX override aspect ratio fix
        /// </summary>
        public static Matrix4x4f MakeProjectionLHFovXY(float near, float far, float fovy = Mathelp.Rad45, float fovx = Mathelp.Rad45)
        {
            float A = (float)(1.0 / Math.Tan(fovx * 0.5));
            float B = (float)(1.0 / Math.Tan(fovy * 0.5));

            // far positive infinite
            float C = far > 1e16 ? 1 : far / (far - near);

            // near zero ???
            float D = near < 1e-16 ? -1 : -C * near;

            // notice the D and 1 are trasposed respect standard directx because i use colum_major
            return new Matrix4x4f(
                A, 0, 0, 0,
                0, B, 0, 0,
                0, 0, C, D,
                0, 0, 1, 0);
        }

        /// <summary>
        /// Make a Left-Handle view matrix.
        /// <code>
        ///  rx  ry  rz  -r.e 
        ///  ux  uy  uz  -u.e 
        ///  dx  dy  dz  -d.e 
        ///  0   0   0    1  
        /// </code>
        /// </summary>
        /// <remarks>
        /// The "up" vector by default is "Y" but will be ortogonalized in the matrix so the "up" component will be different.
        /// Carefull when eye-target axes is vertical, the up vector can't be "Y" otherwise the cross product return a random rotation
        /// where eye direction is +Z
        /// </remarks>
        public static Matrix4x4f MakeViewLH(Vector3f eye, Vector3f target, Vector3f up)
        {
            // you pass a zero vector
            if (up.Normalize() < 1e-6) up = Vector3f.UnitY;

            // targhet and eye have same coordinates
            Vector3f d = target - eye;
            if (d.Normalize() < 1e-6) d = Vector3f.UnitZ;

            // z vector is parallel to up vector , the cross == (0,0,0)
            Vector3f r = Vector3f.Cross(up, d);
            if (r.Normalize() < 1e-6) r = Vector3f.UnitX;

            // y vector is parallel to x vector, the cross == (0,0,0)
            Vector3f u = Vector3f.Cross(d, r);
            if (u.Normalize() < 1e-6) u = Vector3f.UnitY;

            return new Matrix4x4f(
                r.x, r.y, r.z, -Vector3f.Dot(r, eye),
                u.x, u.y, u.z, -Vector3f.Dot(u, eye),
                d.x, d.y, d.z, -Vector3f.Dot(d, eye),
                0, 0, 0, 1);
        }
        /// <summary><inheritdoc cref="MakeViewLH(Vector3f, Vector3f, Vector3f)"/></summary>
        public static Matrix4x4f MakeViewLH(Vector3f eye, Vector3f target) => MakeViewLH(eye, target, Vector3f.UnitY);

        public static Matrix4x4f Rotating(Quaternion4f quat) => Rotating(quat.x, quat.y, quat.z, quat.w);
        /// <summary>
        /// from Quaternion
        /// </summary>
        public static Matrix4x4f Rotating(float x, float y, float z, float w)
        {
            Matrix4x4f m = new Matrix4x4f();

            float xx = x * x;
            float xy = x * y;
            float xz = x * z;
            float xw = x * w;

            float yy = y * y;
            float yz = y * z;
            float yw = y * w;

            float zz = z * z;
            float zw = z * w;

            m.m00 = 1 - 2 * (yy + zz);
            m.m01 = 2 * (xy - zw);
            m.m02 = 2 * (xz + yw);

            m.m10 = 2 * (xy + zw);
            m.m11 = 1 - 2 * (xx + zz);
            m.m12 = 2 * (yz - xw);

            m.m20 = 2 * (xz - yw);
            m.m21 = 2 * (yz + xw);
            m.m22 = 1 - 2 * (xx + yy);

            m.m03 = m.m13 = m.m23 = m.m30 = m.m31 = m.m32 = 0;
            m.m33 = 1;

            return m;
        }
        public static Matrix4x4f RotateAxis(Vector3f vect, float angle) => RotateAxis(vect.x, vect.y, vect.z, angle);
        public static Matrix4x4f RotateAxis(in Vector3f vect, float angle) => RotateAxis(vect.x, vect.y, vect.z, angle);


        /// <summary>
        /// Left-Hand coordinate : if axe direction is your thumb, other fingers indicate a positive angle direction 
        /// Vector must be normalized
        /// </summary>
        /// <remarks>for a Right-Hand rule, do a traspose</remarks>
        public static Matrix4x4f RotateAxis(float x, float y, float z, float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            float t = 1 - c;

            return new Matrix4x4f(
                c + x * x * t, x * y * t - z * s, x * z * t + y * s, 0,
                y * x * t + z * s, c + y * y * t, y * z * t - x * s, 0,
                z * x * t - y * s, z * y * t + x * s, c + z * z * t, 0,
                0, 0, 0, 1);
        }
        public static Matrix4x4f RotationX(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            //  0  0  0  0
            //  0  c -s  0
            //  0  s  c  0
            //  0  0  0  1
            Matrix4x4f matrix = Identity;
            matrix.m11 = cos;
            matrix.m12 = -sin;
            matrix.m21 = sin;
            matrix.m22 = cos;
            return matrix;
        }
        public static Matrix4x4f RotationY(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            //  c  0  s  0
            //  0  0  0  0
            // -s  0  c  0
            //  0  0  0  1

            Matrix4x4f matrix = Identity;
            matrix.m00 = cos;
            matrix.m02 = sin;
            matrix.m20 = -sin;
            matrix.m22 = cos;
            return matrix;
        }
        public static Matrix4x4f RotationZ(float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            //  c -s  0  0
            //  s  c  0  0
            //  0  0  0  0
            //  0  0  0  1
            Matrix4x4f matrix = Identity;
            matrix.m00 = cos;
            matrix.m01 = -sin;
            matrix.m10 = sin;
            matrix.m11 = cos;
            return matrix;
        }
        public static Matrix4x4f Scaling(Vector3f scale) => Scaling(scale.x, scale.y, scale.z);
        public static Matrix4x4f Scaling(in Vector3f scale) => Scaling(scale.x, scale.y, scale.z);
        public static Matrix4x4f Scaling(float x, float y, float z)
        {
            Matrix4x4f matrix = Identity;
            matrix.m00 = x;
            matrix.m11 = y;
            matrix.m22 = z;
            return matrix;
        }

        public static Matrix4x4f Shearing(float xy, float xz, float yx, float yz, float zx, float zy)
        {
            throw new NotImplementedException();
            /*
            Matrix4x4f matrix = Identity;
            matrix.m01 = xy;
            matrix.m02 = xz;
            matrix.m10 = yx;
            matrix.m12 = yz;
            matrix.m20 = zx;
            matrix.m21 = zy;
            return matrix;
            */
        }

        public void PreTranslating(float x, float y, float z)
        {
            m03 += m00 * x + m01 * y + m02 * z;
            m13 += m10 * x + m11 * y + m12 * z;
            m23 += m20 * x + m21 * y + m22 * z;
            m33 += m30 * x + m31 * y + m32 * z;
        }

        public static Matrix4x4f Translating(Vector3f vector) => Translating(ref vector);

        public static Matrix4x4f Translating(ref Vector3f vector)
        {
            Matrix4x4f matrix = Matrix4x4f.Identity;
            matrix.m03 = vector.x;
            matrix.m13 = vector.y;
            matrix.m23 = vector.z;
            return matrix;
        }
        public static Matrix4x4f Translating(float x, float y, float z)
        {
            Matrix4x4f matrix = Matrix4x4f.Identity;
            matrix.m03 = x;
            matrix.m13 = y;
            matrix.m23 = z;
            return matrix;
        }


        public static Matrix4x4f Orienting(ref Vector3f axisY)
        {
            Matrix4x4f matrix = Identity;

            Vector3f row2 = Vector3f.Cross(Vector3f.UnitX, axisY);

            if (row2.LengthSq < 1e-6f) row2 = Vector3f.UnitZ;

            Vector3f row0 = Vector3f.Cross(axisY, row2);
            row2 = Vector3f.Cross(row0, axisY);


            matrix.setCol(0, new Vector4f(row0, 0));
            matrix.setCol(1, new Vector4f(axisY, 0));
            matrix.setCol(2, new Vector4f(row2, 0));

            return matrix;
        }
        public static Matrix4x4f Traspose(Matrix4x4f matrix)
        {
            matrix.Traspose();
            return matrix;
        }

        /// <summary>
        /// Invert the matrix. <see cref="ArithmeticException"/> if can't be inverted.
        /// </summary>
        /// <exception cref="ArithmeticException"></exception>
        public void Invert()
        {
            float d = Determinant;

            if (d > -float.Epsilon && d < float.Epsilon)
            {
                throw new ArithmeticException("matrix can't be inverted");
            }
            else
            {
                this = Adjoint();
                Multiply(1f / d);
            }
        }
        /// <summary>
        ///  Returns an inverted matrix, if not generate Exception
        /// </summary>
        public Matrix4x4f Inverse() => Inverse(this);

        /// <summary>
        /// Returns a new inverted matrix. <see cref="ArithmeticException"/> if can't be inverted.
        /// </summary>
        /// <exception cref="ArithmeticException"></exception>
        public static Matrix4x4f Inverse(Matrix4x4f mat)
        {
            float det = mat.Determinant;
            if (det > -float.Epsilon && det < float.Epsilon)
#if DEBUG
                throw new ArithmeticException("matrix can't be inverted");
#else
            det = 0;
#endif
            else det = 1f / det;

            mat = mat.Adjoint();
            mat.Multiply(det);
            return mat;
        }

        /// <summary>
        /// Inverse of affine transformation (only traslation)
        /// </summary>
        /// <remarks>
        /// If the affine trasformation is T , the inverse T^-1 = (-T)
        /// </remarks>
        public static void InvertTraslation(ref Matrix4x4f T)
        {
            T.m03 *= -1;
            T.m13 *= -1;
            T.m23 *= -1;
        }
        /// <summary>
        /// Inverse of affine transformation (only rotation)
        /// </summary>
        /// <remarks>
        /// If the affine trasformation is R , the inverse R^-1 = R^t
        /// </remarks>
        public static void InvertRotation(ref Matrix4x4f R)
        {
            Mathelp.SWAP(ref R.m01, ref R.m10);
            Mathelp.SWAP(ref R.m02, ref R.m20);
            Mathelp.SWAP(ref R.m12, ref R.m21);
        }


        public Matrix4x4f InvertProjection()
        {
            Matrix4x4f invproj = this;

            invproj.m00 = 1 / invproj.m00;
            invproj.m11 = 1 / invproj.m11;



            return invproj;
        }


        /// <summary>
        /// Inverse of affine transformation where RT = first rotation then traslation = T * R.
        /// example for view matrix
        /// </summary>
        /// <remarks>
        /// If the affine trasformation is T*R , the inverse (T*R)^-1 = R^-1 * T^-1 = R^t * (-T)
        /// is X6 faster than calculate inverse
        /// </remarks>
        public static void InvertRotationTraslation(ref Matrix4x4f RT)
        {
            // | a  b  c  x |     | a  d  g  -(ax+dy+gz) |
            // | d  e  f  y | ->  | b  e  h  -(bx+ey+hz) |
            // | g  h  i  z |     | c  f  i  -(cx+fy+iz) |
            // | 0  0  0  1 |     | 0  0  0       1      |

            InvertRotation(ref RT);

            float x = RT.m03;
            float y = RT.m13;
            float z = RT.m23;

            RT.m03 = -RT.m00 * x - RT.m01 * y - RT.m02 * z;
            RT.m13 = -RT.m10 * x - RT.m11 * y - RT.m12 * z;
            RT.m23 = -RT.m20 * x - RT.m21 * y - RT.m22 * z;
        }

        public static Matrix4x4f InvertAffineTransform(Matrix4x4f SRT)
        {
            InvertAffineTransform(ref SRT);
            return SRT;
        }
        /// <summary>
        /// Inverse of affine transformation where SRT = first scale , second rotation then traslation = T * R * S
        /// example for model matrix. Not performance difference with SR transformation then can be used also for T=0
        /// </summary>
        /// <remarks>
        /// If the affine trasformation is T*R*S , the inverse (T*R*S)^-1 = (1/S) * (R^t) * (-T) 
        /// is X2 faster than calculate inverse
        /// </remarks>
        public static void InvertAffineTransform(ref Matrix4x4f SRT)
        {
            // | aSx bSy  cSz  x |     | a/sx  d/sx  g/sx  -(ax+dy+gz)/sx |
            // | dSx eSy  fSz  y | ->  | b/sy  e/sy  h/sy  -(bx+ey+hz)/sy |
            // | gSx hSy  iSz  z |     | c/sz  f/sz  i/sz  -(cx+fy+iz)/sz |
            // | 0    0   0    1 |     |   0     0     0           1      |


            // if is orthogonal then a^2 + d^2 + g^2 = 1 (valid for both row and colum of Rotation matrix)
            // need to extract scale component
            float sx = (float)Math.Sqrt(SRT.m00 * SRT.m00 + SRT.m10 * SRT.m10 + SRT.m20 * SRT.m20);
            float sy = (float)Math.Sqrt(SRT.m01 * SRT.m01 + SRT.m11 * SRT.m11 + SRT.m21 * SRT.m21);
            float sz = (float)Math.Sqrt(SRT.m02 * SRT.m02 + SRT.m12 * SRT.m12 + SRT.m22 * SRT.m22);

            SRT.m00 /= sx; SRT.m10 /= sx; SRT.m20 /= sx;
            SRT.m01 /= sy; SRT.m11 /= sy; SRT.m21 /= sy;
            SRT.m02 /= sz; SRT.m12 /= sz; SRT.m22 /= sz;

            // do (RT)^-1
            InvertRotationTraslation(ref SRT);

            // multiply by (S)^-1
            SRT.m00 /= sx; SRT.m01 /= sx; SRT.m02 /= sx; SRT.m03 /= sx;
            SRT.m10 /= sy; SRT.m11 /= sy; SRT.m12 /= sy; SRT.m13 /= sy;
            SRT.m20 /= sz; SRT.m21 /= sz; SRT.m22 /= sz; SRT.m23 /= sz;
        }
        public static Matrix4x4f WorldInverseTraspose(Matrix4x4f SRT)
        {
            WorldInverseTraspose(ref SRT);
            return SRT;
        }
        /// <summary>
        /// World Inverse Traspose (WIT) is the world matrix used to transform correctly the normals.
        /// X7 time faster than WIT = Inverse(Traspose(World))
        /// </summary>
        /// <remarks>
        /// If the affine trasformation is T*R*S, we need to remove traslation because don't have effect to normals
        /// the inverse-traspose of ((R*S)^-1)^t = (S^-1 * R^-1)^t = R^-1^t * S^-1*t = R * (1/S)
        /// example if RS.m00 == a*Sx we need to obtain WIT.m00 = a/Sx = RS.m00 / Sx^2. Lucky Sx^2 is optain removing sqrt
        /// </remarks>
        public static void WorldInverseTraspose(ref Matrix4x4f SRT)
        {
            // remove traslation
            SRT.m03 = SRT.m13 = SRT.m23 = 0;
            // extract squared scale
            float sxsq = SRT.m00 * SRT.m00 + SRT.m10 * SRT.m10 + SRT.m20 * SRT.m20;
            float sysq = SRT.m01 * SRT.m01 + SRT.m11 * SRT.m11 + SRT.m21 * SRT.m21;
            float szsq = SRT.m02 * SRT.m02 + SRT.m12 * SRT.m12 + SRT.m22 * SRT.m22;
            // convert
            SRT.m00 /= sxsq; SRT.m01 /= sysq; SRT.m02 /= szsq;
            SRT.m10 /= sxsq; SRT.m11 /= sysq; SRT.m12 /= szsq;
            SRT.m20 /= sxsq; SRT.m21 /= sysq; SRT.m22 /= szsq;
        }

        public Matrix4x4f WorldInverseTraspose()
        {
            Matrix4x4f wit = this;
            WorldInverseTraspose(ref wit);
            return wit;
        }

        /// <summary>
        /// Composed rotation for z y x axis
        /// TODO : make direclty in math notation
        /// </summary>
        /// <param name="roll">counterclockwise rotation about the x axis</param>
        /// <param name="pitch">counterclockwise rotation about the y axis</param>
        /// <param name="yaw">counterclockwise rotation about the z axis</param>
        /// <returns></returns>
        public static Matrix4x4f RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            Matrix4x4f matrix = Identity;

            float a = yaw;
            float b = pitch;
            float c = roll;

            double cosA = Math.Cos(a);
            double cosB = Math.Cos(b);
            double cosC = Math.Cos(c);
            double sinA = Math.Sin(a);
            double sinB = Math.Sin(b);
            double sinC = Math.Sin(c);


            matrix.m00 = (float)(cosA * cosB);
            matrix.m01 = (float)(cosA * sinB * sinC - sinA * cosC);
            matrix.m02 = (float)(cosA * sinB * cosC + sinA * sinC);

            matrix.m10 = (float)(sinA * cosB);
            matrix.m11 = (float)(sinA * sinB * sinC + cosA * cosC);
            matrix.m12 = (float)(sinA * sinB * cosC - cosA * sinC);

            matrix.m20 = (float)-sinB;
            matrix.m21 = (float)(cosB * sinC);
            matrix.m22 = (float)(cosB * cosC);

            matrix.Traspose();

            return matrix;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        public static Matrix4x4f Orthogonalize(Matrix4x4f matrix)
        {
            Vector3f axisX = Vector3f.Zero;
            Vector3f axisY = Vector3f.Zero;
            Vector3f axisZ = Vector3f.Zero;
            Vector3f scale = Vector3f.One;

            Matrix4x4f result = Identity;

            axisX.x = matrix.m00;
            axisX.y = matrix.m10;
            axisX.z = matrix.m20;
            scale.x = axisX.Length;


            axisY.x = matrix.m01;
            axisY.y = matrix.m11;
            axisY.z = matrix.m21;
            scale.y = axisY.Length;

            axisZ.x = matrix.m02;
            axisZ.y = matrix.m12;
            axisZ.z = matrix.m22;
            scale.z = axisZ.Length;

            axisY = Vector3f.Cross(axisX, axisZ);
            axisZ = Vector3f.Cross(axisX, axisY);

            axisX.Normalize();
            axisY.Normalize();
            axisZ.Normalize();

            result.m00 = axisX.x;
            result.m10 = axisX.y;
            result.m20 = axisX.z;
            result.m01 = axisY.x;
            result.m11 = axisY.y;
            result.m21 = axisY.z;
            result.m02 = axisZ.x;
            result.m12 = axisZ.y;
            result.m22 = axisZ.z;

            result.Position = matrix.Position;

            result *= Scaling(scale.x, scale.y, scale.z);

            return result;
        }
#endregion


        #region Operations on itself to avoid new() constructor

        /// <summary>
        /// Switch from Column-Major to Row-Major vector
        /// </summary>
        public void Traspose()
        {
            // diagonals are the same
            Mathelp.SWAP(ref m01, ref m10);
            Mathelp.SWAP(ref m02, ref m20);
            Mathelp.SWAP(ref m03, ref m30);
            Mathelp.SWAP(ref m12, ref m21);
            Mathelp.SWAP(ref m13, ref m31);
            Mathelp.SWAP(ref m23, ref m32);
        }
        public void Translate(Vector3f vector) => Translate(in vector);
        public void Translate(in Vector3f vector) => Translate(vector.x, vector.y, vector.z);
        public void Translate(float x, float y, float z)
        {
            m03 += x;
            m13 += y;
            m23 += z;
        }

        public void Scale(float x, float y, float z)
        {
            m00 *= x; m10 *= x; m20 *= x; m30 *= x;
            m01 *= y; m11 *= y; m21 *= y; m31 *= y;
            m02 *= z; m12 *= z; m22 *= z; m32 *= z;
        }


        public void Multiply(float scalar)
        {
            m00 *= scalar; m01 *= scalar; m02 *= scalar; m03 *= scalar;
            m10 *= scalar; m11 *= scalar; m12 *= scalar; m13 *= scalar;
            m20 *= scalar; m21 *= scalar; m22 *= scalar; m23 *= scalar;
            m30 *= scalar; m31 *= scalar; m32 *= scalar; m33 *= scalar;
        }

        /// <summary>
        /// This * Right , i used reference to optain the maximum performance
        /// </summary>
        public void Multiply(in Matrix4x4f right)
        {
            float a, b, c, d; // i used 4 temperany value without generate a new Matrix4 struct

            a = m00 * right.m00 + m01 * right.m10 + m02 * right.m20 + m03 * right.m30;
            b = m00 * right.m01 + m01 * right.m11 + m02 * right.m21 + m03 * right.m31;
            c = m00 * right.m02 + m01 * right.m12 + m02 * right.m22 + m03 * right.m32;
            d = m00 * right.m03 + m01 * right.m13 + m02 * right.m23 + m03 * right.m33;
            m00 = a; m01 = b; m02 = c; m03 = d;

            a = m10 * right.m00 + m11 * right.m10 + m12 * right.m20 + m13 * right.m30;
            b = m10 * right.m01 + m11 * right.m11 + m12 * right.m21 + m13 * right.m31;
            c = m10 * right.m02 + m11 * right.m12 + m12 * right.m22 + m13 * right.m32;
            d = m10 * right.m03 + m11 * right.m13 + m12 * right.m23 + m13 * right.m33;
            m10 = a; m11 = b; m12 = c; m13 = d;

            a = m20 * right.m00 + m21 * right.m10 + m22 * right.m20 + m23 * right.m30;
            b = m20 * right.m01 + m21 * right.m11 + m22 * right.m21 + m23 * right.m31;
            c = m20 * right.m02 + m21 * right.m12 + m22 * right.m22 + m23 * right.m32;
            d = m20 * right.m03 + m21 * right.m13 + m22 * right.m23 + m23 * right.m33;
            m20 = a; m21 = b; m22 = c; m23 = d;

            a = m30 * right.m00 + m31 * right.m10 + m32 * right.m20 + m33 * right.m30;
            b = m30 * right.m01 + m31 * right.m11 + m32 * right.m21 + m33 * right.m31;
            c = m30 * right.m02 + m31 * right.m12 + m32 * right.m22 + m33 * right.m32;
            d = m30 * right.m03 + m31 * right.m13 + m32 * right.m23 + m33 * right.m33;
            m30 = a; m31 = b; m32 = c; m33 = d;
        }

        /// <summary>
        /// Left * This
        /// </summary>
        public void PreMultiply(in Matrix4x4f left)
        {
            float a, b, c, d; // i used 4 temperany value without generate a new Matrix4 struct

            a = left.m00 * m00 + left.m01 * m10 + left.m02 * m20 + left.m03 * m30;
            b = left.m10 * m00 + left.m11 * m10 + left.m12 * m20 + left.m13 * m30;
            c = left.m20 * m00 + left.m21 * m10 + left.m22 * m20 + left.m23 * m30;
            d = left.m30 * m00 + left.m31 * m10 + left.m32 * m20 + left.m33 * m30;
            m00 = a; m10 = b; m20 = c; m30 = d;

            a = left.m00 * m01 + left.m01 * m11 + left.m02 * m21 + left.m03 * m31;
            b = left.m10 * m01 + left.m11 * m11 + left.m12 * m21 + left.m13 * m31;
            c = left.m20 * m01 + left.m21 * m11 + left.m22 * m21 + left.m23 * m31;
            d = left.m30 * m01 + left.m31 * m11 + left.m32 * m21 + left.m33 * m31;
            m01 = a; m11 = b; m21 = c; m31 = d;

            a = left.m00 * m02 + left.m01 * m12 + left.m02 * m22 + left.m03 * m32;
            b = left.m10 * m02 + left.m11 * m12 + left.m12 * m22 + left.m13 * m32;
            c = left.m20 * m02 + left.m21 * m12 + left.m22 * m22 + left.m23 * m32;
            d = left.m30 * m02 + left.m31 * m12 + left.m32 * m22 + left.m33 * m32;
            m02 = a; m12 = b; m22 = c; m32 = d;

            a = left.m00 * m03 + left.m01 * m13 + left.m02 * m23 + left.m03 * m33;
            b = left.m10 * m03 + left.m11 * m13 + left.m12 * m23 + left.m13 * m33;
            c = left.m20 * m03 + left.m21 * m13 + left.m22 * m23 + left.m23 * m33;
            d = left.m30 * m03 + left.m31 * m13 + left.m32 * m23 + left.m33 * m33;
            m03 = a; m13 = b; m23 = c; m33 = d;

        }
        #endregion

        public unsafe static explicit operator float*(Matrix4x4f matrix) => (float*)&matrix;


        public static implicit operator Matrix4x4f(Matrix3x3f matrix)
        {
            Matrix4x4f result = Identity;

            result.m00 = matrix.m00;
            result.m01 = matrix.m01;
            result.m02 = matrix.m02;
            result.m10 = matrix.m10;
            result.m11 = matrix.m11;
            result.m12 = matrix.m12;
            result.m20 = matrix.m20;
            result.m21 = matrix.m21;
            result.m22 = matrix.m22;

            return result;
        }
        public static implicit operator Matrix4x4f(Quaternion4f q)
        {
            Matrix4x4f matrix = Identity;

            matrix.m00 = 1 - 2 * q.y * q.y - 2 * q.z * q.z;
            matrix.m01 = 2 * q.x * q.y - 2 * q.z * q.w;
            matrix.m02 = 2 * q.x * q.z + 2 * q.y * q.w;

            matrix.m10 = 2 * q.x * q.y + 2 * q.z * q.w;
            matrix.m11 = 1 - 2 * q.x * q.x - 2 * q.z * q.z;
            matrix.m12 = 2 * q.y * q.z - 2 * q.x * q.w;

            matrix.m20 = 2 * q.x * q.z - 2 * q.y * q.w;
            matrix.m21 = 2 * q.y * q.z + 2 * q.x * q.w;
            matrix.m22 = 1 - 2 * q.x * q.x - 2 * q.y * q.y;

            return matrix;
        }

        // for visual studio debugger visualization
#if DEBUG
        public Vector4f Row0 => getRow(0);
        public Vector4f Row1 => getRow(1);
        public Vector4f Row2 => getRow(2);
        public Vector4f Row3 => getRow(3);
        public Vector4f Col0 => getCol(0);
        public Vector4f Col1 => getCol(1);
        public Vector4f Col2 => getCol(2);
        public Vector4f Col3 => getCol(3);
#endif
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ m00.GetHashCode();
                hash = (hash * 16777619) ^ m01.GetHashCode();
                hash = (hash * 16777619) ^ m02.GetHashCode();
                hash = (hash * 16777619) ^ m03.GetHashCode();
                hash = (hash * 16777619) ^ m10.GetHashCode();
                hash = (hash * 16777619) ^ m11.GetHashCode();
                hash = (hash * 16777619) ^ m12.GetHashCode();
                hash = (hash * 16777619) ^ m13.GetHashCode();
                hash = (hash * 16777619) ^ m20.GetHashCode();
                hash = (hash * 16777619) ^ m21.GetHashCode();
                hash = (hash * 16777619) ^ m22.GetHashCode();
                hash = (hash * 16777619) ^ m23.GetHashCode();
                hash = (hash * 16777619) ^ m30.GetHashCode();
                hash = (hash * 16777619) ^ m31.GetHashCode();
                hash = (hash * 16777619) ^ m32.GetHashCode();
                hash = (hash * 16777619) ^ m33.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj) => obj is Matrix4x4f mat && Equals(in mat);
        public bool Equals(Matrix4x4f other) => Equals(in other);
        public bool Equals(in Matrix4x4f mat)
        {
            return
                m00 == mat.m00 &&
                m01 == mat.m01 &&
                m02 == mat.m02 &&
                m03 == mat.m03 &&

                m10 == mat.m10 &&
                m11 == mat.m11 &&
                m12 == mat.m12 &&
                m13 == mat.m13 &&

                m20 == mat.m20 &&
                m21 == mat.m21 &&
                m22 == mat.m22 &&
                m23 == mat.m23 &&

                m30 == mat.m30 &&
                m31 == mat.m31 &&
                m32 == mat.m32 &&
                m33 == mat.m33;
        }

        public override string ToString()
        {
            return ((FormattableString)
                $"{m00} {m01} {m02} {m03} " +
                $"{m10} {m11} {m12} {m13} " +
                $"{m20} {m21} {m22} {m23} " +
                $"{m30} {m31} {m32} {m33}").ToString(Mathelp.DotCulture);
        }

        public string ToStringRounded() =>
            string.Format(Mathelp.DotCulture,
                $"{m00,0:F4} {m01,0:F4} {m02,0:F4} {m03,0:F4} " +
                $"{m10,0:F4} {m11,0:F4} {m12,0:F4} {m13,0:F4} " +
                $"{m20,0:F4} {m21,0:F4} {m22,0:F4} {m23,0:F4} " +
                $"{m30,0:F4} {m31,0:F4} {m32,0:F4} {m33,0:F4} ");


    }
}