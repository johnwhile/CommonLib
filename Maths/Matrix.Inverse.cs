using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace Common.Maths
{
    public partial class Matrix
    {
        /// <summary>
        /// Inverse of a upper triangular of a square matrix
        /// </summary>
        public static Matrix InverseUpperTriangular(Matrix upper)
        {
            if (upper == null || upper.Rows != upper.Columns) throw new ArithmeticException("upper is not a square matrix");

            int n = upper.Rows;
            var result = Identity(n, n);

            for (int k = 0; k < n; ++k)
                for (int j = 0; j < n; ++j)
                {
                    for (int i = 0; i < k; ++i)
                        result[j, k] -= result[j, i] * upper[i, k];
                    result[j, k] /= upper[k, k];
                }
            return result;
        }
        /// <summary>
        /// Inverse of a lower triangular of a square matrix
        /// </summary>
        public static Matrix InverseLowerTriangular(Matrix lower)
        {
            if (lower == null || lower.Rows != lower.Columns) throw new ArithmeticException("lower is not a square matrix");

            /*
            var result = lower.Traspose();
            result = InverseUpperTriangular(result);
            Traspose(ref result);
            return result;
            */

            int n = lower.Rows;
            var result = Identity(n, n);

            for (int k = 0; k < n; ++k)
                for (int j = 0; j < n; ++j)
                {
                    for (int i = 0; i < k; ++i)
                        result[k, j] -= result[i, j] * lower[k, i];
                    result[k, j] /= lower[k, k];
                }
            return result;
        }

        /// <summary>
        /// Calculates the inverse of the given matrix using the 
        /// Gauss-Jordan Method (see https://en.wikipedia.org/wiki/Gaussian_elimination).
        /// </summary>
        /// <param name="A">The matrix to invert.</param>
        /// <returns>The inverse of the given matrix.</returns>
        [Obsolete]
        public static Matrix GetInverse(Matrix A)
        {
            // Initialize the augmented matrix B.
            int n = A.Rows;
            Matrix B = new Matrix(n, 2 * n);

            // In the augmented matrix B, the first 3 columns are the original 
            // matrix A, and the last 3 columns are the identity matrix C.
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    B[i, j] = A[i, j];

                for (int j = n; j < 2 * n; j++)
                    if (i == j - n) B[i, j] = 1;
            }

            // Swap rows of the augmented matrix B.
            for (int i = n - 1; i > 0; i--)
            {
                if (B[i - 1, 0] >= B[i, 0]) continue;

                for (int j = 0; j < 2 * n; j++)
                {
                    float temp = B[i, j];
                    B[i, j] = B[i - 1, j];
                    B[i - 1, j] = temp;
                }
            }

            // Substract each row by a multiple of another row.
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    float temp = B[j, i] / B[i, i];
                    for (int k = 0; k < 2 * n; k++)
                        B[j, k] -= B[i, k] * temp;

                }


            // Divide each row element by the diagonal element.
            for (int i = 0; i < n; i++)
            {
                float temp = B[i, i];
                for (int j = 0; j < 2 * n; j++)
                    B[i, j] = B[i, j] / temp;
            }

            // Strip the augmented matrix B of the first three columns
            // to get the inverse matrix C of the original matrix A.
            Matrix C = new Matrix(n, n);

            for (int i = 0; i < n; i++)
                for (int j = n; j < 2 * n; j++)
                    C[i, j - n] = B[i, j];

            // Return the inverse matrix C.
            return C;
        }

        public static bool Inverse(Matrix A)=> Inverse(A.m);

        /// <summary>
        /// </summary>
        /// <param name="A">matrix to inverts</param>
        /// <param name="n">max order of matrix</param>
        public static unsafe bool Inverse(float* A, int n)
        {
            int i, j, k;

            int idx(int r, int c) => r * n + c;          
            void swap(ref float x, ref float y) { float t = x; x = y; y = t; }

            // Initialize the augmented matrix [A|I].
            Span<float> a = new Span<float>(A, n * n);
            Span<float> inv = stackalloc float[n * n];

            //initilize identity
            inv.Clear();
            for (i = 0; i < n; i++) inv[idx(i, i)] = 1;

            // Swap rows of the augmented matrix B.
            for (i = n - 1; i > 0; i--)
            {
                if (a[idx(i - 1, 0)] >= a[idx(i, 0)]) continue;
                for (j = 0; j < n; j++)
                    swap(ref inv[idx(i, j)], ref inv[idx(i - 1, j)]);
            }

            // Substract each row by a multiple of another row.
            for (i = 0; i < n; i++)
                for (j = 0; j < n; j++)
                {
                    if (i == j) continue;        
                    float ratio = a[idx(j, i)] / a[idx(i, i)];
                    for (k = 0; k < n; k++)
                    {
                        a[idx(j, k)] -= a[idx(i, k)] * ratio;
                        inv[idx(j, k)] -= inv[idx(i, k)] * ratio;
                    }
                }

            // Divide each row element by the diagonal element.
            for (i = 0; i < n; i++)
            {
                float temp = a[idx(i, i)];
                for (j = 0; j < n; j++)
                {
                    a[idx(i, j)] /= temp;
                    inv[idx(i, j)] /= temp;
                }
            }

            inv.CopyTo(a);
                
            return true;
        }
        public unsafe static bool Inverse(float[,] A)
        {
            int n = A.GetLength(0);
            if (n != A.GetLength(1)) throw new ArithmeticException("not a square matrix");
            fixed (float* p = A) { return Inverse(p, n); }
        }
        public unsafe static bool Inverse(float[] A)
        {
            int n = (int)Math.Sqrt(A.Length);
            fixed (float* p = A) { return Inverse(p, n); }
        }
        /// <summary>
        /// </summary>
        /// <param name="n">minimum rank of matrix</param>
        /// <typeparam name="T">must be castable to float</typeparam>
        public unsafe static bool Inverse<T>(ref T A, int n) where T : unmanaged { fixed (T* p = &A) return Inverse((float*)p, n); }
        public unsafe static bool Inverse<T>(ref T A) where T : unmanaged { fixed (T* p = &A) return Inverse((float*)p, (int)Math.Sqrt(Marshal.SizeOf<T>() / sizeof(float))); }
    }
}
