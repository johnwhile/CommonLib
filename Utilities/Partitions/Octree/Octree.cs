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
    /// Octree for 3d box partition
    /// </summary>
    public class Octree<T> where T : OctNode
    {
        public int NodeCount = 0;
        public T root;
        public readonly sbyte Depth;


        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, Maximum value = 127</param>
        public Octree(int LevelCount)
        {
            if (LevelCount > 127) throw new ArgumentOutOfRangeException("max 127");
            this.Depth = (sbyte)LevelCount;
        }

        ~Octree()
        {
            Destroy();
        }

        public void Destroy()
        {
            this.root.Destroy();
        }

        /// <summary>
        /// calculate number of quad with : (4^(l+1)-1)/3
        /// see: http://en.wikipedia.org/wiki/Geometric_series
        /// </summary>
        public static int MaximumNodes(int depth)
        {
            return ((1 << (3 * depth)) - 1) / 7;
        }

#if DEBUG
        /// <summary>
        /// This array must match with QuadNodeEnumerator iterator
        /// </summary>
        public static int[] DebugNodeIDsequence(int depth)
        {
            int count = MaximumNodes(depth);
            int[] sequence = new int[count];

            int pos = 0;
            splitsequence(sequence, 0, depth - 1, ref pos);

            return sequence;
        }

        static void splitsequence(int[] sequence, int nodeid, int level, ref int pos)
        {
            sequence[pos++] = nodeid;

            if (level > 0)
            {
                splitsequence(sequence, (nodeid << 2) + 1, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 2, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 3, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 4, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 5, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 6, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 7, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 8, level - 1, ref pos);
            }
        }
#endif
    }
}
