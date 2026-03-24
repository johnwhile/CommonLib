using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Partitions
{
    public static class VoxelConst2d
    {
        //   2------3
        //   |      |
        //   |      |
        //   0------1
        //
        //   v6--v1--v7
        //   |        |
        //   v0      v2
        //   |        |
        //   v4--v3--v5

        /// <summary>
        /// index -1 = none
        /// [0,1,2,3] are the standard voxel case line
        /// [4,5,6,7,8,9,10,11] are respectivly the left,top,right,bottom pair for border case
        /// 
        /// IMPORTANT, all edge or pair of indices, are oriented in direction of density
        /// </summary>
        public static sbyte[,] edgetable;
        public const int LeftPos = 4;
        public const int TopPos = 6;
        public const int RightPos = 8;
        public const int BottomPos = 10;


        public static Vector2f[] verts = new Vector2f[] 
        { 
            // middle edge points
            -Vector2f.UnitX, //v0
            Vector2f.UnitY,  //v1
            Vector2f.UnitX,  //v2
            -Vector2f.UnitY, //v3
            // corners
            new Vector2f(-1,-1),//p0
            new Vector2f(1,-1), //p1
            new Vector2f(-1,1), //p2
            new Vector2f(1,1),  //p3
        };

        static VoxelConst2d()
        {
            edgetable = new sbyte[,] 
            { 
                { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 }, 
                { 0, 3, 8, 8, 4, 0, 8, 8, 8, 8, 3, 4 },
                { 3, 2, 8, 8, 8, 8, 8, 8, 2, 5, 5, 3 }, 
                { 0, 2, 8, 8, 4, 0, 8, 8, 2, 5, 5, 4 }, 
                { 1, 0, 8, 8, 0, 6, 6, 1, 8, 8, 8, 8 }, 
                { 1, 3, 8, 8, 4, 6, 6, 1, 8, 8, 3, 4 }, 
                { 3, 2, 1, 0, 0, 6, 6, 1, 2, 5, 5, 3 }, 
                { 1, 2, 8, 8, 4, 6, 6, 1, 2, 5, 5, 4 }, 
                { 2, 1, 8, 8, 8, 8, 1, 7, 7, 2, 8, 8 },
                { 0, 3, 2, 1, 4, 0, 1, 7, 7, 2, 3, 4 },
                { 3, 1, 8, 8, 8, 8, 1, 7, 7, 5, 5, 3 },
                { 0, 1, 8, 8, 4, 0, 1, 7, 7, 5, 5, 4 }, 
                { 2, 0, 8, 8, 0, 6, 6, 7, 7, 2, 8, 8 }, 
                { 2, 3, 8, 8, 4, 6, 6, 7, 7, 2, 3, 4 }, 
                { 3, 0, 8, 8, 0, 6, 6, 7, 7, 5, 5, 3 }, 
                { 8, 8, 8, 8, 4, 6, 6, 7, 7, 5, 5, 4 } 
            };
        }
    }
}
