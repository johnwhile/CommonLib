using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Maths
{
    /// <summary>
    /// Generic float matrix, use only for linear algebra
    /// </summary>
    [DebuggerDisplay("M{Rows}x{Columns}")]
    public partial class Matrix : IEquatable<Matrix>
    {
        float[,] m;

        public int Rows => m.GetLength(0);
        public int Columns => m.GetLength(1);

        #region Constructors
        public Matrix(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentException("dimension can't be zero");
            m = new float[rows, cols];
        }
        /// <summary>
        /// the property <see cref="m"/> are passed by reference
        /// </summary>
        public Matrix(float[,] matrix) { m = matrix; }

        public unsafe Matrix(Matrix3x3f matrix)
        {
            m = new float[3, 3];
            int b = sizeof(float) * 9;
            fixed (float* ptr = m)
                Buffer.MemoryCopy(matrix.field, ptr, b, b);
        }
        public unsafe Matrix(Matrix4x4f matrix)
        {
            m = new float[4, 4];
            int b = sizeof(float) * 16;
            fixed (float* ptr = m)
                Buffer.MemoryCopy(matrix.field, ptr, b, b);
        }
        public static Matrix Identity(int rows, int colums)
        {
            Matrix matrix = new Matrix(rows, colums);
            int n = rows < colums ? rows : colums;
            for (int i = 0; i < n; i++) matrix.m[i, i] = 1;
            return matrix;
        }
        public float this[int i, int j]
        {
            get => m[i, j];
            set => m[i, j] = value;
        }
        public unsafe float this[int i]
        {
            get { fixed (float* ptr = m) return ptr[i]; }
            set { fixed (float* ptr = m) ptr[i] = value; }
        }

        /// <summary>
        /// fill data or create a new vector for column c
        /// </summary>
        public void GetColumn(int c, ref Vector column)
        {
            if (column == null) column = new Vector(Columns);
            for (int i = 0; i < Columns; i++) column[i] = m[i, c];
        }
        /// <summary>
        /// fill data or create a new vector for row r
        /// </summary>
        public void GetRow(int r, ref Vector row)
        {
            if (row == null) row = new Vector(Rows);
            for (int i = 0; i < Rows; i++) row[i] = m[r, i];
        }


        #endregion

        #region Algebra
        /// <summary>
        /// A*B, matrix C can be pre-initialized.
        /// C can't have same reference of A and B
        /// </summary>
        public static void Multiply(Matrix A, Matrix B, ref Matrix C)
        {
            int rA = A.Rows;
            int cA = A.Columns;
            int rB = B.Rows;
            int cB = B.Columns;
            int N = cA;

            if (cA != rB)
                throw new ArgumentException("incompatible A and B matrix dimension");

            if (C != null && C.Rows != rA && C.Columns != cB)
                throw new ArgumentException("incompatible C matrix dimension");
            else C = new Matrix(rA, cB);

            if (ReferenceEquals(A, C) || ReferenceEquals(B, C))
                throw new ArgumentException("matrix C must be a difference class than A and B");

            for (int r = 0; r < rA; r++)
                for (int c = 0; c < cB; c++)
                    for (int k = 0; k < N; k++)
                        C[r, c] += A[r, k] * B[k, c];
        }
        /// <summary>
        /// A*s, matrix C can be pre-initialized.
        /// C can have same reference of A
        /// </summary>
        public static void Multiply(Matrix A, float scalar, ref Matrix C)
        {
            if (C == null) C = new Matrix(A.Rows, A.Columns);
            for (int r = 0; r < A.Rows; r++)
                for (int c = 0; c < A.Columns; c++)
                    C[r, c] = A[r, c] * scalar;
        }
        /// <summary>
        /// A+B, matrix C can be pre-initialized.
        /// C can have same reference of A
        /// </summary>
        public static void Add(Matrix A, Matrix B, ref Matrix C)
        {
            if (C == null) C = new Matrix(A.Rows, A.Columns);

            if (B.Rows != A.Rows || B.Columns != A.Columns)
                throw new ArgumentException("incompatible B matrix dimension");

            for (int r = 0; r < A.Rows; r++)
                for (int c = 0; c < A.Columns; c++)
                    C[r, c] = A[r, c] + B[r, c];
        }
        /// <summary>
        /// A-B, matrix C can be pre-initialized.
        /// C can have same reference of A
        /// </summary>
        public static void Sub(Matrix A, Matrix B, ref Matrix C)
        {
            if (C == null) C = new Matrix(A.Rows, A.Columns);

            if (B.Rows != A.Rows || B.Columns != A.Columns)
                throw new ArgumentException("incompatible B matrix dimension");

            for (int r = 0; r < A.Rows; r++)
                for (int c = 0; c < A.Columns; c++)
                    C[r, c] = A[r, c] - B[r, c];
        }
        /// <summary>
        /// A+s, matrix C can be pre-initialized.
        /// C can have same reference of A
        /// </summary>
        public static void Add(Matrix A, float scalar, ref Matrix C)
        {
            if (C == null) C = new Matrix(A.Rows, A.Columns);

            for (int r = 0; r < A.Rows; r++)
                for (int c = 0; c < A.Columns; c++)
                    C[r, c] = A[r, c] + scalar;
        }
        /// <summary>
        /// Traspose this matrix, if not square a new <see cref="m"/> array are generated
        /// </summary>
        public static void Traspose(ref Matrix A)
        {
            int R = A.Rows;
            int C = A.Columns;
            //traspose a square matrix
            if (R == C) A.squaretraspose();
            else A = A.Traspose();
        }

        /// <summary>
        /// return a new trasposed matrix
        /// </summary>
        public unsafe Matrix Traspose()
        {
            int C = Columns;
            int R = Rows;
            Matrix T = new Matrix(C, R);
            for (int r = 0; r < R; r++)
                for (int c = 0; c < C; c++)
                    T.m[c, r] = m[r, c];
            return T;
        }

        void squaretraspose()
        {
            int N = Rows;
            for (int r = 0; r < N; r++)
                for (int c = 1; c < N; c++)
                {
                    float tmp = m[r, c];
                    m[r, c] = m[c, r];
                    m[c, r] = tmp;
                }
        }
        #endregion

        #region operator overload
        public static Matrix operator *(Matrix A, Matrix B)
        {
            Matrix C = new Matrix(A.Rows, B.Columns);
            Multiply(A, B, ref C);
            return C;
        }
        public static Matrix operator *(Matrix A, float scalar)
        {
            Matrix C = default;
            Multiply(A, scalar, ref C);
            return C;
        }
        public static Matrix operator +(Matrix A, Matrix B)
        {
            Matrix C = default;
            Add(A, B, ref C);
            return C;
        }
        public static Matrix operator +(Matrix A, float scalar)
        {
            Matrix C = default;
            Add(A, scalar, ref C);
            return C;
        }
        public static Matrix operator -(Matrix A, Matrix B)
        {
            Matrix C = default;
            Sub(A, B, ref C);
            return C;
        }
        public static Matrix operator -(Matrix A, float scalar) => A + (-scalar);
        public static Matrix operator -(Matrix A) => A * -1;
        #endregion

        public override bool Equals(object obj) => obj is Matrix matrix && Equals(matrix);
        public bool Equals(Matrix matrix) => this == matrix;
        public override int GetHashCode() => base.GetHashCode();

        public Matrix Clone()
        {
            Matrix copy = new Matrix(Rows, Columns);
            Buffer.BlockCopy(m, 0, copy.m, 0, Rows * Columns * sizeof(float));
            return copy;
        }

        public static implicit operator Matrix(Matrix3x3f m) => new Matrix(m);
        public static implicit operator Matrix(Matrix4x4f m) => new Matrix(m);

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            for (int r = 0; r < Rows; r++)
            {
                int c;
                for (c = 0; c < Columns - 1; c++) str.Append($"{m[r, c]} ");
                str.AppendLine($"{m[r, c]}");
            }
            return str.ToString();
        }
    }
}
