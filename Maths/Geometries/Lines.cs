using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common.Maths
{
    /// <summary>
    /// Infinite line
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ToString()}")]
    public struct Line
    {
        // in this case, p is the closest point what satisfies dot(P,D) = 0
        public Vector3f orig;
        public Vector3f dir;

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="dir">will be normalized, if lenght is zero, it'll be set as UnitX</param>
        public Line(Vector3f point, Vector3f dir)
        {
            float length = dir.Normalize();
            if (length < 1e-8f) dir = Vector3f.UnitX;
            float t = Vector3f.Dot(in point, in dir);
            this.dir = dir;
            orig.x = point.x - t * dir.x;
            orig.y = point.y - t * dir.y;
            orig.z = point.z - t * dir.z;
        }

        /// <summary>
        /// Get the point using equation : Line(t) = Line.P + t*Line.D
        /// </summary>
        public Vector3f this[float t] => orig + dir * t;
        /// <summary>
        /// A ray can be generalized to line but not viceversa
        /// </summary>
        public static implicit operator Line(Ray ray)=> new Line(ray.orig, ray.dir);
        /// <summary>
        /// A segment can be generalized to line but not viceversa
        /// </summary>
        public static implicit operator Line(Segment seg)=> new Line(seg.orig, seg.Direction);
        public static Line FromPointAndDirection(Vector3f Point, Vector3f Direction)=> new Line(Point, Direction);
        public static Line FromTwoPoint(Vector3f PointA, Vector3f PointB)=> new Line(PointA, PointA - PointB);
        
        #region Distances Line-Point
        /// <summary>
        /// <inheritdoc cref="GetPointDistance(Vector3f)"/><br/>
        /// </summary>
        public static float GetPointDistance(Vector3f orig, Vector3f dir, Vector3f point)=>Vector3f.Cross(dir, point - orig).Length;
        
        /// <summary>
        /// TO CHECK
        /// <inheritdoc cref="GetPointDistance(Vector3f)"/><br/>
        /// <i>Line defined by two generic points</i>
        /// </summary>
        /// <param name="l0">point on line</param>
        /// <param name="l1">point on line</param>
        /// <param name="p">point to get distance</param>
        public static float GetPointDistance2(Vector3f l0, Vector3f l1, Vector3f p)
        {
            var num = Vector3f.Cross(p - l0, p - l1);
            var dem = l1 - l0;
            return num.Length / dem.Length;
        }

        /// <summary>
        /// Shorter distance of generic line from point
        /// http://geomalgorithms.com/a02-_lines.html
        /// </summary>
        public float GetPointDistance(Vector3f point)=> Vector3f.Cross(dir, point - orig).Length;

        /// <summary>
        /// Shorter point (as line's parameter instead point) between lines and a point.
        /// </summary>
        public static float GetPointParam(in Vector3f orig, in Vector3f dir, Vector3f point)
        {
            point.Sub(in orig);
            return Vector3f.Dot(in point, in dir);
        }
        /// <summary>
        /// Shorter point (as line's parameter instead point) between lines and a point.
        /// </summary>
        public float GetPointParam(Vector3f point)
        {
            point.Sub(in orig);
            return Vector3f.Dot(in point, in dir);
        }
        #endregion

        #region Distances Line-Line
        /// <summary>
        /// Shorter distance between two line
        /// </summary>
        public static float GetLineDistance(Vector3f orig0, Vector3f dir0,Vector3f orig1, Vector3f dir1)
        {
            Vector3f p01 = orig1 - orig0;

            if (p01.isZero) return 0;

            Vector3f N = Vector3f.Cross(dir0, dir1);
            float length = N.Normalize();

            if (length < 1e-6f)
            {
                // if two line are parallel the distance is between line and point.
                return Vector3f.Cross(dir0, p01).Length;
            }
            else
            {
                return Vector3f.Dot(orig0, N) / length;
            }
        }
        /// <summary>
        /// Shorter distance between two line
        /// </summary>
        public float GetLineDistance(Vector3f lineOrig, Vector3f lineDir)=> GetLineDistance(orig, dir, lineOrig, lineDir);
        
        /// <summary>
        /// Shorter points (as line's parameters instead points) between two lines
        /// </summary>
        public static void GetPointsParams(
            in Vector3f orig0, in Vector3f dir0,
            in Vector3f orig1, in Vector3f dir1,
            out float t0, out float t1)
        {
            Vector3f p = orig1 - orig0;
            float a = Vector3f.Dot(in dir0, in dir1);
            float b = Vector3f.Dot(in p, in dir1);
            float c = Vector3f.Dot(in p, in dir0);
            float aa = 1 + a * a;
            t0 = (c + a * b) / aa;
            t1 = (a * c - b) / aa;
        }
        /// <summary>
        /// <see cref="GetPointsParams"/>
        /// </summary>
        public static void GetPointsParams(in Line line0, in Line line1, out float t0, out float t1)
            => GetPointsParams(in line0.orig, in line0.dir, in line1.orig, in line1.dir, out t0, out t1);

        #endregion

        public override string ToString() => $"closer: {orig} dir: {dir}";
        
    }
    /// <summary>
    /// Equivalent to a semi-Line in 3d with a point to start
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ToString()}")]
    public struct Ray
    {
        internal Vector3f orig, dir;


        /// <summary>
        /// Direction will be normalized for safety
        /// </summary>
        public Ray(Vector3f Origin, Vector3f Direction)
        {
            orig = Origin;
            dir = Direction;
            float length = dir.Normalize();
            Debug.Assert(length > float.Epsilon, "invalid direction value");
        }
        /// <summary>
        /// Get ray from screen picking. Pass the clip(x,y) coordinate. 
        /// Eye position can't be derived from matrix, the origin is on near plane
        /// </summary>
        public Ray(Vector3f eye, float clipx, float clipy, in Matrix4x4f projview_inverse)
        {
            //Vector3f p0 = new Vector3f(clipx, clipy, 0).TransformCoordinate(in projview_inverse);
            Vector3f p1 = new Vector3f(clipx, clipy, 1).TransformCoordinate(in projview_inverse);
            orig = eye;
            dir = p1 - eye;
            float length = dir.Normalize();
            Debug.Assert(length > float.Epsilon, "invalid direction value");
        }

        public Vector3f Origin=>orig;
        public Vector3f Direction => dir;
        
        /// <summary>
        /// If ray isn't initialized or empty the direction length is Zero and invdir can't be used (!DIV0)
        /// </summary>
        public bool IsNaN
        {
            get { return (dir.IsNaN || dir.LengthSq < float.Epsilon); }
        }
        /// <summary>
        /// Return a empty array not usable
        /// </summary>
        public static Ray NaN
        {
            get
            {
                Ray ray = new Ray();
                ray.orig = Vector3f.NaN;
                ray.dir = Vector3f.NaN;
                return ray;
            }

        }
        /// <summary>
        /// Get the function Ray(t)
        /// </summary>
        public Vector3f this[float t] { get { return orig + dir * t; } }

        /// <summary>
        /// A segment can be generalized to ray but not viceversa
        /// </summary>
        public static implicit operator Ray(Segment seg)
        {
            return new Ray(seg.orig, seg.dir);
        }
        
        /// <summary>
        /// Convert ray into different coordinate system
        /// </summary>
        public static Ray TransformCoordinate(Ray ray, in Matrix4x4f coordsys)
        {
            ray.orig.TransformCoordinate(in coordsys);
            ray.dir.TransformNormal(in coordsys);
            return ray;
        }

        /// <summary>
        /// NOT TESTED
        /// http://geomalgorithms.com/a02-_lines.html
        /// </summary>
        public float GetDistance(Vector3f P)
        {
            Vector3f w = P - orig;
            float dot = Vector3f.Dot(in w, in dir);

            return dot <= 0 ?
                Vector3f.Distance(in P, in orig) :// before P0
                Vector3f.Cross(in dir, in w).Length;
        }

        #region Currently Not Used

        /// <summary>
        /// can't do a perfect selection with mouse, so use a EPSILON error
        /// </summary>
        public bool IntersectSegment(Vector3f P0, Vector3f P1, float EPSILON , out float t)
        {
            t = 0;
            Vector3f u = P0 - P1;
            u.Normalize();
            Vector3f cross = Vector3f.Cross(u, dir);
            float mindist = System.Math.Abs(Vector3f.Dot(cross, orig - P0) / cross.Length);
            return mindist < EPSILON;

            /*
            Vector3 u = this.dir;
            Vector3 v = P1 - P0;
            Vector3 w = this.orig - P0;
            float a = Vector3.Dot(u, u);         // always >= 0
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v);         // always >= 0
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float D = a * c - b * b;        // always >= 0
            float sc, tc;

            // compute the line parameters of the two closest points
            if (D < EPSILON)
            {
                // the lines are almost parallel
                sc = 0.0f;
                tc = (b > c ? d / b : e / c);    // use the largest denominator
            }
            else
            {
                sc = (b * e - c * d) / D;
                tc = (a * e - b * d) / D;
            }

            // get the difference of the two closest points
            Vector3 dP = w + (sc * u) - (tc * v);  // =  L1(sc) - L2(tc)
            float mindist = dP.LengthSq();// return the closest distance^2

            return mindist < EPSILON * EPSILON;
            */
        }
        #endregion

        public override string ToString() => $"origin: {orig} dir: {dir}";
    }
    /// <summary>
    /// A line delimited by two point
    /// </summary>
    public struct Segment
    {
        internal Vector3f orig, dir;
        //i store the t value for end point
        internal float length;

        public Vector3f Direction
        {
            get { return dir; }
        }

        /// <summary>
        /// Get or Set start point, if set the Direction and lenght are updated
        /// </summary>
        public Vector3f Start
        {
            get { return orig; }
            set { dir = End - value; orig = value;  length = dir.Normalize(); }

        }
        /// <summary>
        /// Get or Set end point, if set the Direction and lenght are updated
        /// </summary>
        public Vector3f End
        {
            get { return orig + dir * length; }
            set { dir = value - orig; length = dir.Normalize(); }
        
        }
        /// <summary>
        /// Distance from Start to End point,so End point is Direction*Lenght
        /// </summary>
        public float Lenght
        { 
            get { return length; }
        }

        /// <summary>
        /// Remember that the direction is from Start ----&gt; to End
        /// </summary>
        public Segment(Vector3f Start, Vector3f End)
        {
            orig = Start;
            dir = End - Start;
            length = dir.Normalize();
        }

        /// <summary>
        /// Get the interpolated point using equation : Segment(f) = Segment.start * (1-f) + Segment.end * (f);
        /// Start = f:0  End = f:1
        /// </summary>
        /// <remarks>
        /// faster equation is p0 + (p1-p0)*f
        /// </remarks>
        public Vector3f Lerp(float f)
        {
            Vector3f p0 = Start;
            Vector3f p1 = End;

            return new Vector3f(
                p0.x + (p1.x - p0.x) * f,
                p0.y + (p1.y - p0.y) * f,
                p0.z + (p1.x - p0.z) * f);
            /*
            return new Vector3(
                p1.x * f+ p0.x * (1 - f), 
                p1.y * f + p0.y * (1 - f),
                p1.z * f + p0.z * (1 - f));
            */
        }

        /// <summary>
        /// Get the function Segment(t) where t is parametric scalar of direction
        /// </summary>
        public Vector3f this[float t] { get { return orig + dir * t; } }

        /// <summary>
        /// NOT TESTED
        /// http://geomalgorithms.com/a02-_lines.html
        /// </summary>
        public float GetDistance(Vector3f P)
        {
            float dot = Vector3f.Dot(P - orig, dir);

            if (dot <= 0)
                return Vector3f.Distance(P, orig); // before Start
            else if (dot >= length)
                return Vector3f.Distance(P, orig + dir * length); // after End
            else
                return Vector3f.Distance(P, orig + dir * dot); // between Start and End
        }

        public override string ToString()
        {
            return "Start: " + orig.ToString() + " -> End: " + End.ToString();
        }
    }

}
