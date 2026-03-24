using System;
using System.Collections.Generic;
using System.Text;

using Common.Tools;

namespace Common.Maths
{
    public enum Overlap : byte
    {
        ERROR = 0,
        INSIDE,
        INTERSECT,
        OUTSIDE
    }

    public enum IntersectResult : byte
    {
        /// <summary> Not intersection </summary>
        None = 0,
        /// <summary> Intersect in a unique point </summary>
        OnePoint = 1,
        /// <summary> Intersect in two points </summary>
        TwoPoint = 2,
        /// <summary> The intersected point is in segment projection </summary>
        OnePointProjected = 3,
        /// <summary> The two intersected points are in segment projection </summary>
        TwoPointProjected = 4,
        /// <summary> Line lies to Plane (all line's points are in plane)</summary>
        Lies = 5,
        /// <summary> Not intersection but Plane or Line parallel to plane </summary>
        Parallel = 6,
    }

    /// <summary>
    /// A collection of all possible intersection between primitives geometries. The purpose is only to get boolean value of intersection
    /// </summary>
    public static class PrimitiveIntersections
    {
        /// <summary>
        /// Positive constant
        /// </summary>
        const float SMALL = (float)(1e-8);

        #region Utils

        static float ABS(float f) { return f > 0 ? f : -f; }
        static float DOT(Vector3f a, Vector3f b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
        static Vector3f CROSS(Vector3f a, Vector3f b) { return Vector3f.Cross(a, b); }


        static Vector3f v0, v1, v2, boxhalfsize;
        static float minf, maxf, rad;

        // box-sphere squared distance
        static void AABBSPHERESQDIST(float c, float min, float max, ref float d)
        {
            if (c < min) d += (c - min) * (c - min);
            else if (c > max) d += (c - max) * (c - max);
        }
        // find box positive and negative vertex from plane 
        static void PLANEPOSNEG(float n, float min, float max, out float pos, out float neg)
        {
            if (n > 0)
            {
                pos = max;
                neg = min;
            }
            else
            {
                pos = min;
                neg = max;
            }
        }
        // box-sphere squared distance and quick reject
        static bool QRI(float c, float r, float min, float max, ref float d)
        {
            float e;
            if ((e = c - min) < 0)
            {
                if (e < -r) return false;
                d += e * e;
            }
            else if ((e = c - max) > 0)
            {
                if (e > r) return false;
                d += e * e;
            }
            return true;
        }

        // from Real-Time Collision Detection by Christer Ericson, published
        // by Morgan Kaufmann Publishers, Copyright 2005 Elsevier Inc
        static Vector3f CLOSESTPOINTTRIANGLE(Vector3f p, Vector3f a, Vector3f b, Vector3f c)
        {
            Vector3f ab = b - a;
            Vector3f ac = c - a;
            Vector3f bc = c - b;

            // Compute parametric position s for projection P' of P on AB,
            // P' = A + s*AB, s = snom/(snom+sdenom)
            float snom = Vector3f.Dot(p - a, ab), sdenom = Vector3f.Dot(p - b, a - b);

            // Compute parametric position t for projection P' of P on AC,
            // P' = A + t*AC, s = tnom/(tnom+tdenom)
            float tnom = Vector3f.Dot(p - a, ac), tdenom = Vector3f.Dot(p - c, a - c);

            if (snom <= 0.0f && tnom <= 0.0f) return a; // Vertex region early out

            // Compute parametric position u for projection P' of P on BC,
            // P' = B + u*BC, u = unom/(unom+udenom)
            float unom = Vector3f.Dot(p - b, bc), udenom = Vector3f.Dot(p - c, b - c);

            if (sdenom <= 0.0f && unom <= 0.0f) return b; // Vertex region early out
            if (tdenom <= 0.0f && udenom <= 0.0f) return c; // Vertex region early out

            // P is outside (or on) AB if the triple scalar product [N PA PB] <= 0
            Vector3f n = Vector3f.Cross(b - a, c - a);
            float vc = Vector3f.Dot(n, Vector3f.Cross(a - p, b - p));
            // If P outside AB and within feature region of AB,
            // return projection of P onto AB
            if (vc <= 0.0f && snom >= 0.0f && sdenom >= 0.0f)
                return a + snom / (snom + sdenom) * ab;

            // P is outside (or on) BC if the triple scalar product [N PB PC] <= 0
            float va = Vector3f.Dot(n, Vector3f.Cross(b - p, c - p));
            // If P outside BC and within feature region of BC,
            // return projection of P onto BC
            if (va <= 0.0f && unom >= 0.0f && udenom >= 0.0f)
                return b + unom / (unom + udenom) * bc;

            // P is outside (or on) CA if the triple scalar product [N PC PA] <= 0
            float vb = Vector3f.Dot(n, Vector3f.Cross(c - p, a - p));
            // If P outside CA and within feature region of CA,
            // return projection of P onto CA
            if (vb <= 0.0f && tnom >= 0.0f && tdenom >= 0.0f)
                return a + tnom / (tnom + tdenom) * ac;

            // P must project inside face region. Compute Q using barycentric coordinates
            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc / (va + vb + vc)
            return u * a + v * b + w * c;
        }
        
        static bool AXISTEST_X01(float a, float b, float fa, float fb)
        {
            float p0 = a * v0.y - b * v0.z;
            float p2 = a * v2.y - b * v2.z;
            if (p0 < p2) { minf = p0; maxf = p2; }
            else { minf = p2; maxf = p0; }
            rad = fa * boxhalfsize.y + fb * boxhalfsize.z;

            return (minf > rad || maxf < -rad);
        }
        static bool AXISTEST_Y02(float a, float b, float fa, float fb)
        {
            float p0 = -a * v0.x + b * v0.z;
            float p2 = -a * v2.x + b * v2.z;
            if (p0 < p2) { minf = p0; maxf = p2; }
            else { minf = p2; maxf = p0; }
            rad = fa * boxhalfsize.x + fb * boxhalfsize.z;
            return (minf > rad || maxf < -rad);
        }
        static bool AXISTEST_Z12(float a, float b, float fa, float fb)
        {
            float p1 = a * v1.x - b * v1.y;
            float p2 = a * v2.x - b * v2.y;
            if (p1 < p2) { minf = p1; maxf = p2; }
            else { minf = p2; maxf = p1; }
            rad = fa * boxhalfsize.x + fb * boxhalfsize.y;
            return (minf > rad || maxf < -rad);
        }
        static bool AXISTEST_Z0(float a, float b, float fa, float fb)
        {
            float p0 = a * v0.x - b * v0.y;
            float p1 = a * v1.x - b * v1.y;
            if (p0 < p1) { minf = p0; maxf = p1; }
            else { minf = p1; maxf = p0; }
            rad = fa * boxhalfsize.x + fb * boxhalfsize.y;
            return (minf > rad || maxf < -rad);
        }
        static bool AXISTEST_X2(float a, float b, float fa, float fb)
        {
            float p0 = a * v0.y - b * v0.z;
            float p1 = a * v1.y - b * v1.z;
            if (p0 < p1) { minf = p0; maxf = p1; }
            else { minf = p1; maxf = p0; }
            rad = fa * boxhalfsize.y + fb * boxhalfsize.z;
            return (minf > rad || maxf < -rad);
        }
        static bool AXISTEST_Y1(float a, float b, float fa, float fb)
        {
            float p0 = -a * v0.x + b * v0.z;
            float p1 = -a * v1.x + b * v1.z;
            if (p0 < p1) { minf = p0; maxf = p1; }
            else { minf = p1; maxf = p0; }
            rad = fa * boxhalfsize.x + fb * boxhalfsize.z;
            return (minf > rad || maxf < -rad);
        }

        #endregion

        #region 2D
        /// <summary>
        /// Line - AABRectangle intersection, generalized for all lines primitives.
        /// </summary>
        /// <param name="tmax">the line parametric value of first intersection point</param>
        public static bool IntersectLineAABR(Vector2f point, Vector2f dir, Vector2f min, Vector2f max, out float tmin, out float tmax)
        {
            float divx = 1 / dir.x;
            float divy = 1 / dir.y;
            float tx0, tx1, ty0, ty1;

            if (divx < 0)
            {
                tx0 = (max.x - point.x) * divx;
                tx1 = (min.x - point.x) * divx;
            }
            else
            {
                tx0 = (min.x - point.x) * divx;
                tx1 = (max.x - point.x) * divx;
            }

            if (divy < 0)
            {
                ty0 = (max.y - point.y) * divy;
                ty1 = (min.y - point.y) * divy;
            }
            else
            {
                ty0 = (min.y - point.y) * divy;
                ty1 = (max.y - point.y) * divy;
            }

            tmin = Mathelp.MAX(tx0, ty0);
            tmax = Mathelp.MIN(tx1, ty1);

            return tmax > tmin;
        }
        /// <summary>
        /// Line - Circle intersection
        /// </summary>
        public static bool Intersect_Line_Circle(Vector2f point, Vector2f dir, Vector2f center, float radius, out float tmin, out float tmax)
        {
            tmin = tmax = float.NegativeInfinity;

            if (radius < 0) return false;

            Vector2f v = point - center;

            float a = Vector2f.Dot(dir, dir);
            float b = 2.0f * Vector2f.Dot(v, dir);
            float c = Vector2f.Dot(v, v) - (radius * radius);
            float d = (b * b) - (4 * a * c);

            // if discriminant is negative the picking ray missed the sphere, otherwise it intersected the sphere.
            if (d < 0.0f) return false;

            d = (float)System.Math.Sqrt(d);
            tmin = 0.5f * (-b - d) / a;
            tmax = 0.5f * (-b + d) / a;

            return true;
        }

        /// <summary>
        /// Circle - AABRectangle intersection
        /// TODO : check OverlapType
        /// </summary>
        /// <param name="rectangle_overlap">rectangle overlap respect circle</param>
        public static bool Intersect_AABR_Circle(Vector2f min, Vector2f max, Vector2f center, float radius, out Overlap rectangle_overlap)
        {
            rectangle_overlap = Overlap.ERROR;

            // middle point of rectangle
            float midx = (max.x + min.x) * 0.5f;
            float midy = (max.y + min.y) * 0.5f;

            // center of circle relative to rectangle
            float cx = Mathelp.ABS(center.x - midx);
            float cy = Mathelp.ABS(center.y - midy);

            // half-size of rectangle
            float hx = (max.x - min.x) * 0.5f;
            float hy = (max.y - min.y) * 0.5f;

            // center is outsize rectangle + radius boundary
            if (cx > hx + radius) { rectangle_overlap = Overlap.OUTSIDE; return false; }
            if (cy > hy + radius) { rectangle_overlap = Overlap.OUTSIDE; return false; }

            // center is in rectangle
            if (cx <= hx) { rectangle_overlap = Overlap.INSIDE; return true; }
            if (cy <= hy) { rectangle_overlap = Overlap.INSIDE; return true; }

            // center is inside arc-corner zone
            float sqCornerdist = (cx - hx) * (cx - hx) + (cy - hy) * (cy - hy);

            rectangle_overlap = Overlap.INTERSECT;

            return sqCornerdist < radius * radius;
        }
       
        /// <summary>
        /// Circle - AABRectangle intersection
        /// </summary>
        public static bool Intersect_AABR_Circle(Vector2f min, Vector2f max, Vector2f center, float radius)
        {
            Overlap rectangle_overlap;
            return Intersect_AABR_Circle(min,max,center,radius,out rectangle_overlap);
        }

        /// <summary>
        /// Circle - AABRectangle intersection
        /// Faster than IntersectAABRCircle()
        /// </summary>
        public static bool Intersect_AABR_Circle_v1(Vector2f rect_center, Vector2f halfsize, Vector2f circl_center, float radius)
        {
            // center of circle relative to rectangle
            float cx = Mathelp.ABS(circl_center.x - rect_center.x);
            float cy = Mathelp.ABS(circl_center.y - rect_center.y);

            // center is outsize rectangle + radius boundary
            if (cx > halfsize.x + radius) return false;
            if (cy > halfsize.y + radius) return false;

            // center is in rectangle
            if (cx <= halfsize.x) return true;
            if (cy <= halfsize.y) return true;

            // center is inside arc-corner zone
            float sqCornerdist = (cx - halfsize.x) * (cx - halfsize.x) + (cy - halfsize.y) * (cy - halfsize.y);

            return sqCornerdist < radius * radius;
        }

        /// <summary>
        /// Circle - AABRectangle intersection
        /// Faster than both
        /// </summary>
        public static bool Intersect_AABR_Circle_v2(Vector2f min, Vector2f max, Vector2f center, float radius)
        {
            float dsq = 0;
            float rsq = radius * radius;

            AABBSPHERESQDIST(center.x, min.x, max.x, ref dsq);
            if (dsq > rsq) return false;

            AABBSPHERESQDIST(center.y, min.y, max.y, ref dsq);
            if (dsq > rsq) return false;

            return true;
        }

        /// <summary>
        /// AABRectangle - AABRectangle intersection
        /// </summary>
        public static bool Intersect_AABR_AABR(Vector2f amin, Vector2f amax, Vector2f bmin, Vector2f bmax)
        {
            if (amin.x > bmax.x || bmin.x > amax.x) return false;
            if (amin.y > bmax.y || bmin.y > amax.y) return false;
            return true;
        }
        /// <summary>
        /// Circle - Circle intersection
        /// </summary>
        public static bool Intersect_Circle_Circle(Vector2f acenter, float aradius, Vector2f bcenter, float bradius)
        {
            Vector2f c = bcenter - acenter;
            float r = aradius + bradius;
            return Vector2f.Dot(c, c) < r * r;
        }

        /// <summary>
        /// Oriented_Bounding_Rectangle - Oriented_Bounding_Rectangle intersection using separating axis theorem
        /// http://www.jkh.me/files/tutorials/Separating%20Axis%20Theorem%20for%20Oriented%20Bounding%20Boxes.pdf
        /// </summary>
        /// <remarks>
        /// <para>   C3--------C2 </para>
        /// <para>   |          | </para>
        /// <para>   C0--------C1 </para>
        /// <para>   Ax = Normal(C1-C0)</para>
        /// <para>   Ay = Normal(C3-C0)</para>
        /// <para>   Wa = Lenght(C1-C0)/2 </para>
        /// <para>   Ha = Lenght(C3-C0)/2 </para>
        /// </remarks>
        /// <param name="centerA">Center of box A, is (C0+C1+C2+C3)/4</param>
        /// <param name="Ax">First Axis of box A, is the normalized vector of C1-C0</param>
        /// <param name="Ay">Second Axis of box A, is the normalized vector of C3-C0</param>
        /// <param name="Hfa">Half Size of box A, relative to First Axis (Ax) </param>
        public static bool Intersect_OBR_OBR(
            Vector2f centerA , Vector2f Ax , Vector2f Ay , Vector2f Ha ,
            Vector2f centerB , Vector2f Bx , Vector2f By , Vector2f Hb)
        {
            Vector2f T = centerA - centerB;
            float s, ha, hb;
            
            // case L = Ax
            s = ABS(Vector2f.Dot(T, Ax));
            ha = Ha.x;
            hb = ABS(Hb.x * Vector2f.Dot(Bx, Ax)) + ABS(Hb.y * Vector2f.Dot(By, Ax));
            if (s > ha + hb) return false; // if exist at last one separate axis, the two box are separate

            // case L = Ay
            s = ABS(Vector2f.Dot(T, Ay));
            ha = Ha.y;
            hb = ABS(Hb.x * Vector2f.Dot(Bx, Ay)) + ABS(Hb.y * Vector2f.Dot(By, Ay));
            if (s > ha + hb) return false;

            // case L = Bx
            s = ABS(Vector2f.Dot(T, Bx));
            ha = ABS(Ha.x * Vector2f.Dot(Ax, Bx)) + ABS(Ha.y * Vector2f.Dot(Ay, Bx));
            hb = Hb.x;
            if (s > ha + hb) return false;

            // case L = By
            s = ABS(Vector2f.Dot(T, By));
            ha = ABS(Ha.x * Vector2f.Dot(Ax, By)) + ABS(Ha.y * Vector2f.Dot(Ay, By));
            hb = Hb.y;
            if (s > ha + hb) return false;

            return true;
        }


        #endregion

        #region 3D
        
        #region lines
        /// <summary>
        /// Line - AABBox intersection, generalized for all lines primitives
        /// </summary>
        /// <param name="tmax">the parametric value is the scalar componed of direction,so the intesection point are t*d</param>
        public static bool Intersect_Line_AABB(Vector3f point, Vector3f dir, Vector3f min, Vector3f max, out float tmin, out float tmax)
        {
            // there is a issue if d=-0.0, in floating point -0.0 == 0.0 and the "if(tmin > tmax)" return a wrong state
            float divx = 1 / dir.x;
            float divy = 1 / dir.y;

            if (divx >= 0)
            {
                tmin = (min.x - point.x) * divx;
                tmax = (max.x - point.x) * divx;
            }
            else
            {
                tmin = (max.x - point.x) * divx;
                tmax = (min.x - point.x) * divx;
            }

            float tymin, tymax;
            if (divy >= 0)
            {
                tymin = (min.y - point.y) * divy;
                tymax = (max.y - point.y) * divy;
            }
            else
            {
                tymin = (max.y - point.y) * divy;
                tymax = (min.y - point.y) * divy;
            }

            if ((tmin > tymax) || (tymin > tmax))
            {
                tmin = tmax = float.NaN;
                return false;
            }
            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;


            float divz = 1 / dir.z;
            float tzmin, tzmax;
            if (divz >= 0)
            {
                tzmin = (min.z - point.z) * divz;
                tzmax = (max.z - point.z) * divz;
            }
            else
            {
                tzmin = (max.z - point.z) * divz;
                tzmax = (min.z - point.z) * divz;
            }

            if ((tmin > tzmax) || (tzmin > tmax))
            {
                tmin = tmax = float.NaN;
                return false;
            }

            if (tzmin > tmin) tmin = tzmin;
            if (tzmax < tmax) tmax = tzmax;

            return true;
        }
        /// <summary>
        /// Line - Plane intersection
        /// </summary>
        public static bool IntersectLinePlane(Vector3f point, Vector3f dir, Plane plane, out float t)
        {
            t = float.NaN;

            float D = Vector3f.Dot(plane.norm, dir);
            //float N = Vector3.Dot(plane.norm, w);
            float N = plane.mindist - Vector3f.Dot(plane.norm, point); // just to reduce a little

            if (Mathelp.ABS(D) < SMALL)
            {
                if (Mathelp.ABS(N) < SMALL)
                {
                    // line lies on plane, t = NaN
                    return true;
                }
                else
                {
                    return false;
                }
            }
            t = N / D;
            return true;
        }
        /// <summary>
        /// Line - BasePlane intersection
        /// </summary>
        public static bool IntersectLinePlane(Vector3f point, Vector3f dir, eAxis plane, out float t)
        {
            t = float.NaN;
            float D, N;

            // example : (~eAxis.XZ & mask) = eAxis.Y
            eAxis axe = ~plane & eAxis.XYZ;
            switch (axe)
            {
                case eAxis.X: D = dir.x; N = -point.x; break;
                case eAxis.Y: D = dir.y; N = -point.y; break;
                case eAxis.Z: D = dir.z; N = -point.z; break;
                default: throw new ArgumentException("please pass a valid plane value, correct flags are XY, XZ , YZ");
            }
            if (Mathelp.ABS(D) < SMALL)
            {
                if (Mathelp.ABS(N) < SMALL)
                    return true;
                else
                    return false;
            }
            t = N / D;
            return true;
        }
        /// <summary>
        /// Line - Sphere intersection
        /// </summary>
        public static bool IntersectLineSphere(Vector3f point, Vector3f dir, Vector3f center, float radius, out float tmin, out float tmax)
        {
            tmin = tmax = float.NaN;

            if (radius < 0) return false;

            Vector3f I = point - center;

            float a = Vector3f.Dot(dir, dir);
            float b = 2.0f * Vector3f.Dot(I, dir);
            float c = Vector3f.Dot(I, I) - (radius * radius);
            float d = (b * b) - (4 * a * c);

            // if discriminant is negative the picking ray missed the sphere, otherwise it intersected the sphere.
            if (d < 0.0f) return false;

            d = (float)System.Math.Sqrt(d);
            tmin = 0.5f * (-b + d) / a;
            tmax = 0.5f * (-b - d) / a;

            if (tmax < tmin) Mathelp.SWAP(ref tmax, ref tmin);

            return true;
        }
        /// <summary>
        /// Line - Sphere intersection
        /// </summary>
        public static bool IntersectLineSphere2(in Vector3f point, in Vector3f dir, in Vector3f center, in float radius, out float t)
        {
            t = 0;

            if (radius < 0) return false;

            Vector3f I = point - center;
            float d = Vector3f.Dot(I, dir);
            float Isq = Vector3f.Dot(I, I);
            float rsq = radius * radius;

            if (d < 0 && Isq > rsq) return false;
            float msq = Isq - d * d;
            if (msq > rsq) return false;
            float q = (float)System.Math.Sqrt(rsq - msq);

            t = Isq > rsq ? d - q : d + q;
            return true;
        }
        /// <summary>
        /// Line - Triangle (in CounterClockWire) intersection. If TestCull = false, both triangle side are valid. t is NaN if line lies on triangle plane
        /// </summary>
        public static bool IntersectLineTriangle(in Vector3f point, in Vector3f dir, in Vector3f P0, in Vector3f P1, in Vector3f P2, bool TestCull, out float t)
        {
            t = float.NaN;

            // find vectors for two edges sharing vert
            Vector3f e1 = P1 - P0;
            Vector3f e2 = P2 - P0;

            // begin calculating determinant, also used to calculate U parameter
            Vector3f pvec = Vector3f.Cross(dir, e2);

            // if determinant is near zero, ray lies in plane of triangle
            float det = Vector3f.Dot(e1, pvec);

            // define TEST_CULL if culling is desired
            if (TestCull)
            {
                if (det < SMALL)
                {
                    if (det > -SMALL)
                    {
                        return true;
                    }
                    return false;
                }
                // calculate distance from vert0 to ray origin
                Vector3f tvec = point - P0;

                // calculate U parameter and test bounds
                float u = Vector3f.Dot(tvec, pvec);
                if (u < 0 || u > det) return false;

                // prepare to test V parameter
                Vector3f qvec = Vector3f.Cross(tvec, e1);

                // calculate V parameter and test bounds
                float v = Vector3f.Dot(dir, qvec);
                if (v < 0 || u + v > det) return false;

                // calculate t, scale parameters, ray intersects triangle
                t = Vector3f.Dot(e2, qvec) / det;

                //u *= 1.0f / det;
                //v *= 1.0f / det;
                //intersection = (1 - u - v) * P0 + u * P1 + v * P2;
            }
            else
            {
                //for cullmode.none
                if (det < SMALL && det > -SMALL)
                {
                    return true;
                }
                // calculate distance from vert0 to ray origin
                Vector3f tvec = point - P0;

                float inv_det = 1.0f / det;

                // calculate U parameter and test bounds
                float u = Vector3f.Dot(tvec, pvec) * inv_det;
                if (u < 0 || u > 1) return false;


                // prepare to test V parameter
                Vector3f qvec = Vector3f.Cross(tvec, e1);

                // calculate V parameter and test bounds */
                float v = Vector3f.Dot(dir, qvec) * inv_det;
                if (v < 0 || u + v > 1) return false;


                // calculate t, scale parameters, ray intersects triangle
                t = Vector3f.Dot(e2, qvec) * inv_det;
                //u *= 1.0f / det;
                //v *= 1.0f / det;

                //intersection = (1 - u - v) * P0 + u * P1 + v * P2;
                return true;
            }
            return false;
        }
        
        #endregion

        #region sphere
        /// <summary>
        /// Sphere - Sphere intersection, TestOK
        /// </summary>
        public static bool IntersectSphereSphere(Vector3f acenter, float aradius, Vector3f bcenter, float bradius)
        {
            Vector3f c = bcenter - acenter;
            float r = aradius + bradius;
            return Vector3f.Dot(c, c) < r * r;
        }
        /// <summary>
        /// Sphere - AABBox intersection, TestOK
        /// </summary>
        public static bool IntersectAABBSphere2(Vector3f min, Vector3f max, Vector3f center, float radius)
        {
            float dsq = 0;
            float rsq = radius * radius;

            AABBSPHERESQDIST(center.x, min.x, max.x, ref dsq);
            if (dsq > rsq) return false;

            AABBSPHERESQDIST(center.y, min.y, max.y, ref dsq);
            if (dsq > rsq) return false;

            AABBSPHERESQDIST(center.z, min.z, max.z, ref dsq);
            if (dsq > rsq) return false;

            return true;
        }
        /// <summary>
        /// Sphere - AABBox intersection : QRI algorithm (quick rejections intertwined), TestOK
        /// </summary>
        public static bool IntersectAABBSphere(Vector3f min, Vector3f max, Vector3f center, float radius)
        {
            float d = 0;
            if (!QRI(center.x, radius, min.x, max.x, ref d)) return false;
            if (!QRI(center.y, radius, min.y, max.y, ref d)) return false;
            if (!QRI(center.z, radius, min.z, max.z, ref d)) return false;
            return d <= radius * radius;
        }
        /// <summary>
        /// Sphere - Plane intersection, TestOK
        /// </summary>
        public static bool IntersectSpherePlane(Vector3f center, float radius, Plane plane)
        {
            float dist = plane.GetDistance(center.x, center.y, center.z);
            if (Mathelp.ABS(dist) <= radius) return true;
            return false;
        }
        /// <summary>
        /// Sphere - Triangle intersection, TestOK
        /// </summary>
        public static bool IntersectSphereTriangle(Vector3f center, float radius, Vector3f p0, Vector3f p1, Vector3f p2)
        {
            // Find point P on triangle ABC closest to sphere center
            Vector3f p = CLOSESTPOINTTRIANGLE(center, p0, p1, p2);

            // Sphere and triangle intersect if the (squared) distance from sphere
            // center to point p is less than the (squared) sphere radius
            return (p - center).LengthSq <= radius * radius;
        }
        
        #endregion

        #region box
        /// <summary>
        /// AABBox - AABBox intersection, TestOK
        /// </summary>
        public static bool Intersect_AABB_AABB(Vector3f amin, Vector3f amax, Vector3f bmin, Vector3f bmax)
        {
            if (amin.x > bmax.x || bmin.x > amax.x) return false;
            if (amin.y > bmax.y || bmin.y > amax.y) return false;
            if (amin.z > bmax.z || bmin.z > amax.z) return false;
            return true;
        }
        /// <summary>
        /// AABBox - Plane intersection, TestOK
        /// </summary>
        public static bool Intersect_AABB_Plane(Vector3f min, Vector3f max, Plane plane)
        {
            Vector3f pos = Vector3f.Zero;
            Vector3f neg = Vector3f.Zero;

            PLANEPOSNEG(plane.norm.x, min.x, max.x, out pos.x, out neg.x);
            PLANEPOSNEG(plane.norm.y, min.y, max.y, out pos.y, out neg.y);
            PLANEPOSNEG(plane.norm.z, min.z, max.z, out pos.z, out neg.z);

            if (plane.GetDistance(neg) > 0) return false;
            if (plane.GetDistance(pos) >= 0) return true;
            return false;
        }

        /// <summary>
        /// AABBox - Triangle intersection by Tomas Akenine-Möller
        /// Ok Testato
        /// </summary>
        /// <remarks>
        /// use separating axis theorem to test overlap between triangle and box, need to test for overlap in these directions:
        /// <list type="number">
        /// <item>the {x,y,z}-directions (actually, since we use the AABB of the triangle we do not even need to test these)</item>
        /// <item>normal of the triangle</item>
        /// <item>crossproduct(edge from tri, {x,y,z}-directin) this gives 3x3=9 more tests</item>
        /// </list>
        /// </remarks>
        public static bool IntersectAABBTriangle(Vector3f min, Vector3f max, Vector3f p0, Vector3f p1, Vector3f p2)
        {
            // using AABB version 2 can remove this computation
            Vector3f boxcenter = (max + min) / 2;
            boxhalfsize = (max - min) / 2;

            // boxcenter is in (0,0,0)
            v0 = p0 - boxcenter;
            v1 = p1 - boxcenter;
            v2 = p2 - boxcenter;

            // compute triangle edges
            Vector3f e0 = v1 - v0;
            Vector3f e1 = v2 - v1;
            Vector3f e2 = v0 - v2;

            // Bullet 3:
            //  test the 9 tests first (this was faster)
            float fex = Mathelp.ABS(e0.x);
            float fey = Mathelp.ABS(e0.y);
            float fez = Mathelp.ABS(e0.z);

            if (AXISTEST_X01(e0.z, e0.y, fez, fey)) return false;
            if (AXISTEST_Y02(e0.z, e0.x, fez, fex)) return false;
            if (AXISTEST_Z12(e0.y, e0.x, fey, fex)) return false;

            fex = Mathelp.ABS(e1.x);
            fey = Mathelp.ABS(e1.y);
            fez = Mathelp.ABS(e1.z);

            if (AXISTEST_X01(e1.z, e1.y, fez, fey)) return false;
            if (AXISTEST_Y02(e1.z, e1.x, fez, fex)) return false;
            if (AXISTEST_Z0(e1.y, e1.x, fey, fex)) return false;

            fex = Mathelp.ABS(e2.x);
            fey = Mathelp.ABS(e2.y);
            fez = Mathelp.ABS(e2.z);

            if (AXISTEST_X2(e2.z, e2.y, fez, fey)) return false;
            if (AXISTEST_Y1(e2.z, e2.x, fez, fex)) return false;
            if (AXISTEST_Z12(e2.y, e2.x, fey, fex)) return false;


            // Bullet 1:
            //  first test overlap in the {x,y,z}-directions
            //  find min, max of the triangle each direction, and test for overlap in
            //  that direction -- this is equivalent to testing a minimal AABB around
            //  the triangle against the AABB
            
            // test in X-direction
            Mathelp.MINMAX(v0.x, v1.x, v2.x, out minf, out maxf);
            if (minf > boxhalfsize.x || maxf < -boxhalfsize.x) return false;
            // test in Y-direction
            Mathelp.MINMAX(v0.y, v1.y, v2.y, out minf, out  maxf);
            if (minf > boxhalfsize.y || maxf < -boxhalfsize.y) return false;
            // test in Z-direction
            Mathelp.MINMAX(v0.z, v1.z, v2.z, out minf, out maxf);
            if (minf > boxhalfsize.z || maxf < -boxhalfsize.x) return false;

            // Bullet 2:
            //  test if the box intersects the plane of the triangle
            //  compute plane equation of triangle: normal*x+d=0
            Vector3f norm =  Vector3f.Cross( e0, e1);

            Vector3f pos = Vector3f.Zero;
            Vector3f neg = Vector3f.Zero;

            PLANEPOSNEG(norm.x, min.x - p0.x, max.x - p0.x, out pos.x, out neg.x);
            PLANEPOSNEG(norm.y, min.y - p0.y, max.y - p0.y, out pos.y, out neg.y);
            PLANEPOSNEG(norm.z, min.z - p0.z, max.z - p0.z, out pos.z, out neg.z);

            if (Vector3f.Dot(norm, neg) > 0.0f) return false;
            if (Vector3f.Dot(norm, pos) >= 0.0f) return true;
            return false;
        }
        /// <summary>
        /// Oriented Bounding Box - Plane
        /// </summary>
        public static bool Intersect_OBB_Plane(Matrix4x4f trs, Plane plane)
        {
            Vector3f n = plane.norm;

            float radius = Mathelp.ABS(plane.GetDistance(trs.m03, trs.m13, trs.m23));

            float dist = Mathelp.ABS(trs.m00 * n.x + trs.m10 * n.y + trs.m20 * n.z) +
                         Mathelp.ABS(trs.m01 * n.x + trs.m11 * n.y + trs.m21 * n.z) +
                         Mathelp.ABS(trs.m02 * n.x + trs.m12 * n.y + trs.m22 * n.z);

            if (dist > radius) return true;

            return false;
        }

        /// <summary>
        /// Oriented Bounding Box (tested)
        /// http://www.jkh.me/files/tutorials/Separating%20Axis%20Theorem%20for%20Oriented%20Bounding%20Boxes.pdf
        /// </summary>
        /// <param name="Ax">x axe</param>
        /// <param name="Ay">y axe</param>
        /// <param name="Az">z axe</param>
        /// <param name="Wa">width along axe x</param>
        /// <param name="Ha">height along axe y</param>
        /// <param name="Da">depth along axe z</param>
        /// <param name="T">centerB - centerA</param>
        /// <returns></returns>
        public static bool Intersect_OBB_OBB(
             ref Vector3f Ax, ref Vector3f Ay, ref Vector3f Az, float Wa, float Ha, float Da,
             ref Vector3f Bx, ref Vector3f By, ref Vector3f Bz, float Wb, float Hb, float Db,
             ref Vector3f T)
        {
            // case1 L = Ax : |T*Ax| > Wa + |Wb*Rxx| + |Hb*Rxy| + |Db*Rxz| 
            // if exist at last one separate axis, the two box are separate
            float Rxx = DOT(Ax, Bx);
            float Rxy = DOT(Ax, By);
            float Rxz = DOT(Ax, Bz);
            if (ABS(DOT(T, Ax)) > Wa + ABS(Wb * Rxx) + ABS(Hb * Rxy) + ABS(Db * Rxz)) return false;

            // case2 L = Ay : |T*Ay| > Ha + |Wb*Ryx| + |Hb*Ryy| + |Db*Ryz| 
            float Ryx = DOT(Ay, Bx);
            float Ryy = DOT(Ay, By);
            float Ryz = DOT(Ay, Bz);
            if (ABS(DOT(T, Ay)) > Ha + ABS(Wb * Ryx) + ABS(Hb * Ryy) + ABS(Db * Ryz)) return false;

            // case3 L = Az : |T*Az| > Da + |Wb*Rzx| + |Hb*Rzy| + |Db*Rzz| 
            float Rzx = DOT(Az, Bx);
            float Rzy = DOT(Az, By);
            float Rzz = DOT(Az, Bz);
            if (ABS(DOT(T, Az)) > Da + ABS(Wb * Rzx) + ABS(Hb * Rzy) + ABS(Db * Rzz)) return false;

            // case4 L = Bx : |T*Bx| > Wb + |Wa*Rxx| + |Ha*Ryx| + |Da*Rzx| 
            if (ABS(DOT(T, Bx)) > Wb + ABS(Wa * Rxx) + ABS(Ha * Ryx) + ABS(Da * Rzx)) return false;

            // case5 L = By : |T*By| > Hb + |Wa*Rxy| + |Ha*Ryy| + |Da*Rzy| 
            if (ABS(DOT(T, By)) > Hb + ABS(Wa * Rxy) + ABS(Ha * Ryy) + ABS(Da * Rzy)) return false;

            // case6 L = Bz : |T*Bz| > Db + |Wa*Rxz| + |Ha*Ryz| + |Da*Rzz| 
            if (ABS(DOT(T, Bz)) > Db + ABS(Wa * Rxz) + ABS(Ha * Ryz) + ABS(Da * Rzz)) return false;

            // case7 L = Ax x Bx :  |T*(Ax x Bx)| > |Ha*Rzx| + |Da*Ryx| + |Hb*Rxz| + |Db*Rxy| 
            if (ABS(DOT(T, CROSS(Ax, Bx))) > ABS(Ha * Rzx) + ABS(Da * Ryx) + ABS(Hb * Rxz) + ABS(Db * Rxy)) return false;

            // case8 L = Ax x By :  |T*(Ax x By)| > |Ha*Rzy| + |Da*Ryy| + |Wb*Rxz| + |Db*Rxx| 
            if (ABS(DOT(T, CROSS(Ax, By))) > ABS(Ha * Rzy) + ABS(Da * Ryy) + ABS(Wb * Rxz) + ABS(Db * Rxx)) return false;

            // case9 L = Ax x Bz :  |T*(Ax x Bz)| > |Ha*Rzz| + |Da*Ryz| + |Wb*Rxy| + |Hb*Rxx| 
            if (ABS(DOT(T, CROSS(Ax, Bz))) > ABS(Ha * Rzz) + ABS(Da * Ryz) + ABS(Wb * Rxy) + ABS(Hb * Rxx)) return false;

            // case10 L = Ay x Bx :  |T*(Ay x Bx)| > |Wa*Rzx| + |Da*Rxx| + |Hb*Ryz| + |Db*Ryy| 
            if (ABS(DOT(T, CROSS(Ay, Bx))) > ABS(Wa * Rzx) + ABS(Da * Rxx) + ABS(Hb * Ryz) + ABS(Db * Ryy)) return false;

            // case11 L = Ay x By :  |T*(Ay x By)| > |Wa*Rzy| + |Da*Rxy| + |Wb*Ryz| + |Db*Ryx| 
            if (ABS(DOT(T, CROSS(Ay, By))) > ABS(Wa * Rzy) + ABS(Da * Rxy) + ABS(Wb * Ryz) + ABS(Db * Ryx)) return false;

            // case12 L = Ay x Bz :  |T*(Ay x Bz)| > |Wa*Rzz| + |Da*Rxz| + |Wb*Ryy| + |Hb*Ryx| 
            if (ABS(DOT(T, CROSS(Ay, Bz))) > ABS(Wa * Rzz) + ABS(Da * Rxz) + ABS(Wb * Ryy) + ABS(Hb * Ryx)) return false;

            // case13 L = Az x Bx :  |T*(Az x Bx)| > |Wa*Ryx| + |Ha*Rxx| + |Hb*Rzz| + |Db*Rzy| 
            if (ABS(DOT(T, CROSS(Az, Bx))) > ABS(Wa * Ryx) + ABS(Ha * Rxx) + ABS(Hb * Rzz) + ABS(Db * Rzy)) return false;

            // case14 L = Az x By :  |T*(Az x By)| > |Wa*Ryy| + |Ha*Rxy| + |Wb*Rzz| + |Db*Rzx| 
            if (ABS(DOT(T, CROSS(Az, By))) > ABS(Wa * Ryy) + ABS(Ha * Rxy) + ABS(Wb * Rzz) + ABS(Db * Rzx)) return false;

            // case15 L = Az x Bz :  |T*(Az x Bz)| > |Wa*Ryz| + |Ha*Rxz| + |Wb*Rzy| + |Hb*Rzx| 
            if (ABS(DOT(T, CROSS(Az, Bz))) > ABS(Wa * Ryz) + ABS(Ha * Rxz) + ABS(Wb * Rzy) + ABS(Hb * Rzx)) return false;

            return true;
        }

        /// <summary>
        /// Oriented Bounding Box (tested)
        /// </summary>
        public static bool Intersect_OBB_OBB(Matrix4x4f trs0, Matrix4x4f trs1)
        {
            Vector3f T = trs1.Position - trs0.Position;
            Vector3f Ax = trs0.getCol(0);
            Vector3f Ay = trs0.getCol(1);
            Vector3f Az = trs0.getCol(2);
            float Wa = Ax.Normalize();
            float Ha = Ay.Normalize();
            float Da = Az.Normalize();
            Vector3f Bx = trs1.getCol(0);
            Vector3f By = trs1.getCol(1);
            Vector3f Bz = trs1.getCol(2);
            float Wb = Bx.Normalize();
            float Hb = By.Normalize();
            float Db = Bz.Normalize();
            return Intersect_OBB_OBB(ref Ax, ref Ay, ref Az, Wa, Ha, Da, ref Bx, ref By, ref Bz, Wb, Hb, Db, ref T);
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// A collection of all possible intersection between primitives geometries. The purpose is to extend 'PrimitiveIntersections' and calculate
    /// also intersections points
    /// </summary>
    public static class PrimitiveIntersections2
    {
        #region Line case
        /// <summary>
        /// Line - Plane intersection
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Line line, Plane plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(line.orig, line.dir, plane, out t);
            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;
        }

        /// <summary>
        /// Line - BasePlane(XZ,XY,YZ) intersection.
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Line line, eAxis plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(line.orig, line.dir, plane, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;

        }

        /// <summary>
        /// Line - AxisAllignedBoundyBox intersection. is possible that tmin == tmax for corner
        /// </summary>
        /// <returns>
        /// <para>None</para>
        /// <para>TwoPoint</para>
        /// </returns>
        public static IntersectResult Intersect(Line line, BoundingBoxMinMax box, out float tmin, out float tmax)
        {
            bool intersect = PrimitiveIntersections.Intersect_Line_AABB(line.orig, line.dir, box.min, box.max, out tmin, out tmax);
            return intersect ? IntersectResult.TwoPoint : IntersectResult.None;
        }
        /// <summary>
        /// Line - Sphere intersection
        /// </summary>
        /// <returns>
        /// <see cref="IntersectResult.TwoPoint"/>: all two intersections are in positive ray direction<br/>
        /// <see cref="IntersectResult.OnePoint"/>: and second intersection are using on ray negative projection<br/>
        /// <see cref="IntersectResult.None"/><br/>
        /// </returns>
        public static IntersectResult Intersect(Line line, Sphere sphere, out float tmin, out float tmax)
        {
            bool intersect = PrimitiveIntersections.IntersectLineSphere(line.orig, line.dir, sphere.center, sphere.radius, out tmin, out tmax);
            return intersect ? IntersectResult.TwoPoint : IntersectResult.None;
        }
        /// <summary>
        /// Line - TriangleCW intersection, both triangle side (CW,CCW) are valid because lines are considered without direction
        /// </summary>
        /// <returns>
        /// <see cref="IntersectResult.Lies"/><br/>
        /// <see cref="IntersectResult.OnePoint"/><br/>
        /// <see cref="IntersectResult.None"/><br/>
        /// </returns>
        public static IntersectResult Intersect(in Line line, in Vector3f p0, in Vector3f p1, in Vector3f p2, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLineTriangle(in line.orig, in line.dir, in p0, in p1, in p2, false, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                return IntersectResult.OnePoint;
            }
            return IntersectResult.None;
        }
        #endregion

        #region Segment case
        /// <summary>
        /// Segment - Plane intersection, float t, are segment scalar parameters, if point is outsize segment
        /// the t parameter is outsize [0-1] range and the interpolated point is the intersection of projection
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePointProjected</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Segment seg, Plane plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(seg.orig, seg.dir, plane, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return (t < 0 || t > seg.length) ?
                        IntersectResult.OnePointProjected :
                        IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;
        }
        /// <summary>
        /// Segment - BasePlane(XZ,XY,YZ) intersection. I semplified math for faster result
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePointProjected</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Segment seg, eAxis plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(seg.orig, seg.dir, plane, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return (t < 0 || t > seg.length) ?
                        IntersectResult.OnePointProjected :
                        IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;
        }
        /// <summary>
        /// Segment - AxisAllignedBoundyBox intersection.
        /// </summary>
        /// <returns>
        /// <para>None : if not intersect</para>
        /// <para>TwoPoint : if intersect and the two intersected points are inside segment lenght</para>
        /// <para>OnePointProjected : if intersect and there is only one intersected points inside segment lenght (set in t0)</para>
        /// <para>TwoPointProjected : if intersect but segment are inside box</para>
        /// </returns>
        public static IntersectResult Intersect(Segment seg, BoundingBoxMinMax box, out float t0, out float t1)
        {
            bool intersect = PrimitiveIntersections.Intersect_Line_AABB(seg.orig, seg.dir, box.min, box.max, out t0, out t1);

            if (intersect)
            {
                bool tminInside = t0 >= 0 && t0 <= seg.length;
                bool tmaxInside = t1 >= 0 && t1 <= seg.length;

                if (tminInside || tmaxInside)
                {
                    if (tminInside && tmaxInside)
                        return IntersectResult.TwoPoint;
                    else
                    {
                        if (tmaxInside) Mathelp.SWAP(ref t0, ref t1);
                        return IntersectResult.OnePointProjected;
                    }
                }
                else
                {
                    return IntersectResult.TwoPointProjected;
                }
            }
            return IntersectResult.None;
        }
        #endregion

        #region Ray case
        /// <summary>
        /// Ray - AxisAllignedBoundyBox intersection
        /// </summary>
        /// <para>TwoPoint : all two intersections are in positive ray direction</para>
        /// <para>OnePoint : and second intersection are using on ray negative projection</para>
        /// <para>None</para>
        public static IntersectResult Intersect(Ray ray, BoundingBoxMinMax box, out float tmin, out float tmax)
        {
            bool intersect = PrimitiveIntersections.Intersect_Line_AABB(ray.orig, ray.dir, box.min, box.max, out tmin, out tmax);

            if (intersect)
            {
                if (tmin >= 0 && tmax >= 0)
                {
                    return IntersectResult.TwoPoint;
                }
                return IntersectResult.OnePoint;
            }
            return IntersectResult.None;
        }
        /// <summary>
        /// Ray - Plane intersection
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePointProjected</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Ray ray, Plane plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(ray.orig, ray.dir, plane, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return (t < 0) ?
                        IntersectResult.OnePointProjected :
                        IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;
        }
        /// <summary>
        /// Ray - BasePlane intersection
        /// </summary>
        /// <returns>
        /// <para>Parallel</para>
        /// <para>Lies</para>
        /// <para>OnePointProjected</para>
        /// <para>OnePoint</para>
        /// </returns>
        public static IntersectResult Intersect(Ray ray, eAxis plane, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLinePlane(ray.orig, ray.dir, plane, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                else
                {
                    return (t < 0) ?
                        IntersectResult.OnePointProjected :
                        IntersectResult.OnePoint;
                }
            }
            return IntersectResult.Parallel;
        }

        /// <summary>
        /// Ray - Sphere intersection
        /// </summary>
        /// <para>TwoPoint : all two intersections are in positive ray direction</para>
        /// <para>OnePoint : and second intersection are using on ray negative projection</para>
        /// <para>None</para>
        public static IntersectResult Intersect(Ray ray, Sphere sphere, out float tmin, out float tmax)
        {
            bool intersect = PrimitiveIntersections.IntersectLineSphere(ray.orig, ray.dir, sphere.center, sphere.radius, out tmin, out tmax);
            if (intersect)
            {
                if (tmin >= 0 && tmax >= 0)
                {
                    return IntersectResult.TwoPoint;
                }
                return IntersectResult.OnePoint;
            }
            return IntersectResult.None;
        }

        /// <summary>
        /// Ray - TriangleCW intersection, if culltest==false both triangle side (CW,CCW) are valid
        /// </summary>
        /// <para>Lies</para>
        /// <para>OnePointProjected : the intersection are on ray negative projection</para>
        /// <para>OnePoint</para>
        /// <para>None</para>
        public static IntersectResult Intersect(Ray ray, Triangle triangle, bool culltest, out float t)
        {
            bool intersect = PrimitiveIntersections.IntersectLineTriangle(ray.orig, ray.dir, triangle.p0, triangle.p1, triangle.p2, culltest, out t);

            if (intersect)
            {
                if (float.IsNaN(t))
                {
                    t = 0;
                    return IntersectResult.Lies;
                }
                if (t < 0)
                    return IntersectResult.OnePointProjected;

                return IntersectResult.OnePoint;
            }
            return IntersectResult.None;
        }
        #endregion
    }
}
