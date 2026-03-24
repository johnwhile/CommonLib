using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Common.Maths
{
    /// <summary>
    /// A 3x3 Matrix, is a transform version for 2d case
    /// </summary>
    /// <remarks>
    /// Remember the correct order of transformations L = T * R * S mean
    /// first scale, second rotation and last traslate.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Matrix3x3f
    {
        [FieldOffset(0)]
        public float m00;
        [FieldOffset(4)]
        public float m01;
        [FieldOffset(8)]
        public float m02;
        [FieldOffset(12)]
        public float m10;
        [FieldOffset(16)]
        public float m11;
        [FieldOffset(20)]
        public float m12;
        [FieldOffset(24)]
        public float m20;
        [FieldOffset(28)]
        public float m21;
        [FieldOffset(32)]
        public float m22;

        [FieldOffset(0)]
        public unsafe fixed float field[16];



        public static readonly Matrix3x3f Identity = new Matrix3x3f(1.0f, 0, 0, 0, 1.0f, 0, 0, 0, 1.0f);
        public static readonly Matrix3x3f Zero = new Matrix3x3f(0.0f, 0, 0, 0, 0, 0, 0, 0, 0);

        public Matrix3x3f(float[,] m) : this(
            m[0, 0], m[0, 1], m[0, 2],
            m[1, 0], m[1, 1], m[1, 2],
            m[2, 0], m[2, 1], m[2, 2]) 
        { }

        public Matrix3x3f(float[] m) :
            this(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8]) 
        { }

        public Matrix3x3f(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22) :
            this((float)m00, (float)m01, (float)m02, (float)m10, (float)m11, (float)m12, (float)m20, (float)m21, (float)m22) { }

        public Matrix3x3f(Vector3f right, Vector3f up, Vector3f forward)
            : this(right.x, up.x, forward.x,
                   right.y, up.y, forward.y,
                   right.z, up.z, forward.z) { }

        public Matrix3x3f(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
        }

        public static unsafe float[,] Convert(in Matrix3x3f m)
        {
            float[,] tmp = new float[3, 3];
            fixed (float* src = m.field)
            fixed (float* dst = tmp)
                Buffer.MemoryCopy(src, dst, 36, 36);
            return tmp;
        }

        public static unsafe Matrix3x3f Convert(in float[,] m)
        {
            Matrix3x3f matrix = default;
            fixed (float* src = m)
                Buffer.MemoryCopy(src, matrix.field, 36, 36);
            return matrix;
        }

        /// <summary>
        /// The most used transformation matrix create with order : Scale -> Rotation -> Traslation affine transformation.
        /// TRS name is used to remember you that this matrix are calulation with T * R * S
        /// 
        /// <para>|Cos*Sx  Sin*Sy Tx|</para>
        /// <para>|-Sin*Sx Cos*Sy Ty|</para>
        /// <para>|    0      0    1|</para>
        /// </summary>
        public static Matrix3x3f TRS(float CenterX, float CenterY, float SizeX, float SizeY, float Radians)
        {
            return new Matrix3x3f(
                (float)Math.Cos(Radians) * SizeX, (float)Math.Sin(Radians) * SizeY, CenterX,
                -(float)Math.Sin(Radians) * SizeX, (float)Math.Cos(Radians) * SizeY, CenterY,
                0, 0, 1);

            // equivalent of :
            // return Matrix3.Translating(CenterX, CenterY) * Matrix3.Rotation(Radians) * Matrix3.Scaling(SizeX, SizeY);

        }

        /// <summary>
        /// <para>|Cos*Sx  Sin*Sy Tx|</para>
        /// <para>|-Sin*Sx Cos*Sy Ty|</para>
        /// <para>|    0      0    1|</para>
        /// </summary>
        public static void DecomposeTRS(Matrix3x3f matrix, out Vector2f traslation, out Vector2f scaling, out float angle)
        {
            traslation = new Vector2f(matrix.m02, matrix.m12);

            // scale component are optain from lenght of colum vector
            // |Cos*Sx  Sin*Sy Tx|
            // |-Sin*Sx Cos*Sy Ty|
            // |    0      0    1|
            // S.x = Sqrt{(Cos*Sx)^2 + (-Sin*Sx)^2} = Sqrt{ (Cos^2 + Sin^2) * Sx^2} = Sqrt(Sx^2) = Sx

            scaling = new Vector2f(
                Math.Sqrt(matrix.m00 * matrix.m00 + matrix.m10 * matrix.m10),
                Math.Sqrt(matrix.m01 * matrix.m01 + matrix.m11 * matrix.m11));

            angle = (float)Math.Acos(matrix.m00 / scaling.x);

        }

        public static readonly int sizeinbyte = sizeof(float) * 9;

        /// <summary>
        /// Indexer for accessing the matrix like a 2d array (i.e. matrix[2,3]).
        /// </summary>
        public unsafe float this[int row, int col]
        {
            get
            {
                fixed (float* pM = &m00)
                    return *(pM + ((3 * row) + col));
            }
            set
            {
                fixed (float* pM = &m00)
                    *(pM + ((3 * row) + col)) = value;
            }
        }

        /// <summary>
        ///		Allows the Matrix to be accessed linearly (m[0] -> m[8]).  
        /// </summary>
        public unsafe float this[int index]
        {
            get
            {
                fixed (float* pMatrix = &m00)
                    return *(pMatrix + index);
            }
            set
            {
                fixed (float* pMatrix = &m00)
                    *(pMatrix + index) = value;
            }
        }


        public void FromEulerAnglesXYZ(float yaw, float pitch, float roll)
        {
            float cos = (float)Math.Cos(yaw);
            float sin = (float)Math.Sin(yaw);
            Matrix3x3f xMat = new Matrix3x3f(1, 0, 0, 0, cos, -sin, 0, sin, cos);

            cos = (float)Math.Cos(pitch);
            sin = (float)Math.Sin(pitch);
            Matrix3x3f yMat = new Matrix3x3f(cos, 0, sin, 0, 1, 0, -sin, 0, cos);

            cos = (float)Math.Cos(roll);
            sin = (float)Math.Sin(roll);
            Matrix3x3f zMat = new Matrix3x3f(cos, -sin, 0, sin, cos, 0, 0, 0, 1);

            this = xMat * (yMat * zMat);
        }

        public Vector3f ToEulerAnglesXYZ()
        {
            double yAngle;
            double rAngle;
            double pAngle;

            pAngle = Math.Asin( m01);
            if (pAngle < Math.PI / 2)
            {
                if (pAngle > -Math.PI / 2)
                {
                    yAngle = Math.Atan2( m21,  m11);
                    rAngle = Math.Atan2( m02,  m00);
                }
                else
                {
                    // WARNING. Not a unique solution.
                    var fRmY = (float)Math.Atan2(- m20,  m22);
                    rAngle = 0.0f; // any angle works
                    yAngle = rAngle - fRmY;
                }
            }
            else
            {
                // WARNING. Not a unique solution.
                var fRpY = Math.Atan2(- m20,  m22);
                rAngle = 0.0f; // any angle works
                yAngle = fRpY - rAngle;
            }

            return new Vector3f((float)yAngle, (float)rAngle, (float)pAngle);
        }
        /// <summary>
        ///	 Get or Set the translation value of the matrix using math notation
        ///		| 1 0 Tx |
        ///		| 0 1 Ty |
        ///		| 0 0  1 |
        /// </summary>
        public Vector2f Position
        {
            get { return new Vector2f(m02, m12); }
            set { m02 = value.x; m12 = value.y; }
        }

        /// <summary>
        /// Get the determinant of matrix.
        /// </summary>
        public float Determinant
        {
            get
            {
                float result = 0.0f;
                result += m00 * (m11 * m22 - m12 * m21);
                result -= m01 * (m10 * m22 - m12 * m20);
                result += m02 * (m10 * m21 - m11 * m20);
                return result;
            }
        }
        /// <summary>
        /// Internal multiplication (avoid new())
        /// </summary>
        public void Multiply(float scalar)
        {
            m00 *= scalar; m01 *= scalar; m02 *= scalar;
            m10 *= scalar; m11 *= scalar; m12 *= scalar;
            m20 *= scalar; m21 *= scalar; m22 *= scalar;
        }

        /// <summary>
        /// Cofactors matrix
        /// </summary>
        public Matrix3x3f Adjoint => new Matrix3x3f(
                m11 * m22 - m12 * m21,
                -m01 * m22 + m02 * m21,
                m01 * m12 - m02 * m11,
                -m10 * m22 + m12 * m20,
                m00 * m22 - m02 * m20,
                -m00 * m12 + m02 * m10,
                m10 * m21 - m11 * m20,
                -m00 * m21 + m01 * m20,
                m00 * m11 - m01 * m10);


        /// <summary>
        /// Returns a new inverted matrix. <see cref="ArithmeticException"/> if can't be inverted.
        /// </summary>
        /// <exception cref="ArithmeticException"></exception>
        public Matrix3x3f Inverse()
        {
            // cofactors matrix
            Matrix3x3f adj = Adjoint;
            // determinant
            float det = m00 * adj.m00 + m01 * adj.m01 + m02 * adj.m02;
            
            if (det > -float.Epsilon && det < float.Epsilon)
#if DEBUG
            throw new ArithmeticException("matrix can't be inverted");
#else
            det = 0;
#endif
            else det = 1f / det;
            
            // trasposed adjoint matrix
            // inverse = (1/det)(Adjoint^T)

            adj.Multiply(det);
            return adj;
        }

        /// <summary>
        /// Get a 2D rotation matrix
        /// </summary>
        /// <param name="radians">if XY are the plane, the only rotation available is Z</param>
        /// <returns></returns>
        public static Matrix3x3f Rotation(float radians)
        {
            Matrix3x3f matrix = Identity;
            matrix.m00 = (float)Math.Cos(radians);
            matrix.m01 = (float)Math.Sin(radians);
            matrix.m10 = -(float)Math.Sin(radians);
            matrix.m11 = (float)Math.Cos(radians);
            return matrix;
        }
        public static Matrix3x3f Scaling(Vector3f scale)
        {
            return Scaling(scale.x, scale.y);
        }
        public static Matrix3x3f Scaling(float x, float y)
        {
            Matrix3x3f matrix = Identity;
            matrix.m00 = x;
            matrix.m11 = y;
            return matrix;
        }
        public static Matrix3x3f Translating(Vector2f vector)=>Translating(vector.x, vector.y);
        
        public static Matrix3x3f Translating(float x, float y)
        {
            Matrix3x3f matrix = Identity;
            matrix.m02 = x;
            matrix.m12 = y;
            return matrix;
        }
        /// <summary>
        /// Traslation component is indipendent and can work also for TRS transformations
        /// </summary>
        public void Translate(Vector2f vector)
        {
            TranslateX(vector.x);
            TranslateY(vector.y);
        }
        /// <summary>
        /// </summary>
        public void TranslateX(float move)
        {
            m02 += move;
        }
        public void TranslateY(float move)
        {
            m12 += move;
        }

        public void Traspose()
        {
            // diagonals are the same
            Mathelp.SWAP(ref m01, ref m10);
            Mathelp.SWAP(ref m02, ref m20);
            Mathelp.SWAP(ref m12, ref m21);
        }

        public static Matrix3x3f Traspose(Matrix3x3f matrix)
        {
            Matrix3x3f m = new Matrix3x3f();
            m.m00 = matrix.m00;
            m.m01 = matrix.m10;
            m.m02 = matrix.m20;

            m.m10 = matrix.m01;
            m.m11 = matrix.m11;
            m.m12 = matrix.m21;

            m.m20 = matrix.m02;
            m.m21 = matrix.m12;
            m.m22 = matrix.m22;

            return m;
        }

        /// <summary>
        /// Rotate the rotation component of TRS transformations by radians value (mean rotation multiplication)
        /// </summary>
        public void RotateTRSmul(float radians)
        {
            // decomposition
            float tx = m02;
            float ty = m12;

            float sx = (float)Math.Sqrt( m00 *  m00 +  m10 *  m10);
            float sy = (float)Math.Sqrt( m01 *  m01 +  m11 *  m11);

             m02 = 0;
             m12 = 0;
             m00 /= sx;
             m10 /= sx;
             m01 /= sy;
             m11 /= sy;

            // recomposition
            this *= Rotation(radians);
             m00 *= sx;
             m10 *= sx;
             m01 *= sy;
             m11 *= sy;
             m02 = tx;
             m12 = ty;
        }
        /// <summary>
        /// Substitute the rotation component of TRS transformations by new radians value,
        /// require less computation than <see cref="RotateTRSmul"/>
        /// </summary>
        public void RotateTRSsub(float radians)
        {
            // decomposition
            float tx =  m02;
            float ty =  m12;

            float sx = (float)Math.Sqrt( m00 *  m00 +  m10 *  m10);
            float sy = (float)Math.Sqrt( m01 *  m01 +  m11 *  m11);

            // recomposition
            this = Rotation(radians);

             m00 *= sx;
             m10 *= sx;
             m01 *= sy;
             m11 *= sy;
             m02 = tx;
             m12 = ty;
        }
        /// <summary>
        /// Multiply the scale component of TRS transformations (mean that scalex is muliplied to existing scale value),
        /// require less computation than <see cref="ScaleTRSsub"/>
        /// </summary>
        public void ScaleTRSmul(float scalex, float scaley)
        {
             m00 *= scalex;
             m10 *= scalex;
             m01 *= scaley;
             m11 *= scaley;
        }
        /// <summary>
        /// Substitute the scale component of TRS transformations (mean the are set the scale value to current value)
        /// </summary>
        public void ScaleTRSsub(float scalex, float scaley)
        {
            // decomposition
            float sx = (float)Math.Sqrt( m00 *  m00 +  m10 *  m10);
            float sy = (float)Math.Sqrt( m01 *  m01 +  m11 *  m11);
            // recomposition
             m00 *= scalex / sx;
             m10 *= scalex / sx;
             m01 *= scaley / sy;
             m11 *= scaley / sy;
        }

        /// <summary>
        ///  Returns an inverted matrix, if not exit return Exception
        /// </summary>
        public static Matrix3x3f Inverse(Matrix3x3f matrix) => matrix.Inverse();
        



        #region operators

        public static Matrix3x3f operator *(Matrix3x3f left, Matrix3x3f right)
        {
            //   A = MxN  B=NxP   C = AB = MxP
            Matrix3x3f result = default;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;

            return result;
        }

        /// <summary>
        ///  vector * matrix [1x3 * 3x3 = 1x3]
        /// </summary>
        public static Vector3f operator *(Vector3f vector, Matrix3x3f matrix)
        {
            var product = new Vector3f();

            product.x = matrix.m00 * vector.x + matrix.m01 * vector.y + matrix.m02 * vector.z;
            product.y = matrix.m10 * vector.x + matrix.m11 * vector.y + matrix.m12 * vector.z;
            product.z = matrix.m20 * vector.x + matrix.m21 * vector.y + matrix.m22 * vector.z;

            return product;
        }
        /// <summary>
        ///  M[3x3]*V[3x1] = V'[3x1]
        /// </summary>
        public static Vector3f operator *(Matrix3x3f matrix, Vector3f vector)
        {
            var product = new Vector3f();

            product.x = matrix.m00 * vector.x + matrix.m01 * vector.y + matrix.m02 * vector.z;
            product.y = matrix.m10 * vector.x + matrix.m11 * vector.y + matrix.m12 * vector.z;
            product.z = matrix.m20 * vector.x + matrix.m21 * vector.y + matrix.m22 * vector.z;

            return product;
        }
        /// <summary>
        ///  M[3x3]*V[3x1] = V'[3x1] is used to make a transformation, the V*M isn't used
        ///  http://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/
        /// </summary>
        public static Vector2f operator *(Matrix3x3f matrix, Vector2f vector)
        {
            //       3x3   *   3x1  =   3x1
            //		| a b c |   |x|    |ax + by + c|
            //		| d e f | * |y| =  |dx + ey + f| = Vresult
            //		| g h i |   |1|    |gx + hy + i|
            //
            //   Vresult /= (gx + hy + i)  omogeneus vector 
            //  if is a regular transform matrix, g=h=0 and inverseW = 1

            Vector2f product = new Vector2f();
            float inverseW = matrix.m20 * vector.x + matrix.m21 * vector.y + matrix.m22;
            inverseW = inverseW < -1e-6f || inverseW > 1e-6f ? 1.0f / inverseW : 1.0f;

            product.x = (matrix.m00 * vector.x + matrix.m01 * vector.y + matrix.m02) * inverseW;
            product.y = (matrix.m10 * vector.x + matrix.m11 * vector.y + matrix.m12) * inverseW;

            return product;
        }


        /// <summary>
        /// Negates all the items in the Matrix.
        /// </summary>
        public static Matrix3x3f operator -(Matrix3x3f matrix)
        {
            matrix.Multiply(-1);
            return matrix;
        }
        /// <summary>
        ///  Used to subtract two matrices.
        /// </summary>
        public static Matrix3x3f operator -(Matrix3x3f left, Matrix3x3f right)
        {
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    left[row, col] -= right[row, col];
            return left;
        }
        #endregion

        /// <summary>
        /// Find the Transform from two rectangles defined by 4 corners, (slow method)
        /// </summary>
        /// <remarks>
        /// Calculates coefficients of perspective transformation
        /// which maps(xi, yi) to(ui, vi), (i=1,2,3,4):
        /// <code>
        ///      c00*xi + c01*yi + c02
        /// ui = ──────────────────────
        ///      c20*xi + c21*yi + c22
        ///
        ///      c10*xi + c11*yi + c12
        /// vi = ──────────────────────
        ///      c20*xi + c21*yi + c22
        ///</code>
        /// Coefficients are calculated by solving linear system:
        ///<code>
        /// │ x0 y0  1  0  0  0 -x0* u0 -y0* u0 │ │c00│ │u0│
        /// │ x1 y1  1  0  0  0 -x1* u1 -y1* u1 │ │c01│ │u1│
        /// │ x2 y2  1  0  0  0 -x2* u2 -y2* u2 │ │c02│ │u2│
        /// │ x3 y3  1  0  0  0 -x3* u3 -y3* u3 │.│c10│=│u3│,
        /// │  0  0  0 x0 y0  1 -x0* v0 -y0* v0 │ │c11│ │v0│
        /// │  0  0  0 x1 y1  1 -x1* v1 -y1* v1 │ │c12│ │v1│
        /// │  0  0  0 x2 y2  1 -x2* v2 -y2* v2 │ │c20│ │v2│
        /// │  0  0  0 x3 y3  1 -x3* v3 -y3* v3 │ │c21│ │v3│
        ///              A x = b
        ///</code>
        ///where:
        ///   cij - matrix coefficients, c22 = 1
        /// </remarks>
        public static Matrix3x3f MakeTransformFromTwoRectangle(Vector2f[] src, Vector2f[] dst)
        {
            Vector b = new Vector(8);
            Matrix a = new Matrix(8, 8);
            
            for (int i = 0; i < 4; ++i)
            {
                a[i, 0] = a[i + 4, 3] = src[i].x;
                a[i, 1] = a[i + 4, 4] = src[i].y;
                a[i, 2] = a[i + 4, 5] = 1;
                a[i, 3] = a[i, 4] = a[i, 5] =
                a[i + 4, 0] = a[i + 4, 1] = a[i + 4, 2] = 0;
                a[i, 6] = -src[i].x * dst[i].x;
                a[i, 7] = -src[i].y * dst[i].x;
                a[i + 4, 6] = -src[i].x * dst[i].y;
                a[i + 4, 7] = -src[i].y * dst[i].y;

                b[i] = dst[i].x;
                b[i + 4] = dst[i].y;
            }

            Matrix.QRDecomposition(a, out var q, out var r);

            a = Matrix.InverseUpperTriangular(r) * q.Traspose();

            var x = a * b;

            return new Matrix3x3f()
            {
                m00 = x[0],
                m01 = x[1],
                m02 = x[2],
                m10 = x[3],
                m11 = x[4],
                m12 = x[5],
                m20 = x[6],
                m21 = x[7],
                m22 = 1,
            };
        }
        
        /// <summary>
        /// System matrix can implement only T R S matrix
        /// </summary>
        public static implicit operator System.Drawing.Drawing2D.Matrix(Matrix3x3f matrix)
        {
            DecomposeTRS(matrix, out var t, out var s, out var a);

            /*
            var m = new System.Drawing.Drawing2D.Matrix(
                matrix.m00, matrix.m10,
                matrix.m01, matrix.m11,
                matrix.m02, matrix.m12);
            */
            var m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(t.x, t.y);
            m.Rotate(Mathelp.RadianToDegree(a));
            m.Scale(s.x, s.y);
            return m;
        }

        public override string ToString() => 
            string.Format(Mathelp.DotCulture, 
                $"{m00} {m01} {m02} " +
                $"{m10} {m11} {m12} " +
                $"{m20} {m21} {m22}");
        

        public string ToStringRounded() =>
            string.Format(Mathelp.DotCulture,
                $"{m00,0:F4} {m01,0:F4} {m02,0:F4} " +
                $"{m10,0:F4} {m11,0:F4} {m12,0:F4} " +
                $"{m20,0:F4} {m21,0:F4} {m22,0:F4}");
    }
}