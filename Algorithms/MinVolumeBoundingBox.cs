using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Tools
{
    public static class MinVolumeBoundingBox
    {
        public static OBBox GetMinimumEnclosedOBB2(IList<Vector3f> Points)
        {
            Vector3d mean = Vector3d.Zero;
            int Count = Points.Count;
            
            // Compute the center of mass and inertia tensor of the points.
            //for (int i = 0; i < Count; i++) mean += Points[i]; mean /= Count;
            
            // different approc to avoid float increase too many.
            for (int i = 0; i < Count; i++)
            {
                mean.x += Points[i].x / Count;
                mean.y += Points[i].y / Count;
                mean.z += Points[i].z / Count;
            }


            // Covariance matrix, is simmetric so Cxy = Cyx etc...
            //   | Cxx  Cxy  Cxz |
            //   | Cyx  Cyy  Cyz |
            //   | Czx  Czy  Czz |

            double Cxx, Cxy, Cxz, Cyy, Cyz, Czz;
            Cxx = Cxy = Cxz = Cyy = Cyz = Czz = 0;
 
            for (int i = 0; i < Count; i++)
            {
                Vector3f p = Points[i];
                Cxx += p.x * p.x - mean.x * mean.x;
                Cxy += p.x * p.y - mean.x * mean.y;
                Cxz += p.x * p.z - mean.x * mean.z;
                Cyy += p.y * p.y - mean.y * mean.y;
                Cyz += p.y * p.z - mean.y * mean.z;
                Czz += p.z * p.z - mean.z * mean.z;
            }
            Matrix3x3f C = new Matrix3x3f(
                Cxx, Cxy, Cxz,
                Cxy, Cyy, Cyz,
                Cxz, Cyz, Czz);

            // extract the eigenvalues and eigenvectors from C.
            // det(C-yI) = 0;

            Vector3d[] eigvec = new Vector3d[3]
            {
                Vector3d.UnitX, 
                Vector3d.UnitY,
                Vector3d.UnitZ 
            };
            Vector3d eigval = Vector3d.Zero;

            // find the right, up and forward vectors from the eigenvectors
            Vector3d r = eigvec[0];
            Vector3d u = eigvec[1];
            Vector3d f = eigvec[2];
            r.Normalize();
            u.Normalize();
            f.Normalize();

            // set the rotation matrix using the eigvenvectors
            Matrix3x3f m_rot = new Matrix3x3f(r, u, f);

            // now build the bounding box extents in the rotated frame
            Vector3d m_min = Vector3d.PosInf;
            Vector3d m_max = Vector3d.NegInf;
            for (int i = 0; i < Count; i++)
            {
                Vector3f p = Points[i];
                // dot product to project point to axis of new obb
                double x = r.x * p.x + r.y * p.y + r.z * p.z;
                double y = u.x * p.x + u.y * p.y + u.z * p.z;
                double z = f.x * p.x + f.y * p.y + f.z * p.z;
                m_min.Min(x, y, z);
                m_max.Max(x, y, z);
            }

            // set the center of the OBB to be the average of the 
            // minimum and maximum, and the extents be half of the
            // difference between the minimum and maximum
            Vector3d center = (m_max + m_min) * 0.5;
            Vector3d extend = (m_max - m_min) * 0.5;

            // do center in world coordinate system
            return new OBBox(r, u, f, extend, center);

            /*
            double s0, s1, s2;
            int solutions = ComputeRoots(Cxx, Cyy, Czz, Cxy, Cxz, Cyz, out s0, out s1, out s2);
            Vector3 V0, V1, V2;
            switch (solutions)
            {
                case 1:
                case 2:
                case 3:
                    float m00 = C.m00;
                    float m11 = C.m11;
                    float m22 = C.m22;

                    C.m00 = m00 - (float)s0;
                    C.m11 = m11 - (float)s0;
                    C.m22 = m22 - (float)s0;
                    Solve3Equation3Variable(C, out V0.x, out V0.y, out V0.z);
                    C.m00 = m00 - (float)s1;
                    C.m11 = m11 - (float)s1;
                    C.m22 = m22 - (float)s1;
                    Solve3Equation3Variable(C, out V1.x, out V1.y, out V1.z);
                    C.m00 = m00 - (float)s2;
                    C.m11 = m11 - (float)s2;
                    C.m22 = m22 - (float)s2;
                    Solve3Equation3Variable(C, out V2.x, out V2.y, out V2.z);
                    break;
                default:
                    // set them to arbitrary orthonormal basis set
                    V0 = Vector3.UnitX;
                    V1 = Vector3.UnitY;
                    V2 = Vector3.UnitZ;
                    break;
            }
            V0.Normalize();
            V1.Normalize();
            V2.Normalize();

            return new OBBox(V0, V1, V2, Vector3.One, mean);
            */
        }


        /// <summary>
        /// Find the approximate minimum oriented bounding box containing a set of 
        /// points.  Exact computation of minimum oriented bounding box is possible but 
        /// is slower and requires a more complex algorithm.
        /// The algorithm works by computing the inertia tensor of the points and then
        /// using the eigenvectors of the intertia tensor as the axes of the box.
        /// Computing the intertia tensor of the convex hull of the points will usually 
        /// result in better bounding box but the computation is more complex. 
        /// Exact computation of the minimum oriented bounding box is possible but the
        /// best know algorithm is O(N^3) and is significanly more complex to implement.
        /// http://www.gamedev.net/topic/548644-help-needed-computing-a-minimum-bounding-box-for-arbitrary-vertices/
        /// https://github.com/jeanlauliac/eru/blob/master/source/xnaCollision.cpp
        /// </summary>
        public static OBBox GetMinimumEnclosedOBB(IList<Vector3f> Points)
        {
           
            Vector3f CenterOfMass = Vector3f.Zero;
            int Count = Points.Count;
            // Compute the center of mass and inertia tensor of the points.
            for (int i = 0; i < Count; i++)
            {
                CenterOfMass += Points[i];
            }
            CenterOfMass /= Count;

            // Compute the inertia tensor of the points around the center of mass.
            // Using the center of mass is not strictly necessary, but will hopefully
            // improve the stability of finding the eigenvectors.
            Vector3f XX_YY_ZZ = Vector3f.Zero;
            Vector3f XY_XZ_YZ = Vector3f.Zero;

            for (int i = 0; i < Count; i++)
            {
                Vector3f Point = Points[i] - CenterOfMass;

                XX_YY_ZZ += Point * Point;

                Vector3f XXY = new Vector3f(Point.x, Point.x, Point.y);
                Vector3f YZZ = new Vector3f(Point.y, Point.z, Point.z);

                XY_XZ_YZ += XXY * YZZ;
            }

            Vector3f v1 = Vector3f.Zero;
            Vector3f v2 = Vector3f.Zero;
            Vector3f v3 = Vector3f.Zero;

            // Compute the eigenvectors of the inertia tensor.
            CalculateEigenVectorsFromCovarianceMatrix(XX_YY_ZZ.x, XX_YY_ZZ.y,XX_YY_ZZ.z,
                                                       XY_XZ_YZ.x, XY_XZ_YZ.y,XY_XZ_YZ.z,
                                                       ref v1, ref v2, ref v3);

            // Put them in a matrix.
            Matrix4x4f R = new Matrix4x4f(
                v1.x, v1.y, v1.z, 0,
                v2.x, v2.y, v2.z, 0,
                v3.x, v3.y, v3.z, 0,
                0, 0, 0, 1);


            // Multiply by -1 to convert the matrix into a right handed coordinate 
            // system (Det ~= 1) in case the eigenvectors form a left handed 
            // coordinate system (Det ~= -1) because XMQuaternionRotationMatrix only 
            // works on right handed matrices.
            float Det = R.Determinant;

            if (Det<0)
            {
                R *= -1;
            }

            // Get the rotation quaternion from the matrix.
            Quaternion4f Orientation = R.GetQuaternion();

            // Make sure it is normal (in case the vectors are slightly non-orthogonal).
            Orientation.Normalize();

            // Rebuild the rotation matrix from the quaternion.
            R = (Matrix4x4f)Orientation;

            // Build the rotation into the rotated space.
            Matrix4x4f InverseR = Matrix4x4f.Traspose(R);

            // Find the minimum OBB using the eigenvectors as the axes.
            Vector3f min = new Vector3f(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3f max = new Vector3f(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < Count; i++)
            {
                Vector3f point = Points[i].TransformCoordinate(in InverseR);

                if (point.x > max.x) max.x = point.x;
                if (point.y > max.y) max.y = point.y;
                if (point.z > max.z) max.z = point.z;
                if (point.x < min.x) min.x = point.x;
                if (point.y < min.y) min.y = point.y;
                if (point.z < min.z) min.z = point.z;
            }

            // Rotate the center into world space.
            Vector3f Center = ((min + max) * 0.5f).TransformCoordinate(in R);

            // Store center, extents, and orientation.
            Vector3f Extend = (max - min) * 0.5f;

            Matrix4x4f T = Matrix4x4f.Translating(Center);
            Matrix4x4f S = Matrix4x4f.Scaling(in Extend);
            Matrix4x4f TRS = T * R * S;
            return new OBBox(TRS);
        }

        static bool CalculateEigenVectorsFromCovarianceMatrix(
            float Cxx, float Cyy, float Czz,
            float Cxy, float Cxz, float Cyz,
            ref Vector3f pV1, ref Vector3f pV2, ref Vector3f pV3)
        {
            float e, f, g, ev1, ev2, ev3;

            // Calculate the eigenvalues by solving a cubic equation.
            e = -(Cxx + Cyy + Czz);
            f = Cxx * Cyy + Cyy * Czz + Czz * Cxx - Cxy * Cxy - Cxz * Cxz - Cyz * Cyz;
            g = Cxy * Cxy * Czz + Cxz * Cxz * Cyy + Cyz * Cyz * Cxx - Cxy * Cyz * Cxz * 2.0f - Cxx * Cyy * Czz;

            if (!SolveCubic(e, f, g, out ev1, out ev2, out ev3))
            {
                // set them to arbitrary orthonormal basis set
                pV1 = Vector3f.UnitX;
                pV2 = Vector3f.UnitY;
                pV3 = Vector3f.UnitZ;
                return false;
            }
            return CalculateEigenVectors(Cxx, Cxy, Cxz, Cyy, Cyz, Czz, ref ev1, ref ev2, ref ev3, ref pV1, ref pV2, ref pV3);
        }

        /// <summary>
        /// Trigonometric (and hyperbolic) method
        /// 
        /// </summary>
        static bool SolveCubic(float e, float f, float g, out float t, out float u, out float v)
        {
            // t^3+pt+q=0

            float p = f - e * e / 3.0f;
            float q = g - e * f / 3.0f + e * e * e * 2.0f / 27.0f;
            float h = q * q / 4.0f + p * p * p / 27.0f;

            if (h > 0.0)
            {
                t = 0;
                u = 0;
                v = 0;
                return false; // only one real root
            }

            if ((h == 0.0) && (q == 0.0)) // all the same root
            {
                t = -e / 3;
                u = -e / 3;
                v = -e / 3;

                return true;
            }

            float rc, theta, costh3, sinth3;
            float d = (float)System.Math.Sqrt(q * q / 4.0f - h);
            
            if (d < 0)
                rc = -(float)System.Math.Pow(-d, 1.0f / 3.0f);
            else
                rc = (float)System.Math.Pow(d, 1.0f / 3.0f);

            theta = (float)System.Math.Acos(-q / (2.0f * d));
            costh3 = (float)System.Math.Cos(theta / 3.0f);
            sinth3 = (float)(Maths.Mathelp.Sqrt3 * System.Math.Sin(theta / 3.0f));
            t = 2.0f * rc * costh3 - e / 3.0f;
            u = -rc * (costh3 + sinth3) - e / 3.0f;
            v = -rc * (costh3 - sinth3) - e / 3.0f;

            return true;
        }


        static bool CalculateEigenVectors(float m11, float m12, float m13,
                                          float m22, float m23, float m33,
                                          ref float e1, ref float e2, ref float e3,
                                          ref Vector3f pV1, ref Vector3f pV2, ref Vector3f pV3)
        {
            Vector3f vTmp, vUp, vRight;

            bool e12, e13, e23;

            vUp = new Vector3f(0, 1, 0);
            vRight = new Vector3f(1, 0, 0);

            pV1 = CalculateEigenVector(m11, m12, m13, m22, m23, m33, e1);
            pV2 = CalculateEigenVector(m11, m12, m13, m22, m23, m33, e2);
            pV3 = CalculateEigenVector(m11, m12, m13, m22, m23, m33, e3);


            bool v1z = (pV1 == Vector3f.Zero);
            bool v2z = (pV2 == Vector3f.Zero);
            bool v3z = (pV3 == Vector3f.Zero);

            e12 = (Maths.Mathelp.ABS(Vector3f.Dot(pV1, pV2))) > 0.1f; // check for non-orthogonal vectors
            e13 = (Maths.Mathelp.ABS(Vector3f.Dot(pV1, pV3))) > 0.1f;
            e23 = (Maths.Mathelp.ABS(Vector3f.Dot(pV2, pV3))) > 0.1f;

            if ((v1z && v2z && v3z) || (e12 && e13 && e23) ||
                (e12 && v3z) || (e13 && v2z) || (e23 && v1z)) // all eigenvectors are 0- any basis set
            {
                pV1 = new Vector3f(1, 0, 0);
                pV2 = new Vector3f(0, 1, 0);
                pV3 = new Vector3f(0, 0, 1);
                return true;
            }

            if (v1z && v2z)
            {
                vTmp = Vector3f.Cross(vUp, pV3);
                if (vTmp.LengthSq < 1e-5f)
                {
                    vTmp = Vector3f.Cross(vRight, pV3);
                }
                pV1 = vTmp.Normal;
                pV2 = Vector3f.Cross(pV3, pV1);
                return true;
            }

            if (v3z && v1z)
            {
                vTmp = Vector3f.Cross(vUp, pV2);
                if (vTmp.LengthSq < 1e-5f)
                {
                    vTmp = Vector3f.Cross(vRight, pV2);
                }
                pV3 = vTmp.Normal;
                pV1 = Vector3f.Cross(pV2, pV3);
                return true;
            }

            if (v2z && v3z)
            {
                vTmp = Vector3f.Cross(vUp,pV1);
                if (vTmp.LengthSq < 1e-5f)
                {
                    vTmp = Vector3f.Cross(vRight, pV1);
                }
                pV2 = vTmp.Normal;
                pV3 = Vector3f.Cross(pV1, pV2);
                return true;
            }

            if ((v1z) || e12)
            {
                pV1 = Vector3f.Cross(pV2, pV3);
                return true;
            }

            if ((v2z) || e23)
            {
                pV2 = Vector3f.Cross(pV3, pV1);
                return true;
            }

            if ((v3z) || e13)
            {
                pV3 = Vector3f.Cross(pV1, pV2);
                return true;
            }

            return true;
        }


        static Vector3f CalculateEigenVector(float m11, float m12, float m13,
                                             float m22, float m23, float m33, float e)
        {
            float f1, f2, f3;

            Vector3f vTmp;
            vTmp.x = (m12 * m23 - m13 * (m22 - e));
            vTmp.y = (m13 * m12 - m23 * (m11 - e));
            vTmp.z = ((m11 - e) * (m22 - e) - m12 * m12);


            if (vTmp == Vector3f.Zero) // planar or linear
            {
                // we only have one equation - find a valid one
                if ((m11 - e != 0.0) || (m12 != 0.0) || (m13 != 0.0))
                {
                    f1 = m11 - e; f2 = m12; f3 = m13;
                }
                else if ((m12 != 0.0) || (m22 - e != 0.0) || (m23 != 0.0))
                {
                    f1 = m12; f2 = m22 - e; f3 = m23;
                }
                else if ((m13 != 0.0) || (m23 != 0.0) || (m33 - e != 0.0))
                {
                    f1 = m13; f2 = m23; f3 = m33 - e;
                }
                else
                {
                    // error, we'll just make something up - we have NO context
                    f1 = 1.0f; f2 = 0.0f; f3 = 0.0f;
                }

                if (f1 == 0.0)
                    vTmp.x = 0.0f;
                else
                    vTmp.x = 1.0f;

                if (f2 == 0.0)
                    vTmp.y = 0.0f;
                else
                    vTmp.y = 1.0f;

                if (f3 == 0.0)
                {
                    vTmp.z = 0.0f;
                    // recalculate y to make equation work
                    if (m12 != 0.0)
                        vTmp.y = -f1 / f2;
                }
                else
                {
                    vTmp.z = (f2 - f1) / f3;
                }
            }

            if (vTmp.LengthSq > 1e-5f)
            {
                return vTmp.Normal;
            }
            else
            {
                // Multiply by a value large enough to make the vector non-zero.
                vTmp *= 1e5f;
                return vTmp.Normal;
            }
        }

        /// <summary>
        /// Trovare gli autovalori della matrice simmetrica 3x3
        /// det(M - xI) = 0
        /// </summary>
        public static int ComputeRoots(double m00, double m11, double m22, double m01, double m02, double m12,
            out double x0, out double x1, out double x2)
        {
            //                    | a00-λ   a01   a02  |
            // 0 = − det(A − λI)= |  a10   a11-λ  a12  | = λ^3 − c2*λ^2 + c1*λ − c0
            //                    |  a20    a21  a22-λ |
            //
            // c0 = a00a11a22 + 2a01a02a12 − a00a212 − a11a202 − a22a201
            // c1 = a00a11 − a201 + a00a22 − a202 + a11a22 − a212
            // c2 = a00 + a11 + a22

            double c0 = m01 * m01 * m22 + m02 * m02 * m11 + m12 * m12 * m00 - 2 * (m01 * m12 * m02) - m00 * m11 * m22;
            double c1 = m00 * m11 + m11 * m22 + m22 * m00 - m01 * m01 - m02 * m02 - m12 * m12;
            double c2 = -(m00 + m11 + m22);

            double inv3 = 1.0 / 3.0;
            double c2Div3 = c2 * inv3;
            double aDiv3 = (c1 - c2 * c2Div3) * inv3;

            if (aDiv3 > 0) aDiv3 = 0;
            double mbDiv2 = 0.5 * (c0 + c2Div3 * (2.0 * c2Div3 * c2Div3 - c1));


            return SolveCubicPolynomial(c2, c1, c0, out x0, out x1, out x2);
        }



        /// <summary>
        /// x^3 + A x^2 + B x + C = 0
        /// </summary>
        /// <returns>return the number of solutions</returns>
        public static int SolveCubicPolynomial(double A, double B, double C, out double x0, out double x1, out double x2)
        {
            int num = 0;

            x0 = x1 = x2 = 0;

            // substitute x = y - A / 3 to eliminate the quadric term: x^3 + px + q = 0

            double sq_A = A * A;
            double p = 1.0 / 3.0 * (-1.0 / 3.0 * sq_A + B);
            double q = 1.0 / 2.0 * (2.0 / 27.0 * A * sq_A - 1.0 / 3.0 * A * B + C);

            // use Cardano's formula
            double cb_p = p * p * p;
            double D = q * q + cb_p;

            if (Maths.Mathelp.isZero(D))
            {
                if (Maths.Mathelp.isZero(q))
                {
                    // one triple solution
                    x0 = 0;
                    num = 1;
                }
                else
                {
                    // one single and one double solution
                    double u = Maths.Mathelp.Cbrt(-q);
                    x0 = 2.0 * u;
                    x1 = -u;
                    num = 2;
                }
            }
            else
                if (D < 0.0)
                {
                    // casus irreductibilis: three real solutions
                    double phi = 1.0 / 3.0 * System.Math.Acos(-q / System.Math.Sqrt(-cb_p));
                    double t = 2.0 * System.Math.Sqrt(-p);
                    x0 = t * System.Math.Cos(phi);
                    x1 = -t * System.Math.Cos(phi + System.Math.PI / 3.0);
                    x2 = -t * System.Math.Cos(phi - System.Math.PI / 3.0);
                    num = 3;
                }
                else
                {
                    // one real solution
                    double sqrt_D = System.Math.Sqrt(D);
                    double u = Maths.Mathelp.Cbrt(sqrt_D + System.Math.Abs(q));
                    if (q > 0.0)
                        x0 = -u + p / u;
                    else
                        x0 = u - p / u;
                    num = 1;
                }

            // resubstitute
            double sub = 1.0 / 3.0 * A;

            if (num > 0)
            {
                x0 -= sub;
                if (num > 1)
                {
                    x1 -= sub;
                    if (num > 2)
                    {
                        x2 -= sub;
                    }
                }

            }
            return num;
        }


        public static void Solve3Equation3Variable(Matrix3x3f m, out float x, out float y, out float z)
        {
            Matrix3x3f mx = m;
            Matrix3x3f my = m;
            Matrix3x3f mz = m;

            mx.m00 = mx.m10 = m.m20 = 0;
            mx.m01 = mx.m11 = m.m21 = 0;
            mx.m02 = mx.m12 = m.m22 = 0;

            float D = m.Determinant;
            if (D > -1e-7 || D < 1e-7)
            {
                x = y = z = 0;
            }
            else
            {
                float Dx = mx.Determinant;
                float Dy = my.Determinant;
                float Dz = mz.Determinant;

                x = Dx > -1e-7 || Dx < 1e-7 ? 0 : Dx / D;
                y = Dy > -1e-7 || Dy < 1e-7 ? 0 : Dy / D;
                z = Dz > -1e-7 || Dz < 1e-7 ? 0 : Dz / D;
            }
        }

    }

}
