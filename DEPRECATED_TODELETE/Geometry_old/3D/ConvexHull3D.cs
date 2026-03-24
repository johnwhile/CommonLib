/*
This code is described in "Computational Geometry in C" (Second Edition),
Chapter 4.  It is not written to be comprehensible without the 
explanation in that book.

Written by Joseph O'Rourke, with contributions by 
Kristy Anderson, John Kutcher, Catherine Schevon, Susan Weller.
Last modified: May 2000
Questions to orourke@cs.smith.edu.

This code is Copyright 2000 by Joseph O'Rourke.  It may be freely 
redistributed in its entirety provided that this copyright notice is 
not removed.
*/
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using Common.Tools;

#if DELETE
namespace Common.Geometry
{
    /// <summary>
    /// Implement the incremental algorithm
    /// </summary>
    public class ConvexHull3D
    {
        public enum Result
        {
            ERR,
            OK,
            Not4Points,
            AllCoincident,
            AllColinear,
            AllCoplanar,
        }

        static void Swap<T>(ref T a, ref T b) where T : class
        {
            T t = a;
            a = b;
            b = t;
        }

        /// <summary>
        /// Point pointer
        /// </summary>
        class VertNode : ILink<VertNode>
        {
            public VertNode Next { get; set; }
            public VertNode Prev { get; set; }
            public int idx { get; set; }

            public Vector3f pt;
            public int origIdx = 0;

            public bool onhull = false;
            public bool processed = false;
            public EdgeNode duplicate = null;


            public static bool removeFunction(VertNode node)
            {
                return node.processed && !node.onhull;
            }

            /// <summary>
            /// Create a new Vertex node and add to circular list, not change next prev value;
            /// </summary>
            public VertNode(ConvexHull3D main, Vector3f pt)
            {
                this.pt = pt;

                /*
                float eps = 0.0001f;
                if (pt.x < eps || pt.x > -eps) pt.x = 0.0f;
                if (pt.y < eps || pt.y > -eps) pt.y = 0.0f;
                if (pt.z < eps || pt.z > -eps) pt.z = 0.0f;
                */

                origIdx = idx = main.vertsList.Count;
                main.vertsList.Add(this);
            }
            public override string ToString()
            {
                return "v" + origIdx + ": " + pt.x + " " + pt.y + " " + pt.z;
            }

            public bool marked2remove
            {
                get { return processed && !onhull; }
            }
        }
        /// <summary>
        /// Face pointer
        /// </summary>
        class FaceNode : ILink<FaceNode>
        {
            public FaceNode Next { get; set; }
            public FaceNode Prev { get; set; }
            public int idx { get; set; }
            public bool marked2remove
            {
                get { return visible; }
            }
            public EdgeNode[] edges = new EdgeNode[3];
            public VertNode[] vertices = new VertNode[3];
            public bool visible = false;
            public Vector3f normal = new Vector3f(0, 0, 0);
            public Vector3f center = new Vector3f(0, 0, 0);
            /// <summary>
            /// Make a face using three vertices and an adjacent face, add to circular list
            /// </summary>
            public FaceNode(ConvexHull3D main, VertNode v1, VertNode v2, VertNode v3, FaceNode f)
            {
                // create edges of the initial triangle
                if (f == null)
                {
                    edges[0] = new EdgeNode(main);
                    edges[1] = new EdgeNode(main);
                    edges[2] = new EdgeNode(main);
                }
                else
                {
                    edges[0] = f.edges[2];
                    edges[1] = f.edges[1];
                    edges[2] = f.edges[0];
                }
                edges[0].endpoint[0] = v1; edges[0].endpoint[1] = v2;
                edges[1].endpoint[0] = v2; edges[1].endpoint[1] = v3;
                edges[2].endpoint[0] = v3; edges[2].endpoint[1] = v1;

                // create face for triangle
                vertices[0] = v1;
                vertices[1] = v2;
                vertices[2] = v3;
                visible = false;

                // links edges to face
                edges[0].adjface[0] = edges[1].adjface[0] = edges[2].adjface[0] = this;

                idx = main.facesList.Count;
                Update();
                main.facesList.Add(this);
            }

            /// <summary>
            /// Make cone face using a edge and a vertex, add to circular list
            /// </summary>
            public FaceNode(ConvexHull3D main, EdgeNode e, VertNode v)
            {
                EdgeNode[] new_edges = new EdgeNode[2];

                // make two new edges (if don't already exist)   
                for (int i = 0; i < 2; ++i)
                {
                    // if the edge exists, copy it into new_edges
                    new_edges[i] = e.endpoint[i].duplicate;

                    if (new_edges[i] == null)
                    {
                        // otherwise (duplicate is NULL)   
                        new_edges[i] = new EdgeNode(main);
                        new_edges[i].endpoint[0] = e.endpoint[i];
                        new_edges[i].endpoint[1] = v;
                        e.endpoint[i].duplicate = new_edges[i];
                    }
                }
                // make the new face   
                edges[0] = e;
                edges[1] = new_edges[0];
                edges[2] = new_edges[1];
                visible = false;

                MakeCCW(e, v);

                // set the adjacent face pointers  
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        // only the NULL link should be set to this face  
                        if (new_edges[i].adjface[j] == null)
                        {
                            new_edges[i].adjface[j] = this;
                            break;
                        }
                    }
                }
                Update();
                idx = main.facesList.Count;
                main.facesList.Add(this);
            }

            /// <summary>
            /// Update face to orient in the CounterClockWire mode
            /// </summary>
            public void MakeCCW(EdgeNode e, VertNode v)
            {
                //index of e->end_points[0] in fv 
                int i;

                //the visible face adjacent to e
                FaceNode fv = e.adjface[0].visible ? e.adjface[0] : e.adjface[1];

                // set vertices[0] & vertices[1] to have the same orientation as do  
                // the corresponding  vertices of fv 
                for (i = 0; fv.vertices[i] != e.endpoint[0]; i++) ;

                // orient this the same as fv   
                if (fv.vertices[(i + 1) % 3] != e.endpoint[1])
                {
                    this.vertices[0] = e.endpoint[1];
                    this.vertices[1] = e.endpoint[0];
                }
                else
                {
                    this.vertices[0] = e.endpoint[0];
                    this.vertices[1] = e.endpoint[1];

                    Swap<EdgeNode>(ref this.edges[1], ref this.edges[2]);
                }

                // this swap is tricky. e is edges[0]. edges[1] is based on end_points[0],  
                //   edges[2] on end_points[1]. So if e is oriented "forwards", we need to  
                //   move  edges[1] to follow [0], because it precedes
                this.vertices[2] = v;
            }

            /// <summary>
            /// Need to call vertList.UpdateIndex() before get face
            /// </summary>
            public Vector3us face { get { return new Vector3us(vertices[0].idx, vertices[1].idx, vertices[2].idx); } }

            public override string ToString()
            {
                return "f" + idx + ", " + marked2remove + ", " + vertices[0].origIdx + " " + vertices[1].origIdx + " " + vertices[2].origIdx;
            }

            void Update()
            {
                normal = Vector3f.Cross(vertices[1].pt - vertices[0].pt, vertices[2].pt - vertices[0].pt);
                float mag = normal.LengthSq;

                if (mag < float.Epsilon * 100)
                    throw new Exception("degenerate face");

                mag = (float)(1.0 / Math.Sqrt(mag));
                normal *= mag;


                center = vertices[0].pt + vertices[1].pt + vertices[2].pt;
                center *= 1f / 3f;

                //normal.Normalize();
            }
        }
        /// <summary>
        /// Edge pointer
        /// </summary>
        class EdgeNode : ILink<EdgeNode>
        {
            public EdgeNode Next { get; set; }
            public EdgeNode Prev { get; set; }
            public int idx { get; set; }
            
            public bool marked2remove
            {
                get { return todelete; }
            }

            public FaceNode[] adjface = new FaceNode[2];
            public VertNode[] endpoint = new VertNode[2];
            public FaceNode newface;
            public bool todelete = false;

            /// <summary>
            /// Initialize a void Edge and add to circular list
            /// </summary>
            public EdgeNode(ConvexHull3D main)
            {
                idx = main.edgesList.Count;
                main.edgesList.Add(this);
            }

            /// <summary>
            /// Need to call vertList.UpdateIndex() before get face
            /// </summary>
            public Vector2us edge { get { return new Vector2us(endpoint[0].idx, endpoint[1].idx); } }

            public override string ToString()
            {
                return "e" + idx + " , " + endpoint[0].origIdx + " " + endpoint[1].origIdx;
            }
        }

        /// <summary>
        /// Points pointer manager
        /// </summary>
        CircularLinkedList<VertNode> vertsList;
        /// <summary>
        /// Faces pointer manager
        /// </summary>
        CircularLinkedList<FaceNode> facesList;
        /// <summary>
        /// Edges pointer manager
        /// </summary>
        CircularLinkedList<EdgeNode> edgesList;
        /// <summary>
        /// the current node added with incremental algorithm.
        /// </summary>
        VertNode current;
        /// <summary>
        /// Debug error message
        /// </summary>
        public Result result { get; private set; }
        bool debug = false;
        bool finish = false;
        int num_points_input = 0;
        /// <summary>
        /// Initialize the Convex hull 3d class with a list of points. Points Count must be at last 4, if all 4 points are coplanar
        /// the "result" properties contain the error message
        /// </summary>
        /// <param name="points">list or array of points</param>
        /// <param name="debug">if true algorithm will made some internal check of data structure consistency, used to find some bugs</param>
        public ConvexHull3D(IList<Vector3f> points, bool debug)
        {
            this.debug = debug;

            vertsList = new CircularLinkedList<VertNode>();
            facesList = new CircularLinkedList<FaceNode>();
            edgesList = new CircularLinkedList<EdgeNode>();

            foreach (var item in points)
            {
                VertNode vertex = new VertNode(this, item);
            }

            num_points_input = vertsList.Count;

            if (vertsList.Count > 3)
            {
                result = double_triangle();

                if (result != Result.OK)
                {
                    Console.WriteLine(result);
                    vertsList.Clear();
                    facesList.Clear();
                    edgesList.Clear();
                }
                else
                {
                    current = vertsList.Head;
                }
            }
            else result = Result.Not4Points;
        }

        ~ConvexHull3D()
        {
            vertsList.Clear();
            facesList.Clear();
            edgesList.Clear();
            vertsList = null;
            facesList = null;
            edgesList = null;
        }
        /// <summary>
        /// Return exential geometry of hull in the default format
        /// </summary>
        /// <param name="vertices">vector3 structure</param>
        /// <param name="faces">face structure</param>
        public bool Extract(out Vector3f[] vertices, out Vector3us[] faces)
        {
            int i = 0;
            if (vertsList.Count < 3 || facesList.Count < 2)
            {
                vertices = null;
                faces = null;
                return false;
            }
            vertices = new Vector3f[vertsList.Count];
            faces = new Vector3us[facesList.Count];

            // update internal indices
            i = 0;
            VertNode vnode = vertsList.Head;
            do vnode.idx = i++;
            while ((vnode = vnode.Next) != vertsList.Head);
            
            //set_edge_order_on_faces();

            i = 0;
            foreach (VertNode v in vertsList) vertices[i++] = v.pt;
            i = 0;
            foreach (FaceNode f in facesList) faces[i++] = f.face;

            return true;
        }

        /// <summary>
        /// Return the geometry of hull in the default format
        /// </summary>
        /// <param name="vertices">vector3 structure</param>
        /// <param name="faces">face structure</param>
        /// <param name="hulls">points original index used to build convex hull</param>
        /// <param name="normals">faces normals</param>
        public bool Extract(out Vector3f[] vertices, out Vector3us[] faces, out Vector3f[] normals, out List<int> hulls)
        {
            if (vertsList.Count < 3 || facesList.Count < 2)
            {
                vertices = null;
                faces = null;
                normals = null;
                hulls = new List<int>();
                return false;
            }
            vertices = new Vector3f[vertsList.Count];
            faces = new Vector3us[facesList.Count];
            normals = new Vector3f[facesList.Count];
            hulls = new List<int>(vertsList.Count);

            // update internal indices
            int i = 0;
            VertNode vnode = vertsList.Head;
            do vnode.idx = i++;
            while ((vnode = vnode.Next) != vertsList.Head);


            set_edge_order_on_faces();

            BitArray vflags = new BitArray(num_points_input, false);

            i = 0;
            foreach (VertNode node in vertsList) vertices[i++] = node.pt;
            i = 0;
            foreach (FaceNode node in facesList)
            {
                faces[i] = node.face;
                normals[i] = node.normal;

                for (int j = 0; j < 3; j++)
                {
                    int origIdx = node.vertices[j].origIdx;
                    if (!vflags[origIdx])
                    {
                        hulls.Add(origIdx);
                        vflags[origIdx] = true;
                    }
                }
                i++;
            }
            hulls.Sort();
            return true;
        }

        /// <summary>
        /// Return true if there are others steps
        /// </summary>
        /// <example>
        /// <code>
        /// CHull3D algorithm = new CHull3D(points)
        /// check algorithm.result;
        /// try 
        /// {
        ///     while(MoveNext());
        ///     TriMesh mesh = new TriMesh{};
        ///     Extract(out mesh.vertices,out mesh.faces);
        /// }
        /// catch{}
        /// </code>
        /// </example>
        public bool MoveNext()
        {
            if (result != Result.OK || finish)
            {
                Console.WriteLine("stop");
                return false;
            }

            //Console.WriteLine("Process v " + current.origIdx);

            //Console.WriteLine("Process v " + current.origIdx);
            // the clean_up() process can delete this current vertex, so memorize the next
            VertNode next = current.Next;

            if (!current.processed)
            {
                current.processed = true;
                add_one(current);
                clean_up(ref next);

                if (debug) Checks();
            }
            current = next;
            finish = current == vertsList.Head;

            return !finish;
        }
        /// <summary>
        /// add_one is passed a vertex. It first determines all faces visible  
        /// from that point. If none are visible then the point is marked as  
        /// not on hull. Next is a loop over edges. If both faces adjacent to  
        /// an edge are visible, then the edge is marked for deletion. If just  
        /// one of the adjacent faces is visible then a new face is constructed.  
        /// </summary>
        bool add_one(VertNode v)
        {
            bool vis = false;

            // marks faces visible from v
            foreach (FaceNode f in facesList)
            {
                double vol;
                if (normal_sign(f, v, out vol) <= 0)
                //if (volume_sign(f, v,out vol) <= 0)
                {
                    //float d0 = Vector3.Length(f.vertices[0].pt - v.pt);
                    //float d1 = Vector3.Length(f.vertices[1].pt - v.pt);
                    //float d2 = Vector3.Length(f.vertices[2].pt - v.pt);
                    f.visible = true;
                    vis = true;
                }
            }

            // if no faces are visible from v, then v is inside the hull  
            if (!vis)
            {
                v.onhull = false;
                return false;
            }

            // mark edges in interior of visible region for deletion.  
            //   erect a new face based on each border edge  
            EdgeNode e = edgesList.Head;
            do
            {
                EdgeNode temp = e.Next;

                if (e.adjface[0].visible && e.adjface[1].visible)
                    /* e interior: mark for deletion */
                    e.todelete = true;

                else if (e.adjface[0].visible || e.adjface[1].visible)
                    /* e border: make a new face */
                    e.newface = new FaceNode(this, e, v);

                e = temp;
            }
            while (e != edgesList.Head);

            return true;
        }
        /// <summary>
        /// goes through each data structure list and clears all flags  
        /// and NULLs out some pointers.
        /// </summary>
        /// <param name="vnext">if vnext must be removed, change it with vnext = vnext.Next</param>
        void clean_up(ref VertNode vnext)
        {
            clean_edges();
            clean_faces();
            clean_vertices(ref vnext);
        }
        /// <summary>
        /// runs through the edge list and cleans up the structure.  
        /// If there is a newface then it will put that face in place  
        /// of the visible face and NULL out newface. It also deletes  
        /// so marked edges.  
        /// </summary>
        void clean_edges()
        {
            if (edgesList.Count == 0) return;

            // integrate the new face's into the data structure 
            // check every edge  
            foreach (EdgeNode edge in edgesList)
            {
                if (edge.newface != null)
                {
                    if (edge.adjface[0].visible) edge.adjface[0] = edge.newface;
                    else edge.adjface[1] = edge.newface;
                    edge.newface = null;
                }
            }
            // delete any edges marked for deletion
            //carefull when removing, need to ensure all nodes will be tested
            edgesList.Clear();

            if (debug) foreach (EdgeNode ee in edgesList) if (ee.todelete)
                        throw new Exception("Remove edge fail");
        }
        /// <summary>
        /// runs through the face list and deletes any face marked visible.  
        /// </summary>
        void clean_faces()
        {
            facesList.Clear();
            if (debug) foreach (FaceNode ff in facesList) if (ff.visible)
                        throw new Exception("Remove faces fail");
        }
        /// <summary>
        /// runs through the vertex list and deletes the vertice  
        /// that are marked as processed but are not incident to  
        /// any undeleted edges.   
        /// The pointer to vnext, is used to alter vnext  
        /// in construct_hull() if we are about to delete vnext.  
        /// </summary>
        void clean_vertices(ref VertNode vnext)
        {
            // mark all vertices incident to some undeleted edge as on the hull   
            foreach (EdgeNode e in edgesList)
                e.endpoint[0].onhull = e.endpoint[1].onhull = true;


            // delete all vertices that have been processed but are not on the hull
            //carefull when removing, need to ensure all nodes will be tested
            vertsList.Clear();

            //se next è rimosso, trovo il più vicino non rimosso
            // TODO : test this function
            int i = vertsList.Count;
            while (vnext.marked2remove && i > 0)
            {

                Console.WriteLine("next vertex " + vnext.origIdx + "are removed , set vnext as " + vnext.Next.origIdx);
                vnext = vnext.Next;
                i--;
            }

            /*
            while (vertsList.Count > 0 && vertsList.Head.processed && !vertsList.Head.onhull)
            {
                //If about to delete vnext, advance it first.
                v = vertsList.Head;
                if (v == vnext)
                    vnext = v.Next;
                vertsList.Remove(v);
            }
            v = vertsList.Head.Next;
            do
            {
                if (v.processed && !v.onhull)
                {
                    VertNode tmp = v;
                    v = v.Next;
                    if (tmp == vnext)
                        vnext = tmp.Next;
                    vertsList.Remove(tmp);
                }
                else
                {
                    v = v.Next;
                }
            }
            while (v != vertsList.Head);
            */
            if (debug)
            {
                foreach (VertNode vv in vertsList) if (vv.processed && !vv.onhull)
                        throw new Exception("Remove verteices fail");

                if (current == null || current.Next == null)
                    throw new Exception("Remove verteices fail");
            }


            // reset flags
            foreach (VertNode item in vertsList)
            {
                item.duplicate = null;
                item.onhull = false;
            }
        }
        /// <summary>
        /// volume_sign returns the sign of the volume of the tetrahedron determined by f and p.
        /// Volume_sign is +1 if p is on the negative side of f, where the positive side is determined by the rh-rule.
        /// So the volume is positive if the ccw normal to f points outside the tetrahedron.  
        /// The final fewer-multiplications form is due to Bob Williamson. 
        /// </summary>
        sbyte volume_sign(FaceNode f, VertNode v, out double vol)
        {
            double ax = f.vertices[0].pt.x - v.pt.x;
            double ay = f.vertices[0].pt.y - v.pt.y;
            double az = f.vertices[0].pt.z - v.pt.z;

            double bx = f.vertices[1].pt.x - v.pt.x;
            double by = f.vertices[1].pt.y - v.pt.y;
            double bz = f.vertices[1].pt.z - v.pt.z;

            double cx = f.vertices[2].pt.x - v.pt.x;
            double cy = f.vertices[2].pt.y - v.pt.y;
            double cz = f.vertices[2].pt.z - v.pt.z;

            vol = ax * (by * cz - bz * cy) +
                  ay * (bz * cx - bx * cz) +
                  az * (bx * cy - by * cx);

            float vol2 = (float)vol;

            // this epsilon is very important to consider coplanar the volume too small and 
            // will would generate a non-convexity error
            if (vol2 > 0) return 1;
            else if (vol2 < 0) return -1;
            else return 0;
        }
        /// <summary>
        /// same of <see cref="volume_sign"/> but in my opinion using dotis more accurate to avoid tollerance errors.
        /// </summary>
        sbyte normal_sign(FaceNode f, VertNode v, out double dot)
        {
            dot = Vector3f.Dot(f.normal, v.pt - f.center);
            if (dot < -1e-7) return 1;
            else if (dot > 1e-7) return -1;
            else return 0;
        }
        /// <summary>
        /// are_collinear checks to see if the three points given are  
        /// collinear by checking to see if each element of the cross 
        /// product is zero.  
        /// </summary>
        bool are_collinear(VertNode A, VertNode B, VertNode C)
        {
            //    |(V2-v1)x(v3-v1)|/|v2-v1|
            // triangle area : [ Ax * (By - Cy) + Bx * (Cy - Ay) + Cx * (Ay - By) ] / 2

            double a = (C.pt.z - A.pt.z) * (B.pt.y - A.pt.y) - (B.pt.z - A.pt.z) * (C.pt.y - A.pt.y);
            double b = (B.pt.z - A.pt.z) * (C.pt.x - A.pt.x) - (B.pt.x - A.pt.x) * (C.pt.z - A.pt.z);
            double c = (B.pt.x - A.pt.x) * (C.pt.y - A.pt.y) - (B.pt.y - A.pt.y) * (C.pt.x - A.pt.x);

            if (a < 0) a *= -1;
            if (b < 0) b *= -1;
            if (c < 0) c *= -1;

            double eps = 1e-9;

            return (a < eps) && (b < eps) && (c < eps);

            //return a == 0 && b == 0 && c == 0;
        }

        /// <summary>
        /// squared distance from a segment defined by AB and a point C
        /// </summary>
        float distSqPointSegment(VertNode A, VertNode B, VertNode C)
        {
            Vector3f ab = B.pt - A.pt;
            Vector3f ac = C.pt - A.pt;
            Vector3f bc = C.pt - B.pt;
            float e = Vector3f.Dot(ac, ab);

            if (e < 0) return Vector3f.Dot(ac, ac);
            float f = Vector3f.Dot(ab, ab);
            if (e >= f) return Vector3f.Dot(bc, bc);
            return Vector3f.Dot(ac, ac) - e * e / f;
        }

        /// <summary>
        /// builds the initial double triangle 
        /// </summary>
        Result double_triangle()
        {
            VertNode v0, v1, v2, v3;

            // find 3 non collinear points
            v0 = vertsList.Head;

            while (are_collinear(v0, v0.Next, v0.Next.Next))
            {
                v0 = v0.Next;
                if (v0 == vertsList.Head)
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

            // create the two "twins" faces 
            FaceNode f0 = new FaceNode(this, v0, v1, v2, null);
            FaceNode f1 = new FaceNode(this, v2, v1, v0, f0);

            // link adjacent face fields   
            f0.edges[0].adjface[1] = f1;
            f0.edges[1].adjface[1] = f1;
            f0.edges[2].adjface[1] = f1;

            f1.edges[0].adjface[1] = f0;
            f1.edges[1].adjface[1] = f0;
            f1.edges[2].adjface[1] = f0;

            // find a fourth, non coplanar point to form tetrahedron
            v3 = v2.Next;

            double vol;
            while (volume_sign(f0, v3, out vol) == 0)
            {
                v3 = v3.Next;
                if (v3 == v0)
                {
                    return Result.AllCoplanar;
                }
            }

            // insure that v3 will be the first added this because algorithm
            // will build the thethaeron at first add_one()
            vertsList.Head = v3;

            return Result.OK;
        }
        /// <summary>
        /// builds the initial tetrahedron finding max size
        /// </summary>
        Result double_triangle_2()
        {
            VertNode[] extreme = new VertNode[] 
            { 
                vertsList.Head, vertsList.Head, //[0]x_min, [1]x_max 
                vertsList.Head, vertsList.Head, //[2]y_min, [3]y_max 
                vertsList.Head, vertsList.Head  //[4]z_min, [5]z_max 
            };


            // find 6 extreme points in x, y and z (min and max)
            foreach (VertNode v in vertsList)
            {
                if (v.pt.x < extreme[0].pt.x) extreme[0] = v;
                if (v.pt.x > extreme[1].pt.x) extreme[1] = v;
                if (v.pt.y < extreme[2].pt.y) extreme[2] = v;
                if (v.pt.y > extreme[3].pt.y) extreme[3] = v;
                if (v.pt.z < extreme[4].pt.z) extreme[4] = v;
                if (v.pt.z > extreme[5].pt.z) extreme[5] = v;
            }

            // line segment: find the 2 most distant points.
            VertNode v0 = null;
            VertNode v1 = null;

            float dmax = 0.0f;
            for (int i = 0; i < 6; i++)
                for (int j = i + 1; j < 6; j++)
                {
                    // squared lenght
                    float dcur = Vector3f.Dot(extreme[i].pt, extreme[j].pt);
                    if (dmax < dcur)
                    {
                        dmax = dcur;
                        v0 = extreme[i];
                        v1 = extreme[j];
                    }
                }

            if (v0 == null || v1 == null) return Result.AllCoincident;


            // triangle: find the most distant point to the line segment.
            dmax = 0.0f;
            VertNode v2 = null;

            for (int i = 0; i < 6; i++)
            {
                float dcur = distSqPointSegment(v0, v1, extreme[i]);
                if (dmax < dcur)
                {
                    dmax = dcur;
                    v2 = extreme[i];
                }
            }
            if (v2 == null) return Result.AllColinear;

            // pyramid: find the most distant point to the triangle.
            Vector3f N = Vector3f.Cross(v1.pt - v0.pt, v2.pt - v0.pt);
            N.Normalize();
            
            float D = Vector3f.Dot(N, v0.pt);
            VertNode v3 = null;

            foreach (VertNode v in vertsList)
            {
                float dcur = Maths.Maths.ABS(Vector3f.Dot(v.pt, N) - D);
                if (dmax < dcur)
                    v3 = v;
            }

            if (v3 == null) return Result.AllCoplanar;

            // create intial mesh



            //return new float[][]{ v1,v2,v0,v3 }; // order matters!

            return Result.OK;
        }

#region DEBUGGER
        public void Checks()
        {
            int V = 0, E = 0, F = 0;

            foreach (VertNode v in vertsList) if (v.processed) V++;
            foreach (EdgeNode e in edgesList) E++;
            foreach (FaceNode f in facesList) F++;

            if (V == 0 && F == 0 && E == 0) return;

            check_consistency();
            check_convexity();
            check_euler(V, E, F);
            check_endpoints();
        }

        /// <summary>
        /// Consistency runs through the edge list and checks that all
        /// adjacent faces have their endpoints in opposite order.  This verifies
        /// that the vertices are in counterclockwise order.
        /// </summary>
        void check_consistency()
        {
            int i, j;
            foreach (EdgeNode e in edgesList)
            {
                // find index of endpoint[0] in adjacent face[0]
                for (i = 0; e.adjface[0].vertices[i] != e.endpoint[0]; ++i) ;
                // find index of endpoint[0] in adjacent face[1]
                for (j = 0; e.adjface[1].vertices[j] != e.endpoint[0]; ++j) ;

                // check if the endpoints occur in opposite order
                if (!(e.adjface[0].vertices[(i + 1) % 3] ==
                    e.adjface[1].vertices[(j + 2) % 3] ||
                    e.adjface[0].vertices[(i + 2) % 3] ==
                    e.adjface[1].vertices[(j + 1) % 3]))
                    throw new Exception("Checks: edges are NOT consistent");
            }
        }
        /// <summary>
        /// Convexity checks that the volume between every face and every
        /// point is negative.  This shows that each point is inside every face
        /// and therefore the hull is convex.
        /// </summary>
        void check_convexity()
        {
            foreach (FaceNode f in facesList)
            {
                foreach (VertNode v in vertsList)
                {
                    if (v.processed)
                    {
                        double vol;
                        //int sign = volume_sign(f, v,out vol);
                        int sign = normal_sign(f, v, out vol);
                        if (sign < 0)
                        {
                            throw new Exception("Checks: NOT convex, vertex v" + v.origIdx + " out of face " + f.ToString() + "\n at add_one : " + current.origIdx);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks that, for each face, for each i={0,1,2}, the [i]th vertex of
        /// that face is either the [0]th or [1]st endpoint of the [ith] edge of
        /// the face.
        /// </summary>
        void check_endpoints()
        {
            foreach (FaceNode f in facesList)
            {
                for (int i = 0; i < 3; i++)
                {
                    VertNode v = f.vertices[i];
                    EdgeNode e = f.edges[i];
                    if (v != e.endpoint[0] && v != e.endpoint[1])
                    {
                        throw new Exception("Checks: ERROR found");
                    }
                }
            }
        }
        /// <summary>
        /// CheckEuler checks Euler's relation, as well as its implications when
        /// all faces are known to be triangles.  Only prints positive information
        /// when debug is true, but always prints negative information.
        /// </summary>
        void check_euler(int V, int E, int F)
        {
            if ((V - E + F) != 2)
                throw new Exception("Checks: V-E+F != 2");

            if (F != (2 * V - 4))
                throw new Exception("Checks: F != 2V-4");

            if ((2 * E) != (3 * F))
                throw new Exception("Checks: 2E != 3F");
        }

        /// <summary>
        ///   EdgeOrderOnFaces: puts e0 between v0 and v1, e1 between v1 and v2,
        ///   e2 between v2 and v0 on each face.  This should be unnecessary, alas.
        ///   Not used in code, but useful for other purposes.
        /// </summary>
        void set_edge_order_on_faces()
        {
            foreach (FaceNode f in facesList)
            {
                int i, j;
                for (i = 0; i < 3; i++)
                {
                    if (!(((f.edges[i].endpoint[0] == f.vertices[i]) &&
                           (f.edges[i].endpoint[1] == f.vertices[(i + 1) % 3])) ||
                          ((f.edges[i].endpoint[1] == f.vertices[i]) &&
                           (f.edges[i].endpoint[0] == f.vertices[(i + 1) % 3]))))
                    {
                        // Change the order of the edges on the face:
                        for (j = 0; j < 3; j++)
                        {
                            // find the edge that should be there
                            if (((f.edges[j].endpoint[0] == f.vertices[i]) &&
                                 (f.edges[j].endpoint[1] == f.vertices[(i + 1) % 3])) ||
                                ((f.edges[j].endpoint[1] == f.vertices[i]) &&
                                 (f.edges[j].endpoint[0] == f.vertices[(i + 1) % 3])))
                            {
                                // Swap it with the one erroneously put into its place: 

                                EdgeNode newEdge = f.edges[i];
                                f.edges[i] = f.edges[j];
                                f.edges[j] = newEdge;
                            }
                        }
                    }
                }
            }
        }

#endregion
    }

    /// <summary>
    /// revisited with my half-edge mesh implementation 
    /// </summary>
    public class ConvexHull3D_v2
    {
        PolygonMesh hull;

        /// <summary>
        /// the ID of vertices added to hull, used for information
        /// </summary>
        List<int> added = new List<int>();

        public ConvexHull3D_v2(IList<Vector3f> points, bool debug)
        {
            hull = new PolygonMesh();

            // the pseudo code for incremental algorithm : process one vertex by one, can be O(n) if optimal

            int i=0;
            int count = 0;
            for ( i = 0; i < points.Count; i++)
            {
                if (AddTetrahedron(points[i].position))
                {
                    added.Add(i);
                    count++;
                }
                if (count==4) break;
            }
            for (; i < points.Count; i++)
            {
                if (AddVertexToHull(points[i].position))
                {
                    added.Add(i);
                }
            }
        }
        bool AddTetrahedron(Vector3f v)
        {
            return false;
        }
        /// <summary>
        /// Add vertex to hull, return false if can't add it
        /// </summary>
        bool AddVertexToHull(Vector3f v)
        {
            return true;
        }
    }

}
#endif