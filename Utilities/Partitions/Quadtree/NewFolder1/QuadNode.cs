
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common;
using Common.Tools;

namespace Common.Quadtree
{
    /// <summary>
    /// Contain the precomputer value that are not necessary store foreach node
    /// </summary>
    public abstract class QuadTree
    {
        /// <summary>
        /// Maximum depth of tree, must be set at the beginning
        /// </summary>
        public int DepthCount;
        /// <summary>
        /// precomputed pow2 for all depth, where first (or depth = 0) is zero
        /// </summary>
        public int[] pow2;
        /// <summary>
        /// precomputed hierarchy indices, used for one interation when walk in the tree
        /// </summary>
        public byte[] hierarchy;

        public QuadTree(int DepthCount)
        {
            this.DepthCount = DepthCount;

            if (DepthCount < 1 || DepthCount > 16) throw new ArgumentOutOfRangeException("Maximum suddivision = 16 because tilecoord use ushort, minimum is 1 because root node is the first suddivision");

            pow2 = new int[DepthCount];
            pow2[0] = 1;
            for (int d = 1; d < DepthCount; d++) pow2[d] = (ushort)(pow2[d - 1] * 2);

            hierarchy = new byte[DepthCount];
        }

        /// <summary>
        /// calculate number of quad with : (4^(l+1)-1)/3
        /// see: http://en.wikipedia.org/wiki/Geometric_series
        /// </summary>
        /// <param name="depth">must be &lt; 32</param>
        public static ulong MaximumNodes(int depth)
        {
            ulong i = 1;
            i = ((i << (2 * depth)) - 1) / 3;
            return i;
        }

    }


    /// <summary>
    /// Contain the precomputer value that are not necessary store foreach node
    /// </summary>
    public abstract class QuadTree<N> : QuadTree
        where N : QuadNode
    {
        public N root;

        public QuadTree(int DepthCount)
            : base(DepthCount)
        {
            this.root = null;
        }

    }


    /// <summary>
    /// Base Class to contain common values
    /// </summary>
    public abstract class QuadNode : IDestroyable
    {
        // quad tree can be very expansive, show now many instances existing in the garbage collector
        public static int InstanceCounter = 0;

        public const byte ALL = 0xFF;
        public const byte Q0 = 1 << 0;
        public const byte Q1 = 1 << 1;
        public const byte Q2 = 1 << 2;
        public const byte Q3 = 1 << 3;

        protected sbyte level;
        protected byte index;
        protected int nodeid = -1;
        protected int subnodecount = 0;
        protected byte childrenFlag = 0;
        
        public TileCoord16 tilecoord;
        
        public QuadNode(int DepthCount)
        {
            InstanceCounter++;
            this.level = (sbyte)(DepthCount - 1);
            this.index = 0;
            this.nodeid = 0;
            this.tilecoord = new TileCoord16(0, 0);
        }

        public QuadNode(QuadNode parent , byte index)
        {
            InstanceCounter++;
            this.index = index;
            // tile coord can be derive from index
            int x = parent.tilecoord.x * 2;
            int y = parent.tilecoord.y * 2;
            if ((index & 1) != 0) x++;
            if ((index & 2) != 0) y++;
            this.tilecoord = new TileCoord16(x, y);
        }

        ~QuadNode()
        {
            InstanceCounter--;
        }


        /// <summary>
        /// Return the Q0,1,2,3 flag for child used, if 0 mean no child
        /// </summary>
        public byte ChildrenFlag
        {
            get { return childrenFlag; }
        }

        /// <summary>
        /// level 0 is the leaf node
        /// </summary>
        public int Level { get { return level; } }

        /// <summary>
        /// Return the total count of nodes tree that this node contain
        /// </summary>
        public int SubNodeCount
        {
            get { return subnodecount; }
        }
        /// <summary>
        /// This formula return the nodeid of parent
        /// </summary>
        public static int GetParentById(int nodeid)
        {
            return (nodeid - 1) >> 2;
        }
        /// <summary>
        /// This formula return the index of nodeid
        /// </summary>
        public static int GetIndexById(int nodeid)
        {
            return (nodeid - 1) & 3;
        }

        /// <summary>
        /// Reseting this instance restore to initialization state, example when you implement a pool memory mechanism
        /// </summary>
        protected void Reset()
        {
            level = -1;
            index = 0;
            subnodecount = nodeid = -1;
            childrenFlag = 0;
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// +----+----+
    /// | 2  |  3 |
    /// +----+----+
    /// | 0  |  1 |
    /// +----+----+
    /// </remarks>
    public abstract class QuadNode<N,T> : QuadNode
        where N : QuadNode<N,T>
        where T : QuadTree<N>
    {

        public T tree;
        public N parent;
        public N[] child;

        /// <summary>
        /// Depth 0 is the root node
        /// </summary>
        public int Depth { get { return tree.DepthCount - level - 1; } }

        
        /// <summary>
        /// Initialize as Root Node
        /// </summary>
        /// <param name="DepthCount">depth of tree, maximum 16 , minimum 1</param>
        public QuadNode(T tree)
            : base(tree.DepthCount)
        {
            this.parent = null;
            this.tree = tree;
            this.tree.root = this as N;
        }
        
        /// <summary>
        /// Initialize as Child Node
        /// </summary>
        public QuadNode(N parent, byte index)
            : base(parent, index)
        {
            this.level = (sbyte)(parent.level - 1);
            this.nodeid = (parent.nodeid << 2) + (index + 1);

            this.parent = parent;
            this.parent.child[index] = this as N;
            this.parent.childrenFlag |= (byte)(1 << index);

            this.tree = parent.tree;
        }

        /// <summary>
        /// Return the neightbour of current quad, obviosly the neighbour have depth less or equal to it, but is possible that
        /// there are a lot of other neighbours after returned neighbour is was splitted
        /// </summary>
        /// <param name="n">0:left 1:top 2:right 3:bottom</param>
        public N GetNeighbour(int n)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Only for debug purpose, in Tag properties are linked the QUadNode class
        /// </summary>
        public TreeNode GetDebugView()
        {
            TreeNode node = new TreeNode(this.ToString());
            node.Tag = this as N;
            if (child != null) foreach (N quad in child) if (quad != null) node.Nodes.Add(quad.GetDebugView());
            return node;
        }
        
        public override string ToString()
        {
            return string.Format("d{0} i{1} c{2}", Depth, index, tilecoord);
        }
    }
}
