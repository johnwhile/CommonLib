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
    /// Quadtree for a quad partition
    /// </summary>
    public abstract class Quadtree
    {
        /// <summary>
        /// The coordinates bounds define the basic leaf nodes addressing, example for a depth4 coordsize = (8,8)
        /// so bottomleft corned is (0,0) and topright is (7,7)
        /// </summary>
        public Vector2us CoordSize;
        /// <summary>
        /// Precumputed delta for all levels (width of root node
        /// </summary>
        public ushort[] delta;

        /// <summary>
        /// The depth of quadtree, is the number of suddivision where : 0 = no-nodes, 1 = only root node
        /// The root level = 'depth-1' and leaf children level = '0'.
        /// </summary>
        public readonly int Depth;

        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, Maximum value supported = 17</param>
        /// <remarks>
        /// due to the fact coord bits size = 2*(Depth-1) , with coord hash 32bit, max depth = 17.
        /// with hash 64bit max depth = 35
        /// </remarks>
        public Quadtree(int Depth)
        {
            if (Depth > 17 || Depth < 0) throw new ArgumentOutOfRangeException("not in range [0,17]");

            this.Depth = Depth;

            delta = new ushort[Depth];
            for (int level = Depth - 1, d = (int)System.Math.Pow(2, Depth - 1); level >= 0; level--, d /= 2)
                delta[level] = (ushort)d;

            if (delta[0] != 1) throw new Exception("math exeption");
        }



        /// <summary>
        /// Gets count of all nodes in the tree
        /// </summary>
        public abstract int NodeCount { get; }

        /// <summary>
        /// calculate number of quad with : (4^(l+1)-1)/3
        /// see: http://en.wikipedia.org/wiki/Geometric_series
        /// </summary>
        public static int MaximumNodes(int depth)
        {
            return ((1 << (2 * depth)) - 1) / 3;
        }


        /// <summary>
        /// <see cref="QuadNode.GetTileHash(Vector2us,int)"/>
        /// </summary>
        public UInt32 GetTileHash(Vector2us tilecoord)
        {
            return QuadNode.GetTileHash(tilecoord, Depth);
        }
        /// <summary>
        /// <see cref="QuadNode.GetTileHash(ushort,ushort,int)"/>
        /// </summary>
        public UInt32 GetTileHash(int tilex, int tiley)
        {
            return QuadNode.GetTileHash((ushort)tilex, (ushort)tiley, Depth);
        }
        /// <summary>
        /// <see cref="QuadNode.GetTileCoord(UInt32,int)"/>
        /// </summary>
        public Vector2us GetTileCoord(UInt32 tilehash)
        {
            return QuadNode.GetTileCoord(tilehash, Depth);
        }


#if DEBUG
        /// <summary>
        /// This array must match with QuadNodeEnumerator iterator
        /// see : http://en.wikipedia.org/wiki/Z-order_curve
        /// </summary>
        public static int[] DebugNodeIDsequence(int depth)
        {
            int count = MaximumNodes(depth);
            int[] sequence = new int[count];

            int pos = 0;
            splitsequence(sequence, 0, depth - 1, ref pos);

            return sequence;
        }
        static void splitsequence(int[] sequence, int nodeid , int level , ref int pos)
        {
            sequence[pos++] = nodeid;

            if (level > 0)
            {
                splitsequence(sequence, (nodeid << 2) + 1, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 2, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 3, level - 1, ref pos);
                splitsequence(sequence, (nodeid << 2) + 4, level - 1, ref pos);
            }
        }
#endif
    }

    /// <summary>
    /// base Quadtree
    /// </summary>
    public abstract class Quadtree<N,T> : Quadtree
        where N : QuadNode<N, T>, new()
        where T : Quadtree<N,T>
    {
        public N root;

        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, safety value = maximum 127</param>
        public Quadtree(int Depth)
            : base(Depth)
        { }

        ~Quadtree()
        {
            DestroyNode(root);
        }

        /// <summary>
        /// Destroys all nodes and update the count of sub-nodes in the current node
        /// </summary>
        public void DestroyNode(N node)
        {
            int removed = 1;
            node.Destroy(ref removed);
            node.subnodecount -= removed;
        }

        /// <summary>
        /// Gets count of all nodes in the tree
        /// </summary>
        public override int NodeCount
        {
            get { return root != null ? root.subnodecount + 1 : 0; }
        }

        /// <summary>
        /// Return the nobe defined by its address, if not exist return nearest parent or null.
        /// Not recursive function
        /// see: http://msdn.microsoft.com/en-us/library/bb259689.aspx
        /// </summary>
        public N WalkByAdress(UInt32 tilehash)
        {
            if (root == null) return null;
            
            int count = Depth - 1;
            N node = root;
            int hash = (int)tilehash;

            for (int i = 0; i < count; i++, hash >>= 2)
            {
                int qi = hash & 3;
                if (node.child == null || node.child[qi] == null) break;
                node = node.child[qi];
            }
            return node;
        }
        
        /// <summary>
        /// <seealso cref="WalkByAdress(UInt32)"/>
        /// </summary>
        public N WalkByAdress(Vector2us tilecoord)
        {
            if (root == null) return null;

            int count = Depth - 1;
            
            N node = root;

            for (int i = count-1; i >=0; i--)
            {
                int qi = (((tilecoord.y >> i) & 1) << 1) | ((tilecoord.x >> i) & 1);
                if (node.child == null || node.child[qi] == null) return node;
                node = node.child[qi];
            }
            return node;
        }

    }


    /// <summary>
    /// Quadtree for 2d rectangle partition
    /// </summary>
    public class QuadRectree<N, T> : Quadtree<N, T>
        where N : QuadNode<N, T>, IRectangleAA, new()
        where T : Quadtree<N, T>  
    {
        protected RectangleAA2 boundsize;
        /// <summary>
        /// The conversion value between quad coord and spatial value
        /// </summary>
        public Vector2f CoordDelta;
        /// <summary>
        /// Need to store the origin of coordinates
        /// </summary>
        public Vector2f Min;

        
        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, Maximum value supported = 17</param>
        /// <remarks>
        /// due to the fact coord bits size = 2*(Depth-1) , with coord hash 32bit, max depth = 17.
        /// with hash 64bit max depth = 35
        /// </remarks>
        public QuadRectree(int Depth, IRectangleAA Bound )
            : base(Depth)
        {
            this.Bound = (AABRminmax)Bound;
        }
        public QuadRectree(int Depth) : this(Depth, AABRminmax.UnitXY) { }

        /// <summary>
        /// Gets or sets the bound size of quadtree, when set where are some internal recalculations
        /// </summary>
        public AABRminmax Bound
        {
            get { return boundsize; }
            set
            {
                int maxsudd = delta[Depth - 1];
                Vector2f rectsize = value.Max - value.Min;
                this.boundsize = value;
                this.Min = value.Min;
                this.CoordDelta = rectsize / maxsudd;
                this.CoordSize = new Vector2us(maxsudd, maxsudd);
            }
        }
    }

}
