using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Common.Maths
{
    /// <summary>
    /// Generic float vector, use only for linear algebra
    /// </summary>
    [DebuggerDisplay("{ToStringRounded()}")]
    public class Vector : IEquatable<Vector>
    {
        float[] v;

        public int Length => v.Length;

        #region Constructors
        public Vector(int length)
        {
            if (length <= 0)
                throw new ArithmeticException("dimension can't be zero");
            v = new float[length];
        }
        /// <summary>
        /// the property <see cref="v"/> are passed by reference
        /// </summary>
        public Vector(params float[] vector) { v = vector; }
        
        public Vector(IEnumerable<float> enumerable)
        {
            v = new float[enumerable.Count()];
            int i = 0;
            foreach (var f in enumerable) v[i++] = f;
        }
        
        public unsafe Vector(Vector4f vector)
        {
            v = new float[4];
            fixed (float* ptr = v)
                Buffer.MemoryCopy(vector.field, ptr, 16, 16);
        }
        public unsafe Vector(Vector3f vector)
        {
            v = new float[3];
            fixed (float* ptr = v)
                Buffer.MemoryCopy(vector.field, ptr, 12, 12);
        }
        
        public float this[int i]
        {
            get => v[i];
            set => v[i] = value;
        }

        #endregion

        #region Algebra


        public float Magnitude => (float)Math.Sqrt(Dot(this, this));

        /// <summary>
        /// M.a = b
        /// b can't be the same reference of a
        /// </summary>
        public static void Multiply(Matrix M, Vector A, ref Vector B)
        {
            int n = A.Length;
            if (M.Columns != n) throw new ArithmeticException("incompatible vector dimension");
            if (ReferenceEquals(A, B)) throw new Exception("vector b can't be the same reference of a");
            if (B == null) B = new Vector(M.Rows);

            for (int i = 0; i < M.Rows; i++)
                for (int j = 0; j < n; j++)
                    B[i] += M[i, j] * A[j];
        }
        /// <summary>
        /// a.M = b
        /// b can't be the same reference of a
        /// </summary>
        public static void Multiply(Vector A, Matrix M, ref Vector B)
        {
            int n = A.Length;
            if (M.Rows != n) throw new ArithmeticException("incompatible vector dimension");
            if (ReferenceEquals(A, B)) throw new Exception("vector b can't be the same reference of a");
            if (B == null) B = new Vector(M.Columns);

            for (int j = 0; j < M.Columns; j++)
                for (int i = 0; i < n; i++)
                    B[j] += M[i, j] * A[i];
        }

        /// <summary>
        /// A*s =C
        /// </summary>
        public static void Multiply(Vector A, float scalar, ref Vector C)
        {
            int N = A.Length;
            if (C == null) C = new Vector(N);
            for (int i = 0; i < N; i++) C[i] = A[i] * scalar;
        }
        /// <summary>
        /// AxB (only for 3d vector)
        /// C can't be the same reference of A
        /// </summary>
        public static void Cross(Vector A, Vector B, ref Vector C)
        {
            if (A.Length != 3 || A.Length != B.Length) throw new ArithmeticException("incompatible dimension, must be a 3d vector");

            if (ReferenceEquals(A, C) || ReferenceEquals(B, C)) throw new ArithmeticException("Vector C can't have same reference");

            if (C == null) C = new Vector(3);
            C[0] = A[1] * B[2] - A[2] * B[1];
            C[1] = A[2] * B[0] - A[0] * B[2];
            C[2] = A[0] * B[1] - A[1] * B[0];
        }

        /// <summary>
        /// A[mx1] × B[1xn] = M[mxn]
        /// Calculates the outer product between two vectors.
        /// </summary>
        public static void OuterProduct(Vector A, Vector B, ref Matrix product)
        {
            int R = A.Length;
            int C = B.Length;

            if (product == null) product = new Matrix(R,C);
            for (int i = 0; i < R; i++)
                for (int j = 0; j < C; j++)
                    product[i, j] = A[i] * B[j];
        }

        /// <summary>
        /// A.B
        /// </summary>
        public static float Dot(Vector A, Vector B)
        {
            int N = A.Length;
            if (N != B.Length) throw new ArithmeticException("incompatible B dimension");
            float sum = 0;
            for (int i = 0; i < N; i++) sum += A[i] * B[i];
            return sum;
        }
        /// <summary>
        /// A+B=C
        /// C can be the same reference of A
        /// </summary>
        public static void Add(Vector A, Vector B, ref Vector C)
        {
            int N = A.Length;
            if (C == null) C = new Vector(N);
            if (N != B.Length) throw new ArithmeticException("incompatible B dimension");
            for (int i = 0; i < N; i++) C[i] = A[i] + B[i];
        }
        /// <summary>
        /// A-B=C
        /// C can be the same reference of A
        /// </summary>
        public static void Sub(Vector A, Vector B, ref Vector C)
        {
            int N = A.Length;
            if (C == null) C = new Vector(N);
            if (N != B.Length) throw new ArithmeticException("incompatible B dimension");
            for (int i = 0; i < N; i++) C[i] = A[i] - B[i];
        }
        /// <summary>
        /// A+s=C
        /// C can be the same reference of A
        /// </summary>
        public static void Add(Vector A, float scalar, ref Vector C)
        {
            int N = A.Length;
            if (C == null) C = new Vector(N);
            for (int i = 0; i < N; i++) C[i] = A[i] + scalar;
        }
        #endregion

        #region operator overload
        public static Vector operator *(Vector A, float scalar)
        {
            Vector C = default;
            Multiply(A, scalar, ref C);
            return C;
        }
        
        
        public static Vector operator *(Matrix M, Vector a)
        {
            Vector b = default;
            Multiply(M, a, ref b);
            return b;
        }
        public static Vector operator *(Vector a, Matrix M)
        {
            Vector b = default;
            Multiply(a, M, ref b);
            return b;
        }

        public static Vector operator +(Vector A, Vector B)
        {
            Vector C = default;
            Add(A, B, ref C);
            return C;
        }
        public static Vector operator +(Vector A, float scalar)
        {
            Vector C = default;
            Add(A, scalar, ref C);
            return C;
        }
        public static Vector operator -(Vector A, Vector B)
        {
            Vector C = default;
            Sub(A, B, ref C);
            return C;
        }
        public static Vector operator -(Vector A, float scalar) => A + (-scalar);
        public static Vector operator -(Vector A) => A * -1;
        #endregion

        public override bool Equals(object obj) => obj is Vector vector && Equals(vector);
        public bool Equals(Vector vector) => this == vector;
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => v.ToString();

        public string ToStringRounded()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in v) builder.Append(item.ToString("0.000", Mathelp.DotCulture) + " ");
            return builder.ToString();


            //return v.AsEnumerable().Aggregate("", (a, e) => a += string.Format(" {0.000}", e), r => r.Trim());
        }

        public Vector Clone()
        {
            Vector copy = new Vector(Length);
            Buffer.BlockCopy(v, 0, copy.v, 0, Length * sizeof(float));
            return copy;
        }
    }
}