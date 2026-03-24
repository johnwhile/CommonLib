using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Bounding Volume Hierarchy for 3d box partition
    /// </summary>
    public class BvhTree<T> where T : BvhNode
    {
        public int NodeCount = 0;
        public T root;
        public readonly sbyte Depth;


        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, Maximum value = 127</param>
        public BvhTree(int LevelCount)
        {
            if (LevelCount > 127) throw new ArgumentOutOfRangeException("max 127");
            this.Depth = (sbyte)LevelCount;
        }

        ~BvhTree()
        {
            Destroy();
        }

        public void Destroy()
        {
            this.root.Destroy();
        }

        /// <summary>
        /// </summary>
        public static int MaximumNodes(int depth)
        {
            return 0;
        }

    }
}
