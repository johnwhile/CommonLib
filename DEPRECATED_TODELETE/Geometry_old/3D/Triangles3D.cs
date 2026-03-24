using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

#if DELETE
namespace Common.Geometry
{
    /// <summary>
    /// Abstract class, used to derive all similar Primitives Triangles
    /// </summary>
    public abstract class BaseTriGeometry : BaseGeometry3D
    {
        // the all possible data, to optimize them, they are set to null all 
        // exept vertices because is the base information
        public List<Vector3f> vertices;
        public List<Vector3f> normals;
        public List<Vector3f> tangents;
        public List<Vector2f> texcoords;
        public List<Vector4b> colors;
        /// <summary>
        /// handedness of binormals direction
        /// </summary>
        public int[] bitangent_sign;

        /// <summary>
        /// Empty : all vertex attribute to NULL
        /// </summary>
        public BaseTriGeometry()
            : base("BaseTriGeometry")
        {
            vertices = null;
            normals = null;
            texcoords = null;
            colors = null;
            tangents = null;
        }
        
        /// <summary>
        /// Copy attributes
        /// </summary>
        public BaseTriGeometry(BaseTriGeometry src)
            : base(src)
        {
            vertices = src.vertices;
            normals = src.normals;
            texcoords = src.texcoords;
            tangents = src.tangents;
            colors = src.colors;
        }
        
        /// <summary>
        /// Copy 2d attributes from 3d, use eAxis for conversion
        /// </summary>
        public BaseTriGeometry(BaseTriPolygon src, eAxis coordX, eAxis coordY)
            : base(src)
        {
            texcoords = src.textures;
            colors = src.colors;
            normals = null;
            tangents = null;

            if (src.numVertices > 0)
            {
                if (coordX == coordY) throw new ArgumentException("invalid coordX and coordY value");
                vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, src.numVertices, Vector3f.Zero);
                switch (coordX)
                {
                    case eAxis.X: for (int i = 0; i < src.numVertices; i++) vertices.data[i].x = src.vertices[i].x; break;
                    case eAxis.Y: for (int i = 0; i < src.numVertices; i++) vertices.data[i].y = src.vertices[i].x; break;
                    case eAxis.Z: for (int i = 0; i < src.numVertices; i++) vertices.data[i].z = src.vertices[i].x; break;
                    default: throw new ArgumentException("invalid coordX value");
                }
                switch (coordY)
                {
                    case eAxis.X: for (int i = 0; i < src.numVertices; i++) vertices.data[i].x = src.vertices[i].y; break;
                    case eAxis.Y: for (int i = 0; i < src.numVertices; i++) vertices.data[i].y = src.vertices[i].y; break;
                    case eAxis.Z: for (int i = 0; i < src.numVertices; i++) vertices.data[i].z = src.vertices[i].y; break;
                    default: throw new ArgumentException("invalid coordX value");
                }
            }
        }

        /// <summary>
        /// Get number of vertices using vertices attribute
        /// </summary>
        public override int numVertices 
        { 
            get { return (vertices != null) ? vertices.Count : 0; }
        }
        /// <summary>
        /// Get the vertex indices of this primitive
        /// </summary>
        public abstract void GetTriangle(int primitive, out int i, out int j, out int k);
        /// <summary>
        ///  Calculate all normal (create a new VertexAttribute)
        /// </summary>
        public void CalculateNormal()
        {
            int nverts = numVertices;
            int ntriangles = numPrimitives;

            if (nverts < 1) return;

            normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, nverts);

            for (int t = 0; t < ntriangles; t++)
            {
                int i, j, k;
                GetTriangle(t, out i, out j, out k);

                Vector3f v0 = vertices[i];
                Vector3f v1 = vertices[j];
                Vector3f v2 = vertices[k];

                Vector3f e0 = v1 - v0;
                Vector3f e1 = v2 - v0;
                Vector3f e2 = v2 - v1;

                Vector3f n = Vector3f.Cross(e0, e1);
                float dot0 = e0.LengthSq;
                float dot1 = e1.LengthSq;
                float dot2 = e2.LengthSq;

                if (dot0 < 0.0001) dot0 = 1.0f;
                if (dot1 < 0.0001) dot1 = 1.0f;
                if (dot2 < 0.0001) dot2 = 1.0f;

                normals.data[i] += n * (1.0f / (dot0 * dot1));
                normals.data[j] += n * (1.0f / (dot2 * dot0));
                normals.data[k] += n * (1.0f / (dot1 * dot2));
            }
            for (int i = 0; i < nverts; i++)
                normals.data[i].Normalize();

        }
        /// <summary>
        /// Recalculate all texture using a projection plane
        /// </summary>
        public void CalculateTextureDefault()
        {
            texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, GeometryTools.GetPlanarProjection(vertices.data, new Vector2f(0, 0), new Vector2f(1, 1), Matrix4x4f.Identity));
        }
        /// <summary>
        /// Calculate Tangent and Binormal space using Normals and Textures 
        /// </summary>
        public void CalculateTBN()
        {
            if (normals == null || texcoords == null) throw new ArgumentNullException("missing normals of textures data");

            tangents = new VertexAttribute<Vector3f>(DeclarationUsage.Tangent, numVertices, Vector3f.Zero);
            bitangent_sign = new int[numVertices];

            Vector3f[] tan1 = new Vector3f[numVertices];
            Vector3f[] tan2 = new Vector3f[numVertices];

            for (int t = 0; t < numPrimitives; t++)
            {
                int i, j, k;
                GetTriangle(t, out i, out j, out k);
                Vector3f v1 = vertices[i];
                Vector3f v2 = vertices[j];
                Vector3f v3 = vertices[k];
        
                Vector2f w1 = texcoords[i];
                Vector2f w2 = texcoords[j];
                Vector2f w3 = texcoords[k];
        
                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;
        
                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;
        
                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3f sdir = new Vector3f((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,(t2 * z1 - t1 * z2) * r);
                Vector3f tdir = new Vector3f((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,(s1 * z2 - s2 * z1) * r);
        
                tan1[i] += sdir;
                tan1[j] += sdir;
                tan1[k] += sdir;
        
                tan2[i] += tdir;
                tan2[j] += tdir;
                tan2[k] += tdir;
            }
            for (int i = 0; i < numVertices; i++)
            {
                Vector3f n = normals[i];
                Vector3f t = tan1[i];
                // Gram-Schmidt orthogonalize
                tangents[i] = t - n * Vector3f.Dot(n, t);
                tangents[i].Normalize();
                // Calculate handedness
                bitangent_sign[i] = (Vector3f.Dot(Vector3f.Cross(n, t), tan2[i]) < 0) ? -1 : 1;
            }
        }
        /// <summary>
        /// The binormals vectors are not precomputed to reduce space
        /// </summary>
        public Vector3f GetBitangent(int index)
        {
            // in the cross function there are a "new" function, in normalize and mulscalar we don't create a new
            // instance, will be more fast;
            Vector3f b = Vector3f.Cross(normals[index], tangents[index]);
            b.Normalize();
            b.Multiply(bitangent_sign[index]);
            return b;
        }

        /// <summary>
        /// change transform but vertices will be in the same position
        /// </summary>
        /// <param name="newtransform"></param>
        public override void changeTransform(Matrix4x4f newtransform)
        {
            Matrix4x4f matrix = base.transform * Matrix4x4f.Inverse(newtransform);
            // remove traslation componenent because normal are a direction (position = 0,0,0)
            Matrix4x4f matrix_normal = matrix;
            matrix_normal.Position = new Vector3f(0, 0, 0);

            if (vertices != null)
                for (int i = 0; i < numVertices; i++)
                    vertices[i] = matrix * vertices[i];

            if (normals != null)
                for (int i = 0; i < numVertices; i++)
                    normals[i] = matrix_normal * normals[i];

            transform = newtransform;
        }

        /// <summary>
        /// append vertices attribute from other geometries, this geometry have the priority about 
        /// existing attribute, if source geometry don't have attribute but this yes, populate missing data with empty values
        /// </summary>
        protected int appendVertexAttribute(BaseTriGeometry geometry)
        {
            int nverts1 = this.numVertices;
            int nverts2 = geometry.numVertices;
            Matrix4x4f transformation = geometry.globalcoord * globalcoord_inv;


            // vertices must not be null
            if (this.vertices != null)
            {
                VertexAttribute<Vector3f> tmp = new VertexAttribute<Vector3f>(DeclarationUsage.Position, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.vertices[i];

                for (int i = 0; i < nverts2; i++)
                    tmp[i + nverts1] = Vector3f.TransformCoordinate(geometry.vertices[i], transformation);

                this.vertices = tmp;
            }

            // other attribute can be omitted, the destination class have the priority
            if (this.normals != null)
            {
                VertexAttribute<Vector3f> tmp = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.normals[i];
                if (geometry.normals != null)
                    for (int i = 0; i < nverts2; i++)
                        tmp[i + nverts1] = Vector3f.TransformNormal(geometry.normals[i], transformation);
                
                this.normals = tmp;
            }
            if (this.tangents != null)
            {
                VertexAttribute<Vector3f> tmp = new VertexAttribute<Vector3f>(DeclarationUsage.Tangent, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.tangents[i];
                if (geometry.tangents != null)
                    for (int i = 0; i < nverts2; i++)
                        tmp[i + nverts1] = Vector3f.TransformNormal(geometry.tangents[i], transformation);

                this.tangents = tmp;
            }
            if (this.texcoords != null)
            {
                VertexAttribute<Vector2f> tmp = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.texcoords[i];
                if (geometry.texcoords != null) for (int i = 0; i < nverts2; i++) tmp[i + nverts1] = geometry.texcoords[i];
                this.texcoords = tmp;
            }
            if (this.colors != null)
            {
                VertexAttribute<Vector4b> tmp = new VertexAttribute<Vector4b>(DeclarationUsage.Color, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.colors[i];
                if (geometry.colors != null) for (int i = 0; i < nverts2; i++) tmp[i + nverts1] = geometry.colors[i];
                this.colors = tmp;
            }

            return nverts1 + nverts2;
        }

        /// <summary>
        /// filling entire vertestream with these vertices attributes
        /// </summary>
        public bool WriteAttibutes(VertexStream vertexstream)
        {
            if (vertices != null) vertexstream.WriteCollection<Vector3f>(vertices, 0, vertices.Count, 0);
            if (texcoords != null) vertexstream.WriteCollection<Vector2f>(texcoords, 0, texcoords.Count, 0);
            if (tangents != null) vertexstream.WriteCollection<Vector3f>(tangents, 0, tangents.Count, 0);
            if (normals != null) vertexstream.WriteCollection<Vector3f>(normals, 0, normals.Count, 0);
            if (colors != null) vertexstream.WriteCollection<Vector4b>(colors, 0, colors.Count, 0);
            return true;
        }
        /// <summary>
        /// filling entire indexstream with these indices attributes
        /// </summary>
        public abstract bool WriteAttributesIndex(IndexStream indexstream);


        #region Precalculated primitives
        /// <summary>
        /// Usefull for billboard, center in vector3(0,0,0) and with lenght = 1
        /// </summary>
        /// <param name="plane">the axes where quad look, example eAxis.Z for Z = 0</param>
        public static TriFanGeometry Billboard(eAxis lookaxe)
        {
            //     0______1
            //     | \_   |
            //     |   \_ |
            //     |_____\|
            //     3      2
            TriFanGeometry quad = new TriFanGeometry();
            quad.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, new Vector2f[]
            {
                new Vector2f(0,0),
                new Vector2f(1,0),
                new Vector2f(1,1),
                new Vector2f(0,1),
            });
            quad.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, new Vector4b[]
            {
                Color.White,
                Color.Red,
                Color.Green,
                Color.Blue
            });
            if ((lookaxe & eAxis.X) != 0)
            {
                // YZ plane
                quad.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[]
                {
                    new Vector3f(0,1,0),
                    new Vector3f(0,1,1),
                    new Vector3f(0,0,1),
                    new Vector3f(0,0,0)
                });
                quad.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, 4, new Vector3f(1, 0, 0));
            }
            else if ((lookaxe & eAxis.Y) != 0)
            {
                // XZ plane
                quad.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[]
                {
                    new Vector3f(0,0,0),
                    new Vector3f(0,0,1),
                    new Vector3f(1,0,1),
                    new Vector3f(1,0,0)
                });
                quad.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, 4, new Vector3f(0, 1, 0));
            }
            else if ((lookaxe & eAxis.Z) != 0)
            {
                // XY plane
                quad.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[]
                {
                    new Vector3f(0,1,0),
                    new Vector3f(1,1,0),
                    new Vector3f(1,0,0),
                    new Vector3f(0,0,0)
                });
                quad.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, 4, new Vector3f(0, 0, -1));
            }
            else
            {
                throw new ArgumentException("please give me a correct eAxis value");
            }

            return quad;
        }
        /// <summary>
        /// A simply wall with boundary values draw on XY plane, all Z = 0
        /// </summary>
        /// <param name="xsuddivision">if 1 is equal to a quad with 4 vertices</param>
        public static TriStripGeometry TessellatedRectangle(float minx, float miny, float maxx, float maxy, int xsuddivision)
        {
            if (xsuddivision < 1) throw new ArgumentOutOfRangeException("suddivision must be >= 1");

            TriStripGeometry mesh = new TriStripGeometry();

            int numverts = 2 * (xsuddivision + 1);

            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);
            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, numverts, new Vector3f(0, 0, -1));

            for (int i = 0; i <= xsuddivision; i++)
            {
                float dx = (float)i / xsuddivision;
                float x = minx + dx * (maxx - minx);

                mesh.vertices[i * 2 + 0] = new Vector3f(x, miny, 0);
                mesh.vertices[i * 2 + 1] = new Vector3f(x, maxy, 0);

                mesh.texcoords[i * 2 + 0] = new Vector2f(i % 2, 1);
                mesh.texcoords[i * 2 + 1] = new Vector2f(i % 2, 0);

                mesh.colors[i * 2 + 0] = (i % 2 == 0) ? Color.Red : Color.Blue;
                mesh.colors[i * 2 + 1] = (i % 2 == 0) ? Color.Red : Color.Blue;
            }

            return mesh;
        }
        /// <summary>
        /// A simply Triangle with 3 vertices
        /// </summary>
        public static TriListGeometry Triangle(Vector3f p0, Vector3f p1, Vector3f p2)
        {
            TriListGeometry mesh = new TriListGeometry();
            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[] { p0, p1, p2 });
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, new Vector4b[] { Color.Red, Color.Green, Color.Blue });
            mesh.boundSphere = Sphere.FromDataFast(mesh.vertices.data);
            return mesh;
        }
        
        /// <summary>
        /// A simply Tetrahedron with 4 vertices
        /// </summary>
        public static TriListGeometry Tetrahedron()
        {
            Vector3f p0 = new Vector3f(0, 1, 0);
            Vector3f p1 = new Vector3f(1, 0, 0);
            Vector3f p2 = new Vector3f(0, 0, -1);
            Vector3f p3 = new Vector3f(-1, 0, 0);
            Vector3f p4 = new Vector3f(0, 0, 1);

            TriListGeometry mesh = new TriListGeometry();

            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[]{
                p0,p1,p2,
                p0,p2,p3,
                p0,p3,p4,
                p0,p4,p1,
                p1,p3,p2,
                p1,p4,p3});

            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, mesh.vertices.Count);
            mesh.colors[0] = mesh.colors[1] = mesh.colors[2] = Color.Red;
            mesh.colors[3] = mesh.colors[4] = mesh.colors[5] = Color.Green;
            mesh.colors[6] = mesh.colors[7] = mesh.colors[8] = Color.Blue;
            mesh.colors[9] = mesh.colors[10] = mesh.colors[11] = Color.Cyan;
            mesh.colors[12] = mesh.colors[13] = mesh.colors[14] = Color.Yellow;
            mesh.colors[15] = mesh.colors[16] = mesh.colors[17] = Color.Magenta;

            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, mesh.vertices.Count);
            for (int i = 0; i < mesh.texcoords.Count; i += 3)
            {
                mesh.texcoords[i + 0] = new Vector2f(0.5f, 1);
                mesh.texcoords[i + 1] = new Vector2f(1, 0);
                mesh.texcoords[i + 2] = new Vector2f(0, 0);
            }

            mesh.CalculateNormal();

            mesh.boundSphere = Sphere.FromDataFast(mesh.vertices.data);

            return mesh;
        }
        /// <summary>
        /// A cube with 6 quads detached, usefull when use texture for each quad.
        /// </summary>
        public static MeshListGeometry CubeOpen()
        {
            MeshListGeometry mesh = new MeshListGeometry { name = "CubeOpen" };

            mesh.indices = new IndexAttribute<Face16>(12);
            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, 24);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, 24);
            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, 24);
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, 24);

            //front
            mesh.indices[0] = new Face16(3, 1, 0);
            mesh.indices[1] = new Face16(3, 0, 2);
            mesh.vertices[0] = new Vector3f(1, 1, 1);
            mesh.vertices[1] = new Vector3f(1, 1, -1);
            mesh.vertices[2] = new Vector3f(1, -1, 1);
            mesh.vertices[3] = new Vector3f(1, -1, -1);
            mesh.normals[0] = mesh.normals[1] = mesh.normals[2] = mesh.normals[3] = new Vector3f(1, 0, 0);
            mesh.colors[0] = mesh.colors[1] = mesh.colors[2] = mesh.colors[3] = Color.Red;
            //right
            mesh.indices[2] = new Face16(3, 1, 0) + 4;
            mesh.indices[3] = new Face16(3, 0, 2) + 4;
            mesh.vertices[4] = new Vector3f(1, 1, 1);
            mesh.vertices[5] = new Vector3f(1, -1, 1);
            mesh.vertices[6] = new Vector3f(-1, 1, 1);
            mesh.vertices[7] = new Vector3f(-1, -1, 1);
            mesh.normals[4] = mesh.normals[5] = mesh.normals[6] = mesh.normals[7] = new Vector3f(0, 0, 1);
            mesh.colors[4] = mesh.colors[5] = mesh.colors[6] = mesh.colors[7] = Color.Green;
            //back
            mesh.indices[4] = new Face16(3, 0, 1) + 8;
            mesh.indices[5] = new Face16(3, 2, 0) + 8;
            mesh.vertices[8] = new Vector3f(-1, 1, 1);
            mesh.vertices[9] = new Vector3f(-1, 1, -1);
            mesh.vertices[10] = new Vector3f(-1, -1, 1);
            mesh.vertices[11] = new Vector3f(-1, -1, -1);
            mesh.normals[8] = mesh.normals[9] = mesh.normals[10] = mesh.normals[11] = new Vector3f(-1, 0, 0);
            mesh.colors[8] = mesh.colors[9] = mesh.colors[10] = mesh.colors[11] = Color.Blue;
            //left
            mesh.indices[6] = new Face16(3, 0, 1) + 12;
            mesh.indices[7] = new Face16(3, 2, 0) + 12;
            mesh.vertices[12] = new Vector3f(1, 1, -1);
            mesh.vertices[13] = new Vector3f(1, -1, -1);
            mesh.vertices[14] = new Vector3f(-1, 1, -1);
            mesh.vertices[15] = new Vector3f(-1, -1, -1);
            mesh.normals[12] = mesh.normals[13] = mesh.normals[14] = mesh.normals[15] = new Vector3f(0, 0, -1);
            mesh.colors[12] = mesh.colors[13] = mesh.colors[14] = mesh.colors[15] = Color.Yellow;
            //top
            mesh.indices[8] = new Face16(3, 0, 1) + 16;
            mesh.indices[9] = new Face16(3, 2, 0) + 16;
            mesh.vertices[16] = new Vector3f(1, 1, 1);
            mesh.vertices[17] = new Vector3f(1, 1, -1);
            mesh.vertices[18] = new Vector3f(-1, 1, 1);
            mesh.vertices[19] = new Vector3f(-1, 1, -1);
            mesh.normals[16] = mesh.normals[17] = mesh.normals[18] = mesh.normals[19] = new Vector3f(0, 1, 0);
            mesh.colors[16] = mesh.colors[17] = mesh.colors[18] = mesh.colors[19] = Color.Magenta;
            //bottom
            mesh.indices[10] = new Face16(3, 1, 0) + 20;
            mesh.indices[11] = new Face16(3, 0, 2) + 20;
            mesh.vertices[20] = new Vector3f(1, -1, 1);
            mesh.vertices[21] = new Vector3f(1, -1, -1);
            mesh.vertices[22] = new Vector3f(-1, -1, 1);
            mesh.vertices[23] = new Vector3f(-1, -1, -1);
            mesh.normals[20] = mesh.normals[21] = mesh.normals[22] = mesh.normals[23] = new Vector3f(0, -1, 0);
            mesh.colors[20] = mesh.colors[21] = mesh.colors[22] = mesh.colors[23] = Color.Cyan;

            for (int i = 0; i < 6; i++)
            {
                mesh.texcoords[i * 4 + 0] = new Vector2f(0, 0);
                mesh.texcoords[i * 4 + 1] = new Vector2f(0, 1);
                mesh.texcoords[i * 4 + 2] = new Vector2f(1, 0);
                mesh.texcoords[i * 4 + 3] = new Vector2f(1, 1);
            }
            mesh.boundSphere.radius = (float)Math.Sqrt(3);
            return mesh;
        }
        /// <summary>
        /// A default cube with 8 vertices and 12 faces.
        /// </summary>
        /// <param name="w">size x</param>
        /// <param name="h">size y</param>
        /// <param name="l">size z</param>
        public static MeshListGeometry BoxClose(float sx,float sy,float sz)
        {
            //           5______4
            //           /     /|         Y
            //         1/_____/0|         |
            //          | 7   | /6        *---> Z
            //          |_____|/         /
            //         3      2         X

            MeshListGeometry cube = new MeshListGeometry { name = "CubeClose" };

            sx /= 2.0f;
            sy /= 2.0f;
            sz /= 2.0f;

            cube.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[]{
                new Vector3f( sx, sy, sz),
                new Vector3f( sx, sy,-sz),
                new Vector3f( sx,-sy, sz),
                new Vector3f( sx,-sy,-sz),
                new Vector3f(-sx, sy, sz),
                new Vector3f(-sx, sy,-sz),
                new Vector3f(-sx,-sy, sz),
                new Vector3f(-sx,-sy,-sz)});

            cube.indices = new IndexAttribute<Face16>(new Face16[]{
                //front
                new Face16(3,1,0),
                new Face16(3,0,2),
                //top
                new Face16(1,5,4),
                new Face16(1,4,0),
                //back
                new Face16(4,7,6),
                new Face16(4,5,7),
                //bottom
                new Face16(7,2,6),
                new Face16(7,3,2),
                //right
                new Face16(0,4,6),
                new Face16(0,6,2),
                //left
                new Face16(3,5,1),
                new Face16(3,7,5)});

            cube.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, new Vector4b[]{
                Color.Red,
                Color.Blue,
                Color.Green,
                Color.Magenta,
                Color.Cyan,
                Color.Yellow,
                Color.White,
                Color.Black});

            cube.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, (Vector3f[])cube.vertices.data.Clone());

            cube.boundSphere = new Sphere(Vector3f.Zero, (float)Math.Sqrt(3));

            return cube;
        }
        
        /// <summary>
        /// A default icosahedron with radius 1.0
        /// </summary>
        public static MeshListGeometry Icosahedron()
        {
            return GeoSphere(0);
        }
        /// <summary>
        /// A suddivided icosahedron with radius 1.0
        /// </summary>
        public static MeshListGeometry GeoSphere(int iterations)
        {
            MeshListGeometry mesh = new MeshListGeometry { name = "Icosa" };

            mesh.boundSphere = new Sphere(Vector3f.Zero, 1.0f);

            mesh.indices = new IndexAttribute<Face16>(new Face16[]{
                new Face16(1,4,0),
                new Face16(4,9,0),
                new Face16(4,5,9),
                new Face16(8,5,4),
                new Face16(1,8,4),
                new Face16(1,10,8),
                new Face16(10,3,8),
                new Face16(8,3,5),
                new Face16(3,2,5),
                new Face16(3,7,2),
                new Face16(3,10,7),
                new Face16(10,6,7),
                new Face16(6,11,7),
                new Face16(6,0,11),
                new Face16(6,1,0),
                new Face16(10,1,6),
                new Face16(11,0,9),
                new Face16(2,11,9),
                new Face16(5,2,9),
                new Face16(11,2,7)});

            float t = (float)(1 + Math.Sqrt(5.0)) / 2.0f;
            float s = (float)Math.Sqrt(1 + t * t);

            float X = 0.525731112119133606f;
            float Z = 0.850650808352039932f;

            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, new Vector3f[] {
			    new Vector3f(-X, 0, Z),
			    new Vector3f(X, 0, Z),
			    new Vector3f(-X, 0, -Z),
			    new Vector3f(X, 0, -Z),
			    new Vector3f(0, Z, X),
			    new Vector3f(0, Z, -X),
			    new Vector3f(0, -Z, X),
			    new Vector3f(0, -Z, -X),
			    new Vector3f(Z, X, 0),
			    new Vector3f(-Z, X, 0),
			    new Vector3f(Z, -X, 0),
			    new Vector3f(-Z, -X, 0) });

            for (int i = 0; i < iterations; i++)
                TriangleTessellatorTool.subdivide(ref mesh);

            int numvertices = mesh.vertices.Count;

            for (int i = 0; i < numvertices; i++)
                mesh.vertices[i] = mesh.vertices[i].Normal;

            return mesh;
        }       
        /// <summary>
        /// A classic sphere loke globe
        /// </summary>
        /// <param name="slices">verticals lines</param>
        /// <param name="stacks">orizontals lines</param>
        public static MeshListGeometry GlobeSphere(int slices, int stacks, float radius)
        {
            MeshListGeometry mesh = new MeshListGeometry { name = "Sphere" };

            int numVerticesPerRow = slices + 1;
            int numVerticesPerColumn = stacks + 1;

            int numverts = numVerticesPerRow * numVerticesPerColumn;
            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);
            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, numverts);

            float theta = 0.0f;
            float phi = 0.0f;
            float verticalAngularStride = (float)Math.PI / (float)stacks;
            float horizontalAngularStride = ((float)Math.PI * 2) / (float)slices;
            int i = 0;

            for (int ivertical = 0; ivertical < numVerticesPerColumn; ivertical++)
            {
                // beginning on top of the sphere:
                theta = ((float)Math.PI / 2.0f) - verticalAngularStride * ivertical;

                for (int ihorizontal = 0; ihorizontal < numVerticesPerRow; ihorizontal++)
                {
                    phi = horizontalAngularStride * ihorizontal;
                    float x = radius * (float)Math.Cos(theta) * (float)Math.Cos(phi);
                    float y = radius * (float)Math.Cos(theta) * (float)Math.Sin(phi);
                    float z = radius * (float)Math.Sin(theta);
                    mesh.vertices[i++] = new Vector3f(x, z, y);
                }
            }

            for (i = 0; i < numverts; i++)
            {
                mesh.normals[i] = mesh.vertices[i].Normal;
                mesh.colors[i] = Color.Blue;
            }


            int numfaces = slices * stacks * 2;
            i = 0;
            mesh.indices = new IndexAttribute<Face16>(numfaces);

            for (int verticalIt = 0; verticalIt < stacks; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    int lt = horizontalIt + verticalIt * (numVerticesPerRow);
                    int rt = (horizontalIt + 1) + verticalIt * (numVerticesPerRow);

                    int lb = horizontalIt + (verticalIt + 1) * (numVerticesPerRow);
                    int rb = (horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow);

                    mesh.indices[i++] = new Face16(lt, rt, lb);
                    mesh.indices[i++] = new Face16(rt, rb, lb);
                }
            }
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, GeometryTools.GetCilindralProjection(mesh.vertices.data, Matrix4x4f.Identity));
            mesh.boundSphere.radius = radius;

            return mesh;
        }

        /// <summary>
        /// A semi-globe-sphere
        /// </summary>
        /// <param name="slices">verticals lines</param>
        /// <param name="stacks">orizontals lines</param>
        public static MeshListGeometry Hemisphere(int slices, int stacks, float radius)
        {
            MeshListGeometry mesh = new MeshListGeometry { name = "Sphere" };

            int numVerticesPerRow = slices + 1;
            int numVerticesPerColumn = stacks + 1;

            int numverts = numVerticesPerRow * numVerticesPerColumn;
            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);
            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, numverts);

            float theta = 0.0f;
            float phi = 0.0f;
            float verticalAngularStride = (float)Math.PI / (float)stacks;
            float horizontalAngularStride = ((float)Math.PI * 2) / (float)slices;
            int i = 0;

            for (int ivertical = 0; ivertical < numVerticesPerColumn; ivertical++)
            {
                // beginning on top of the sphere:
                theta = ((float)Math.PI / 2.0f) - verticalAngularStride * ivertical;

                for (int ihorizontal = 0; ihorizontal < numVerticesPerRow; ihorizontal++)
                {
                    phi = horizontalAngularStride * ihorizontal;
                    float x = radius * (float)Math.Cos(theta) * (float)Math.Cos(phi);
                    float y = radius * (float)Math.Cos(theta) * (float)Math.Sin(phi);
                    float z = radius * (float)Math.Sin(theta);
                    mesh.vertices[i++] = new Vector3f(x, z, y);
                }
            }

            for (i = 0; i < numverts; i++)
            {
                mesh.normals[i] = mesh.vertices[i].Normal;
                mesh.colors[i] = Color.Blue;
            }


            int numfaces = slices * stacks * 2;
            i = 0;
            mesh.indices = new IndexAttribute<Face16>(numfaces);

            for (int verticalIt = 0; verticalIt < stacks; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    int lt = horizontalIt + verticalIt * (numVerticesPerRow);
                    int rt = (horizontalIt + 1) + verticalIt * (numVerticesPerRow);

                    int lb = horizontalIt + (verticalIt + 1) * (numVerticesPerRow);
                    int rb = (horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow);

                    mesh.indices[i++] = new Face16(lt, rt, lb);
                    mesh.indices[i++] = new Face16(rt, rb, lb);
                }
            }
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, GeometryTools.GetCilindralProjection(mesh.vertices.data, Matrix4x4f.Identity));
            mesh.boundSphere.radius = radius;

            return mesh;
        }

        /// <summary>
        /// Cone with Y peak , heigth = 1.0 , base circle in XZ plane with radius = 1.0  
        /// </summary>
        public static MeshListGeometry Cone(int basePoints)
        {
            float dang = (float)Math.PI * 2.0f / basePoints;
            int numverts = basePoints + 1;

            MeshListGeometry obj = new MeshListGeometry { name = "Cone" };

            obj.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            obj.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);
            obj.indices = new IndexAttribute<Face16>(basePoints * 2 - 2);

            // base vertices 
            for (int i = 0; i < numverts; i++)
            {
                obj.vertices[i] = new Vector3f((float)Math.Sin(dang * i), 0, (float)Math.Cos(dang * i));
                obj.colors[i] = Color.Blue;
            }
            // peak vertex
            obj.vertices[numverts - 1] = new Vector3f(0, 1, 0);

            // cone faces
            int f = 0;
            for (int i = 0; i < basePoints - 1; i++)
                obj.indices[f++] = new Face16(i, i + 1, numverts - 1);
            obj.indices[f++] = new Face16(basePoints - 1, 0, basePoints);

            // base faces
            for (int i = 0; i < basePoints - 2; i++)
                obj.indices[f++] = new Face16(0, i + 2, i + 1);

            obj.boundSphere = Sphere.FromDataFast(obj.vertices.data);

            return obj;
        }

        /// <summary>
        /// Cylinder with base center [0,0,0]
        /// </summary>
        public static MeshListGeometry Cylinder(float radius, float height, int slices)
        {
            MeshListGeometry mesh = new MeshListGeometry { name = "Cylinder" };

            int numverts = (slices + 1) * 2 + 2;
            float theta = 0.0f;
            float horizontalAngularStride = ((float)Math.PI * 2) / (float)slices;
            int i = 0;

            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);

            for (i = 0; i < numverts; i++) mesh.colors[i] = Color.Blue;

            i = 0;
            for (int verticalIt = 0; verticalIt < 2; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices + 1; horizontalIt++)
                {
                    float x;
                    float y;
                    float z;

                    theta = (horizontalAngularStride * horizontalIt);

                    if (verticalIt == 0)
                    {
                        // upper circle
                        x = radius * (float)Math.Cos(theta);
                        z = radius * (float)Math.Sin(theta);
                        y = height;

                    }
                    else
                    {
                        // lower circle
                        x = radius * (float)Math.Cos(theta);
                        z = radius * (float)Math.Sin(theta);
                        y = 0;
                    }
                    mesh.vertices[i++] = new Vector3f(x, y, z);
                }
            }
            mesh.vertices[i++] = new Vector3f(0, height, 0);
            mesh.vertices[i++] = new Vector3f(0, 0, 0);

            int nfaces = slices * 4;
            mesh.indices = new IndexAttribute<Face16>(nfaces);
            i = 0;
            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    ushort lt = (ushort)(horizontalIt + verticalIt * (slices + 1));
                    ushort rt = (ushort)((horizontalIt + 1) + verticalIt * (slices + 1));
                    ushort lb = (ushort)(horizontalIt + (verticalIt + 1) * (slices + 1));
                    ushort rb = (ushort)((horizontalIt + 1) + (verticalIt + 1) * (slices + 1));

                    mesh.indices[i++] = new Face16(lt, rt, lb);
                    mesh.indices[i++] = new Face16(rt, rb, lb);
                }
            }

            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    ushort lt = (ushort)(horizontalIt + verticalIt * (slices + 1));
                    ushort rt = (ushort)((horizontalIt + 1) + verticalIt * (slices + 1));

                    ushort patchIndexTop = (ushort)((slices + 1) * 2);
                    mesh.indices[i++] = new Face16(lt, patchIndexTop, rt);
                }
            }

            for (int verticalIt = 0; verticalIt < 1; verticalIt++)
            {
                for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
                {
                    ushort lb = (ushort)(horizontalIt + (verticalIt + 1) * (slices + 1));
                    ushort rb = (ushort)((horizontalIt + 1) + (verticalIt + 1) * (slices + 1));
                    ushort patchIndexBottom = (ushort)((slices + 1) * 2 + 1);
                    mesh.indices[i++] = new Face16(lb, rb, patchIndexBottom);
                }
            }

            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, GeometryTools.CalculateNormals(mesh.vertices.data, mesh.indices.data));
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, GeometryTools.GetCilindralProjection(mesh.vertices.data, Matrix4x4f.Identity));
            mesh.boundSphere.center = new Vector3f(0, height * 0.5f, 0);
            mesh.boundSphere.radius = (float)Math.Sqrt(radius * radius + height * height);
            return mesh;
        }

        /// <summary>
        /// A simply on XZ plane, all Y = 0
        /// </summary>
        /// <param name="xsuddivision"> if 1, plane is equal to a quad with 4 vertices</param>
        public static MeshListGeometry TessellatedPlane(float minx, float minz, float maxx, float maxz, int xsuddivision, int zsuddivision)
        {
            if (xsuddivision < 1 || zsuddivision<1) throw new ArgumentOutOfRangeException("suddivision must be >= 1");

            MeshListGeometry mesh = new MeshListGeometry();

            int numverts = (xsuddivision + 1) * (zsuddivision + 1);
            int numtris = xsuddivision * zsuddivision * 2;

            if (numverts > ushort.MaxValue - 1) throw new OverflowException("too many vertices for 16bit indices");

            mesh.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, numverts);
            mesh.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);
            mesh.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, numverts, new Vector3f(0, 0, -1));
            mesh.indices = new IndexAttribute<Face16>(numtris);

            for (int n = 0, j = 0; j <= zsuddivision; j++)
                for (int i = 0; i <= xsuddivision; i++)
                    mesh.vertices[n++] = new Vector3f(((float)i / xsuddivision) * (maxx - minx) + minx, 0, ((float)j / zsuddivision) * (maxz - minz) + minz);

            int col = xsuddivision+1;
            int row = zsuddivision+1;

            for (int n = 0, j = 0; j < zsuddivision; j++)
                for (int i = 0; i < xsuddivision; i++)
                {
                    mesh.indices[n++] = new Face16(i + j * col, i + (j + 1) * col, i + 1 + (j + 1) * col);
                    mesh.indices[n++] = new Face16(i + j * col, i + 1 + (j + 1) * col, i + 1 + (j * col));
                }

            return mesh;
        }

        #endregion
    }

    #region TRIANGLE FAN
    /// <summary>
    /// Primitives Triangles Fan geometries NOT-INDEXED
    /// </summary>
    public class TriFanGeometry : BaseTriGeometry
    {
        //    TRIANGLE-FAN
        //     [0]______1
        //      /|\    /
        //     / | \  /
        //    /__|__\/
        //   4   3   2
        // 0-1-2-3-4 in clockwire
        
        /// <summary>
        /// Empty PrimitiveType.TriangleFan
        /// </summary>
        public TriFanGeometry()
            : base()
        {
        }
        /// <summary>
        /// Copy vertex attribute
        /// </summary>
        /// <param name="tri"></param>
        public TriFanGeometry(BaseTriGeometry tri)
            : base(tri)
        {
        }
        public override bool IsIndexed
        {
            get { return false; }
        }
        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleFan; }
        }
        public override int numPrimitives
        {
            get { return (numVertices - 2); }
        }
        public override int numIndices
        {
            get { return 0; }
        }
        public override void GetTriangle(int primitive , out int i,out int j,out int k)
        {
            i = 0;
            j = primitive + 1;
            k = primitive + 2;
        }
        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            return false;
        }
    }
    /// <summary>
    /// Primitives Triangles Fan geometries INDEXED
    /// Implement degenerated triangle algorithm using indices attribute 
    /// </summary>
    public class MeshFanGeometry : BaseTriGeometry
    {
        public IndexAttribute<ushort> indices;
        ushort MainIndex { get { return indices[0]; } }

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshFanGeometry()
            : base()
        {
        }

        /// <summary>
        /// Convert to upper class, generate default indices
        /// </summary>
        public MeshFanGeometry(BaseTriGeometry tri)
            : base(tri)
        {
        }
        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleFan; }
        }
        public override bool IsIndexed
        {
            get { return true; }
        }
        public override int numPrimitives
        {
            get { return (indices != null || indices.Count < 3) ? indices.Count - 2 : 0; }
        }
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count : 0; }
        }

        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = 0;
            J = indices[primitive + 1];
            K = indices[primitive + 2];
        }
       
        /// <summary>
        /// Create the initial sequence that directx use to build triangles, same result not
        /// assigning to device.indices
        /// </summary>
        void initializeIndices()
        {
            indices = new IndexAttribute<ushort>(numVertices);
            for (ushort i = 0; i < numVertices; i++)
                indices[i] = i;
        }

        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            if (indices != null) indexstream.WriteCollection<ushort>(indices, 0, 0);
            return true;
        }

        /// <summary>
        /// Add a triangle strip mesh, to do this in directx9 i add 3 degenerate triangles.
        /// The restart index 0xFFFF are not supported by directx9
        /// </summary>
        /// <param name="showconnection">if false, make 3 degenerated faces to hide</param>
        public void Concatenate(MeshFanGeometry trifan, bool showconnection)
        {
            //      {5}        [0]_____1
            //      /|\        /|\    /
            //     / | \      / | \  /
            //    /__|__\    /__|__\/
            //   8   7   6  4   3   2
            // 
            // append a TriFanGeometry is not easy, the best way is append a new serie of vertices
            // so mainindex {5} will be eliminated
            // indices = 01234 + 0 + 678  and to faces .. 034 , 040 , 006 , 067 ...

            // RESULT ok, my ascii art is not perfect...
            //            ___ [0]_____1
            //         __/ /  /|\    /
            //     /  /  _/  / | \  /
            //    /__/__/   /__|__\/
            //   8  7   6   4   3   2

            int nverts1 = this.numVertices;
            int nverts2 = trifan.numVertices;

            // invalid strip do nothing
            if (nverts2 < 2) return;

            int nvertsTot = appendVertexAttribute(trifan);

            int nindis1 = this.indices.Count;
            int nindis2 = trifan.indices.Count;

            // resize array
            IndexAttribute<ushort> itmp = new IndexAttribute<ushort>(nindis1 + nindis2 + (showconnection ? 0 : 2));
            for (int i = 0; i < nindis1; i++) itmp[i] = this.indices[i];

            // add 2 degenerate triangles
            if (!showconnection)
            {
                itmp[nindis1++] = this.MainIndex;
            }

            // append new indices
            for (int i = 1; i < nindis2; i++)
                itmp[nindis1 + i - 1] = (ushort)(trifan.indices[i] + nverts1);

            this.indices = itmp;

            throw new NotImplementedException("Not Tested");
        }

        public static implicit operator MeshFanGeometry(TriFanGeometry trifan)
        {
            MeshFanGeometry mesh = new MeshFanGeometry(trifan);
            mesh.initializeIndices();
            return mesh;
        }

    }
    #endregion

    #region TRIANGLE STRIP
    /// <summary>
    /// Primitives Triangles Strip geometries NOT-INDEXED
    /// </summary>
    public class TriStripGeometry : BaseTriGeometry
    {
        //      TRIANGLE-STRIP
        //       1_____3____ 5
        //      /\    /\    /
        //     /  \  /  \  /
        //    /____\/____\/
        //   0    2      4
        //  notice the odd indices are down and even are up
        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleStrip; }
        }
        public override bool IsIndexed
        {
            get { return false; }
        }
        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = primitive;
            // and change the clock wire order for odd triangles
            if (primitive % 2 == 0)
            {
                J = primitive + 1;
                K = primitive + 2;
            }
            else
            {
                K = primitive + 1;
                J = primitive + 2;
            }
        }
        /// <summary>
        /// Number of all primitives renderer.
        /// </summary>
        public override int numPrimitives
        {
            get { return (numVertices - 2); }
        }
        public override int numIndices
        {
            get { return 0; }
        }
        /// <summary>
        /// Empty
        /// </summary>
        public TriStripGeometry()
            : base()
        {
        }
        /// <summary>
        /// Copy attributes
        /// </summary>
        public TriStripGeometry(BaseTriGeometry tri)
            : base(tri)
        {
        }
        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            return false;
        }
        /// <summary>
        /// List a Triangle Strip, will generate 2 new faces
        /// </summary>
        public void Concatenate(TriStripGeometry tristrip)
        {
            //       1_____3_____5_____7_____9
            //      /\    /\    /\    /\    /
            //     /  \  /  \     \  /  \  /
            //    /____\/____\/____\/____\/   
            //   0     2     4     6     8
            // 01234 + 56789
            // do 2 triangles : 345 , 456

            int nverts1 = this.numVertices;
            int nverts2 = tristrip.numVertices;

            // invalid strip do nothing
            if (nverts2 < 2) return;

            int nvertsTot = appendVertexAttribute(tristrip);
        }
    }

    /// <summary>
    /// Primitives Triangles Strip geometries INDEXED
    /// Implement degenerated triangle algorithm using indices attribute 
    /// </summary>
    public class MeshStripGeometry : BaseTriGeometry
    {
        //      TRIANGLE-STRIP
        //       1_____3____ 5
        //      /\    /\    /
        //     /  \  /  \  /
        //    /____\/____\/
        //   0     2      4
        //  indices = 012345 ; face made using truplet 012,123,234,... and flip oder for odd faces

        public IndexAttribute<ushort> indices;

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshStripGeometry()
            : base() 
        {
        }

        /// <summary>
        /// Convert to upper class, generate default indices
        /// </summary>
        public MeshStripGeometry(TriStripGeometry tri)
            : base(tri)
        {
            this.initializeIndices();
        }

        public override bool IsIndexed
        {
            get { return true; }
        }
        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        { 
            // device get triangle using indices ...
            I = indices[primitive];

            // and change the clock wire order for odd triangles
            if (primitive % 2 == 0)
            {
                J = indices[primitive + 1];
                K = indices[primitive + 2];
            }
            else
            {
                K = indices[primitive + 1];
                J = indices[primitive + 2];
            }
        }
        
        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleStrip; }
        }
        /// <summary>
        /// Number of all primitives renderer, i don't know if directx skip degenerate faces, but is the
        /// correct value to pass to DrawPrimitive()
        /// </summary>
        public override int numPrimitives
        {
            get { return (indices != null || indices.Count < 3) ? indices.Count - 2 : 0; }
        }
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count : 0; }
        }
        /// <summary>
        /// Write all indices to unmanaged buffer (like the graphic buffer)
        /// </summary>
        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            if (indices != null) indexstream.WriteCollection<ushort>(indices, 0, 0);
            return true;
        }
        /// <summary>
        /// Create the initial sequence that directx use to build triangles, same result not
        /// assigning to device.indices
        /// </summary>
        void initializeIndices()
        {
            indices = new IndexAttribute<ushort>(numVertices);
            for (ushort i = 0; i < numVertices; i++)
                indices[i] = i;
        }
        
        /// <summary>
        /// Add a triangle strip mesh, to do this in directx9 i add 3 degenerate triangles.
        /// The restart index 0xFFFF are not supported by directx9
        /// </summary>
        /// <param name="showconnection">if false, make 3 degenerated faces to hide</param>
        public void Concatenate(MeshStripGeometry tristrip, bool showconnection)
        {
            //       1_____3 ..... 5_____7_____9
            //      /\    /\     .  \    /\    /
            //     /  \  /  \   .    \  /  \  /
            //    /____\/____\ ...... \/____\/     
            //   0     2     4        6     8
            // 01234 + 45 + 56789
            // do 4 generate triangles : 344 , 445 , 455 , 556

            //       1_____3 ----5_____7_____9
            //      /\    /\    , \    /\    /
            //     /  \  /  \  ,   \  /  \  /
            //    /____\/____\,-----\/____\/     
            //   0     2     4        6     8
            // 01234 + 56789
            // do 2 triangles : 345 , 456

            int nverts1 = this.numVertices;
            int nverts2 = tristrip.numVertices;

            // invalid strip do nothing
            if (nverts2 < 2) return;

            int nvertsTot = appendVertexAttribute(tristrip);

            int nindis1 = this.indices.Count;
            int nindis2 = tristrip.indices.Count;

            // resize array
            IndexAttribute<ushort> itmp = new IndexAttribute<ushort>(nindis1 + nindis2 + (showconnection ? 0 : 2));
            for (int i = 0; i < nindis1; i++) itmp[i] = this.indices[i];

            // add degenerate triangles
            if (!showconnection)
            {
                itmp[nindis1++] = (ushort)(nverts1 - 1);
                itmp[nindis1++] = (ushort)nverts1;
            }

            // append new indices
            for (int i = 0; i < nindis2; i++)
                itmp[nindis1 + i] = (ushort)(tristrip.indices[i] + nverts1);

            this.indices = itmp;
        }
        /// <summary>
        /// Add a triangle strip mesh, to do this in directx9 i add 3 degenerate triangles.
        /// The restart index 0xFFFF are not supported by directx9
        /// </summary>
        /// <param name="showconnection">if false, make 3 degenerated faces to hide</param>
        public void Concatenate(TriStripGeometry tristrip, bool showconnection)
        {
            //       1_____3 ..... 5_____7_____9
            //      /\    /\     .  \    /\    /
            //     /  \  /  \   .    \  /  \  /
            //    /____\/____\ ...... \/____\/     
            //   0     2     4        6     8
            // 01234 + 45 + 56789
            // do 4 generate triangles : 344 , 445 , 455 , 556

            //       1_____3 ----5_____7_____9
            //      /\    /\    , \    /\    /
            //     /  \  /  \  ,   \  /  \  /
            //    /____\/____\,-----\/____\/     
            //   0     2     4        6     8
            // 01234 + 56789
            // do 2 triangles : 345 , 456

            int nverts1 = this.numVertices;
            int nverts2 = tristrip.numVertices;

            // invalid strip do nothing
            if (nverts2 < 2) return;

            int nvertsTot = appendVertexAttribute(tristrip);

            int nindis1 = this.indices.Count;

            // resize array
            IndexAttribute<ushort> itmp = new IndexAttribute<ushort>(nindis1 + nverts2 + (showconnection ? 0 : 2));
            for (int i = 0; i < nindis1; i++) itmp[i] = this.indices[i];

            // add degenerate triangles
            if (!showconnection)
            {
                itmp[nindis1++] = (ushort)(nverts1 - 1);
                itmp[nindis1++] = (ushort)nverts1;
            }

            // append new indices
            for (int i = 0; i < nverts2; i++)
                itmp[nindis1 + i] = (ushort)(i + nverts1);

            this.indices = itmp;
        }
        /// <summary>
        /// A not-indexed tristrip can be converted in indexed version, but not viceversa because 
        /// the degenerated faces aren't supported
        /// </summary>
        public static implicit operator MeshStripGeometry(TriStripGeometry generic)
        {
            MeshStripGeometry mesh = new MeshStripGeometry(generic);
            return mesh;
        }
    }
    #endregion

    #region TRIANGLE LIST
    /// <summary>
    /// Primitives Triangles List geometries NOT-INDEXED
    /// All triangles type can be collapsed to this
    /// </summary>
    public class TriListGeometry : BaseTriGeometry
    {    
        //   TRIANGLE-LIST
        //   0______1   3        
        //    \    /   /\  
        //     \  /   /  \ 
        //      \/   /____\ 
        //      2   5      4 

        /// <summary>
        /// Empty
        /// </summary>
        public TriListGeometry()
            : base()
        {
        }

        /// <summary>
        /// Copy attributes
        /// </summary>
        public TriListGeometry(BaseTriGeometry src)
            : base(src)
        {
        }
        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleList; }
        }

        public override bool IsIndexed
        {
            get { return false; }
        }
        /// <summary>
        /// a not-indexed geometry have always a primitives count
        /// </summary>
        public override int numPrimitives
        {
            get { return (numVertices / 3); }
        }
        public override int numIndices
        {
            get { return 0; }
        }
        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = primitive * 3;
            J = I + 1;
            K = J + 1;
        }
        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            return false;
        }

        #region Down casting of triangles primitives
        public static implicit operator TriListGeometry(TriFanGeometry generic)
        {
            TriListGeometry trilist = new TriListGeometry();
            trilist.convertcommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListGeometry(TriStripGeometry generic)
        {
            TriListGeometry trilist = new TriListGeometry();
            trilist.convertcommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListGeometry(MeshFanGeometry generic)
        {
            TriListGeometry trilist = new TriListGeometry();
            trilist.convertcommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListGeometry(MeshStripGeometry generic)
        {
            TriListGeometry trilist = new TriListGeometry();
            trilist.convertcommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListGeometry(MeshListGeometry generic)
        {
            TriListGeometry trilist = new TriListGeometry();
            trilist.convertcommonformat(generic);
            return trilist;
        }
        void convertcommonformat(BaseTriGeometry src)
        {
            List<Face16> triangles = new List<Face16>(src.numPrimitives);

            for (int i = 0; i < src.numPrimitives; i++)
            {
                int I, J, K;
                src.GetTriangle(i, out I, out J, out K);
                Face16 tris = new Face16(I, J, K);
                if (!tris.Degenerated) triangles.Add(tris);
            }

            int nverts = triangles.Count * 3;

            this.vertices = new VertexAttribute<Vector3f>(DeclarationUsage.Position, nverts);

            int iv = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                Face16 tris = triangles[i];
                this.vertices[iv++] = src.vertices[tris.x];
                this.vertices[iv++] = src.vertices[tris.y];
                this.vertices[iv++] = src.vertices[tris.z];
            }

            if (src.normals != null)
            {
                iv = 0;
                this.normals = new VertexAttribute<Vector3f>(DeclarationUsage.Normal, nverts);
                for (int i = 0; i < triangles.Count; i++)
                {
                    Face16 tris = triangles[i];
                    this.normals[iv++] = src.normals[tris.x];
                    this.normals[iv++] = src.normals[tris.y];
                    this.normals[iv++] = src.normals[tris.z];
                }
            }
            if (src.tangents != null)
            {
                iv = 0;
                this.tangents = new VertexAttribute<Vector3f>(DeclarationUsage.Tangent, nverts);
                for (int i = 0; i < triangles.Count; i++)
                {
                    Face16 tris = triangles[i];
                    this.tangents[iv++] = src.tangents[tris.x];
                    this.tangents[iv++] = src.tangents[tris.y];
                    this.tangents[iv++] = src.tangents[tris.z];
                }
            }
            if (src.texcoords != null)
            {
                iv = 0;
                this.texcoords = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, nverts);
                for (int i = 0; i < triangles.Count; i++)
                {
                    Face16 tris = triangles[i];
                    this.texcoords[iv++] = src.texcoords[tris.x];
                    this.texcoords[iv++] = src.texcoords[tris.y];
                    this.texcoords[iv++] = src.texcoords[tris.z];
                }
            }
            if (src.colors != null)
            {
                iv = 0;
                this.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, nverts);
                for (int i = 0; i < triangles.Count; i++)
                {
                    Face16 tris = triangles[i];
                    this.colors[iv++] = src.colors[tris.x];
                    this.colors[iv++] = src.colors[tris.y];
                    this.colors[iv++] = src.colors[tris.z];
                }
            }

        }
        #endregion
    }

    /// <summary>
    /// Primitives Triangles List geometries INDEXED
    /// All triangles type can be collapsed to this
    /// </summary>
    public class MeshListGeometry : BaseTriGeometry
    {
        public IndexAttribute<Face16> indices;

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshListGeometry()
            : base()
        {

        }

        /// <summary>
        /// Copy attributes
        /// </summary>
        public MeshListGeometry(BaseTriGeometry src)
            : base(src)
        {

        }
        /// <summary>
        /// Duplicate same type
        /// </summary>
        public MeshListGeometry(MeshListGeometry src)
            : base(src)
        {

            indices = src.indices;
        }

        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleList; }
        }
        public override bool IsIndexed
        {
            get { return true; }
        }
        public override int numPrimitives
        {
            get { return (indices != null) ? indices.Count : 0; }
        }
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count * 3 : 0; }
        }
        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = indices[primitive].x;
            J = indices[primitive].y;
            K = indices[primitive].z;
        }

        /// <summary>
        /// Write all indices to unmanaged buffer (like the graphic buffer)
        /// </summary>
        public override bool WriteAttributesIndex(IndexStream indexstream)
        {
            if (indices != null) indexstream.WriteCollection<Face16>(indices, 0, 0);
            return true;
        }

        #region Down casting of triangles primitives

        /// <summary>
        /// Can convert any type of triangles geometry in this, with indices preserve the
        /// conectivity
        /// </summary>
        public static implicit operator MeshListGeometry(TriListGeometry generic)
        {
            //copy vertexattribute
            MeshListGeometry mesh = new MeshListGeometry((BaseTriGeometry)generic);
            mesh.convertcommonformat(generic);
            return mesh;
        }
        /// <summary>
        /// Can convert any type of triangles geometry in this, with indices preserve the
        /// conectivity
        /// </summary>
        public static implicit operator MeshListGeometry(TriStripGeometry generic)
        {
            //copy vertexattributes
            MeshListGeometry mesh = new MeshListGeometry((BaseTriGeometry)generic);
            mesh.convertcommonformat(generic);
            return mesh;
        }
        /// <summary>
        /// Can convert any type of triangles geometry in this, with indices preserve the
        /// conectivity
        /// </summary>
        public static implicit operator MeshListGeometry(TriFanGeometry generic)
        {
            //copy vertexattributes
            MeshListGeometry mesh = new MeshListGeometry((BaseTriGeometry)generic);
            mesh.convertcommonformat(generic);
            return mesh;
        }
        /// <summary>
        /// Decode trianglestrip and trianglefan primitives in the default face's format, and remove
        /// degenerated case for a better conversion
        /// </summary>
        void convertcommonformat(BaseTriGeometry src)
        {
            // build indices , the getTriangle methods return indices used for TriangleList
            if (src.numPrimitives > 0)
            {
                List<Face16> faces = new List<Face16>(src.numPrimitives);
                int I, J, K;

                for (int i = 0; i < src.numPrimitives; i++)
                {
                    src.GetTriangle(i, out I, out J, out K);
                    Face16 f = new Face16(I, J, K);
                    if (!f.Degenerated) faces.Add(f);
                }
                this.indices = new IndexAttribute<Face16>(faces);
            }
        }
        #endregion

        /// <summary>
        /// Append vertices and faces attribute
        /// </summary>
        public void Concatenate(MeshListGeometry mesh)
        {
            int nverts1 = this.numVertices;
            int nverts2 = mesh.numVertices;
            int nindis1 = this.indices.Count;
            int nindis2 = mesh.indices.Count;

            if (nverts1 + nverts2 > ushort.MaxValue - 1)
                throw new OverflowException("the sum of two mesh generate a overflow in the faces, you need to seriously consider the option of using 32 bit indices");

            base.appendVertexAttribute(mesh);

            IndexAttribute<Face16> faces = new IndexAttribute<Face16>(nindis1 + nindis2);
            for (int i = 0; i < nindis1; i++)
                faces[i] = this.indices[i];

            for (int i = 0; i < nindis2; i++)
                faces[i + nindis1] = mesh.indices[i] + nverts1;

            this.indices = faces;
        }




    }
    #endregion

    /// <summary>
    /// With the standard mesh format instead half-edge format is more difficult tesselate
    /// TODO : optimization
    /// </summary>
    public static class TriangleTessellatorTool
    {
        // cache of midpoint indices, used only once at time
        static Dictionary<int, int> midpointIndices = new Dictionary<int, int>();

        public static void subdivide(ref MeshListGeometry mesh)
        {
            midpointIndices.Clear();

            List<Face16> indexList = new List<Face16>(mesh.indices.Count);
            List<Vector3f> vertexList = new List<Vector3f>(mesh.vertices.data);

            int numface = mesh.indices.Count;

            // subdivide each triangle
            for (int i = 0; i < numface; i++)
            {
                // grab indices of triangle
                Face16 f = mesh.indices[i];
                ushort i0 = f.x;
                ushort i1 = f.y;
                ushort i2 = f.z;

                // calculate new indices
                int m01 = getMidpointIndex(ref vertexList, i0, i1);
                int m12 = getMidpointIndex(ref vertexList, i1, i2);
                int m02 = getMidpointIndex(ref vertexList, i2, i0);

                indexList.Add(new Face16(i0, m01, m02));
                indexList.Add(new Face16(i1, m12, m01));
                indexList.Add(new Face16(i2, m02, m12));
                indexList.Add(new Face16(m02, m01, m12));
            }

            // save
            mesh.indices.data = indexList.ToArray();
            mesh.vertices.data = vertexList.ToArray();
        }
        
        static int getMidpointIndex(ref List<Vector3f> vertices, ushort i0, ushort i1)
        {
            // create a unique key, work only for 16 bit indices
            int edgekey = i1 > i0 ? i1 << 16 + i0 : i0 << 16 + i1;

            int midpointIndex = -1;

            // if there is not index already...
            if (!midpointIndices.TryGetValue(edgekey, out midpointIndex))
            {
                Vector3f midpoint = (vertices[i0] + vertices[i1]) * 0.5f;

                midpointIndex = vertices.IndexOf(midpoint);

                if (midpointIndex < 0)
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                }
            }
            return midpointIndex;
        }
    }
}

#endif