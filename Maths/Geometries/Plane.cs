using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Common.Maths
{
    /// <summary>
    /// Defines a plane in 3D space using form [ax + by + cz = d]
    /// in this case d = +(N dot P)
    /// Overwise if you use the form [ax + by + cz + d = 0] then d = -(N dot P)
    /// </summary>
    /// <remarks>
    /// d is the minimum distance from negative side of plane to origin
    /// <code>
    ///     y     n
    ///     |  \\/
    ///     |  / \\
    ///     |+d    \\
    ///     |/_____________x
    /// </code>   
    /// A plane is defined in 3D space by the equation
    /// Ax + By + Cz + D = 0
    ///
    /// This equates to a vector (the normal of the plane, whose x, y
    /// and z components equate to the coefficients A, B and C
    /// respectively), and a constant (D) which is the distance along
    /// the normal you have to go to move the plane back to the origin.
    /// 
    /// To use the vector[A,B,C] as normal, need to call Normalize() example a not
    /// normalized plane is 4x+4y+4z-8=0 but isn't a good idea this form
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct Plane
    {
        /// <summary>
        /// Plane sign of a generic 3d point
        /// </summary>
        public enum eSide : sbyte
        {
            /// <summary>
            /// The Point or objects are all in the negative side
            /// </summary>
            BACK = -1,
            /// <summary>
            /// The Point are on plane, if a 3d objects is intersect
            /// </summary>
            INTERSECT = 0,
            /// <summary>
            /// The Point or objects are all in the positive side
            /// </summary>
            FRONT = 1
        }

        public Vector3f norm;
        public float mindist;

        // Scalar equation, is necessary a normalization to standardize my plane math
        public float A { get { return norm.x; } set { norm.x = value; } }
        public float B { get { return norm.y; } set { norm.y = value; } }
        public float C { get { return norm.z; } set { norm.z = value; } }
        /// <summary>
        /// Shorter distance from the origin. each point satisfy Dot(N,p) = D
        /// </summary>       
        public float D { get { return mindist; } set { mindist = value; } }


        /// <summary>
        /// Normal * Distance is the projection of Vector3.Zero to plane (nearest plane's point to origin)
        /// </summary>
        /// <remarks>
        /// Can be calculated also with Plane.GetProject(Vector3.Zero)
        /// </remarks>
        public Vector3f Origin
        {
            get { return norm * mindist; }
        }
        /// <summary>
        /// if plane isn't initialized of infinite or invalid return true;
        /// </summary>
        public bool IsNaN { get => norm.LengthSq < float.Epsilon; }
        

        private static readonly Plane Empty = new Plane { norm = Vector3f.Zero, mindist = 0 };

        /// <summary>
        /// Construct a plane using a normal and a distance to move the plane along the normal.
        /// </summary>
        public Plane(Vector3f normal, float mindistance)
        {
            norm = normal;
            mindist = mindistance;
            Normalize();
        }
        /// <summary>
        /// Construct a plane using a normal and a point lies on it
        /// </summary>
        public Plane(Vector3f normal, Vector3f point)
        {
            norm = normal;
            mindist = Vector3f.Dot(norm, point);
            Normalize();
        }
        /// <summary>
        /// Construct a plane from 3 coplanar points.
        /// </summary>
        public Plane(Vector3f p0, Vector3f p1, Vector3f p2)
        {
            Vector3f e0 = p1 - p0;
            Vector3f e1 = p2 - p0;
            norm = Vector3f.Cross(e0, e1);
            mindist = Vector3f.Dot(norm, p0);
            Normalize();
        }


        public static float GetDistance(ref Plane plane, ref Vector3f point)
        {
            return plane.norm.x * point.x + plane.norm.y * point.y + plane.norm.z * point.z - plane.mindist;
        }


        /// <summary>
        /// The sign of distance is positive if the point is on the positive side of the plane.
        /// </summary>
        /// <returns>
        /// It's the true distance only when normal is normalized : 
        /// dot(N,[P-O]) = dot(N ,[P-N*D]) = dot(N,P) - dot(N,N)*D
        /// if N is normalized the dot(N,N) = 1
        /// </returns>
        public float GetDistance(float x, float y, float z)
        {
            return norm.x * x + norm.y * y + norm.z * z - mindist;
        }
        public float GetDistance(Vector3f v)
        {
            return norm.x * v.x + norm.y * v.y + norm.z * v.z - mindist;
        }

        /// <summary>
        /// DEPRECATED BECAUSE UNSTABLE.
        /// Get the side of cylinder from plane, giving as parameters bottom-center , top-center and radius
        /// </summary>
        /// <remarks>
        /// Not optimized, only for test purpose
        /// </remarks>
        [Obsolete("Unstable")]
        public eSide GetCylindrerSide(float bx, float by, float bz, float tx, float ty, float tz, float radius)
        {
            // cos(a) between cylindrer direction and plane normal
            Vector3f dir = new Vector3f(tx - bx, ty - by, tz - bz);

            // degenerate case, to use circle intersection i need an orientation
            if (dir.Normalize() < float.Epsilon)
                throw new Exception("Cylinder degenerate direction, impossible to compute the circle cap orientation. In this case the overload implementation can resolve this issue");

            // adjusted radius , if dir and normal parallel so r' = r*sin(a)
            float fcos = Vector3f.Dot(norm, dir);
            float r = radius * (float)System.Math.Sqrt(1 - fcos * fcos);

            // distance of bottom center
            float d1 = GetDistance(bx, by, bz);
            // distance of top center
            float d2 = GetDistance(tx, ty, tz);
            // top cap circle intersection
            bool in1 = Math.Abs(d1) < r;
            // bottom cap circle intersection
            bool in2 = Math.Abs(d2) < r;

            if (in1)
                return eSide.INTERSECT;
            else
            {
                if (in2)
                    return eSide.INTERSECT;
                else
                {
                    if (d1 < 0)
                    {
                        if (d2 < 0)
                            return eSide.BACK; //both positive sign
                        else
                            return eSide.INTERSECT; //sign d2 != sign d1
                    }
                    else
                    {
                        if (d2 < 0)
                            return eSide.INTERSECT; //sign d1 != sign d2
                        else
                            return eSide.FRONT; //both negative sign
                    }
                }
            }

        }
        /// <summary>
        /// DEPRECATED BECAUSE UNSTABLE 
        /// </summary>
        [Obsolete("Unstable")]
        public eSide GetCylindrerSide(Vector3f bottom, Vector3f top, float radius)
        {
            return GetCylindrerSide(bottom.x, bottom.y, bottom.z, top.x, top.y, top.z, radius);
        }
        /// <summary>
        /// Get the side of cylinder from plane, giving as parameters middle-center , normalized direction, height and radius 
        /// </summary>
        /// <remarks>
        /// Giving a direction as input resolve the degenerate case, make be sure you pass a normalized vector
        /// </remarks>
        /// <param name="semiheight">height of cylinder / 2, ensure center point is exactly in the middle of height vector</param>
        public eSide GetCylindrerSide(float cx, float cy, float cz, float dx, float dy, float dz, float semiheight, float radius)
        {
            float l = dx * dx + dy * dy + dz * dz;

            if (l < float.Epsilon && l > -float.Epsilon)
                throw new Exception("Cylinder degenerate direction, impossible to compute the circle cap orientation. Direction value isn't normalized");

            // cos(a) between cylindrer direction and plane normal
            // adjusted radius , if dir and normal parallel so r' = r*sin(a)
            float fcos = norm.x * dx + norm.y * dy + norm.z * dz;
            float r = radius * (float)System.Math.Sqrt(1 - fcos * fcos);

            // distance of bottom center
            float d1 = GetDistance(cx - dx * semiheight, cy - dx * semiheight, cz - dz * semiheight);
            // distance of top center
            float d2 = GetDistance(cx + dx * semiheight, cy + dx * semiheight, cz + dz * semiheight);
            // top cap circle intersection
            bool in1 = Math.Abs(d1) < r;
            // bottom cap circle intersection
            bool in2 = Math.Abs(d2) < r;

            if (in1)
                return eSide.INTERSECT;
            else
            {
                if (in2)
                    return eSide.INTERSECT;
                else
                {
                    if (d1 < 0)
                    {
                        if (d2 < 0)
                            return eSide.BACK; //both positive sign
                        else
                            return eSide.INTERSECT; //sign d2 != sign d1
                    }
                    else
                    {
                        if (d2 < 0)
                            return eSide.INTERSECT; //sign d1 != sign d2
                        else
                            return eSide.FRONT; //both negative sign
                    }
                }
            }

        }
        public eSide GetCylindrerSide(Vector3f center, Vector3f direction, float semiheight, float radius)
        {
            return GetCylindrerSide(center.x, center.y, center.z, direction.x, direction.y, direction.z, semiheight, radius);
        }

        /// <summary>
        /// TODO : Project a point onto the plane.
        /// </summary>
        public Vector3f GetProject(Vector3f point)
        {
            return point - Vector3f.Dot(point - Origin, norm) * norm;
        }
        /// <summary>
        /// Normalize the plane (normalization of normal vector)
        /// </summary>
        public void Normalize()
        {
            float l = norm.Normalize();
            mindist /= l;
        }
        /// <summary>
        /// TODO : Change the coordinate system of a plane
        /// </summary>
        public static Plane TransformCoordinate(Plane plane, Matrix4x4f coordsys)
        {
            Plane p = new Plane();
            p.norm = plane.norm.TransformNormal(in coordsys);
            // 19 moltiplications
            //p.d = -Vector3.Dot(p.n, Vector3.TransformCoordinate(plane.Origin, coordsys));
            // 4 multiplications + 1 addittion
            //p.d = -Vector3.Dot(p.n, (-plane.d * p.n + coordsys.TranslationComponent));
            p.mindist = plane.mindist - (p.norm.x * coordsys.m03 + p.norm.y * coordsys.m13 + p.norm.z * coordsys.m23);

            return p;
        }

        /// <summary>
        /// TODO : Get the line from planes intersections, if parallel return empty ray
        /// </summary>
        public static Ray Intersection(Plane pa, Plane pb)
        {
            Vector3f na = pa.norm;
            Vector3f nb = pb.norm;

            Vector3f cross = Vector3f.Cross(na, nb);

            if (cross.LengthSq < float.Epsilon) return Ray.NaN;

            float f0 = na.LengthSq;
            float f1 = nb.LengthSq;
            float dot = Vector3f.Dot(na, nb);
            float det = f0 * f1 - dot * dot;

            if (det < float.Epsilon) return Ray.NaN;

            float c0 = (f1 * pa.mindist - dot * pb.mindist) / det;
            float c1 = (f0 * pb.mindist - dot * pa.mindist) / det;

            Vector3f orig = na * c0 + nb * c1;

            return new Ray(orig, cross);
        }

        /// <summary>
        /// TODO : Get the point from plane ray intersection, if line is parallel to plane return
        /// infinite vector
        /// </summary>
        public static Vector3f Intersection(Plane p, Ray l)
        {
            float Vd = Vector3f.Dot(p.norm, l.dir);
            if (Vd * Vd < float.Epsilon) return new Vector3f(float.NaN, float.NaN, float.NaN);

            float Vo = -(Vector3f.Dot(p.norm, l.orig) + p.mindist);
            float t = Vo / Vd;

            return l.orig + l.dir * t;
        }

        public override string ToString()
        {
            return String.Format("N {0,4} D : {1,4}", norm, mindist);
        }

    }
}
