using System;
using System.Collections.Generic;
using System.Text;

using Common.Tools;
using Common.Maths;
using Common.Geometry;

namespace Common.Partitions
{
    public class QuadVoxelTree : QuadRectree<VoxelNode2d, QuadVoxelTree>
    {
                /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, safety value = maximum 127</param>
        public QuadVoxelTree(int Depth, IRectangleAA size)
            : base(Depth, size)
        { }

        public QuadVoxelTree(int Depth)
            : base(Depth, AABRminmax.UnitXY)
        { }
    }


    public class VoxelTree2d
    {
        public Dictionary<UInt32, VoxelNode2d>[] leveladdress;

        public QuadVoxelTree quadtree;
        
        public BitArray2 density;
        public int Width, Height;


        /// <summary>
        /// Initializes a quadtree using a bit grid
        /// </summary>
        public VoxelTree2d(BitArray2 density)
        {
            this.density = density;
            Width = density.Width;
            Height = density.Heigth;
            int depth = GetDepth(Maths.Mathelp.MIN(Width, Height));
            leveladdress = new Dictionary<UInt32, VoxelNode2d>[depth];
            for (int l = 0; l < depth; l++)
                leveladdress[l] = new Dictionary<UInt32, VoxelNode2d>();
            quadtree = new QuadVoxelTree(depth);
        }

        /// <summary>
        /// Get maximum depht from width of heigth
        /// </summary>
        /// <remarks>
        /// S = min(W,H)
        /// D0 : s 0 1,
        /// D1 : s 2,
        /// D2 : s 3 4,
        /// D3 : s 5 6 7 8,
        /// D4 : s 9...
        /// </remarks>
        public int GetDepth(int size)
        {
            int d = 0;
            while (size > 1) { size = (size + 1) / 2; d++; }
            return d;
        }

        public void makeEdge(VoxelNode2d node, int curr, int next)
        {
            // for borders case is possible that some points are not initialized because there aren't a neighbours
            if (node.v[curr] == null) node.v[curr] = new VoxelPoint(node.Center + node.HalfSize * VoxelConst2d.verts[curr]);
            if (node.v[next] == null) node.v[next] = new VoxelPoint(node.Center + node.HalfSize * VoxelConst2d.verts[next]);

            node.v[curr].next = node.v[next];
            node.v[next].prev = node.v[curr];
        }

        public List<Polygon> ExtractPolygons()
        {
            // Now all polygons have a continuos chain of vertices. Some vertices are created in neigbourg node linking but not used,
            // so check if 'next' value is not null.
            // Extract them.
            List<Polygon> polygons = new List<Polygon>();
            List<Vector2f> polygonpointlist = new List<Vector2f>();
            Stack<VoxelPoint> prev_chain = new Stack<VoxelPoint>();

            foreach (KeyValuePair<UInt32, VoxelNode2d> pair in leveladdress[0])
            {
                VoxelNode2d node = pair.Value;
                int vxl = (int)node.voxelcase;

                sbyte[,] indices = VoxelConst2d.edgetable;

                for (int v = 0; v < 4; v++)
                {
                    if (indices[vxl, v] < 8)
                    {
                        VoxelPoint start = node.v[v];
                        if (start != null && !start.processed && start.prev != null)
                        {
                            // Build chain going back from start point
                            prev_chain.Clear();

                            VoxelPoint curr = start;
                            do
                            {
                                //if (curr.processed) throw new Exception("possible intersection ?");
                                curr.processed = true;
                                prev_chain.Push(curr);
                                curr = curr.prev;
                            }
                            while (curr != null && curr != start);
                              
                            polygonpointlist.Clear();
                            polygonpointlist.Capacity = prev_chain.Count;

                            while (prev_chain.Count > 0) polygonpointlist.Add(prev_chain.Pop().value);


                            // point list not close so build remain chain going forward from start point
                            if (curr != start)
                            {
                                curr = start.next;
                                do
                                {
                                    if (curr.processed) throw new Exception("possible intersection ?");
                                    curr.processed = true;
                                    polygonpointlist.Add(curr.value);
                                    curr = curr.next;
                                }
                                while (curr != null && curr != start);
                            }

                            polygons.Add(new Polygon(polygonpointlist));
                        }
                    }
                }
            }

            return polygons;
        }
    }

    public class VoxelTree2d_v1 : VoxelTree2d
    {
        /// <summary>
        /// Initializes a quadtree using a bit grid
        /// </summary>
        public VoxelTree2d_v1(BitArray2 density) :
            base(density)
        {
            VoxelInfo densitycase;
            VoxelInfo result = RecursiveBuilder(quadtree.Depth - 1, -1, 0, 0, 0, Width - 1, Height - 1, out quadtree.root, out densitycase);

            LinkNeighbours();
        }

        /// <summary>
        /// Version 2 : convert a 2d density map to a quadtree structure. The difference from v1 is that the full density leaf node
        /// found at border of map are create to ensure draw also border, otherwise will be empty 
        /// 
        /// To check : can't exist empty leaf or parent nodes, but can exist full leaf node
        /// </summary>
        public VoxelInfo RecursiveBuilder(int level, int index, int nodeid, int sx, int sy, int ex, int ey, out VoxelNode2d node, out VoxelInfo densitycase)
        {
            int mx = (sx + ex) / 2;
            int my = (sy + ey) / 2;

            VoxelNode2d child0, child1, child2, child3;
            QuadIndex voxelcase = 0;
            VoxelInfo result = VoxelInfo.MIXED;
            densitycase = VoxelInfo.MIXED;
            node = null;

            VoxelBorderType bordercase = 0;
            if (sx == 0) bordercase |= VoxelBorderType.LEFT;
            if (sy == 0) bordercase |= VoxelBorderType.BOTTOM;
            if (ex == Width - 1) bordercase |= VoxelBorderType.RIGHT;
            if (ey == Height - 1) bordercase |= VoxelBorderType.TOP;


            if (level == 0)
            {
                child0 = child1 = child2 = child3 = null;

                if (density[sx, sy]) voxelcase |= QuadIndex.Q0;
                if (density[ex, sy]) voxelcase |= QuadIndex.Q1;
                if (density[sx, ey]) voxelcase |= QuadIndex.Q2;
                if (density[ex, ey]) voxelcase |= QuadIndex.Q3;

                if (voxelcase == QuadIndex.None) densitycase = result = VoxelInfo.EMPTY;
                if (voxelcase == QuadIndex.All) densitycase = result = VoxelInfo.FULL;
            }
            else
            {
                VoxelInfo density0, density1, density2, density3;

                VoxelInfo case0 = RecursiveBuilder(level - 1, 0, (nodeid << 2) + 1, sx, sy, mx, my, out child0, out density0);
                VoxelInfo case1 = RecursiveBuilder(level - 1, 1, (nodeid << 2) + 2, mx, sy, ex, my, out child1, out density1);
                VoxelInfo case2 = RecursiveBuilder(level - 1, 2, (nodeid << 2) + 3, sx, my, mx, ey, out child2, out density2);
                VoxelInfo case3 = RecursiveBuilder(level - 1, 3, (nodeid << 2) + 4, mx, my, ex, ey, out child3, out density3);

                // notice that is case-i is full the node wasn't create but voxelcase flag are true, this because is not necessary store
                // a tree completly full
                if (case0 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q0;
                if (case1 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q1;
                if (case2 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q2;
                if (case3 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q3;

                if (case0 == VoxelInfo.EMPTY &&
                    case1 == VoxelInfo.EMPTY &&
                    case2 == VoxelInfo.EMPTY &&
                    case3 == VoxelInfo.EMPTY)
                    result = VoxelInfo.EMPTY;

                if (case0 == VoxelInfo.FULL &&
                    case1 == VoxelInfo.FULL &&
                    case2 == VoxelInfo.FULL &&
                    case3 == VoxelInfo.FULL)
                    result = VoxelInfo.FULL;

                if (density0 == VoxelInfo.FULL &&
                    density1 == VoxelInfo.FULL &&
                    density2 == VoxelInfo.FULL &&
                    density3 == VoxelInfo.FULL)
                    densitycase = VoxelInfo.FULL;

                else if (density0 == VoxelInfo.EMPTY &&
                    density1 == VoxelInfo.EMPTY &&
                    density2 == VoxelInfo.EMPTY &&
                    density3 == VoxelInfo.EMPTY)
                    densitycase = VoxelInfo.EMPTY;
                else
                    densitycase = VoxelInfo.MIXED;
            }

            // TODO : in this version the leaf node completly full can be created but parent see it as mixed only 
            // to continue with algorithm, but in future must contain the correct information.
            if (level == 0 && voxelcase == QuadIndex.All) result = VoxelInfo.FULL;

            if (result == VoxelInfo.MIXED)
            {
                node = new VoxelNode2d();
                node.parent = null;
                node.child = null;
                node.main = quadtree;
                node.index = (sbyte)index;
                node.level = (sbyte)level;
                node.nodeid = nodeid;
                node.tilecoord.x = (ushort)sx;
                node.tilecoord.y = (ushort)sy;
                node.voxelcase = voxelcase;
                //node.bordercase = bordercase;
                node.v = new VoxelPoint[4];

                if (level > 0)
                {
                    // for parent, must exist at least one child not null, otherwise is a error
                    node.child = new VoxelNode2d[] { child0, child1, child2, child3 };
                    bool ok = false;
                    if (child0 != null) { child0.parent = node; node.childrenFlag |= QuadNode.Q0; ok |= true; }
                    if (child1 != null) { child1.parent = node; node.childrenFlag |= QuadNode.Q1; ok |= true; }
                    if (child2 != null) { child2.parent = node; node.childrenFlag |= QuadNode.Q2; ok |= true; }
                    if (child3 != null) { child3.parent = node; node.childrenFlag |= QuadNode.Q3; ok |= true; }

                    if (!ok) throw new Exception("parent node is mixed but don't exist at least one node");
                }
                else
                {

                }

                // coordinates are used only to find the address of node
                //node.coord = new VoxelCoord(sx, sy, ex - sx + 1, ey - sy + 1);

                UInt32 tilehash = quadtree.GetTileHash(node.BottomLeft);

                leveladdress[level].Add(tilehash, node);
            }

            return result;
        }


        public void LinkNeighbours()
        {
            foreach (KeyValuePair<UInt32, VoxelNode2d> pair in leveladdress[0])
            {
                sbyte[,] indices = VoxelConst2d.edgetable;

                VoxelNode2d current = pair.Value;
                Vector2f center = current.Center;
                Vector2f halfsize = current.HalfSize;

                int vxl = (int)current.voxelcase;


                // link the neighbour cells
                leveladdress[0].TryGetValue(quadtree.GetTileHash(current.tilecoord.x, current.tilecoord.y + 1), out current.Top);
                leveladdress[0].TryGetValue(quadtree.GetTileHash(current.tilecoord.x, current.tilecoord.y - 1), out current.Bottom);
                leveladdress[0].TryGetValue(quadtree.GetTileHash(current.tilecoord.x - 1, current.tilecoord.y), out current.Left);
                leveladdress[0].TryGetValue(quadtree.GetTileHash(current.tilecoord.x + 1, current.tilecoord.y), out current.Right);

                // get a list of only necessary points to use in polygons
                VoxelVertexType flag = 0;

                for (int i = 0; i < 4; i++)
                    if (indices[vxl, i] < 8)
                        flag |= (VoxelVertexType)(1 << indices[vxl, i]);

                // link found vertices with neightbours and initializate the class if not already done to generate a reference
                if (current.Top != null)
                {
                    current.v[1] = current.Top.v[3];
                    if (current.v[1] == null && (flag & VoxelVertexType.V1) != 0) 
                        current.Top.v[3] = current.v[1] = new VoxelPoint(center + halfsize * VoxelConst2d.verts[1]);
                }
                if (current.Bottom != null)
                {
                    current.v[3] = current.Bottom.v[1];
                    if (current.v[3] == null && (flag & VoxelVertexType.V3) != 0) 
                        current.Bottom.v[1] = current.v[3] = new VoxelPoint(center + halfsize * VoxelConst2d.verts[3]);
                }
                if (current.Left != null)
                {
                    current.v[0] = current.Left.v[2];
                    if (current.v[0] == null && (flag & VoxelVertexType.V0) != 0) 
                        current.Left.v[2] = current.v[0] = new VoxelPoint(center + halfsize * VoxelConst2d.verts[0]);
                }
                if (current.Right != null)
                {
                    current.v[2] = current.Right.v[0];
                    if (current.v[2] == null && (flag & VoxelVertexType.V2) != 0) 
                        current.Right.v[0] = current.v[2] = new VoxelPoint(center + halfsize * VoxelConst2d.verts[2]);
                }

                // link the default voxel lines
                if (indices[vxl, 0] < 8) makeEdge(current, indices[vxl, 0], indices[vxl, 1]);
                if (indices[vxl, 2] < 8) makeEdge(current, indices[vxl, 2], indices[vxl, 3]);
            }
        }

    }

    public class VoxelTree2d_v2 : VoxelTree2d
    {
        /// <summary>
        /// Initializes a quadtree using a bit grid
        /// </summary>
        public VoxelTree2d_v2(BitArray2 density) : 
            base(density)
        {
            VoxelInfo densitycase;
            VoxelInfo result = RecursiveBuilder(quadtree.Depth - 1, -1, -1, 0, 0, Width - 1, Height - 1, out quadtree.root, out densitycase);
        }

        /// <summary>
        /// </summary>
        public VoxelInfo RecursiveBuilder(int level, int index, int nodeid, int sx, int sy, int ex, int ey, out VoxelNode2d node, out VoxelInfo densitycase)
        {
            int mx = (sx + ex) / 2;
            int my = (sy + ey) / 2;

            VoxelNode2d child0, child1, child2, child3;
            QuadIndex voxelcase = 0;
            VoxelBorderType bordercase = 0;
            VoxelInfo result = VoxelInfo.MIXED;
            densitycase = VoxelInfo.MIXED;
            node = null;

            if (sx == 0) bordercase |= VoxelBorderType.LEFT;
            if (sy == 0) bordercase |= VoxelBorderType.BOTTOM;
            if (ex == Width - 1) bordercase |= VoxelBorderType.RIGHT;
            if (ey == Height - 1) bordercase |= VoxelBorderType.TOP;

            if (level == 0)
            {
                child0 = child1 = child2 = child3 = null;

                if (density[sx, sy]) voxelcase |= QuadIndex.Q0;
                if (density[ex, sy]) voxelcase |= QuadIndex.Q1;
                if (density[sx, ey]) voxelcase |= QuadIndex.Q2;
                if (density[ex, ey]) voxelcase |= QuadIndex.Q3;

                if (voxelcase == QuadIndex.None) densitycase = result = VoxelInfo.EMPTY;
                if (voxelcase == QuadIndex.All) densitycase = result = VoxelInfo.FULL;
            }
            else
            {
                VoxelInfo density0, density1, density2, density3;

                VoxelInfo case0 = RecursiveBuilder(level - 1, 0, (nodeid << 2) + 1, sx, sy, mx, my, out child0, out density0);
                VoxelInfo case1 = RecursiveBuilder(level - 1, 1, (nodeid << 2) + 2, mx, sy, ex, my, out child1, out density1);
                VoxelInfo case2 = RecursiveBuilder(level - 1, 2, (nodeid << 2) + 3, sx, my, mx, ey, out child2, out density2);
                VoxelInfo case3 = RecursiveBuilder(level - 1, 3, (nodeid << 2) + 4, mx, my, ex, ey, out child3, out density3);

                // notice that is case-i is full the node wasn't create but voxelcase flag are true, this because is not necessary store
                // a tree completly full
                if (case0 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q0;
                if (case1 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q1;
                if (case2 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q2;
                if (case3 != VoxelInfo.EMPTY) voxelcase |= QuadIndex.Q3;

                if (case0 == VoxelInfo.EMPTY &&
                    case1 == VoxelInfo.EMPTY &&
                    case2 == VoxelInfo.EMPTY &&
                    case3 == VoxelInfo.EMPTY)
                    result = VoxelInfo.EMPTY;

                if (case0 == VoxelInfo.FULL &&
                    case1 == VoxelInfo.FULL &&
                    case2 == VoxelInfo.FULL &&
                    case3 == VoxelInfo.FULL)
                    result = VoxelInfo.FULL;

                if (density0 == VoxelInfo.FULL &&
                    density1 == VoxelInfo.FULL &&
                    density2 == VoxelInfo.FULL &&
                    density3 == VoxelInfo.FULL)
                    densitycase = VoxelInfo.FULL;

                else if (density0 == VoxelInfo.EMPTY &&
                    density1 == VoxelInfo.EMPTY &&
                    density2 == VoxelInfo.EMPTY &&
                    density3 == VoxelInfo.EMPTY)
                    densitycase = VoxelInfo.EMPTY;
                else
                    densitycase = VoxelInfo.MIXED;
            }

            // TODO : in this version the leaf node completly full can be created but parent see it as mixed only 
            // to continue with algorithm, but in future must contain the correct information.
            if (level == 0 && voxelcase == QuadIndex.All && bordercase != 0) result = VoxelInfo.MIXED;

            if (result == VoxelInfo.MIXED)
            {
                node = new VoxelNode2d();
                node.parent = null;
                node.child = null;
                node.main = quadtree;
                node.index = (sbyte)index;
                node.level = (sbyte)level;
                node.nodeid = nodeid;
                node.tilecoord.x = (ushort)sx;
                node.tilecoord.y = (ushort)sy;
                node.voxelcase = voxelcase;
                node.bordercase = bordercase;

                if (bordercase == VoxelBorderType.NONE)
                    node.v = new VoxelPoint[4];
                else
                    node.v = new VoxelPoint[8];

                if (level > 0)
                {
                    // for parent, must exist at least one child not null, otherwise is a error
                    node.child = new VoxelNode2d[] { child0, child1, child2, child3 };
                    bool ok = false;
                    if (child0 != null) { child0.parent = node; node.childrenFlag |= QuadNode.Q0; ok |= true; }
                    if (child1 != null) { child1.parent = node; node.childrenFlag |= QuadNode.Q1; ok |= true; }
                    if (child2 != null) { child2.parent = node; node.childrenFlag |= QuadNode.Q2; ok |= true; }
                    if (child3 != null) { child3.parent = node; node.childrenFlag |= QuadNode.Q3; ok |= true; }

                    if (!ok) throw new Exception("parent node is mixed but don't exist at least one node");
                }
                else
                {

                }

                // coordinates are used only to find the address of node
                //node.coord = new VoxelCoord(sx, sy, ex - sx + 1, ey - sy + 1);

                UInt32 tilehash = quadtree.GetTileHash(node.BottomLeft);

                leveladdress[level].Add(tilehash, node);
            }

            return result;
        }

    }
}
