using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using Common.Geometry;

#if DELETE
namespace Common.Tools
{
    public class Convexer3D
    {
        CircularLinkedList<VertNode> vertices = null;
        CircularLinkedList<EdgeNode> edges = null;
        CircularLinkedList<FaceNode> triangles = null;

        /// <summary>
        /// Requre
        /// </summary>
        /// <param name="points"></param>
        public Convexer3D(IList<Vector3f> points)
        {
            throw new NotImplementedException();
            /*
            vertices = new CircularLinkedList<VertNode>();
            edges = new CircularLinkedList<EdgeNode>();
            triangles = new CircularLinkedList<FaceNode>();

            for (int i = 0; i < points.Count; i++)
                vertices.Add(new VertNode(points[i], i));

            if (Initialize(points) != Result.OK)
            {
                vertices.Clear();
                edges.Clear();
                triangles.Clear();
            }
            else
            {
                // enumerator start from Head node
                foreach (VertNode vert in vertices)
                {
                    if (!vert.processed)
                    {

                    }
                    vert.processed = true;
                    return;
                }
            }
            */
        }

        /// <summary>
        /// Builds the initial double triangle
        /// </summary>
        Result Initialize(IList<Vector3f> points)
        {
            VertNode v0, v1, v2, v3;

            // find 3 non collinear points
            v0 = vertices.Head;

            while (areCollinear(v0, v0.Next, v0.Next.Next))
            {
                v0 = v0.Next;
                if (v0 == vertices.Head)
                {
                    return Result.AllColinear;
                }
            }
            v1 = v0.Next;
            v2 = v1.Next;

            // mark the vertices as processed
            v0.processed = true;
            v1.processed = true;
            v2.processed = true;
            v0.onhull = true;
            v1.onhull = true;
            v2.onhull = true;

            // create the two "twins" faces 
            FaceNode f0, f1;
            makeDoubleTriangle(v0, v1, v2, out f0, out f1);
            triangles.Add(f0);
            triangles.Add(f1);

            // find a fourth, non coplanar point to form tetrahedron
            v3 = v2.Next;

            float vol;
            while (volumeSign(f0, v3, out vol) == 0)
            {
                v3 = v3.Next;
                if (v3 == v0)
                {
                    return Result.AllCoplanar;
                }
            }
            // insure that v3 will be the first added this because algorithm
            // will build the thethaeron at first add_one()
            vertices.Head = v3;

            return Result.OK;
        }


        /// <summary>
        /// add_one is passed a vertex. It first determines all faces visible  
        /// from that point. If none are visible then the point is marked as  
        /// not on hull. Next is a loop over edges. If both faces adjacent to  
        /// an edge are visible, then the edge is marked for deletion. If just  
        /// one of the adjacent faces is visible then a new face is constructed.  
        /// </summary>
        public bool AddPoint(VertNode v)
        {
            bool visible = false;

            // marks faces visible from v
            foreach (FaceNode f in triangles)
            {
                float vol;
                if (volumeSign(f, v,out vol) < 0)
                {
                    f.visible = true;
                    visible = true;
                }
            }
            // if no faces are visible from v, then v is inside the hull  
            if (!visible)
            {
                v.onhull = false;
                return false;
            }

            // mark edges in interior of visible region for deletion.  
            // erect a new face based on each border edge  
            
            // can't use enumerator because new edges are created
            EdgeNode e = edges.Head.Prev;
            do
            {
                e = e.Prev;

            }
            while (e != edges.Head);

            do
            {
                EdgeNode temp = e.Next;

                if (e.f[0].visible && e.f[1].visible)
                {
                    // e interior: mark for deletion
                    e.todelete = true;
                }
                else if (e.f[0].visible || e.f[1].visible)
                {
                    //e border: make a new face
                    e.newface = makeConeTriangle(e, v);
                    triangles.Add(e.newface);
                }
                e = temp;
            }
            while (e != edges.Head);
            return true;
        }

        /// <summary>
        /// Create two "twins" faces that use same three vertices
        /// </summary>
        void makeDoubleTriangle(VertNode a, VertNode b, VertNode c, out FaceNode f0, out FaceNode f1)
        {
            f0 = new FaceNode(a, b, c);
            f1 = new FaceNode(a, c, b);

            EdgeNode e01 = new EdgeNode(a, b);
            EdgeNode e12 = new EdgeNode(b, c);
            EdgeNode e20 = new EdgeNode(c, a);

            f0.e[0] = f1.e[2] = e01;
            f0.e[1] = f1.e[1] = e12;
            f0.e[2] = f1.e[0] = e20;
            
            // link adjacent face fields
            e01.f[0] = e12.f[0] = e20.f[0] = f0;
            e01.f[1] = e12.f[1] = e20.f[1] = f1;

            edges.Add(e01);
            edges.Add(e12);
            edges.Add(e20);
        }
        
        /// <summary>
        /// Build a Face using an existing edge and an opposite vertex. The two new edge
        /// must respect the edge-chain-order and vertices the CCW order
        /// </summary>
        FaceNode makeConeTriangle(EdgeNode e, VertNode v)
        {
            return null;
        }

        /// <summary>
        /// Update face to orient in the CounterClockWire mode
        /// </summary>
        void makeCCW(FaceNode f, EdgeNode e, VertNode v)
        {

        }
        /// <summary>
        /// are_collinear checks to see if the three points given are  
        /// collinear by checking to see if each element of the cross 
        /// product is zero.  
        /// </summary>
        bool areCollinear(VertNode A, VertNode B, VertNode C)
        {
            // triangle area is |(A-B)x(C-B)| * 1/2
            // in collinear case the area is zero if lenght of cross product is zero, so
            Vector3f cross = Vector3f.Cross(A.position - B.position, C.position - B.position);
            float eps = 1e-6f;

            return Maths.Maths.ABS(cross.x) < eps &&
                   Maths.Maths.ABS(cross.y) < eps &&
                   Maths.Maths.ABS(cross.z) < eps;
        }
        /// <summary>
        /// volume_sign returns the sign of the volume of the tetrahedron determined by f and p.
        /// Volume_sign is +1 if p is on the negative side of f, where the positive side is determined by the rh-rule.
        /// So the volume is positive if the ccw normal to f points outside the tetrahedron.  
        /// The final fewer-multiplications form is due to Bob Williamson. 
        /// </summary>
        sbyte volumeSign(FaceNode f, VertNode v, out float vol)
        {
            Vector3f v0 = f.v[0].position;
            Vector3f v1 = f.v[1].position;
            Vector3f v2 = f.v[2].position;
            Vector3f vv = v.position;

            float ax = v0.x - vv.x;
            float ay = v0.y - vv.y;
            float az = v0.z - vv.z;

            float bx = v1.x - vv.x;
            float by = v1.y - vv.y;
            float bz = v1.z - vv.z;

            float cx = v2.x - vv.x;
            float cy = v2.y - vv.y;
            float cz = v2.z - vv.z;

            vol = ax * (by * cz - bz * cy) +
                  ay * (bz * cx - bx * cz) +
                  az * (bx * cy - by * cx);

            // this epsilon is very important to consider coplanar the volume too small and 
            // will would generate a non-convexity error
            if (vol > 0) return 1;
            else if (vol < 0) return -1;
            else return 0;
        }


        public MeshListGeometry GetConvexMesh()
        {
            int idx = 0;

            foreach (VertNode node in vertices)
            {
                // if vertex is processed and is on hull mean that is used by convex mesh
                if (node.onhull && node.processed) node.ID = idx++;
            }

            MeshListGeometry mesh = new MeshListGeometry();
            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, idx);
            idx = 0;
            foreach (VertNode node in vertices)
            {
                if (!node.onhull && node.processed) mesh.vertices[idx++] = node.position;
            }

            idx = 0;
            mesh.indices = new IndexAttribute<Face16>(triangles.Count);
            foreach (FaceNode node in triangles)
            {
                mesh.indices[idx++] = new Face16(node.v[0].ID, node.v[1].ID, node.v[2].ID);
            }
            return mesh;
        }

        public enum Result
        {
            ERR,
            OK,
            Not4Points,
            AllCoincident,
            AllColinear,
            AllCoplanar,
        }

        
#region HalfEdge structure specified for this algorithm

        public class VertNode : HVertex , ILink<VertNode>
        {      
            public VertNode Next { get; set; }
            public VertNode Prev { get; set; }
            public bool onhull = false;
            public bool processed = false;
            
            public bool marked2remove
            {
                get { return processed && !onhull; } // processed and INSIDE hull
            }

            public EdgeNode duplicate = null;

            public VertNode()
                : base()
            {
            }

            public VertNode(Vector3f vector,int idx ):base()
            {
                base.position = vector;
                base.ID = idx;
            }
        }
        public class EdgeNode : HEdge<VertNode, EdgeNode,FaceNode>, ILink<EdgeNode>
        {
            public EdgeNode Next { get; set; }
            public EdgeNode Prev { get; set; }

            public bool todelete = false;

            public bool marked2remove
            {
                get { return todelete; }
            }

            public FaceNode newface = null;

            public EdgeNode():base()
            {

            }
            public EdgeNode(VertNode a, VertNode b):base()
            {
                v[0] = a;
                v[1] = b;
            }

        } 
        public class FaceNode : HFace<VertNode,EdgeNode,FaceNode> , ILink<FaceNode>
        {
            public FaceNode Next { get; set; }
            public FaceNode Prev { get; set; }

            public bool visible = false;

            public bool marked2remove
            {
                get { return visible; } // if a face was visible to a point mean that need to remove it
            }


            public FaceNode(): base()
            {
            }

            /// <summary>
            /// a,b,c must be in CCW order
            /// </summary>
            public FaceNode(VertNode a, VertNode b, VertNode c)
                : base()
            {
                v[0] = a;
                v[1] = b;
                v[2] = c;
            }
        }
#endregion
    }
}
#endif