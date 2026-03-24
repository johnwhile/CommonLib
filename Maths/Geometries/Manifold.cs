using System;
using System.Collections.Generic;

namespace Common.Maths
{
    /// <summary>
    /// </summary>
    public class Manifold
    {
        protected StructBuffer<Triangle> triangles;
        protected StructBuffer<Edge> edges;
        protected StructBuffer<Vertex> vertices;

        protected Dictionary<int, int> edges_hash;

        public Manifold(int vertCapacity, int triCapacity)
        {

            triangles = new StructBuffer<Triangle>(triCapacity);
            edges = new StructBuffer<Edge>(0);
            edges_hash = new Dictionary<int, int>(0);
        }




        public int TrianglesCount => triangles.Count;
        public int EdgesCount => edges.Count;


        public int AddVertex(int index)
        {
            int count = vertices.Count;

            vertices.Add(new Vertex(index));

            return count;
        }



        public int AddTriangle(int v0, int v1, int v2)
        {
            int t = triangles.Count;

            if (v0 < vertices.Count || v1 < vertices.Count || v2 < vertices.Count) throw new ArgumentOutOfRangeException("vertex not found");


            int e0 = createdge(v0, v1);
            int e1 = createdge(v1, v2);
            int e2 = createdge(v2, v0);

            if (assigntriangletoedge(t, e0) &&
                assigntriangletoedge(t, e1) &&
                assigntriangletoedge(t, e2))
            {
                triangles.Add(new Triangle(v0, v1, v2, e0, e1, e2));
                vertices.GetByRef(v0).refTriCount++;
                vertices.GetByRef(v1).refTriCount++;
                vertices.GetByRef(v2).refTriCount++;

            }
            else
                throw new Exception("Cannot create triangle, ad edge can't contain more than 2 adjacent triangles");

            return t;
        }

        public void GetVerticesByTriangle(int t, out int v0, out int v1, out int v2)
        {
            v0 = triangles[t].v0;
            v1 = triangles[t].v1;
            v2 = triangles[t].v2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="remap">the removed triangle is replace with the last one in the list so also the index change</param>
        public void RemoveTriangle(int t, bool remaptriangles = true)
        {
            if (t < 0) return;

            edge_removeTriangleIndex(triangles[t].e0, t);
            edge_removeTriangleIndex(triangles[t].e1, t);
            edge_removeTriangleIndex(triangles[t].e2, t);

            if (remaptriangles)
            {
                //change with last
                int last_t = triangles.Count - 1;
                triangles[t] = triangles[last_t];
                triangles[last_t] = Triangle.Empty;
                triangles.Count--;

                edge_changeTriangleIndex(triangles[t].e0, last_t, t);
                edge_changeTriangleIndex(triangles[t].e1, last_t, t);
                edge_changeTriangleIndex(triangles[t].e2, last_t, t);
            }

            for (int e = 0; e < edges.Count; e++)
            {
                if (!edges[e].containTriangles)
                {
                    RemoveEdge(e, true);
                    e--;
                }
            }

        }

        public void RemoveEdge(int e, bool remapedges = true)
        {
            var old_edge = edges[e];

            if (remapedges)
            {
                int last_e = edges.Count - 1;

                if (!edges_hash.Remove(old_edge.GetHashCode())) throw new Exception();
                edges[e] = edges[last_e];
                edges[last_e] = Edge.Empty;
                edges_hash[edges[e].GetHashCode()] = e;
                edges.Count--;

                triangle_changeEdgeIndex(edges[e].t0, last_e, e);
                triangle_changeEdgeIndex(edges[e].t1, last_e, e);
            }

            RemoveTriangle(old_edge.t0);
            RemoveTriangle(old_edge.t1);
        }
        
        
        public void RemoveVertex(int v)
        {
            for (int e = 0; e < edges.Count; e++)
            {
                if (!edges[e].containTriangles)
                {
                    RemoveEdge(e, true);
                    e--;
                }
            }

        }
        
        
        /// <summary>
        /// return the index of new edge, if already exist return its position
        /// </summary>
        int createdge(int v0, int v1)
        {
            Edge e = new Edge(v0, v1);
            int hash = e.GetHashCode();
            if (edges_hash.TryGetValue(hash, out int index))
            {
                return index;
            }
            else
            {
                edges_hash.Add(hash, edges.Count);
                edges.Add(e);
                return edges.Count - 1;
            }
        }
        /// <summary>
        /// link the triangle t to edge e, it edge already contain two triangle return false
        /// </summary>
        bool assigntriangletoedge(int t, int e)
        {
            var edge = edges[e];
            if (edge.t0 < 0)
                edge.t0 = t;
            else if (edge.t1 < 0)
                edge.t1 = t;
            else
                return false;
            edges[e] = edge;
            return true;
        }
        void edge_removeTriangleIndex(int e, int t) => edge_changeTriangleIndex(e, t, -1);
        void edge_changeTriangleIndex(int e, int t, int new_t)
        {
            if (e < 0) return;
            var edge = edges[e];
            if (edge.t0 == t) edge.t0 = new_t;
            if (edge.t1 == t) edge.t1 = new_t;
            edges[e] = edge;
        }
        void triangle_changeEdgeIndex(int t, int e, int new_e)
        {
            if (t < 0) return;

            // can't exist same edge index
            var tri = triangles[t];

            if (tri.e0 == e) tri.e0 = new_e;
            else if (tri.e1 == e) tri.e1 = new_e;
            else if (tri.e2 == e) tri.e2 = new_e;

            triangles[t] = tri;
        }


        public void CheckHashMap()
        {
            //if (edges_hash.Count != edges.Count) throw new Exception();
            foreach (var edge in edges) 
            {
                if (!edges_hash.TryGetValue(edge.GetHashCode(), out int index)) throw new Exception();
                if (edges[index]!= edge) throw new Exception();
                
            }


        }

        public struct Vertex
        {
            /// <summary>
            /// index in the vertices array
            /// </summary>
            public int i;
            /// <summary>
            /// </summary>
            public ushort refTriCount;

            public Vertex(int index)
            {
                i = index;
                refTriCount = 0;
            }
            public static Vertex Empty => new Vertex(-1);
            
            public bool IsEmpty => i < 0;
            public bool IsIsolated => refTriCount < 1;

        }


        public struct Edge
        {
            public int v0;
            public int v1;
            public int t0;
            public int t1;
            public bool Selected;

            public Edge(int v0, int v1)
            {
                this.v0 = v0;
                this.v1 = v1;
                t0 = -1;
                t1 = -1;
                Selected = false;
            }

            public Edge(int v0, int v1, int t0, int t1) : this(v0, v1)
            {
                this.t0 = t0;
                this.t1 = t1;
            }
            public bool containTriangles => t0 > -1 || t1 > -1;

            public static Edge Empty => new Edge(-1, -1, -1, -1);

            public int HashCode => GetHashCode();

            public static bool operator ==(Edge left, Edge right)
            {
                if (left.v0 != right.v0) return false;
                if (left.v1 != right.v1) return false;
                if (left.t0 != right.t0) return false;
                if (left.t1 != right.t1) return false;
                return true;
            }
            public static bool operator !=(Edge left, Edge right) => !(left == right);

            public override bool Equals(object obj) => obj is Edge edge && edge == this;

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    if (v1 > v0)
                    {
                        hash = hash * 23 + v0;
                        hash = hash * 23 + v1;
                    }
                    else
                    {
                        hash = hash * 23 + v1;
                        hash = hash * 23 + v0;
                    }
                    return hash;
                }
            }

            public override string ToString() => $"v[{v0}, {v1}], t[{t0}, {t1}] ";

        }
        public struct Triangle
        {
            public int v0, v1, v2;
            public int e0, e1, e2;
            public bool Selected;

            public static Triangle Empty => new Triangle(-1, -1, -1, -1, -1, -1);

            public Triangle(int v0, int v1, int v2, int e0, int e1, int e2)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                this.e0 = e0;
                this.e1 = e1;
                this.e2 = e2;
                Selected = false;
            }


            public int HashCode => GetHashCode();

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + v0;
                    hash = hash * 23 + v1;
                    hash = hash * 23 + v2;
                    return hash;
                }
            }

            public override string ToString() => $"v[{v0}, {v1}, {v2}], e[{e0}, {e1}, {e2}] ";

        }

    }
}
