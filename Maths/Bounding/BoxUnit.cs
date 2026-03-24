
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Common.Maths
{

    /// <summary>
    /// A FrontTopLeft box volume for texture usage, the uvw coordinated system rappresent the xyz values where
    /// minimun corner uvw(0,0,0) is (Left,Top,Front) , maximum cordner uv(1,1,1) is (Right,Bottom,Back)
    /// </summary>
    /// <remarks>
    /// <para>   W,Depth       </para>
    /// <para>  /              </para>
    /// <para> +------ U,Width </para>
    /// <para> |               </para>
    /// <para> |               </para>
    /// <para> V,Height        </para>
    /// </remarks>
    public struct VolumeUVW
    {
        /// <summary>
        /// Minimum corner, are U V W
        /// </summary>
        public int X, Y, Z;
        /// <summary>
        /// Edges size
        /// </summary>
        public int Width, Height, Depth;

        /// <summary>
        /// pixels box using min-size format, int UVW coordinate system (Y is from top left)
        /// </summary>
        public static VolumeUVW FromSize(int u, int v, int w, int width, int height, int depth)
        {
            if (width < 0 || height < 0 || depth < 0) throw new ArgumentOutOfRangeException("must be positive");
            VolumeUVW box = new VolumeUVW();
            box.X = u;
            box.Y = v;
            box.Z = w;
            box.Width = width;
            box.Height = height;
            box.Depth = depth;
            return box;
        }
        /// <summary>
        /// pixels box using min-max coordinate in UVW coordinate system (Y is from top left)
        /// </summary>
        public static VolumeUVW FromPoints(int umin, int umax, int vmin, int vmax, int wmin, int wmax)
        {
            if (umin < 0 || vmin < 0 || wmin < 0 || umax < 0 || vmax < 0 || wmax < 0) throw new ArgumentOutOfRangeException("must be positive");
            if (umin > umax || vmin > vmax || wmin > wmax) throw new ArgumentOutOfRangeException("order must be correct");

            VolumeUVW box = new VolumeUVW();
            box.X = umin;
            box.Y = vmax;
            box.Z = wmin;
            box.Width = umax - umin + 1;
            box.Height = vmin - vmax + 1;
            box.Depth = wmax - wmin + 1;
            return box;
        }

        static void swap(ref int a, ref int b) { int t = a; a = b; b = t; }

        public int Left
        {
            get { return X; }
        }
        public int Top
        {
            get { return Y; }
        }
        public int Front
        {
            get { return Z; }
        }
        public int Right
        {
            get { return X + Width - 1; }
        }
        public int Bottom
        {
            get { return Y + Height - 1; }
        }
        public int Back
        {
            get { return Z + Depth - 1; }
        }
        public override string ToString()
        {
            return string.Format("Left{0},Top{1},Front{2} ; Right{3},Bottom{4},Back{5}", Left, Top, Front, Right, Bottom, Back);
        }
    }
}
