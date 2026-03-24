using System;

namespace Common.Maths
{
#pragma warning disable

    /// <summary>
    /// Frustum Boundary is a piramid section used for prospective camera volume
    /// or cube section for hortogonal camera volume. But can also be used like BoundingBox
    /// </summary>
    public struct Frustum_old
    {
        //     6______________7
        //     |\            /|           
        //      2\__________/3|    
        //     |  |        |  |               
        //---->|  |        |  |<---- Normal           
        //     | 0|________|1 |
        //     | /          \ |
        //    4|/____________\|5
        //
        /// <summary>
        /// Plane index flag
        /// </summary>
        [Flags]
        public enum ePlane : byte
        {
            NONE = 0,
            LEFT = 1, //2^0  or  1<<0
            RIGHT = 2, //2^1  or  1<<1
            TOP = 4, //2^2  or  1<<2
            BOTTOM = 8, //2^3  or  1<<3
            NEAR = 16,//2^4  or  1<<4
            FAR = 32, //2^5  or  1<<5
            ALL = LEFT | RIGHT | TOP | BOTTOM | NEAR | FAR
        }
        /// <summary> 0 left plane</summary>
        public const int LEFT = 0;
        /// <summary> 1 right plane</summary>
        public const int RIGHT = 1;
        /// <summary> 2 top plane</summary>
        public const int TOP = 2;
        /// <summary> 3 bottom plane</summary>
        public const int BOTTOM = 3;
        /// <summary> 4 near plane</summary>
        public const int NEAR = 4;
        /// <summary> 5 far plane</summary>
        public const int FAR = 5;
        /// <summary>
        /// Plane normal direction are from plane to interior of frustum volume, this because the relevant 
        /// information are inside it, example when you render a box the relevant information are outside
        /// </summary>
        public Plane[] m_plane;
        /// <summary>
        /// 8 Corners points
        /// </summary>
        public Vector3f[] m_corner;

        /// <summary>
        /// The eye of camera
        /// </summary>
        public Vector3f m_eye;

        /// <summary>
        /// Build a frustum with size defined by transform matrices
        /// </summary>
        public Frustum_old(Matrix4x4f proj, Matrix4x4f view, Matrix4x4f world)
        {
            Matrix4x4f camera = Matrix4x4f.Inverse(view);
            m_eye = new Vector3f(camera.m03, camera.m13, camera.m23);

            // Calculate the minimum Z distance in the frustum.
            float zMinimum = -proj.m23 / proj.m22;
            float screenDepth = 1.0f;
            float r = screenDepth / (screenDepth - zMinimum);
            //proj.m22 = r;
            //proj.m23 = -r * zMinimum;

            Matrix4x4f PVW = proj * view * world;

            m_plane = new Plane[6];

            // Left plane = Row(3) + Row(0) (in math notation directx col = matrix row)
            m_plane[LEFT].A = PVW.m30 + PVW.m00;
            m_plane[LEFT].B = PVW.m31 + PVW.m01;
            m_plane[LEFT].C = PVW.m32 + PVW.m02;
            m_plane[LEFT].D = PVW.m33 + PVW.m03;

            // Right plane = Row(3) - Row(0)
            m_plane[RIGHT].A = PVW.m30 - PVW.m00;
            m_plane[RIGHT].B = PVW.m31 - PVW.m01;
            m_plane[RIGHT].C = PVW.m32 - PVW.m02;
            m_plane[RIGHT].D = PVW.m33 - PVW.m03;

            // Top plane = Row(3) - Row(1)
            m_plane[TOP].A = PVW.m30 - PVW.m10;
            m_plane[TOP].B = PVW.m31 - PVW.m11;
            m_plane[TOP].C = PVW.m32 - PVW.m12;
            m_plane[TOP].D = PVW.m33 - PVW.m13;

            // Bottom plane = Row(3) + Row(1)
            m_plane[BOTTOM].A = PVW.m30 + PVW.m10;
            m_plane[BOTTOM].B = PVW.m31 + PVW.m11;
            m_plane[BOTTOM].C = PVW.m32 + PVW.m12;
            m_plane[BOTTOM].D = PVW.m33 + PVW.m13;

            // Near plane = Row(3) + Row(2)
            //m_plane[NEAR].A = PVW.m30 + PVW.m20;
            //m_plane[NEAR].B = PVW.m31 + PVW.m21;
            //m_plane[NEAR].C = PVW.m32 + PVW.m22;
            //m_plane[NEAR].D = PVW.m33 + PVW.m23;

            // Near plane = Row(2)
            m_plane[NEAR].A = PVW.m20;
            m_plane[NEAR].B = PVW.m21;
            m_plane[NEAR].C = PVW.m22;
            m_plane[NEAR].D = PVW.m23;

            // Far plane = Row(3) - Row(2)
            m_plane[FAR].A = PVW.m30 - PVW.m20;
            m_plane[FAR].B = PVW.m31 - PVW.m21;
            m_plane[FAR].C = PVW.m32 - PVW.m22;
            m_plane[FAR].D = PVW.m33 - PVW.m23;

            // Normalize planes
            for (int i = 0; i < 6; i++)
            {
                m_plane[i].Normalize();
            }

            //create the 8 points of a cube in unit-space
            m_corner = new Vector3f[8];
            float minDepth = 0;
            float maxDepth = 1;

            m_corner[0] = new Vector3f(-1, -1, minDepth); // xyz
            m_corner[1] = new Vector3f(1, -1, minDepth); // Xyz
            m_corner[2] = new Vector3f(-1, 1, minDepth); // xYz
            m_corner[3] = new Vector3f(1, 1, minDepth); // XYz

            m_corner[4] = new Vector3f(-1, -1, maxDepth); // xyZ
            m_corner[5] = new Vector3f(1, -1, maxDepth); // XyZ
            m_corner[6] = new Vector3f(-1, 1, maxDepth); // xYZ
            m_corner[7] = new Vector3f(1, 1, maxDepth); // XYZ

            // transform the 8 point in the frustum space
            Matrix4x4f inv_PVW = Matrix4x4f.Inverse(PVW);

            for (int i = 0; i < 8; i++)
                m_corner[i] = m_corner[i].TransformCoordinate(in inv_PVW);

            return;
            /*
            m_corner[4] = m_corner[4] + (m_corner[0] - m_corner[4]) * 0.5f;
            m_corner[5] = m_corner[5] + (m_corner[1] - m_corner[5]) * 0.5f;
            m_corner[6] = m_corner[6] + (m_corner[2] - m_corner[6]) * 0.5f;
            m_corner[7] = m_corner[7] + (m_corner[3] - m_corner[7]) * 0.5f;
            */
        }
        /// <summary>
        /// Build a frustum with the 8 corners point, see comment at beginning for points order
        /// </summary>
        private Frustum_old(Vector3f[] corners)
        {
            m_corner = corners;
            m_plane = new Plane[6];

            // TODO : eye is the projection of corners...
            m_eye = Vector3f.NaN;

            SetPlanesFromPoints(m_corner);
        }
        /// <summary>
        /// Build a frustum with a oriented box, the volume will be a cube like hortogonal camera projection
        /// </summary>
        private Frustum_old(OBBox box)
        {
            float x = box.Width / 2;
            float y = box.Height / 2;
            float z = box.Depth / 2;

            m_corner = new Vector3f[8];

            m_corner[0] = new Vector3f(-x, -y, -z); // xyz
            m_corner[1] = new Vector3f(x, -y, -z); // Xyz
            m_corner[2] = new Vector3f(-x, y, -z); // xYz
            m_corner[3] = new Vector3f(x, y, -z); // XYz

            m_corner[4] = new Vector3f(-x, -y, z); // xyZ
            m_corner[5] = new Vector3f(x, -y, z); // XyZ
            m_corner[6] = new Vector3f(-x, y, z); // xYZ
            m_corner[7] = new Vector3f(x, y, z); // XYZ

            for (int i = 0; i < 8; i++)
                m_corner[i] = m_corner[i].TransformCoordinate(in box.trs);

            m_plane = new Plane[6];
            m_eye = (m_corner[0] + m_corner[1] + m_corner[2] + m_corner[3]) * 0.25f;

            SetPlanesFromPoints(m_corner);
        }

        void SetPlanesFromPoints(Vector3f[] corners)
        {
            m_plane[LEFT] = new Plane(-Vector3f.Cross(corners[4] - corners[0], corners[2] - corners[0]), corners[0]);
            m_plane[RIGHT] = new Plane(-Vector3f.Cross(corners[5] - corners[1], corners[1] - corners[3]), corners[1]);
            m_plane[TOP] = new Plane(-Vector3f.Cross(corners[6] - corners[2], corners[3] - corners[2]), corners[2]);
            m_plane[BOTTOM] = new Plane(-Vector3f.Cross(corners[4] - corners[0], corners[0] - corners[1]), corners[0]);
            m_plane[NEAR] = new Plane(-Vector3f.Cross(corners[3] - corners[2], corners[0] - corners[2]), corners[0]);
            m_plane[FAR] = new Plane(-Vector3f.Cross(corners[7] - corners[6], corners[6] - corners[4]), corners[4]);
        }

        /// <summary>
        /// Get a "child" frustum by a portion of screen area, the area must be inside to screen
        /// </summary>
        /// <param name="screen">the viewport used to build frustum</param>
        /// <param name="area">the rectangle selection in the screen</param>
        private Frustum_old GetDerivedArea(ViewportClip screen, ViewportClip area)
        {
            if (screen.X > area.X ||
                screen.Y > area.Y ||
                screen.Height + screen.X < area.Height + area.X ||
                screen.Width + screen.Y < area.Width + area.Y)
                Console.WriteLine("area isn't inside original");

            float W2 = screen.Width / 2f;
            float H2 = screen.Height / 2f;

            float fleft = (W2 - area.X + screen.X) / W2;
            float ftop = (H2 - area.Y + screen.Y) / H2;
            float fright = (area.X + area.Width - screen.X - W2) / W2;
            float fbottom = (area.Y + area.Height - screen.Y - H2) / H2;

            Vector3f vup = (m_corner[2] - m_corner[0]).Normal;
            Vector3f vright = (m_corner[1] - m_corner[0]).Normal;

            Vector3f p0 = m_corner[0] + (m_corner[1] - m_corner[0]) * fleft + (m_corner[2] - m_corner[0]) * fbottom;
            Vector3f p1 = m_corner[1] + (m_corner[0] - m_corner[1]) * fright + (m_corner[2] - m_corner[0]) * fbottom;
            Vector3f p2 = m_corner[2] + (m_corner[3] - m_corner[2]) * fleft + (m_corner[0] - m_corner[2]) * ftop;
            Vector3f p3 = m_corner[3] + (m_corner[1] - m_corner[0]) * fleft + (m_corner[2] - m_corner[0]) * fbottom;

            throw new NotImplementedException("f****, too complicate...");
        }

        /// <summary>
        /// Get a "child" frustum by a portion of original area, the area must be inside to original
        /// </summary>
        /// <remarks>
        /// Near and Far planes are the same but others change orientation, the result is a "inclined" frustum
        /// </remarks>
        /// <param name="screen">the viewport used to build frustum</param>
        /// <param name="area">the rectangle selection in the screen</param>
        public Frustum_old GetDerivedArea(ViewportClip screen, ViewportClip area, Matrix4x4f proj, Matrix4x4f view, Matrix4x4f world)
        {
            int minx = area.X;
            int miny = area.Y;
            int maxx = area.X + area.Width;
            int maxy = area.Y + area.Height;

            // see comment at the beginning for points orders
            Vector3f p0 = Vector3f.Unproject(minx, maxy, 0, screen, proj, view, world);
            Vector3f p1 = Vector3f.Unproject(maxx, maxy, 0, screen, proj, view, world);
            Vector3f p2 = Vector3f.Unproject(minx, miny, 0, screen, proj, view, world);
            Vector3f p3 = Vector3f.Unproject(maxx, miny, 0, screen, proj, view, world);

            Vector3f p4 = Vector3f.Unproject(minx, maxy, 1, screen, proj, view, world);
            Vector3f p5 = Vector3f.Unproject(maxx, maxy, 1, screen, proj, view, world);
            Vector3f p6 = Vector3f.Unproject(minx, miny, 1, screen, proj, view, world);
            Vector3f p7 = Vector3f.Unproject(maxx, miny, 1, screen, proj, view, world);

            return new Frustum_old(new Vector3f[] { p0, p1, p2, p3, p4, p5, p6, p7 });
        }

        /// <summary>
        /// change the coordinate system of frustum
        /// </summary>
        public static Frustum_old TransformCoordinate(Frustum_old frustum, Matrix4x4f coordsys)
        {
            Frustum_old f = new Frustum_old();

            f.m_plane = new Plane[6];
            for (int i = 0; i < 6; i++)
                f.m_plane[i] = Plane.TransformCoordinate(frustum.m_plane[i], coordsys);

            f.m_corner = new Vector3f[8];
            for (int i = 0; i < 8; i++)
                f.m_corner[i] = frustum.m_corner[i].TransformCoordinate(in coordsys);

            return f;
        }

        #region visibility tests : fast but not implement planes flags
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
        public bool isPointVisible(float x, float y, float z, ePlane planes)
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
        /// test if exist a points inside this frustum, test only planes passed as flag, a transform inverse matrix are passed if points
        /// are in a different coordinate system.
        /// </summary>
        /// <param name="transform">the points inverse coodinate system, are used to reduce calculation</param>
        /// <param name="planes">planes to test</param>
        public bool isPointVisible(Vector3f[] points, Matrix4x4f transform, ePlane planes)
        {
            //transform frustum coodinates is less expansive than transform each points
            Frustum_old frustum = Frustum_old.TransformCoordinate(this, transform);

            foreach (Vector3f p in points)
            {
                if (isPointVisible(p.x, p.y, p.z, planes)) return true;
            }
            return false;
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
        public bool isSphereVisible(float x, float y, float z, float radius, ePlane planes)
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
        /// TODO:  test if box is visible, 16 points to test is slow
        /// </summary>
        public bool isBoxVisible(BoundingBoxMinMax box)
        {
            throw new NotImplementedException();

            // case frustum is completly inside box
            for (int i = 0; i < 8; i++)
            {
                if (box.isPointInside(m_corner[i])) return true;
            }
            //case box is completly inside frustum

            if (isPointVisible(box.max.x, box.max.y, box.max.z)) return true;
            if (isPointVisible(box.max.x, box.max.y, box.min.z)) return true;
            if (isPointVisible(box.max.x, box.min.y, box.max.z)) return true;
            if (isPointVisible(box.max.x, box.min.y, box.min.z)) return true;
            if (isPointVisible(box.min.x, box.max.y, box.max.z)) return true;
            if (isPointVisible(box.min.x, box.max.y, box.min.z)) return true;
            if (isPointVisible(box.min.x, box.min.y, box.max.z)) return true;
            if (isPointVisible(box.min.x, box.min.y, box.min.z)) return true;

            return false;
        }
        /// <summary>
        /// TODO: test if cylindrer is visible
        /// </summary>
        public bool isCylindrerVisible(float bx, float by, float bz, float tx, float ty, float tz, float radius, ePlane planes)
        {
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
        }    
        #endregion

        #region side tests : test all planes passed as flags and remove the completly inside planes
        /// <summary>
        /// test sphere - planes intersections, planes to test passed as flags return true is sphere is visible by frustum.
        /// The remains "planes" are the planes who intersect sphere.
        /// </summary>
        /// <remarks>
        /// if you not pass all planes with flag, the intersection will be calculated only to them
        /// </remarks>
        public bool GetSphereSide(float x, float y, float z, float radius, ref ePlane planes)
        {
            byte flag = (byte)planes;
            byte mask = 1;
            for (byte i = 0; i < 6; i++, mask <<= 1)
            {
                if ((flag & mask) != 0)
                {
                    float dist = m_plane[i].GetDistance(x, y, z);

                    // outside a plane                  
                    if (dist + radius < 0)
                    {
                        Console.WriteLine("Out plane " + (ePlane)mask);
                        // planes flags is unusefull, set to zero for safety
                        planes = ePlane.NONE;
                        return false;
                    }
                    // completly in the positive side of planes, remove it from flags
                    if (dist - radius > 0)
                    {
                        Console.WriteLine("Inside plane " + (ePlane)mask);
                        flag &= (byte)~mask;
                    }
                    // intersect the plane, flag plane isn't modified
                    else
                    {

                    }
                }
            }
            // flag contain the planes which intersect the sphere
            planes = (ePlane)flag;
            return true;
        }

        /// <summary>
        /// test cylindrer - planes intersections, planes to test passed as flags return true is cylindrer is visible by frustum.
        /// The remains "planes" are the planes who intersect sphere.
        /// </summary>
        /// <param name="cx">middle center coordinate</param>
        /// <param name="dx">normalized direction vector, if not normalized throw and EXCEPTION </param>
        /// <param name="semiheight">semi-height of cylinder</param>
        public bool GetCylindrerSide(float cx, float cy, float cz, float dx, float dy, float dz, float semiheight , float radius, ref ePlane planes)
        {
            byte flag = (byte)planes;
            byte mask = 1;

            for (byte i = 0; i < 6; i++, mask <<= 1)
            {
                if ((flag & mask) != 0)
                {
                    switch(m_plane[i].GetCylindrerSide(cx, cy, cz, dx, dy, dz,semiheight, radius))
                    {
                        case Plane.eSide.BACK :
                            planes = ePlane.NONE;
                            return false;
                        case Plane.eSide.FRONT :
                            flag &= (byte)~mask;
                            break;
                    }
                }
            }
            planes = (ePlane)flag;
            return true;
        }     
        public bool GetCylindrerSide(Vector3f center, Vector3f dir, float semiheight , float radius, ref ePlane planes)
        {
            return GetCylindrerSide(center.x, center.y, center.z, dir.x, dir.y, dir.z, semiheight, radius, ref planes);                          
        }
        #endregion

        /// <summary>
        /// A 3d lines rappresentation
        /// </summary>
        public void Render(out Vector3f[] position, out Color4b[] colors, out Vector2us[] segments)
        {
            position = new Vector3f[10];
            colors = new Color4b[10];

            for (int i = 0; i < m_corner.Length; i++)
                position[i] = m_corner[i];

            position[8] = (m_corner[0] + m_corner[1] + m_corner[2] + m_corner[3]) / 4f;
            position[9] = (m_corner[4] + m_corner[5] + m_corner[6] + m_corner[7]) / 4f;

            colors[0] = colors[1] = colors[2] = colors[3] = Color4b.Yellow;
            colors[4] = colors[5] = colors[6] = colors[7] = Color4b.Red;
            colors[8] = colors[8] = Color4b.Black;

            segments = new Vector2us[]
            {
                //left and right edges
                new Vector2us(0,1),
                new Vector2us(2,3),
                new Vector2us(4,5),
                new Vector2us(6,7),
                //front edges
                new Vector2us(0,2),
                new Vector2us(4,6),
                new Vector2us(0,4),
                new Vector2us(2,6),
                //back edges
                new Vector2us(1,3),
                new Vector2us(5,7),
                new Vector2us(1,5),
                new Vector2us(3,7),
                //central
                new Vector2us(8,9)
            };

        }
        
        
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < 6; i++)
            {
                str += m_plane[i].ToString() + "\n";
            }
            return str;
        }

    }

#pragma warning restore

}
