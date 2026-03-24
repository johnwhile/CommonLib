
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;

using Common.Tools;
using Common.Maths;

namespace Common.Partitions
{
    /// <summary>
    /// Voxel Version
    /// </summary>
    public class VoxelOctree : IEnumerable<VoxelNode>
    {
        public BitArray3 density;
        public VoxelNode root;
        public int Depth = 0;
        public int VoxelCount = 0;

        public VoxelOctree(string filename)
        {
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                BinaryReader reader = new BinaryReader(file);
                BoundingBoxMinMax size = new BoundingBoxMinMax();
                size.min = Vector3f.Zero;
                size.max.x = reader.ReadSingle();
                size.max.y = reader.ReadSingle();
                size.max.z = reader.ReadSingle();

                Depth = reader.ReadInt32();
                int width = (int)System.Math.Pow(2, Depth);
                int numDensityPt = (width + 1) * (width + 1) * (width + 1);

                byte[] stream = reader.ReadBytes(numDensityPt / 8);

                density = new BitArray3(width + 1, width + 1, width + 1, stream);
            }
        }

        public void InitTree()
        {
            root = new VoxelNode(this, 0, 0, 0, 0, 0, (uint)density.width, (uint)density.heigth, (uint)density.depth);
        }

        public List<VoxelNode> Nodes
        {
            get
            {
                List<VoxelNode> nodes = new List<VoxelNode>();
                foreach (VoxelNode node in this) nodes.Add(node);
                return nodes;
            }
        }

        public IEnumerator<VoxelNode> GetEnumerator()
        {

                Stack<VoxelNode> stack = new Stack<VoxelNode>(VoxelCount);

                if (root == null) yield break;

                VoxelNode current;

                stack.Clear();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    current = stack.Pop();

                    if (current.child != null)
                        for (int i = 7; i >= 0; i--)
                        {
                            if (current.child[i] != null)
                                stack.Push(current.child[i]);
                        }
                    yield return current;
                }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// VoxelVersion
    /// </summary>
    public class VoxelNode
    {
        bool destroied = false;

        VoxelOctree main;
        public VoxelCase voxelcase;
        public VoxelNode[] child;

        int level;
        int node;

        Vector3ui min, max;

        public VoxelNode(VoxelOctree main, int level, int node, uint xmin, uint ymin, uint zmin, uint xmax, uint ymax, uint zmax)
        {
            this.main = main;
            this.level = level;
            this.node = node;

            this.min = new Vector3ui((uint)xmin, (uint)ymin, (uint)zmin);
            this.max = new Vector3ui((uint)xmax, (uint)ymax, (uint)zmax);
            
            main.VoxelCount++;
            
            GenerateChildren(out voxelcase);

            if (child!=null && (voxelcase == VoxelCase.EMPTY || voxelcase == VoxelCase.FULL))
            {
                foreach (VoxelNode voxel in child) voxel.Destroy();
            }
        }

        ~VoxelNode()
        {
            if (!destroied) this.Destroy();
        }

        void GenerateChildren(out VoxelCase density)
        {
            // reach the leaf node, find in density-bits the voxelcase
            if (level == main.Depth-1 )
            {
                density = getVoxelCase();
            }
            else
            {
                Vector3ui cen = (max + min) / 2;
                
                child = new VoxelNode[8];

                child[0] = new VoxelNode(main, level + 1, (node << 2) + 0, min.x, min.y, min.z, cen.x, cen.y, cen.z);
                child[1] = new VoxelNode(main, level + 1, (node << 2) + 1, cen.x, min.y, min.z, max.x, cen.y, cen.z);
                child[2] = new VoxelNode(main, level + 1, (node << 2) + 2, min.x, cen.y, min.z, cen.x, max.y, cen.z);
                child[3] = new VoxelNode(main, level + 1, (node << 2) + 3, cen.x, cen.y, min.z, max.x, max.y, cen.z);
                child[4] = new VoxelNode(main, level + 1, (node << 2) + 4, min.x, min.y, cen.z, cen.x, cen.y, max.z);
                child[5] = new VoxelNode(main, level + 1, (node << 2) + 5, cen.x, min.y, cen.z, max.x, cen.y, max.z);
                child[6] = new VoxelNode(main, level + 1, (node << 2) + 6, min.x, cen.y, cen.z, cen.x, max.y, max.z);
                child[7] = new VoxelNode(main, level + 1, (node << 2) + 7, cen.x, cen.y, cen.z, max.x, max.y, max.z);

                density = VoxelCase.FULL;

                for (int i = 0; i < 8; i++)
                {
                    density &= child[i].voxelcase;
                }
            }
        }

        VoxelCase getVoxelCase()
        {
            VoxelCase density = VoxelCase.EMPTY;
            Vector3ui[] coords = VoxelConst3d.CornerCoord;

            if (main.density[min + coords[0]]) density |= VoxelCase.P0;
            if (main.density[min + coords[1]]) density |= VoxelCase.P1;
            if (main.density[min + coords[2]]) density |= VoxelCase.P2;
            if (main.density[min + coords[3]]) density |= VoxelCase.P3;
            if (main.density[min + coords[4]]) density |= VoxelCase.P4;
            if (main.density[min + coords[5]]) density |= VoxelCase.P5;
            if (main.density[min + coords[6]]) density |= VoxelCase.P6;
            if (main.density[min + coords[7]]) density |= VoxelCase.P7;

            return density;
        }

        public void Destroy()
        {
            if (!destroied)
            {
                if (child != null)
                {
                    foreach (VoxelNode voxel in child)
                    {
                        if (voxel != null) voxel.Destroy();
                    }
                }
                child = null;
                main.VoxelCount--;
            }
            destroied = true;
        }
    }

}
