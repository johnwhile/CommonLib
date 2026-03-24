using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Partitions
{
    public enum VoxelInfo
    {
        EMPTY, MIXED, FULL
    }
    [Flags]
    public enum VoxelBorderType : byte
    {
        NONE = 0,
        LEFT = 1 << 0,  //1
        TOP = 1 << 1,   //2
        RIGHT = 1 << 2, //4
        BOTTOM = 1 << 3 //8
    }
    [Flags]
    public enum VoxelVertexType : short
    {
        NONE = 0,
        V0 = 1 << 0,
        V1 = 1 << 1,
        V2 = 1 << 2,
        V3 = 1 << 3,
        V4 = 1 << 4,
        V5 = 1 << 5,
        V6 = 1 << 6,
        V7 = 1 << 7,
    }


    public class VoxelNode2d : QuadRectNode<VoxelNode2d,QuadVoxelTree>
    {     
        public QuadIndex voxelcase = QuadIndex.None;
       
        public VoxelNode2d Top, Bottom, Left, Right;
        
        /// <summary>
        /// Can be basic 4 vertice or 8 for border case
        /// </summary>
        public VoxelPoint[] v;
        /// <summary>
        /// If node are touch the border of bound, need additional vertices
        /// </summary>
        public VoxelBorderType bordercase;

        public VoxelNode2d() :
            base()
        { }

        public VoxelNode2d(QuadVoxelTree main) :
            base(main)
        { }

        public VoxelNode2d(VoxelNode2d parent, int index, ushort tilex, ushort tiley) :
            base(parent, index, tilex, tiley)
        { }

        public override string ToString()
        {
            return base.ToString() + " vxl: " + voxelcase.ToString();
        }
    }

    public class VoxelPoint
    {
        public static int idcounter = 0;
        public int ID = -1;
        public VoxelPoint next;
        public VoxelPoint prev;

        public bool processed;
        public Vector2f value;

        public VoxelPoint(Vector2f value)
        {
            ID = idcounter++;
            processed = false;
            next = prev = null;
            this.value = value;
        }
        public override string ToString()
        {
            return ID.ToString() + " , " + value.ToString();
        }
    }


}
