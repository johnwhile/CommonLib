using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Common.Maths
{
    public partial class Matrix
    {
        /// <summary>
        /// Calculates the QR decomposition of the given matrix A
        /// (see https://en.wikipedia.org/wiki/QR_decomposition).
        /// </summary>
        /// <param name="A">The matrix to decompose.</param>
        /// <param name="U">The Q part of the decomposition.</param>
        /// <param name="R">The R part of the decomposition.</param>
        public static void QRDecomposition(Matrix A, out Matrix Q, out Matrix R)
        {
            int n = A.Rows;

            // Duplicate the original matrix A so it stays intact.
            Matrix U = A.Clone();

            Vector u = new Vector(n);
            Vector v = new Vector(n);
            Vector uk = new Vector(n);


            // Calculate the U matrix using the Gram–Schmidt process
            // (see https://en.wikipedia.org/wiki/Gram%E2%80%93Schmidt_process).
            // (see https://www.math.ucla.edu/~yanovsky/Teaching/Math151B/handouts/GramSchmidt.pdf).
            for (int j = 1; j < n; j++)
            {
                U.GetColumn(j, ref u);
                U.GetColumn(j, ref v);

                
                for (int k = j - 1; k >= 0; k--)
                {
                    U.GetColumn(k, ref uk);

                    //Projects the vector orthogonally onto vector "u".
                    float scale = Vector.Dot(uk, v) / Vector.Dot(uk, uk);
                    Vector.Multiply(uk, scale, ref uk);
                    
                    Vector.Sub(u, uk, ref u);
                }

                // Update the column entries in U.
                for (int i = 0; i < n; i++)
                    U[i, j] = u[i];

            }

            // Normalize the column vectors of U.
            for (int j = 0; j < n; j++)
            {
                U.GetColumn(j, ref u);

                float magnitude = u.Magnitude;

                // Update the column entries in U.
                for (int i = 0; i < n; i++)
                    U[i, j] = u[i] / magnitude;

            }
            // Calculate the R part of the decomposition.
            // R = U^t * A
            R = U.Traspose() * A;
            // The U matrix is now the Q part of the decomposition.
            Q = U;
        }
    }
}
