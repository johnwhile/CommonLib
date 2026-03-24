using System;
using System.Collections.Generic;

namespace Common.Maths
{
    /// <summary>
    /// Bounding Sphere
    /// </summary>
    public struct Sphere
    {
        Vector3f c;
        float r; // if radius < 0 is "Empty"

        public Vector3f center 
        { 
            get { return c; } 
            set { c = value; } 
        }
        public float radius 
        { 
            get { return r; }
            set { r = value; }
        }

        public Sphere(float x, float y, float z, float Radius)
            : this(new Vector3f(x, y, z), Radius)
        {
        }

        public Sphere(Vector3f Center, float Radius)
        {
            c = Center;
            r = Radius;
        }

        public bool Intersect(Ray ray, out float t0, out float t1)
        {
            return BoundaryIntersectRay(this, ray, out t0, out t1);
        }
        /// <summary> Sphere-Ray intersection, usefull know that if t0 &lt; 0 the ray start inside sphere </summary>
        /// <param name="t0">enter intersection at ray(t0)</param>
        /// <param name="t1">exit intersection at ray(t1)</param>
        /// <returns></returns>
        public static bool BoundaryIntersectRay(Vector3f Center, float Radius, Ray ray, out float t0, out float t1)
        {
            t0 = 0;
            t1 = 0;
            if (Radius < 0) return false;

            Vector3f p = ray.orig - Center;
            float a = Vector3f.Dot(ray.dir, ray.dir);
            float b = 2.0f * Vector3f.Dot(p, ray.dir);
            float c = Vector3f.Dot(p, p) - (Radius * Radius);
            float d = (b * b) - (4 * a * c);

            // if discriminant is negative the picking ray missed the sphere, otherwise it intersected the sphere.
            if (d < 0.0f) return false;

            d = (float)System.Math.Sqrt(d);
            t0 = 0.5f * (-b + d) / a;
            t1 = 0.5f * (-b - d) / a;

            if (t1 < t0) Mathelp.SWAP(ref t1, ref t0);

            return true;
        }
        /// <summary>
        /// avoid the calculation of T0 and T1 , return true also if Ray Origin is inside sphere
        /// </summary>
        public static bool BoundaryIntersectRay(Vector3f Center, float Radius, Ray ray)
        {
            if (Radius < 0) return false;

            Vector3f p = ray.orig - Center;
            float a = Vector3f.Dot(ray.dir, ray.dir);
            float b = 2.0f * Vector3f.Dot(p, ray.dir);
            float c = Vector3f.Dot(p, p) - (Radius * Radius);
            float d = (b * b) - (4 * a * c);

            // if discriminant is negative the picking ray missed the sphere, otherwise it intersected the sphere.
            if (d < 0.0f) return false;
            return true;
        }
              
        public static bool BoundaryIntersectRay(Sphere bs, Ray ray, out float t0, out float t1)
        {
            return BoundaryIntersectRay(bs.center, bs.radius, ray, out t0, out t1);
        }
        /// <summary>
        /// A void sphere radius = -1 , center = Zero, used example when you want never collisions 
        /// </summary>
        public static Sphere Zero
        {
            get { return new Sphere(Vector3f.Zero, 0); }
        }
        /// <summary>
        /// A not usable sphere, not zero, used example when you want considerate the struct not processed or with some errors
        /// radius = -1 , center = NaN
        /// </summary>
        public static Sphere NaN
        {
            get { return new Sphere(Vector3f.NaN, -1); }
        }
        public bool isNaN { get { return r < 0; } }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inscribed">if true use the sphere inside box</param>
        /// <returns></returns>
        public static Sphere FromAABBox(IAABBox box, bool inscribed = false)
        {
            Vector3f halfsize = box.HalfSize;
            
            if (inscribed)
            {
                return new Sphere(box.Center, Mathelp.MIN(halfsize.x, halfsize.y, halfsize.z));
            }
            else
            {
                return new Sphere(box.Center, (float)System.Math.Sqrt(halfsize.x * halfsize.x + halfsize.y * halfsize.y + halfsize.z * halfsize.z));
            }
        }
        /// <summary>
        /// http://softsurfer.com/Archive/algorithm_0107/algorithm_0107.htm
        /// Copyright 2001, softSurfer (www.softsurfer.com)
        /// This code may be freely used and modified for any purpose
        /// providing that this copyright notice is included with it.
        /// SoftSurfer makes no warranty for this code, and cannot be held
        /// liable for any real or imagined damage resulting from its use.
        /// Users of this code must verify correctness for their application.
        /// </summary>
        public static Sphere FromDataFast(IList<Vector3f> Points)
        {
            if (Points == null || Points.Count == 0)
                return Sphere.NaN;
            int count = Points.Count;
            Vector3f C = Vector3f.Zero;
            float R = 0;
            //////////////////////////////////////////////////////// 1
            //  get three pair of points
            //  P1 (minimum x) P2 (maximum x)
            //  Q1 (minimum y) Q2 (maximum y)
            //  R1 (minimum z) R2 (maximum z)
            //  find the pair with mamimum distance (using square distance)
            float xmin = Points[0].x;
            float xmax = Points[0].x;
            float ymin = Points[0].y;
            float ymax = Points[0].y;
            float zmin = Points[0].z;
            float zmax = Points[0].z;
            int Pxmin = 0;
            int Pxmax = 0;
            int Pymin = 0;
            int Pymax = 0;
            int Pzmin = 0;
            int Pzmax = 0;


            //find a large diameter to start with
            for (int i = 0; i < count; i++)
            {
                if (Points[i].x < xmin)
                {
                    xmin = Points[i].x;
                    Pxmin = i;
                }
                else if (Points[i].x > xmax)
                {
                    xmax = Points[i].x;
                    Pxmax = i;
                }
                else if (Points[i].y < ymin)
                {
                    ymin = Points[i].y;
                    Pymin = i;
                }
                else if (Points[i].y > ymax)
                {
                    ymax = Points[i].y;
                    Pymax = i;
                }
                else if (Points[i].z < zmin)
                {
                    zmin = Points[i].y;
                    Pzmin = i;
                }
                else if (Points[i].z > zmax)
                {
                    zmax = Points[i].y;
                    Pzmax = i;
                }
            }
            Vector3f dx = Points[Pxmax] - Points[Pxmin];
            Vector3f dy = Points[Pymax] - Points[Pymin];
            Vector3f dz = Points[Pzmax] - Points[Pzmin];

            float dx2 = dx.LengthSq;
            float dy2 = dy.LengthSq;
            float dz2 = dz.LengthSq;

            float rad2;
            // pair x win
            if (dx2 > dy2 && dx2 > dz2)
            {
                C = Points[Pxmin] + (dx * 0.5f); //Center = midpoint of extremes
                rad2 = (Points[Pxmax] - C).LengthSq;//radius squared
            }
            // pair y win
            else if (dy2 > dx2 && dy2 > dz2)
            {
                C = Points[Pymin] + (dy * 0.5f);
                rad2 = (Points[Pymax] - C).LengthSq;
            }
            // pair z win
            else
            {
                C = Points[Pzmin] + (dz * 0.5f);
                rad2 = (Points[Pzmax] - C).LengthSq;
            }
            float rad = (float)Math.Sqrt(rad2);


            ///////////////////////////////////////////////////////////// 2
            //  now check that all points V[i] are in the ball
            //  and if not, expand the ball just enough to include them

            Vector3f dV = Vector3f.Zero;
            float dist, dist2;

            for (int i = 0; i < count; i++)
            {
                dV = Points[i] - C;
                dist2 = dV.LengthSq;

                if (dist2 > rad2)
                {
                    //V[i] not in ball, so expand ball to include it
                    dist = (float)System.Math.Sqrt(dist2);
                    rad = (rad + dist) / 2.0f; //enlarge radius just enough
                    rad2 = rad * rad;
                    C = Points[i] - dV * (rad / dist);  //shift Center toward V[i]
                    //C = C + dV * ((rad - dist) / dist);  //shift Center toward V[i]
                    Console.Write("");
                }
            }
            R = rad;

            return new Sphere(C, R);
        }
        /// <summary>
        /// calculate O(2n) Center = (Sum all vertices)/(num vertices) ,
        /// Radius = Max(dist(vertex i , center)).
        /// </summary>
        public static Sphere FromDataBasic(IList<Vector3f> Points)
        {
            Vector3f C = Vector3f.Zero;
            float R = 0.0f;

            float rad2 = 0.0f;

            foreach (Vector3f v in Points) C += v;
            C = C * (1.0f / Points.Count);

            foreach (Vector3f v in Points)
            {
                float dist2 = (v - C).LengthSq;
                if (dist2 > rad2)
                {
                    rad2 = dist2;
                }
            }
            R = (float)System.Math.Sqrt(rad2);

            return new Sphere(C, R);
        }
        /// <summary>
        /// Merge two Sphere, if "Empty" aren't added
        /// </summary>
        public static Sphere operator +(Sphere a, Sphere b)
        {
            Vector3f v = b.c - a.c;
            float vl = v.Length;

            // b is inside a, b always inside if is b=="Empty"
            if (a.r >= vl + b.r) return a;
            // a is inside b, a always inside if is a=="Empty"
            if (b.r >= vl + a.r) return b;
            // a + b , if a & b == "Empty" the sum return always a negative radius
            return new Sphere(a.c + b.c + v * ((b.r - a.r) / vl) * 0.5f, (vl + a.r + b.r) * 0.5f);
        }

        public override string ToString()
        {
            if (isNaN) return "NULL_SPHERE";
            return string.Format("Center : {0,4} {1,4} {2,4} Radius : {3,4}\n", c.x, c.y, c.z , r);
        }
    }


}
