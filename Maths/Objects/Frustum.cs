using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common
{
    /// <summary>
    /// Is the Screen Box used by Directx pipeline, then left-right and top-bottom range are screen coordinate [-1,1]
    /// and the near-far is [0,1]
    /// </summary>
    /// <remarks>
    /// <code>
    ///  6______________7
    ///  |\            /|           
    ///  |2\__________/3|          y
    ///  |  |        |  |          | z    
    ///  |  |        |  |          |/___ x 
    ///  | 0|________|1 |
    ///  | /          \ |
    /// 4|/____________\|5
    /// </code>
    /// </remarks>
    public static class FrustumGeometry
    {

        public static Vector3f[] vertices;
        public static Vector3us[] triangles;
        public static Vector2us[] segments;

        static FrustumGeometry()
        {
            vertices = new Vector3f[]
            {
                new Vector3f(-1, -1, 0),
                new Vector3f( 1, -1, 0),
                new Vector3f(-1,  1, 0),
                new Vector3f( 1,  1, 0),

                new Vector3f(-1, -1, 1),
                new Vector3f( 1, -1, 1),
                new Vector3f(-1,  1, 1),
                new Vector3f( 1,  1, 1)
            };

            segments = new Vector2us[]
            {
                //near
                new Vector2us(0,1),
                new Vector2us(2,3),
                new Vector2us(0,2),
                new Vector2us(1,3),
                // far
                new Vector2us(4,5),
                new Vector2us(6,7),
                new Vector2us(4,6),
                new Vector2us(5,7),
                //left
                new Vector2us(0,4),
                new Vector2us(2,6),
                //right
                new Vector2us(1,5),
                new Vector2us(3,7)
            };

            triangles = new Vector3us[]
            {
                // left
                new Vector3us(0,4,6),
                new Vector3us(0,6,2),
                // right
                new Vector3us(1,3,7),
                new Vector3us(1,7,5),
                // top
                new Vector3us(2,6,3),
                new Vector3us(3,6,7),
                // bottom
                new Vector3us(4,0,1),
                new Vector3us(4,1,5),
                // near
                new Vector3us(0,2,1),
                new Vector3us(1,2,3),
                // far
                new Vector3us(5,7,6),
                new Vector3us(5,6,4)
            };
        }

    }

    /// <summary>
    /// http://gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
    /// </summary>
    public class Frustum
    {
        [Flags]
        public enum PlaneIndex : byte
        {
            NONE = 0,

            LEFT = 1,
            RIGHT = 2,
            TOP = 4,
            BOTTOM = 8,
            NEAR = 16,
            FAR = 32,

            LBN = LEFT | BOTTOM | NEAR,
            RBN = RIGHT | BOTTOM | NEAR,
            LTN = LEFT | TOP | NEAR,
            RTN = RIGHT | TOP | NEAR,

            LBF = LEFT | BOTTOM | FAR,
            RBF = RIGHT | BOTTOM | FAR,
            LTF = LEFT | TOP | FAR,
            RTF = RIGHT | TOP | FAR,
        }

        //     6______________7
        //     |\            /|           
        //      2\__________/3|    
        //     |  |        |  |               
        //---->|  |        |  |<---- Normal           
        //     | 0|________|1 |
        //     | /          \ |
        //    4|/____________\|5
        //

        public const int LEFT = 0;
        public const int RIGHT = 1;
        public const int TOP = 2;
        public const int BOTTOM = 3;
        public const int NEAR = 4;
        public const int FAR = 5;

        /// <summary>
        /// Plane normal are oriented to interior of volume
        /// </summary>
        public readonly Plane[] m_plane = new Plane[6];
        /// <summary>
        /// precompute corners, usefull for debug;
        /// </summary>
        public readonly Vector3f[] m_corner = new Vector3f[8];

        public static readonly PlaneIndex[] m_planename = new PlaneIndex[]
        { 
            PlaneIndex.LEFT,
            PlaneIndex.RIGHT,
            PlaneIndex.TOP,
            PlaneIndex.BOTTOM,
            PlaneIndex.NEAR,
            PlaneIndex.FAR
        };


        public static readonly PlaneIndex[] m_cornername = new PlaneIndex[]
        { 
            PlaneIndex.LBN,
            PlaneIndex.RBN,
            PlaneIndex.LTN, 
            PlaneIndex.RTN,
            PlaneIndex.LBF,
            PlaneIndex.RBF,
            PlaneIndex.LTF, 
            PlaneIndex.RTF
        };


        Matrix4x4f m_projview;
        Matrix4x4f m_invprojview;

        /// <summary>
        /// Fake initialization
        /// </summary>
        private Frustum() { }
        
        /// <summary>
        /// </summary>
        public Frustum(in Matrix4x4f projview)
        {
            MakeFrustum(in projview);
        }
        /// <summary>
        /// </summary>
        public Frustum(Matrix4x4f projview) : this(in projview)
        { }


        /// <summary>
        /// Build the frustum's planes
        /// </summary>
        /// <param name="projview">Proj * View</param>
        public void MakeFrustum(in Matrix4x4f projview)
        {
            Matrix4x4f inverse = projview.Inverse();
            MakeFrustum(in projview, in inverse);
        }
        /// <summary>
        /// Build the frustum's planes (precomputed inverse)
        /// </summary>
        /// <param name="projview">Proj * View</param>
        /// <param name="invprojview"> Inverse, used to get corners</param>
        public void MakeFrustum(in Matrix4x4f projview , in Matrix4x4f invprojview)
        {
            m_projview = projview;
            m_invprojview = invprojview;

            // Left plane = Row(3) + Row(0) (in math notation directx col = matrix row)
            m_plane[LEFT].A = projview.m30 + projview.m00;
            m_plane[LEFT].B = projview.m31 + projview.m01;
            m_plane[LEFT].C = projview.m32 + projview.m02;
            m_plane[LEFT].D = projview.m33 + projview.m03;

            // Right plane = Row(3) - Row(0)
            m_plane[RIGHT].A = projview.m30 - projview.m00;
            m_plane[RIGHT].B = projview.m31 - projview.m01;
            m_plane[RIGHT].C = projview.m32 - projview.m02;
            m_plane[RIGHT].D = projview.m33 - projview.m03;

            // Top plane = Row(3) - Row(1)
            m_plane[TOP].A = projview.m30 - projview.m10;
            m_plane[TOP].B = projview.m31 - projview.m11;
            m_plane[TOP].C = projview.m32 - projview.m12;
            m_plane[TOP].D = projview.m33 - projview.m13;

            // Bottom plane = Row(3) + Row(1)
            m_plane[BOTTOM].A = projview.m30 + projview.m10;
            m_plane[BOTTOM].B = projview.m31 + projview.m11;
            m_plane[BOTTOM].C = projview.m32 + projview.m12;
            m_plane[BOTTOM].D = projview.m33 + projview.m13;

            // Near plane = Row(3) + Row(2)  wrong
            //m_plane[NEAR].A = projview.m30 + projview.m20;
            //m_plane[NEAR].B = projview.m31 + projview.m21;
            //m_plane[NEAR].C = projview.m32 + projview.m22;
            //m_plane[NEAR].D = projview.m33 + projview.m23;

            // Near plane = Row(2)
            m_plane[NEAR].A = projview.m20;
            m_plane[NEAR].B = projview.m21;
            m_plane[NEAR].C = projview.m22;
            m_plane[NEAR].D = projview.m23;

            // Far plane = Row(3) - Row(2)
            m_plane[FAR].A = projview.m30 - projview.m20;
            m_plane[FAR].B = projview.m31 - projview.m21;
            m_plane[FAR].C = projview.m32 - projview.m22;
            m_plane[FAR].D = projview.m33 - projview.m23;
           
            // Normalize planes
            for (int i = 0; i < 6; i++)
            {
                // in the paper is write that d is for plane equation ax + by + cz + d = 0 then in my plane struct i'm using 
                // ax + by + cz = d , i need to invert sign
                m_plane[i].D = -m_plane[i].D;
                m_plane[i].Normalize();
            }

            for (int i = 0; i < 8; i++)
            {
                m_corner[i] = FrustumGeometry.vertices[i];
                m_corner[i].TransformCoordinate(m_invprojview);
            }
        }

        /// <summary>
        /// Build the frustum's planes using corners
        /// </summary>
        void MakePlaneFromCorner(Vector3f lbn, Vector3f rbn, Vector3f ltn, Vector3f rtn, Vector3f lbf, Vector3f rbf, Vector3f ltf, Vector3f rtf)
        {
            m_plane[LEFT] = new Plane(-Vector3f.Cross(ltn - lbn, ltn - lbn), lbn);
            m_plane[RIGHT] = new Plane(-Vector3f.Cross(rbf - rbn, rbn - rtn), rbn);
            m_plane[TOP] = new Plane(-Vector3f.Cross(ltf - ltn, rtn - ltn), ltn);
            m_plane[BOTTOM] = new Plane(-Vector3f.Cross(lbf - lbn, lbn - rbn), lbn);
            m_plane[NEAR] = new Plane(-Vector3f.Cross(rtn - ltn, lbn - ltn), lbn);
            m_plane[FAR] = new Plane(-Vector3f.Cross(rtf - ltf, ltf - lbf), lbf);
        }

        /// <summary>
        /// Left Bottom Near
        /// </summary>
        public Vector3f LBN { get { return m_corner[0]; } }
        /// <summary>
        /// Right Bottom Near
        /// </summary>
        public Vector3f RBN { get { return m_corner[1]; } }
        /// <summary>
        /// Left Top Near
        /// </summary>
        public Vector3f LTN { get { return m_corner[2]; } }
        /// <summary>
        /// Right Top Near
        /// </summary>
        public Vector3f RTN { get { return m_corner[3]; } }
        /// <summary>
        /// Left Bottom Far
        /// </summary>
        public Vector3f LBF { get { return m_corner[4]; } }
        /// <summary>
        /// Right Bottom Far
        /// </summary>
        public Vector3f RBF { get { return m_corner[5]; } }
        /// <summary>
        /// Left Top Far
        /// </summary>
        public Vector3f LTF { get { return m_corner[6]; } }
        /// <summary>
        /// Right Top Far
        /// </summary>
        public Vector3f RTF { get { return m_corner[7]; } }

        /// <summary>
        /// Is the Frustum matrix
        /// </summary>
        public Matrix4x4f ProjView
        {
            get { return m_projview; }
        }

        /// <summary>
        /// Is the World matrix if you render as mesh, is the inverse of ProjView
        /// </summary>
        public Matrix4x4f Transform
        {
            get { return m_invprojview; }
        }

        /// <summary>
        /// </summary>
        /// <param name="subwindows">a rectangle in range inside [-1,1] </param>
        /// <returns></returns>
        public Frustum GetSubFrustum(IRectangleAA subwindows)
        {
            float left = subwindows.Min.x;
            float right = subwindows.Max.x;
            float top = subwindows.Max.y;
            float bottom = subwindows.Min.y;


            throw new NotImplementedException();
        }

        public void CloneTo(ref Frustum copy)
        {
            if (copy == null) copy = new Frustum();
            
            m_plane.CopyTo(copy.m_plane, 0);
            m_corner.CopyTo(copy.m_corner, 0);
            
            copy.m_projview = m_projview;
            copy.m_invprojview = m_invprojview;
        }

        #region Intersections

        /// <summary>
        /// test if point is inside or coplanar to planes.
        /// </summary>
        public bool isPointVisible(float x, float y, float z)
        {
            for (int i = 0; i < 6; i++)
            {
                // if distance is negative the point is outside plane
                if (m_plane[i].GetDistance(x, y, z) < 0) return false;
            }
            return true;
        } 
        /// <summary>
        /// test if point is inside or coplanar to planes passed as flags.
        /// </summary>
        public bool isPointVisible(float x, float y, float z, PlaneIndex planes)
        {
            byte flag = (byte)planes;
            byte mask = 1;
            for (int i = 0; i < 6; i++, mask <<= 1)
            {
                if ((flag & mask) != 0)
                {
                    if (m_plane[i].GetDistance(x, y, z) < 0) return false;
                }
            }
            return true;
        }
        /// <summary>
        /// test if frustum see the sphere
        /// </summary>
        public bool isSphereVisible(float x, float y, float z, float radius)
        {
            for (int i = 0; i < 6; i++)
                if (m_plane[i].GetDistance(x, y, z) + radius < 0)
                    return false;
            return true;
        }
        /// <summary>
        /// test if frustum see the sphere, test only planes passed as flags
        /// </summary>
        public bool isSphereVisible(float x, float y, float z, float radius, PlaneIndex planes)
        {
            byte flag = (byte)planes;
            byte mask = 1;
            for (int i = 0; i < 6; i++, mask <<= 1)
            {
                if ((flag & mask) != 0)
                {
                    if (m_plane[i].GetDistance(x, y, z) + radius < 0) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool isCylindrerVisible(float bx, float by, float bz, float tx, float ty, float tz, float radius, PlaneIndex planes)
        {
            throw new NotImplementedException();
            /*
            byte flag = (byte)planes;
            byte mask = 1;
            for (int i = 0; i < 6; i++, mask <<= 1)
            {
                if ((flag & mask) != 0)
                {
                    if (m_plane[i].GetCylindrerSide(bx, by, bz, tx, ty, tz, radius) == Plane.eSide.BACK) return false;
                }
            }
            return true;
            */
        }

        /// <summary>
        /// Frustum - Sphere
        /// </summary>
        public Overlap Intersection_Sphere(Vector3f center, float radius)
        {
            return Intersection_Sphere(center.x, center.y, center.z, radius);
        }
        /// <summary>
        /// Frustum - Sphere
        /// </summary>
        public Overlap Intersection_Sphere(float x, float y, float z, float radius)
        {
            float dist;
            bool intersect = false;
            for (int i = 0; i < 6; i++)
            {
                dist = m_plane[i].GetDistance(x, y, z);
                if (dist < -radius) return Overlap.OUTSIDE;

                if (Maths.Mathelp.ABS(dist) < radius) intersect |= true;
            }
            return intersect ? Overlap.INTERSECT : Overlap.INSIDE;
        }
        /// <summary>
        /// Frustum - AABB (tested)
        /// </summary>
        public Overlap Intersection_AABB(Vector3f min,Vector3f max)
        {
            Overlap result = Overlap.INSIDE;

            int inside, outside;

            // for each plane do ...
            for (int i = 0; i < 6; i++)
            {
                // reset counters for corners in and out
                outside = 0; inside = 0;
                // for each corner of the box do ...
                // get out of the cycle as soon as a box as corners
                // both inside and out of the frustum

                if (m_plane[i].GetDistance(min.x, min.y, min.z) < 0) outside++; else inside++;
                if (inside == 0 || outside == 0)
                {
                    if (m_plane[i].GetDistance(min.x, min.y, max.z) < 0) outside++; else inside++;
                    if (inside == 0 || outside == 0)
                    {
                        if (m_plane[i].GetDistance(min.x, max.y, min.z) < 0) outside++; else inside++;
                        if (inside == 0 || outside == 0)
                        {
                            if (m_plane[i].GetDistance(min.x, max.y, max.z) < 0) outside++; else inside++;
                            if (inside == 0 || outside == 0)
                            {
                                if (m_plane[i].GetDistance(max.x, min.y, min.z) < 0) outside++; else inside++;
                                if (inside == 0 || outside == 0)
                                {
                                    if (m_plane[i].GetDistance(max.x, min.y, max.z) < 0) outside++; else inside++;
                                    if (inside == 0 || outside == 0)
                                    {
                                        if (m_plane[i].GetDistance(max.x, max.y, min.z) < 0) outside++; else inside++;
                                        if (inside == 0 || outside == 0)
                                        {
                                            if (m_plane[i].GetDistance(max.x, max.y, max.z) < 0) outside++; else inside++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //if all corners are out
                if (inside == 0) 
                    return (Overlap.OUTSIDE);
                // if some corners are out and others are in	
                else if (outside != 0) 
                    result = Overlap.INTERSECT;
            }
            return result;
        }
        /// <summary>
        /// Frustum - OBB (tested)
        /// </summary>
        public Overlap Intersection_OBB(Matrix4x4f trs)
        {
            float dist, radius;
            bool intersect = false;

            for (int i = 0; i < 6; i++)
            {
                Vector3f n = m_plane[i].norm;

                radius = m_plane[i].GetDistance(trs.m03, trs.m13, trs.m23);

                dist = Maths.Mathelp.ABS(trs.m00 * n.x + trs.m10 * n.y + trs.m20 * n.z) +
                       Maths.Mathelp.ABS(trs.m01 * n.x + trs.m11 * n.y + trs.m21 * n.z) +
                       Maths.Mathelp.ABS(trs.m02 * n.x + trs.m12 * n.y + trs.m22 * n.z);
                
                if (dist < -radius) return Overlap.OUTSIDE;

                if (dist > Maths.Mathelp.ABS(radius)) intersect |= true;
            }
            return intersect ? Overlap.INTERSECT : Overlap.INSIDE;
        }


        #endregion
    }
}
