using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using Common.Partitions;

namespace Common.Tools
{
    /// <summary>
    /// Generate a octree using triangles intersection.
    /// Is a brute force algorithm
    /// </summary>
    public class Voxelizer
    {
        public int ComputationsCounter { get; private set; }
        BoundingBoxMinMax space;
        List<Vector3f> vertices;
        List<Vector3us> faces;
        int levels;
        public Octree<OctBoxNode> octree;
        int curr;

        Vector3f v0, v1, v2;
        Matrix4x4f transform_inv = Matrix4x4f.Identity;

        public Voxelizer(BoundingBoxMinMax space , Vector3f p0, Vector3f p1, Vector3f p2, int depth)
        {
            this.vertices = new List<Vector3f>();
            this.faces = new List<Vector3us>();
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            faces.Add(new Vector3us(0, 1, 2));
            this.space = space;
            this.levels = depth;
            octree = new Octree<OctBoxNode>(depth);

            
        }

        public Voxelizer(BoundingBoxMinMax space , IList<Vector3f> vertices, IList<Vector3us> faces , int depth , Matrix4x4f transform)
        {
            this.space = space;
            this.vertices = new List<Vector3f>(vertices);
            this.faces = new List<Vector3us>(faces);
            this.levels = depth;

            transform_inv =transform;

            octree = new Octree<OctBoxNode>(depth);
            octree.root = new OctBoxNode(octree, space);


            for (int i = 0; i < faces.Count; i++)
            {
                curr = i;
                Vector3us face = faces[i];
                v0 = vertices[face.x].TransformCoordinate(in transform_inv);
                v1 = vertices[face.y].TransformCoordinate(in transform_inv);
                v2 = vertices[face.z].TransformCoordinate(in transform_inv);

                if (!RecursiveAddTriangle(octree.root))
                {
                    //Console.WriteLine("Octree out mesh");
                }
            }

        }

        bool RecursiveAddTriangle(OctBoxNode node)
        {
            Vector3f max = node.Max;
            Vector3f min = node.Min;

            if (PrimitiveIntersections.IntersectAABBTriangle(min, max, v0, v1, v2))
            {
                ComputationsCounter++;

                //Console.WriteLine("node " + node.flags.NodeID + " intersect triangle " + curr);
                if (!node.IsLeaf)
                {
                    float cx = node.Center.x;
                    float cy = node.Center.y;
                    float cz = node.Center.z;
                    float hx = node.HalfSize.x * 0.5f;
                    float hy = node.HalfSize.y * 0.5f;
                    float hz = node.HalfSize.z * 0.5f;

                    if (node.child == null)
                    {
                        node.Split();
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            OctBoxNode childnode = node.child[i];

                            if (childnode == null)
                            {
                                switch (i)
                                {
                                    case 0: childnode = new OctBoxNode(node, 0, cx - hx, cy - hy, cz - hz, hx, hy, hz); break;
                                    case 1: childnode = new OctBoxNode(node, 1, cx + hx, cy - hy, cz - hz, hx, hy, hz); break;
                                    case 2: childnode = new OctBoxNode(node, 2, cx - hx, cy + hy, cz - hz, hx, hy, hz); break;
                                    case 3: childnode = new OctBoxNode(node, 3, cx + hx, cy + hy, cz - hz, hx, hy, hz); break;
                                    case 4: childnode = new OctBoxNode(node, 4, cx - hx, cy - hy, cz + hz, hx, hy, hz); break;
                                    case 5: childnode = new OctBoxNode(node, 5, cx + hx, cy - hy, cz + hz, hx, hy, hz); break;
                                    case 6: childnode = new OctBoxNode(node, 6, cx - hx, cy + hy, cz + hz, hx, hy, hz); break;
                                    case 7: childnode = new OctBoxNode(node, 7, cx + hx, cy + hy, cz + hz, hx, hy, hz); break;
                                }
                            }

                            if (RecursiveAddTriangle(childnode))
                            {
                                node.child[i] = childnode;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }


    }
}
