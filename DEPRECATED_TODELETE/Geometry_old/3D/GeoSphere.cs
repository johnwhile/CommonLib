using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Common.Maths;

#if DELETE

namespace Common.Geometry
{
    /// <summary>
    /// Tessellated sphere calculated from tetrahedron
    /// A not primitive mesh, contain internal functions example for tessellation.
    /// </summary>
    /// <remarks>
    /// TODO : decrease tessellation
    /// </remarks>
    public class GeoSphere : PolygonMesh
    {
        float m_radius = 0;

        private GeoSphere(float radius)
        {
            m_radius = radius;
            boundSphere = new Sphere(Vector3f.Zero, radius);
        }
        /// <summary>
        /// Generate a base cube to start tessellation
        /// </summary>
        public static GeoSphere Cube(float radius)
        {
            GeoSphere mesh = new GeoSphere(radius);

            List<BaseVert> v = new List<BaseVert>(8);
            v.Add(new HalfVertFull(new Vector3f(radius, radius, radius), Color.Red));
            v.Add(new HalfVertFull(new Vector3f(radius, radius, -radius), Color.Red));
            v.Add(new HalfVertFull(new Vector3f(radius, -radius, radius), Color.Green));
            v.Add(new HalfVertFull(new Vector3f(radius, -radius, -radius), Color.Green));
            v.Add(new HalfVertFull(new Vector3f(-radius, radius, radius), Color.Blue));
            v.Add(new HalfVertFull(new Vector3f(-radius, radius, -radius), Color.Blue));
            v.Add(new HalfVertFull(new Vector3f(-radius, -radius, radius), Color.White));
            v.Add(new HalfVertFull(new Vector3f(-radius, -radius, -radius), Color.White));

            List<BaseFace> f = new List<BaseFace>(12);
            f.Add(new BaseFace(v[3], v[1], v[0]));
            f.Add(new BaseFace(v[3], v[0], v[2]));
            f.Add(new BaseFace(v[2], v[0], v[4]));
            f.Add(new BaseFace(v[2], v[4], v[6]));
            f.Add(new BaseFace(v[6], v[4], v[5]));
            f.Add(new BaseFace(v[6], v[5], v[7]));
            f.Add(new BaseFace(v[7], v[5], v[1]));
            f.Add(new BaseFace(v[7], v[1], v[3]));
            f.Add(new BaseFace(v[1], v[5], v[4]));
            f.Add(new BaseFace(v[1], v[4], v[0]));
            f.Add(new BaseFace(v[7], v[3], v[2]));
            f.Add(new BaseFace(v[7], v[2], v[6]));

            mesh.m_faces = f;
            mesh.m_verts = v;

            mesh.clearlist();
            mesh.calculateEdgeTable();

            return mesh;
        }      
        
        /// <summary>
        /// Uso un retticolo cubico a facce centrate
        /// </summary>
        public static GeoSphere CubeCenterFace(float radius)
        {
            GeoSphere mesh = new GeoSphere(radius);

            List<BaseVert> v = new List<BaseVert>(14);
            v.Add(new HalfVertFull(new Vector3f(radius, radius, radius), Color.Red));
            v.Add(new HalfVertFull(new Vector3f(radius, radius, -radius), Color.Red));
            v.Add(new HalfVertFull(new Vector3f(radius, -radius, radius), Color.Green));
            v.Add(new HalfVertFull(new Vector3f(radius, -radius, -radius), Color.Green));
            v.Add(new HalfVertFull(new Vector3f(-radius, radius, radius), Color.Blue));
            v.Add(new HalfVertFull(new Vector3f(-radius, radius, -radius), Color.Blue));
            v.Add(new HalfVertFull(new Vector3f(-radius, -radius, radius), Color.White));
            v.Add(new HalfVertFull(new Vector3f(-radius, -radius, -radius), Color.White));

            v.Add(new HalfVertFull(new Vector3f(radius, 0, 0), Color.Black));
            v.Add(new HalfVertFull(new Vector3f(0, radius, 0), Color.Black));
            v.Add(new HalfVertFull(new Vector3f(0, 0, radius), Color.Black));

            v.Add(new HalfVertFull(new Vector3f(-radius, 0, 0), Color.Black));
            v.Add(new HalfVertFull(new Vector3f(0, -radius, 0), Color.Black));
            v.Add(new HalfVertFull(new Vector3f(0, 0, -radius), Color.Black));


            List<BaseFace> f = new List<BaseFace>(24);
            f.Add(new BaseFace(v[0], v[8], v[1]));
            f.Add(new BaseFace(v[1], v[8], v[3]));
            f.Add(new BaseFace(v[3], v[8], v[2]));
            f.Add(new BaseFace(v[2], v[8], v[0]));

            f.Add(new BaseFace(v[0], v[9], v[4]));
            f.Add(new BaseFace(v[4], v[9], v[5]));
            f.Add(new BaseFace(v[5], v[9], v[1]));
            f.Add(new BaseFace(v[1], v[9], v[0]));

            f.Add(new BaseFace(v[0], v[10], v[2]));
            f.Add(new BaseFace(v[2], v[10], v[6]));
            f.Add(new BaseFace(v[6], v[10], v[4]));
            f.Add(new BaseFace(v[4], v[10], v[0]));

            f.Add(new BaseFace(v[4], v[11], v[6]));
            f.Add(new BaseFace(v[6], v[11], v[7]));
            f.Add(new BaseFace(v[7], v[11], v[5]));
            f.Add(new BaseFace(v[5], v[11], v[4]));

            f.Add(new BaseFace(v[2], v[12], v[3]));
            f.Add(new BaseFace(v[3], v[12], v[7]));
            f.Add(new BaseFace(v[7], v[12], v[6]));
            f.Add(new BaseFace(v[6], v[12], v[2]));

            f.Add(new BaseFace(v[1], v[13], v[5]));
            f.Add(new BaseFace(v[5], v[13], v[7]));
            f.Add(new BaseFace(v[7], v[13], v[3]));
            f.Add(new BaseFace(v[3], v[13], v[1]));

            mesh.m_faces = f;
            mesh.m_verts = v;
 
            mesh.clearlist();
            mesh.calculateEdgeTable();

            return mesh;
        }
        
        /// <summary>
        /// Generate a base tetrahedron to start tessellation
        /// </summary>
        public static GeoSphere Tetrahedron(float radius)
        {
            GeoSphere mesh = new GeoSphere(radius);
            float _C23 = -(float)Math.Sqrt(2.0) / 3.0f;
            float _C13 = -1f / 3f;
            float _C63 = (float)Math.Sqrt(6.0) / 3.0f;

            List<BaseVert> v = new List<BaseVert>(4);
            v.Add(new HalfVertFull(new Vector3f(0, 0, 1), Color.Red));
            v.Add(new HalfVertFull(new Vector3f(0, 2.0f * (float)Math.Sqrt(2.0) / 3.0f, _C13), Color.Green));
            v.Add(new HalfVertFull(new Vector3f(-_C63, _C23, _C13), Color.Blue));
            v.Add(new HalfVertFull(new Vector3f(_C63, _C23, _C13), Color.White));

            List<BaseFace> f = new List<BaseFace>(4);
            f.Add(new BaseFace(v[2], v[0], v[1]));
            f.Add(new BaseFace(v[3], v[0], v[2]));
            f.Add(new BaseFace(v[1], v[0], v[3]));
            f.Add(new BaseFace(v[1], v[3], v[2]));
            
            mesh.m_faces = f;
            mesh.m_verts = v;
            // update vertex position in a sphere surface
            mesh.projectToSphere();
            // initialize first texture values;
            mesh.CylindricalTextureProjection(Matrix4x4f.Identity);

            mesh.clearlist();
            mesh.calculateEdgeTable();
            return mesh;
        }

        /// <summary>
        /// Generate a base Icosahedron to start tessellation
        /// </summary>
        public static GeoSphere Icosahedron(float radius)
        {
            GeoSphere mesh = new GeoSphere(radius);
            // rapporto aureo
            float t = (1 + (float)Math.Sqrt(5.0)) / 2f;

            // create 12 vertices of a icosahedron
            List<BaseVert> v = new List<BaseVert>(12);
            v.Add(new HalfVertFull(-1, t, 0));
            v.Add(new HalfVertFull(1, t, 0));
            v.Add(new HalfVertFull(-1, -t, 0));
            v.Add(new HalfVertFull(1, -t, 0));
            v.Add(new HalfVertFull(0, -1, t));
            v.Add(new HalfVertFull(0, 1, t));
            v.Add(new HalfVertFull(0, -1, -t));
            v.Add(new HalfVertFull(0, 1, -t));
            v.Add(new HalfVertFull(t, 0, -1));
            v.Add(new HalfVertFull(t, 0, 1));
            v.Add(new HalfVertFull(-t, 0, -1));
            v.Add(new HalfVertFull(-t, 0, 1));

            List<BaseFace> f = new List<BaseFace>(20);
            // 5 faces around point 0
            f.Add(new BaseFace(v[0], v[11], v[5]));
            f.Add(new BaseFace(v[0], v[5], v[1]));
            f.Add(new BaseFace(v[0], v[1], v[7]));
            f.Add(new BaseFace(v[0], v[7], v[10]));
            f.Add(new BaseFace(v[0], v[10], v[11]));

            // 5 adjacent faces
            f.Add(new BaseFace(v[1], v[5], v[9]));
            f.Add(new BaseFace(v[5], v[11], v[4]));
            f.Add(new BaseFace(v[11], v[10], v[2]));
            f.Add(new BaseFace(v[10], v[7], v[6]));
            f.Add(new BaseFace(v[7], v[1], v[8]));

            // 5 faces around point 3
            f.Add(new BaseFace(v[3], v[9], v[4]));
            f.Add(new BaseFace(v[3], v[4], v[2]));
            f.Add(new BaseFace(v[3], v[2], v[6]));
            f.Add(new BaseFace(v[3], v[6], v[8]));
            f.Add(new BaseFace(v[3], v[8], v[9]));

            // 5 adjacent faces
            f.Add(new BaseFace(v[4], v[9], v[5]));
            f.Add(new BaseFace(v[2], v[4], v[11]));
            f.Add(new BaseFace(v[6], v[2], v[10]));
            f.Add(new BaseFace(v[8], v[6], v[7]));
            f.Add(new BaseFace(v[9], v[8], v[1]));

            mesh.m_faces = f;
            mesh.m_verts = v;

            // update vertex position in a sphere surface
            mesh.projectToSphere();
            // initialize first texture values;
            mesh.CylindricalTextureProjection(Matrix4x4f.Identity);

            mesh.clearlist();
            mesh.calculateEdgeTable();
            return mesh;
        }

        /// <summary>
        /// Split all faces in 4 faces, using the default tessellation
        /// </summary>
        /// <param name="mode">mode = 0 : default. mode 1 = triangle fan split mode</param>
        public void IncreaseTesselation(byte mode)
        {
            switch (mode)
            {
                case 0: TriangleTessellate(true); break;
                default: TriangleFanTessellate(true); break;
            }
            projectToSphere();
        }

        void projectToSphere()
        {
            // update vertex position in a sphere surface
            if (m_verts.GetType().GetElementType() == typeof(HalfVertFull))
            {
                
                for (int i = 0; i < m_verts.Count; i++)
                {
                    HalfVertFull v = (HalfVertFull)m_verts[i];
                    Vector3f n = v.position.Normal;
                    v.normal = n;
                    v.position = n * m_radius;
                    m_verts[i] = v;
                }
            }
            else
            {
                for (int i = 0; i < m_verts.Count; i++)
                {
                    BaseVert v = m_verts[i];
                    v.position = v.position.Normal * m_radius;
                    m_verts[i] = v;
                }
            }
        }

    }


#region DEPRECATED
    /*
    class Tetrahedron_DEPRECATED : BaseGeometry
    {
        int vcount = 0;
        float m_radius = 0;
        static float _C23 = -(float)Math.Sqrt(2.0) / 3.0f;
        static float _C13 = -1f / 3f;
        static float _C63 = (float)Math.Sqrt(6.0) / 3.0f;

        Color tricolor;
        public List<Vector3> vertices;
        public List<Vector2> textures;
        public List<Color32> colors;
        public List<Face16> faces;

        public override int numVertices { get { return vertices.Count; } }
        public override int numPrimitives { get { return faces.Count; } }
        public override void changeTransform(Matrix4 newtransform)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Generate a base tetrahedron to start tessellation
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="iterations">with ushort indices this value must be &lt; 8 </param>
        public Tetrahedron_DEPRECATED(float radius, int iterations)
        {
            this.m_radius = radius;
            boundSphere = new BoundarySphere(Vector3.Zero, radius);

            int numtriangles = 4 << (2 * iterations); // = 4^(N+1)
            int numvertices = iterations > 0 ? (4 << (iterations*2))/4 * 3 - 2 : 4; // 4^n * 3 -2

            if (numvertices > ushort.MaxValue - 1)
                throw new OverflowException("uint faces can't contain >65534 vertices");

            vertices = new List<Vector3>(numvertices);
            faces = new List<Face16>(numtriangles);
            textures = new List<Vector2>(numvertices);
            colors = new List<Color32>(numvertices);

            vcount = 0;

            Vector3 p0 = new Vector3(0, 0, 1);
            Vector3 p1 = new Vector3(0, (2.0 * Math.Sqrt(2.0)) / 3.0, _C13);
            Vector3 p2 = new Vector3(-_C63, _C23, _C13);
            Vector3 p3 = new Vector3(_C63, _C23, _C13);

            //indices of corner
            tricolor = Color.Red;
            int i0 = AddVertex(p0);
            tricolor = Color.Green;
            int i1 = AddVertex(p1);
            tricolor = Color.Blue;
            int i2 = AddVertex(p2);
            tricolor = Color.White;
            int i3 = AddVertex(p3);

            // indices of middle points
            int i01, i02, i03, i12, i13, i23;
            i01 = i02 = i03 = i12 = i13 = i23 = -1;

            tricolor = Color.Red;
            RecurseTesselate(i2, i0, i1, ref i02, ref i01, ref i12, iterations);
            tricolor = Color.Green;
            RecurseTesselate(i3, i0, i2, ref i03, ref i02, ref i23, iterations);
            tricolor = Color.Blue;
            RecurseTesselate(i1, i0, i3, ref i01, ref i03, ref i13, iterations);
            tricolor = Color.White;
            RecurseTesselate(i1, i3, i2, ref i13, ref i23, ref i12, iterations);
        }


        /// <summary>
        /// </summary>
        /// <param name="r">right point</param>
        /// <param name="ir">index of right point</param>
        /// <param name="i_ra">index of middle point of edge right-apex</param>
        /// <param name="level">level of tessellation, 0 = leaf triangle</param>
        void RecurseTesselate(int i_r, int i_a, int i_l, ref int i_ra, ref int i_al, ref int i_lr, int level)
        {
            if (level > 0)
            {
                // calculate middle points
                Vector3 ra = (vertices[i_r] + vertices[i_a]) * 0.5f;
                Vector3 al = (vertices[i_a] + vertices[i_l]) * 0.5f;
                Vector3 lr = (vertices[i_l] + vertices[i_r]) * 0.5f;

                // insert if need these points
                if (i_ra < 0) i_ra = AddVertex(ra);
                if (i_al < 0) i_al = AddVertex(al);
                if (i_lr < 0) i_lr = AddVertex(lr);

                // indices of middle-middle neighbour points
                int i_r_ra = -1;
                int i_ra_a = -1;
                int i_a_al = -1;
                int i_al_l = -1;
                int i_l_lr = -1;
                int i_lr_r = -1;

                // indices of middle-middle interior points
                int i_ra_al = -1;
                int i_al_lr = -1;
                int i_lr_ra = -1;

                --level;
                // Subdivide triangle into four triangles
                RecurseTesselate(i_ra, i_a, i_al, ref i_ra_a, ref i_a_al, ref i_ra_al, level);
                RecurseTesselate(i_lr, i_al, i_l, ref i_al_lr, ref i_al_l, ref i_l_lr, level);
                RecurseTesselate(i_al, i_lr, i_ra, ref i_al_lr, ref i_lr_ra, ref i_ra_al, level);
                RecurseTesselate(i_r, i_ra, i_lr, ref i_r_ra, ref i_lr_ra, ref i_lr_r, level);

            }
            else
            {
                faces.Add(new Face16(i_r, i_a, i_l));
            }
        }

        /// <summary>
        /// return the index of vertex added to list
        /// </summary>
        int AddVertex(Vector3 v)
        {
            v.Normalize();

            Vector2 uv = new Vector2();
            uv.x = (float)(Math.Atan2(v.y, v.x) / (Math.PI * 2) + 0.5);
            uv.y = (float)(Math.Asin(v.z) / Math.PI  + 0.5);
            
            Color color = tricolor;
            //color = Color.FromArgb((int)(uv.x * 255), (int)(uv.y * 255), 128);

            v *= m_radius;
            vertices.Add(v);
            textures.Add(uv);
            colors.Add;//nice rainbow gradient
            return vcount++;
        }


        /// <summary>
        /// Down casting from iterative to primitive geometry, all tessellation information will be lost.
        /// Return a static geometry.
        /// </summary>
        public static explicit operator MeshListGeometry(Tetrahedron_DEPRECATED sphere)
        {
            MeshListGeometry mesh = new MeshListGeometry((BaseGeometry)sphere);
            mesh.name = sphere.name;
            mesh.vertices = new VertexAttribute<Vector3>(DeclarationUsage.Position, sphere.vertices);
            mesh.faces16 = new IndexAttribute<Face16>(sphere.faces);
            mesh.textures = new VertexAttribute<Vector2>(DeclarationUsage.TextureCoordinate, sphere.textures);
            mesh.colors = new VertexAttribute<Color32>(DeclarationUsage.Color, sphere.colors);

            mesh.boundSphere = sphere.boundSphere;
            return mesh;
        }
    }

    /// <summary>
    /// TODO : avoid duplicated vertices
    /// Tessellated sphere calculated from icosahedron
    /// </summary>
    class Icosahedron_DEPRECATED : BaseGeometry
    {
        // rapporto aureo
        static float t = (1 + (float)Math.Sqrt(5.0)) / 2f;

        float m_radius;
        int vcount = 0;
        List<Vector3> vertices;
        List<Vector2> textures;
        List<Color32> colors;
        List<Face16> faces;

        public Icosahedron_DEPRECATED(float radius , int iterations)
        {
            vcount = 0;
            m_radius = radius;
            vertices = new List<Vector3>();
            textures = new List<Vector2>();
            colors = new List<Color32>();
            faces = new List<Face16>();

            List<Face16> tmp_f = new List<Face16>();
            List<Vector3> tmp_v = new List<Vector3>();

            // create 12 vertices of a icosahedron
            tmp_v.Add(new Vector3(-1, t, 0));
            tmp_v.Add(new Vector3(1, t, 0));
            tmp_v.Add(new Vector3(-1, -t, 0));
            tmp_v.Add(new Vector3(1, -t, 0));
            tmp_v.Add(new Vector3(0, -1, t));
            tmp_v.Add(new Vector3(0, 1, t));
            tmp_v.Add(new Vector3(0, -1, -t));
            tmp_v.Add(new Vector3(0, 1, -t));
            tmp_v.Add(new Vector3(t, 0, -1));
            tmp_v.Add(new Vector3(t, 0, 1));
            tmp_v.Add(new Vector3(-t, 0, -1));
            tmp_v.Add(new Vector3(-t, 0, 1));

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].Normal * m_radius;
                colors.Add(Color.Blue);
                textures.Add(new Vector2(0, 0));          
            }

            // 5 faces around point 0
            tmp_f.Add(new Face16(0, 11, 5));
            tmp_f.Add(new Face16(0, 5, 1));
            tmp_f.Add(new Face16(0, 1, 7));
            tmp_f.Add(new Face16(0, 7, 10));
            tmp_f.Add(new Face16(0, 10, 11));

            // 5 adjacent faces
            tmp_f.Add(new Face16(1, 5, 9));
            tmp_f.Add(new Face16(5, 11, 4));
            tmp_f.Add(new Face16(11, 10, 2));
            tmp_f.Add(new Face16(10, 7, 6));
            tmp_f.Add(new Face16(7, 1, 8));

            // 5 faces around point 3
            tmp_f.Add(new Face16(3, 9, 4));
            tmp_f.Add(new Face16(3, 4, 2));
            tmp_f.Add(new Face16(3, 2, 6));
            tmp_f.Add(new Face16(3, 6, 8));
            tmp_f.Add(new Face16(3, 8, 9));

            // 5 adjacent faces
            tmp_f.Add(new Face16(4, 9, 5));
            tmp_f.Add(new Face16(2, 4, 11));
            tmp_f.Add(new Face16(6, 2, 10));
            tmp_f.Add(new Face16(8, 6, 7));
            tmp_f.Add(new Face16(9, 8, 1));


            for (int i = 0; i < tmp_f.Count; i++)
            {
                RecurseTessellate(
                    tmp_v[tmp_f[i].I],
                    tmp_v[tmp_f[i].J],
                    tmp_v[tmp_f[i].K],
                    iterations);

                List<Face16> subdiv = new List<Face16>();

                for (int n = 0; n < iterations; n++)
                {

                }
            }
        }
        int AddVertex(Vector3 v)
        {
            vertices.Add(v.Normal * m_radius);
            colors.Add(Color.Blue);
            textures.Add(Vector2.Zero);
            return vcount++;
        }

        void RecurseTessellate(Vector3 v1, Vector3 v2, Vector3 v3, int depth)
        {
            if (depth == 0)
            {
                Face16 f = new Face16();
                f.I = (ushort)AddVertex(v1);
                f.J = (ushort)AddVertex(v2);
                f.K = (ushort)AddVertex(v3);
                faces.Add(f);
            }
            else
            {
                Vector3 v12 = (v1 + v2) * 0.5f;
                Vector3 v23 = (v2 + v3) * 0.5f;
                Vector3 v31 = (v3 + v1) * 0.5f;
                RecurseTessellate(v1, v12, v31, depth - 1);
                RecurseTessellate(v2, v23, v12, depth - 1);
                RecurseTessellate(v3, v31, v23, depth - 1);
                RecurseTessellate(v12, v23, v31, depth - 1);
            }
        }



        public override int numVertices { get { return vertices.Count; } }
        public override int numPrimitives { get { return faces.Count; } }

        public override void changeTransform(Matrix4 newtransform)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Down casting from iterative to primitive geometry, all tessellation information will be lost.
        /// Return a static geometry.
        /// </summary>
        public static explicit operator MeshListGeometry(Icosahedron_DEPRECATED sphere)
        {
            MeshListGeometry mesh = new MeshListGeometry((BaseGeometry)sphere);
            mesh.name = sphere.name;
            mesh.vertices = new VertexAttribute<Vector3>(DeclarationUsage.Position, sphere.vertices);
            mesh.faces16 = new IndexAttribute<Face16>(sphere.faces);
            mesh.textures =new VertexAttribute<Vector2>(DeclarationUsage.TextureCoordinate, sphere.textures);
            mesh.colors = new VertexAttribute<Color32>(DeclarationUsage.Color, sphere.colors);

            mesh.boundSphere = sphere.boundSphere;
            return mesh;
        }
    }

    */
#endregion
}

#endif