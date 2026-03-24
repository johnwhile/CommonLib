using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common.Maths;
using Common.Tools;

namespace Common.Partitions
{
    [Flags]
    public enum QuadIndex : byte
    {
        None = 0,

        All = Q0 | Q1 | Q2 | Q3,
        
        /// <summary> BottomLeft </summary>
        Q0 = 1,
        /// <summary> BottomRight </summary>
        Q1 = 2,
        /// <summary> TopLeft </summary>
        Q2 = 4,
        /// <summary> TopRight </summary>
        Q3 = 8
    }

    /// <summary>
    /// Base Class
    /// </summary>
    public abstract class QuadNode : IDestroyable
    {
        public static int InstanceCounter = 0;
        public const int AvarageSizeInByte = 0;

        public const byte ALL = 0xFF;
        public const byte Q0 = 1 << 0;
        public const byte Q1 = 1 << 1;
        public const byte Q2 = 1 << 2;
        public const byte Q3 = 1 << 3;

        public sbyte level, index;
        public int nodeid = -1;
        public int subnodecount = 0;
        public byte childrenFlag = 0;
        public Vector2us tilecoord;

        /// <summary>
        /// Reseting this instance restore to initialization state, example when you implement a pool memory mechanism
        /// </summary>
        protected void Reset()
        {
            level = index = -1;
            subnodecount = nodeid = -1;
            childrenFlag = 0;
            tilecoord = default(Vector2us);
        }

        /// <summary>
        /// NodeID is unique for all node in the same tree
        /// </summary>
        public override int GetHashCode()
        {
            return nodeid;
        }

        /// <summary>
        /// Gets if is the most detailed node, not the last in tree
        /// </summary>
        public bool IsLeaf
        {
            get { return level == 0; }
        }

        /// <summary>
        /// Return the total count of nodes tree that this node contain
        /// </summary>
        public int SubNodeCount 
        {
            get { return subnodecount; } 
        }

        /// <summary>
        /// take track of children used and initialized in the child array
        /// </summary>
        public QuadIndex childused
        {
            get { return (QuadIndex)childrenFlag; }
        }

        /// <summary>
        /// get the unique coordinate of node (true only for leaf node), 
        /// parent have same BottomLeft but different TopRight.
        /// </summary>
        public Vector2us BottomLeft
        {
            get { return tilecoord; }
        }
        /// <summary>
        /// Empty initialization, hide it from user
        /// </summary>
        public QuadNode()
        {
            InstanceCounter++;
        }

        ~QuadNode()
        {
            InstanceCounter--;
        }
        /// <summary>
        /// Destroying this instance don't remove but reset parameters and remove references
        /// </summary>
        public virtual void Destroy()
        {
            Reset();
        }

        /// <summary>
        /// Divide this parent in 4 children
        /// </summary>
        public abstract void Split();

        #region QuadUtils
        
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
        /// calculate the tile hash code untill leaf nodes (Morton-order-encoding)
        /// see: http://msdn.microsoft.com/en-us/library/bb259689.aspx
        /// </summary>
        /// <remarks>
        /// I prefer encode address hash with current method:
        /// example
        /// depth = 4
        /// parent hierarchy = root.child[1].child[2].child[3] : coordxy = (5,3)
        /// x = 101 ; y = 011 (3 bit because = depth-1) in fact max coord 7 = 111
        /// h = y.0 x.0 y.1 x.1 y.2 x.2 = 111001 (6 bit)
        /// when i decompose address i optain parent hierarchy:
        /// 11=3 10=2 01=1
        /// 
        /// Notice that address is unique only for leaf node
        /// </remarks>
        public static UInt32 GetTileHash(Vector2us tilecoord, int quadtreeDepth)
        {
            return GetTileHash(tilecoord.x, tilecoord.y, quadtreeDepth);
        }


        /// <summary cref="GetTileHash(Vector2us,int)">
        /// (Morton-order-encoding) <see cref="GetTileHash(Vector2us,int)"/>
        /// </summary>
        public static UInt32 GetTileHash(ushort tilex,ushort tiley, int quadtreeDepth)
        {
            int address = 0;
            //appreciate the elegance of formula :)
            for (int i = 2 * (quadtreeDepth - 1); i >= 0; tilex >>= 1, tiley >>= 1)
            {
                address |= (tiley & 1) << (--i);
                address |= (tilex & 1) << (--i);
            }
            return (uint)address;
        }
        
        
        /// <summary>
        /// Decode the tilehash, not used (Morton-order-decoding)
        /// </summary>
        public static Vector2us GetTileCoord(UInt32 tilehash,int quadtreeDepth)
        {
            uint x = 0;
            uint y = 0;

            for (int i = quadtreeDepth - 2; i >= 0; i--)
            {
                x |= (tilehash & 1) << i;
                tilehash >>= 1;
                y |= (tilehash & 1) << i;
                tilehash >>= 1;
            }
            return new Vector2us((ushort)x, (ushort)y);
        }

        /// <summary>
        /// Show the complete walking indices of quadtree to find the leaf node encoded by its tilehash
        /// </summary>
        public static int[] GetTileHashHierarchy(UInt32 tilehash, int quadtreeDepth)
        {
            int count = quadtreeDepth - 1; // last indices is always 0 because leaf node 
            int[] walkindices = new int[count];
            int hash = (int)tilehash;

            for (int i = 0; i < count; i++, hash >>= 2)
                walkindices[i] = hash & 3;

            return walkindices;
        }

        /// <summary>
        /// Show the complete walking indices of quadtree to find the leaf node encoded by its tilecoord.
        /// </summary>
        public static int[] GetTileHashHierarchy(Vector2us tilecoord, int quadtreeDepth)
        {
            int count = quadtreeDepth - 1; // last indices is always 0 because leaf node 
            int[] walkindices = new int[count];

            int x = tilecoord.x;
            int y = tilecoord.y;

            for (int i = 0; i < count; i++, x >>= 1, y >>= 1)
                walkindices[i] = (y & 1) << 1 | (x & 1);

            return walkindices;
        }
          
        #endregion

        public override string ToString()
        {
            return string.Format("Quad{0} [{1},{2}] i:{3} l:{4}", nodeid, tilecoord.x, tilecoord.y, index, level);
        }


    }

    /// <summary>
    /// Base linkable node, it is defined a basic quad node structure, with one parent and 4 children. Are necessary for all enumerators
    /// </summary>
    public abstract class QuadNode<N,T> : QuadNode
        where N : QuadNode<N,T> , new()
        where T : Quadtree<N,T>
    {
        public N parent;
        public N[] child;
        public T main;

        /// <summary>
        /// Fake Initialization
        /// </summary>
        public QuadNode()
            : base()
        { }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public QuadNode(T main)
            : base()
        {
            SetAsRoot(main);
        }

        /// <summary>
        /// Initialization for node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        /// <param name="tilex">define the bottomleft corner</param>
        public QuadNode(N parent, int index, ushort tilex, ushort tiley)
            : base()
        {
            SetAsNode(parent, index, tilex,  tiley);
        }

        /// <summary>
        /// Initialization for node, auto calculate coord
        /// </summary>
        public QuadNode(N parent, int index)
            : base()
        {
            ushort delta = main.delta[level];
            ushort x = parent.tilecoord.x;
            ushort y = parent.tilecoord.y;

            switch (index)
            {
                case 1: x += delta; break;
                case 2: y += delta; break;
                case 3: x += delta; y += delta; break;
            }

            SetAsNode(parent, index, x, y);
        }


        /// <summary>
        /// Sets the node as root
        /// </summary>
        public void SetAsRoot(T main)
        {
            this.main = main;
            this.parent = null;
            this.level = (sbyte)(main.Depth - 1);
            this.nodeid = 0;
            this.index = -1;
            this.subnodecount = 0;
            this.tilecoord = new Vector2us(0, 0);
        }

        /// <summary>
        /// Set this node as child of parent T
        /// </summary>
        /// <param name="coord">the bottomleft corner in quad coordinates</param>
        public void SetAsNode(N parent, int index, ushort tilex, ushort tiley)
        {
            this.parent = parent;
            this.main = parent.main;
            this.level = (sbyte)(parent.level - 1);
            this.index = (sbyte)index;
            this.parent.childrenFlag |= (byte)(1 << index);
            this.nodeid = (parent.nodeid << 2) + (index + 1);
            this.tilecoord.x = tilex;
            this.tilecoord.y = tiley;

            N current = parent;
            while (current != null) { current.subnodecount += this.subnodecount; current = current.parent; }
        }

        /// <summary>
        /// <see cref="QuadNode.BottomLeft"/> Notice what for leaf node is equal to BottomLeft 
        /// </summary>
        public Vector2us TopRight
        {
            get
            {
                int delta = main.delta[level];
                return new Vector2us(tilecoord.x + delta - 1, tilecoord.y + delta - 1);
            }
        }

        /// <summary>
        /// </summary>
        void recurseremove(ref int count)
        {
            if (child != null)
                foreach (N node in child)
                    if (node != null)
                    {
                        node.recurseremove(ref count);
                        node.Reset(); 
                    }

            count++;
            parent = null;
            child = null;
            main = null;
        }

        /// <summary>
        /// TODO : check if childremoved is correct
        /// Recursive function for <seealso cref="QuadNode.Destroy()"/>. An optional value is return to know how many children are removed
        /// </summary>
        /// <param name="childremoved">will be set to zero as initial value</param>
        public void Destroy(ref int childremoved)
        {
            if (parent != null)
            {
                parent.childrenFlag &= (byte)~(Q0 << index);
                if (child != null) child[index] = null;
            }
            childremoved = 0;
            recurseremove(ref childremoved);
        }

        /// <summary>
        /// <seealso cref="QuadNode.Destroy()"/> this node and update parent
        /// </summary>
        public override void Destroy()
        {
            int count = 0;
            Destroy(ref count);
        }

        /// <summary>
        /// Recursive splitting
        /// </summary>
        public virtual void RecursiveSplit()
        {
            if (level > 0)
            {
                Split();
                foreach (N node in child) node.RecursiveSplit();
            }
        }

        /// <summary>
        /// Divide this parent in 4 children, not destroy existing nodes, override it for unboxing performance
        /// </summary>
        public override void Split()
        {
            childrenFlag = ALL;

            int delta = main.delta[level] / 2;
            ushort xm = (ushort)(tilecoord.x + delta);
            ushort ym = (ushort)(tilecoord.y + delta);

            if (child == null) child = new N[4];

            if (child[0] == null)
            {
                child[0] = new N();
                child[0].SetAsNode(this as N, 0, tilecoord.x, tilecoord.y);
            }
            if (child[1] == null)
            {
                child[1] = new N();
                child[1].SetAsNode(this as N, 1, xm, tilecoord.y);
            }
            if (child[2] == null)
            {
                child[2] = new N();
                child[2].SetAsNode(this as N, 2, tilecoord.x, ym);
            }
            if (child[3] == null)
            {
                child[3] = new N();
                child[3].SetAsNode(this as N, 3, xm, ym);
            }

        }

        /// <summary>
        /// Only for debug purpose, in Tag properties are linked the QUadNode class
        /// </summary>
        public TreeNode GetDebugView()
        {
            TreeNode node = new TreeNode(this.ToString() + " :" + this.GetType().Name);
            node.Tag = this;
            for (int i = 0, mask = 1; i < 4; i++, mask <<= 1)
            {
                if ((childrenFlag & mask) != 0) node.Nodes.Add(child[i].GetDebugView());
            }
            return node;
        }
    }


    /// <summary>
    /// Base linkable node, it is defined a basic quad node structure, with one parent and 4 children. Are necessary for all enumerators
    /// </summary>
    public abstract class QuadRectNode<N, T> : QuadNode<N, T> , IRectangleAA
        where N : QuadNode<N, T>, IRectangleAA, new()
        where T : QuadRectree<N, T>
    {
         /// <summary>
        /// Fake Initialization
        /// </summary>
        public QuadRectNode()
            : base()
        { }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public QuadRectNode(T main)
            : base()
        {
            SetAsRoot(main);
        }

        /// <summary>
        /// Initialization for node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        /// <param name="tilex">define the bottomleft corner</param>
        public QuadRectNode(N parent, int index, ushort tilex, ushort tiley)
            : base()
        {
            SetAsNode(parent, index, tilex, tiley);
        }

        /// <summary>
        /// Initialization for node, auto calculate coord
        /// </summary>
        public QuadRectNode(N parent, int index)
            : base()
        {
            ushort delta = main.delta[level];
            ushort x = parent.tilecoord.x;
            ushort y = parent.tilecoord.y;

            switch (index)
            {
                case 1: x += delta; break;
                case 2: y += delta; break;
                case 3: x += delta; y += delta; break;
            }

            SetAsNode(parent, index, x, y);
        }


        public Vector2f Min
        {
            get
            {
                return new Vector2f(
                    main.Min.x + tilecoord.x * main.CoordDelta.x,
                    main.Min.y + tilecoord.y * main.CoordDelta.y);
            }
        }
        public Vector2f Max
        {
            get
            {
                int delta = main.delta[level];
                return new Vector2f(
                    main.Min.x + (tilecoord.x + delta) * main.CoordDelta.x,
                    main.Min.y + (tilecoord.y + delta) * main.CoordDelta.y);
            }
        }
        public Vector2f Center
        {
            get
            {
                // remember that at leaf node delta[level] = 1 so use float
                float halfdelta = main.delta[level] / 2.0f;
                return new Vector2f(
                    main.Min.x + (tilecoord.x + halfdelta) * main.CoordDelta.x,
                    main.Min.y + (tilecoord.y + halfdelta) * main.CoordDelta.y);
            }
        }
        public Vector2f HalfSize
        {
            get
            {
                float halfdelta = main.delta[level] / 2.0f;
                return new Vector2f(
                    main.Min.x + main.CoordDelta.x * halfdelta,
                    main.Min.y + main.CoordDelta.y * halfdelta);
            }
        }


        public Vector2f Size
        {
            get { return HalfSize * 2.0f; }
        }
    }

}
