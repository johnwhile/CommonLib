using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using Common.Maths;

namespace Test_GraphicMath
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static unsafe void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VoronoidForm());
        }
    }
#if FALSE
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static unsafe void Main()
        {
            float[,] m = new float[,]
            {
                { 1, 1, 3},
                { 1, 3,-3},
                {-2,-4,-4}
            };
            Matrix M = new Matrix(m);


            //3   1   1.5
            //-1.25 - 0.25 - 0.75
            //- 0.25 - 0.25 - 0.25

            //Matrix.Inverse(ref m, 4);
            //Debugg.Message(m);

            Matrix.Inverse(m);
            Debugg.Message(new Matrix3x3f(m));

            //Matrix Inv = Matrix.Inverse(M);
            //Debugg.Message(Inv.ToString());



            float x = 0.285714286f;
            float y = 0.8f;
            float z = 0.5f;
            var A = new Matrix3x3f(
                -1, x, x,
                y, -1, y,
                z, z, -1);

            Vector b = new Vector(-2.857142857f, -26f, 25f);
            Vector result = new Vector(20, 50, 10);


            Vector c = A * result;

            Debug.Print(c.ToStringRounded());


            Matrix.QRDecomposition(A, out var Q, out var R);
            var InvQR = Matrix.InverseUpperTriangular(R) * Q.Traspose();




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MatrixFrom4Corners());
        }
    }
#endif
}
