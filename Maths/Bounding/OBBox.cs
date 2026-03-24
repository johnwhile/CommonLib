using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Common.Tools;


namespace Common.Maths
{

    /// <summary>
    /// Oriented Bounding Box defined by a 1°scale-2°rotation-3°traslation (T*R*S)
    /// </summary>
    /// <remarks>
    /// <code>
    ///  Ax.x * Sx  Ay.x * Sy  Az.x * Sz  center.x
    ///  Ax.y * Sx  Ay.y * Sy  Az.y * Sz  center.y
    ///  Ax.z * Sx  Ay.z * Sy  Az.z * Sz  center.z
    ///       0          0          0         1
    /// </code>
    /// </remarks>
    public struct OBBox
    {
        internal static Vector3f[] m_unitcorner = new Vector3f[]
        {
            new Vector3f(-1,-1,-1),
            new Vector3f(-1,-1, 1),
            new Vector3f(-1, 1,-1),
            new Vector3f(-1, 1, 1),
            new Vector3f( 1,-1,-1),
            new Vector3f( 1,-1, 1),
            new Vector3f( 1, 1,-1),
            new Vector3f( 1, 1, 1)
        };

        public Matrix4x4f trs;


        public OBBox(Matrix4x4f TRS)
        {
            trs = TRS;
        }

        /// <summary>
        /// </summary>
        /// <param name="Extend">Scale factor, (Extend.x = Box Width in UnitX space) </param>
        public OBBox(Vector3f UnitX, Vector3f UnitY, Vector3f UnitZ, Vector3f Extend, Vector3f Center)
        {
            trs = new Matrix4x4f(
                UnitX.x * Extend.x, UnitY.x * Extend.y, UnitZ.x * Extend.z, Center.x,
                UnitX.y * Extend.x, UnitY.y * Extend.y, UnitZ.y * Extend.z, Center.y,
                UnitX.z * Extend.x, UnitY.z * Extend.y, UnitZ.z * Extend.z, Center.z,
                0, 0, 0, 1);
        }

        /// <summary>
        /// You must use an affine trasformation
        /// </summary>
        public Matrix4x4f TRS
        {
            get { return trs; }
            set { trs = value; }
        }


        float widthSq { get { return trs.m00 * trs.m00 + trs.m10 * trs.m10 + trs.m20 * trs.m20; } }
        float heightSq { get { return trs.m01 * trs.m01 + trs.m11 * trs.m11 + trs.m21 * trs.m21; } }
        float depthSq { get { return trs.m02 * trs.m02 + trs.m12 * trs.m12 + trs.m22 * trs.m22; } }


        /// <summary>
        /// scale x equal to 2 * HalfLenght
        /// </summary>
        public float Width { get { return (float)System.Math.Sqrt(widthSq); } }
        /// <summary> 
        /// scale Y
        /// </summary>
        public float Height { get { return (float)System.Math.Sqrt(heightSq); } }
        /// <summary>
        /// scale Z
        /// </summary>
        public float Depth { get { return (float)System.Math.Sqrt(depthSq); } }


        public static OBBox NaN => new OBBox(default(Matrix4x4f));

        /// <summary>
        /// Center = zero , Scale = (1,1,1) , Rotation = zero
        /// </summary>
        public static OBBox Unit => new OBBox(Matrix4x4f.Identity);
        
        public Vector3f Corner(int i) => m_unitcorner[i].TransformCoordinate(in trs);

        /// <summary>
        /// if scale factor is negative or zero
        /// </summary>
        public bool isNaN => trs.m00 < 1e-7 || trs.m11 < 1e-7 || trs.m22 < 1e-7;
        

        public bool isPointInside(Vector3f p)=>isPointInside(p.x, p.y, p.z);
        
        public bool isPointInside(float x, float y, float z)
        {
            // if project of T to one axis  dot(T,Ai) > halfW = S[i]/2 return outside
            /*
            Vector3 T = new Vector3(x - trs.m03, y - trs.m13, z - trs.m23); 
            float coefx = UtilsMath.ABS(Vector3.Dot(T, trs.getCol(0).Normal));
            float coefy = UtilsMath.ABS(Vector3.Dot(T, trs.getCol(1).Normal));
            float coefz = UtilsMath.ABS(Vector3.Dot(T, trs.getCol(2).Normal));
            return coefx < Width * 0.5f && coefy < Width * 0.5f && coefz < Depth * 0.5f;
            */

            //  to avoid sqrt i use dot(T,Ai)*S[i] > S[i]*S[i]*0.5 where Ai*S[i] = col0 and Si^2 = dot(col0,col0)

            x -= trs.m03; 
            y -= trs.m13;
            z -= trs.m23;
            return Mathelp.ABS(x * trs.m00 + y * trs.m10 + z * trs.m20) < widthSq * 0.5f &&
                   Mathelp.ABS(x * trs.m01 + y * trs.m11 + z * trs.m21) < heightSq * 0.5f &&
                   Mathelp.ABS(x * trs.m02 + y * trs.m12 + z * trs.m22) < depthSq * 0.5f;
        }


        /// <summary>
        /// Get the AABB from OBB, the orientation will be lost so the viceversa operaton doesn't return same obbox
        /// </summary>
        public static explicit operator BoundingBoxMinMax(OBBox obb)
        {
            // using the sum of all projection size to the unit vector of aabb

            // W = |Sx * (Ax . UnitX)| + |Sy * (Ay . UnitX)| + |Sz * (Az . UnitX)|
            // W = |Sx*Ax.x| + |Sy*Ay.x| + |Sz*Az.x|
            // W = |m00| + |m01| + |m02|

            /*
            AABBox aabb = AABBox.NaN;
            for (int i = 0; i < 8; i++)
                aabb.Merge(Vector3.TransformCoordinate(m_unitcorner[i], obb.trs));
            return aabb;
            */

            float hW = (Mathelp.ABS(obb.trs.m00) + Mathelp.ABS(obb.trs.m01) + Mathelp.ABS(obb.trs.m02)) * 0.5f;
            float hH = (Mathelp.ABS(obb.trs.m10) + Mathelp.ABS(obb.trs.m11) + Mathelp.ABS(obb.trs.m12)) * 0.5f;
            float hD = (Mathelp.ABS(obb.trs.m20) + Mathelp.ABS(obb.trs.m21) + Mathelp.ABS(obb.trs.m22)) * 0.5f;
            return new BoundingBoxMinMax(
                obb.trs.m03 - hW, obb.trs.m13 - hH, obb.trs.m23 - hD,
                obb.trs.m03 + hW, obb.trs.m13 + hH, obb.trs.m23 + hD);
        }
        /// <summary>
        /// Get the AABB2 from OBB, the orientation will be lost so the viceversa operaton doesn't return same obbox
        /// </summary>
        public static explicit operator BoundingBoxCenter(OBBox obb)
        {
            float hW = (Mathelp.ABS(obb.trs.m00) + Mathelp.ABS(obb.trs.m01) + Mathelp.ABS(obb.trs.m02)) * 0.5f;
            float hH = (Mathelp.ABS(obb.trs.m10) + Mathelp.ABS(obb.trs.m11) + Mathelp.ABS(obb.trs.m12)) * 0.5f;
            float hD = (Mathelp.ABS(obb.trs.m20) + Mathelp.ABS(obb.trs.m21) + Mathelp.ABS(obb.trs.m22)) * 0.5f;
            return new BoundingBoxCenter(obb.trs.m03, obb.trs.m13, obb.trs.m23, hW, hH, hD);
        }
        
        public static explicit operator OBBox2(OBBox obb)=>new OBBox2(obb.trs);
        

        public override string ToString()
        {
            if (isNaN) return "NULL_OBB";

            StringBuilder str = new StringBuilder();
            str.Append(string.Format("Center : {0}\n", trs.Position));
            str.Append(string.Format("Dimension : {0,4} {1,4} {2,4}\n", Width, Height, Depth));

            return str.ToString();
        }
    }

    /// <summary>
    /// I prefere store the value separatly instead one matrix
    /// </summary>
    /// <remarks>
    /// <code>
    ///  Ax.x * Sx  Ay.x * Sy  Az.x * Sz  center.x
    ///  Ax.y * Sx  Ay.y * Sy  Az.y * Sz  center.y
    ///  Ax.z * Sx  Ay.z * Sy  Az.z * Sz  center.z
    ///       0          0          0         1
    /// </code>
    /// </remarks>
    public struct OBBox2
    {
        public Vector3f Ax, Ay, Az;
        public Vector3f S;
        public Vector3f C;

        public OBBox2(Vector3f UnitX, Vector3f UnitY, Vector3f UnitZ, Vector3f Extend, Vector3f Center)
        {
            Ax = UnitX;
            Ay = UnitY;
            Az = UnitZ;
            S = Extend;
            C = Center;
        }

        public OBBox2(Matrix4x4f TRS)
            : this()
        {
            extract(ref TRS);
        }

        void extract(ref Matrix4x4f trs)
        {
            this.Ax = trs.getCol(0);
            this.Ay = trs.getCol(1);
            this.Az = trs.getCol(2);
            this.S = Vector3f.Zero;
            this.S.x = Ax.Normalize();
            this.S.y = Ay.Normalize();
            this.S.z = Az.Normalize();
            this.C = trs.Position;
        }

        /// <summary>
        /// i'm use a standard Traslation * Rotation * Scale (first scale, second rotate, last traslate). This standard
        /// but be respected if you want to use an affine trasformation
        /// </summary>
        public Matrix4x4f TRS
        {
            get
            {
                return new Matrix4x4f(
                    Ax.x * S.x, Ay.x * S.y, Az.x * S.z, C.x,
                    Ax.y * S.x, Ay.y * S.y, Az.y * S.z, C.y,
                    Ax.z * S.x, Ay.z * S.y, Az.z * S.z, C.z,
                    0, 0, 0, 1);
            }
            set { extract(ref value); }
        }

        public static OBBox2 NaN
        {
            get { return new OBBox2 { S = Vector3f.Zero, C = Vector3f.Zero }; }
        }
        /// <summary>
        /// if scale factor is negative or zero
        /// </summary>
        public bool isNaN
        {
            get { return (S.x < 1e-7 || S.y < 1e-7 || S.z < 1e-7); }
        }

        /// <summary>
        /// Center = zero , Scale = (1,1,1) , Rotation = zero
        /// </summary>
        public static OBBox2 Unit
        {
            get { return new OBBox2 { C = Vector3f.Zero, S = Vector3f.One, Ax = Vector3f.UnitX, Ay = Vector3f.UnitY, Az = Vector3f.UnitZ }; }
        }


        public bool isPointInside(Vector3f p)
        {
            return isPointInside(p.x, p.y, p.z);
        }

        public bool isPointInside(float x, float y, float z)
        {
            // if project of T to one axis  dot(T,Ai) > halfW = S[i]/2 return outside
            x -= C.x;
            y -= C.y;
            z -= C.z;
            return Mathelp.ABS(x * Ax.x + y * Ax.y + z * Ax.z) < S.x * 0.5f &&
                   Mathelp.ABS(x * Ay.x + y * Ay.y + z * Ay.z) < S.y * 0.5f &&
                   Mathelp.ABS(x * Az.x + y * Az.y + z * Az.z) < S.z * 0.5f;
        }


        public static explicit operator OBBox(OBBox2 obb)
        {
            return new OBBox(obb.TRS);
        }
        /// <summary>
        /// Get the AABB from OBB2, the orientation will be lost so the viceversa operaton doesn't return same obbox
        /// </summary>
        public static explicit operator BoundingBoxMinMax(OBBox2 obb)
        {
            // using the sum of all projection size to the unit vector of aabb

            // W = |Sx * (Ax . UnitX)| + |Sy * (Ay . UnitX)| + |Sz * (Az . UnitX)|
            // W = |Sx*Ax.x| + |Sy*Ay.x| + |Sz*Az.x|
            float hW = (Mathelp.ABS(obb.S.x * obb.Ax.x) + Mathelp.ABS(obb.S.y * obb.Ay.x) + Mathelp.ABS(obb.S.z * obb.Az.x)) * 0.5f;
            float hH = (Mathelp.ABS(obb.S.x * obb.Ax.y) + Mathelp.ABS(obb.S.y * obb.Ay.y) + Mathelp.ABS(obb.S.z * obb.Az.y)) * 0.5f;
            float hD = (Mathelp.ABS(obb.S.x * obb.Ax.z) + Mathelp.ABS(obb.S.y * obb.Ay.z) + Mathelp.ABS(obb.S.z * obb.Az.z)) * 0.5f;
            return new BoundingBoxMinMax(
                obb.C.x - hW, obb.C.y - hH, obb.C.z - hD,
                obb.C.x + hW, obb.C.y + hH, obb.C.z + hD);
        }
        /// <summary>
        /// Get the AABB2 from OBB2, the orientation will be lost so the viceversa operaton doesn't return same obbox
        /// </summary>
        public static explicit operator BoundingBoxCenter(OBBox2 obb)
        {
            float hW = (Mathelp.ABS(obb.S.x * obb.Ax.x) + Mathelp.ABS(obb.S.y * obb.Ay.x) + Mathelp.ABS(obb.S.z * obb.Az.x)) * 0.5f;
            float hH = (Mathelp.ABS(obb.S.x * obb.Ax.y) + Mathelp.ABS(obb.S.y * obb.Ay.y) + Mathelp.ABS(obb.S.z * obb.Az.y)) * 0.5f;
            float hD = (Mathelp.ABS(obb.S.x * obb.Ax.z) + Mathelp.ABS(obb.S.y * obb.Ay.z) + Mathelp.ABS(obb.S.z * obb.Az.z)) * 0.5f;
            return new BoundingBoxCenter(obb.C.x, obb.C.y, obb.C.z, hW, hH, hD);
        }
    }

}