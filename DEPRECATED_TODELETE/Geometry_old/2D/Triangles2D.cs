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
    /// Abstract class, used to derive all similar Primitives Triangles, in 2D space
    /// </summary>
    public abstract class BaseTriPolygon : BaseGeometry2D
    {
        public VertexAttribute<Vector2f> vertices;
        public VertexAttribute<Vector2f> textures;
        public VertexAttribute<Vector4b> colors;
        public eAxis normal;

        public override int numVertices { get { return (vertices != null) ? vertices.Count : 0; } }

        public BaseTriPolygon()
            : base()
        {
            vertices = null;
            textures = null;
            colors = null;
        }
        
        /// <summary>
        /// Copy base data
        /// </summary>
        public BaseTriPolygon(BaseGeometry2D src)
            : base(src) { }        
        
        /// <summary>
        /// Copy attributes
        /// </summary>
        public BaseTriPolygon(BaseTriPolygon src)
            : base(src)
        {
            vertices = src.vertices;
            textures = src.textures;
            colors = src.colors;
        }
        
        /// <summary>
        /// Get the vertex indices of this primitive
        /// </summary>
        public abstract void GetTriangle(int primitive, out int I, out int J, out int K);

        /// <summary>
        /// append vertices attribute from other geometries, this geometry have the priority about 
        /// existing attribute, if source geometry don't have attribute but this yes, populate missing data with empty values
        /// </summary>
        protected int appendVertexAttribute(BaseTriPolygon geometry)
        {
            int nverts1 = this.numVertices;
            int nverts2 = geometry.numVertices;

            // vertices must not be null
            if (this.vertices != null)
            {
                VertexAttribute<Vector2f> tmp = new VertexAttribute<Vector2f>(DeclarationUsage.Position, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.vertices[i];
                for (int i = 0; i < nverts2; i++) tmp[i + nverts1] = geometry.vertices[i];
                this.vertices = tmp;
            }
            if (this.textures != null)
            {
                VertexAttribute<Vector2f> tmp = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, nverts1 + nverts2);
                for (int i = 0; i < nverts1; i++) tmp[i] = this.textures[i];
                if (geometry.textures != null) for (int i = 0; i < nverts2; i++) tmp[i + nverts1] = geometry.textures[i];
                this.textures = tmp;
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

    }


    #region TRIANGLE FAN
    /// <summary>
    /// Primitives Triangles Fan geometries NOT-INDEXED
    /// </summary>
    public class TriFanPolygon : BaseTriPolygon
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
        public TriFanPolygon()
            : base()
        { }
        /// <summary>
        /// Copy vertex attribute
        /// </summary>
        /// <param name="tri"></param>
        public TriFanPolygon(BaseTriPolygon tri)
            : base(tri)
        { }

        public override PrimitiveType primitive
        {
            get { return PrimitiveType.TriangleFan; }
        }
        public override bool IsIndexed
        {
            get { return false; }
        }
        public override int numPrimitives
        {
            get { return (numVertices - 2); }
        }
        public override void GetTriangle(int primitive, out int i, out int j, out int k)
        {
            i = 0;
            j = primitive + 1;
            k = primitive + 2;
        }
        public override int numIndices
        {
            get { return 0; }
        }
        /// <summary>
        /// </summary>
        public static TriFanPolygon Quad()
        {

            //     0______1
            //     | \_   |
            //     |   \_ |
            //     |_____\|
            //     3      2

            TriFanPolygon quad = new TriFanPolygon();

            quad.textures = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, new Vector2f[]
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
            quad.vertices = new VertexAttribute<Vector2f>(DeclarationUsage.Position, new Vector2f[]
            {
                new Vector2f(0,1),
                new Vector2f(1,1),
                new Vector2f(1,0),
                new Vector2f(0,0)
            });
            return quad;
        }
        /// <summary>
        /// Circle
        /// </summary>
        /// <param name="slices">num or suddivision, must be &gt; 3</param>
        public static TriFanPolygon Circle(float radius, int slices)
        {
            TriFanPolygon circle = new TriFanPolygon { name = "Circle" };
            circle.vertices = new VertexAttribute<Vector2f>(DeclarationUsage.Position, slices);
            circle.textures = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, slices);            
            for (int i = 0; i < slices; i++)
            {
                double ang = Math.PI * 2.0 / i;
                float x = (float)Math.Cos(ang);
                float y = (float)Math.Sin(ang);
                circle.vertices[i] = new Vector2f(x * radius, y * radius);
                circle.textures[i] = new Vector2f(x + 1.0f, y + 1.0f);
            }
            return circle;
        }
    }
    /// <summary>
    /// Primitives Triangles Fan geometries INDEXED
    /// Implement degenerated triangle algorithm using indices attribute 
    /// </summary>
    public class MeshFanPolygon : BaseTriPolygon
    {
        public IndexAttribute<ushort> indices;
        ushort MainIndex { get { return indices[0]; } }

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshFanPolygon()
            : base()
        {
        }
        /// <summary>
        /// Convert to upper class, generate default indices
        /// </summary>
        public MeshFanPolygon(BaseTriPolygon tri)
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
        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = 0;
            J = indices[primitive + 1];
            K = indices[primitive + 2];
        }  
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count : 0; }
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
        /// Write all indices to unmanaged buffer (like the graphic buffer)
        /// </summary>
        /// <param name="buffer">the destination buffer</param>
        /// <param name="bufferSize">size in bytes of destination buffer</param>
        /// <param name="bufferOffset">offset in bytes of destination buffer where write beginning</param>
        /// <param name="bufferInfo">descriptor of buffer element to understand indices conversion</param>
        /// <param name="IndexOffset">an usefull value to sum for each indices when use batch algorithm</param>
        /// <returns>return false if found some error</returns>
        public bool WriteIndicesToBuffers(IntPtr buffer, IndexLayout bufferInfo, int bufferSize, int bufferOffset, int IndexOffset = 0)
        {
            if (indices != null) indices.WriteToBuffers(buffer, bufferInfo, bufferSize, bufferOffset, IndexOffset);
            return true;
        }

        /// <summary>
        /// Add a triangle strip mesh, to do this in directx9 i add 3 degenerate triangles.
        /// The restart index 0xFFFF are not supported by directx9
        /// </summary>
        /// <param name="showconnection">if false, make 3 degenerated faces to hide</param>
        public void Concatenate(MeshFanPolygon trifan, bool showconnection)
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

        public static implicit operator MeshFanPolygon(TriFanPolygon trifan)
        {
            MeshFanPolygon mesh = new MeshFanPolygon(trifan);
            mesh.initializeIndices();
            return mesh;
        }
    }
    #endregion

    #region TRIANGLE STRIP
    /// <summary>
    /// Primitives Triangles Strip geometries NOT-INDEXED
    /// </summary>
    public class TriStripPolygon : BaseTriPolygon
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
                K = primitive + 1;
                J = primitive + 2;
            }
            else
            {
                J = primitive + 1;
                K = primitive + 2;
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
        public TriStripPolygon()
            : base()
        {
        }
        /// <summary>
        /// Copy attributes
        /// </summary>
        public TriStripPolygon(BaseTriPolygon tri)
            : base(tri)
        {
        }
        /// <summary>
        /// List a Triangle Strip, will generate 2 new faces
        /// </summary>
        public void Concatenate(TriStripPolygon tristrip)
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

        /// <summary>
        /// A simply wall with boundary values draw on XY plane, all Z = 0
        /// </summary>
        /// <param name="xsuddivision">if 1 is equal to a quad with 4 vertices</param>
        public static TriStripPolygon TessellatedRectangle(float minx, float miny, float maxx, float maxy, int xsuddivision)
        {
            if (xsuddivision < 1) throw new ArgumentOutOfRangeException("suddivision must be >= 1");

            TriStripPolygon mesh = new TriStripPolygon();

            int numverts = 2 * (xsuddivision + 1);

            mesh.vertices = new VertexAttribute<Vector2f>(DeclarationUsage.Position, numverts);
            mesh.textures = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, numverts);
            mesh.colors = new VertexAttribute<Vector4b>(DeclarationUsage.Color, numverts);

            for (int i = 0; i <= xsuddivision; i++)
            {
                float dx = (float)i / xsuddivision;
                float x = minx + dx * (maxx - minx);

                mesh.vertices[i * 2 + 0] = new Vector2f(x, miny);
                mesh.vertices[i * 2 + 1] = new Vector2f(x, maxy);

                mesh.textures[i * 2 + 0] = new Vector2f(i % 2, 1);
                mesh.textures[i * 2 + 1] = new Vector2f(i % 2, 0);

                mesh.colors[i * 2 + 0] = (i % 2 == 0) ? Color.Red : Color.Blue;
                mesh.colors[i * 2 + 1] = (i % 2 == 0) ? Color.Red : Color.Blue;
            }

            return mesh;
        }

    }

    /// <summary>
    /// Primitives Triangles Strip geometries INDEXED
    /// Implement degenerated triangle algorithm using indices attribute 
    /// </summary>
    public class MeshStripPolygon : BaseTriPolygon
    {
        //      TRIANGLE-STRIP
        //       1_____3____ 5
        //      /\    /\    /
        //     /  \  /  \  /
        //    /____\/____\/
        //   0    2      4
        //  indices = 012345 ; face made using truplet 012,123,234,... and flip oder for odd faces

        public IndexAttribute<ushort> indices;

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshStripPolygon()
            : base()
        {
        }
        /// <summary>
        /// Convert to upper class, generate default indices
        /// </summary>
        public MeshStripPolygon(TriStripPolygon tri)
            : base(tri)
        {
            this.initializeIndices();
        }
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count : 0; }
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
                K = indices[primitive + 1];
                J = indices[primitive + 2];
            }
            else
            {
                J = indices[primitive + 1];
                K = indices[primitive + 2];
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
        /// <summary>
        /// Write all indices to unmanaged buffer (like the graphic buffer)
        /// </summary>
        /// <param name="buffer">the destination buffer</param>
        /// <param name="bufferSize">size in bytes of destination buffer</param>
        /// <param name="bufferOffset">offset in bytes of destination buffer where write beginning</param>
        /// <param name="bufferInfo">descriptor of buffer element to understand indices conversion</param>
        /// <param name="IndexOffset">an usefull value to sum for each indices when use batch algorithm</param>
        /// <returns>return false if found some error</returns>
        public bool WriteIndicesToBuffers(IntPtr buffer, IndexLayout bufferInfo, int bufferSize, int bufferOffset, int IndexOffset = 0)
        {
            if (indices != null) indices.WriteToBuffers(buffer, bufferInfo, bufferSize, bufferOffset, IndexOffset);
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
        public void Concatenate(MeshStripPolygon tristrip, bool showconnection)
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
        public void Concatenate(TriStripPolygon tristrip, bool showconnection)
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
        public static implicit operator MeshStripPolygon(TriStripPolygon generic)
        {
            MeshStripPolygon mesh = new MeshStripPolygon(generic);
            return mesh;
        }
    }
    #endregion

    #region TRIANGLE LIST
    /// <summary>
    /// Primitives Triangles List geometries NOT-INDEXED
    /// All triangles type can be collapsed to this
    /// </summary>
    public class TriListPolygon : BaseTriPolygon
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
        public TriListPolygon()
            : base()
        {
        }

        /// <summary>
        /// Copy attributes
        /// </summary>
        public TriListPolygon(BaseTriPolygon src)
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


        #region Down casting of triangles primitives
        public static implicit operator TriListPolygon(TriFanPolygon generic)
        {
            TriListPolygon trilist = new TriListPolygon();
            trilist.convercommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListPolygon(TriStripPolygon generic)
        {
            TriListPolygon trilist = new TriListPolygon();
            trilist.convercommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListPolygon(MeshFanPolygon generic)
        {
            TriListPolygon trilist = new TriListPolygon();
            trilist.convercommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListPolygon(MeshStripPolygon generic)
        {
            TriListPolygon trilist = new TriListPolygon();
            trilist.convercommonformat(generic);
            return trilist;
        }
        public static implicit operator TriListPolygon(MeshListPolygon generic)
        {
            TriListPolygon trilist = new TriListPolygon();
            trilist.convercommonformat(generic);
            return trilist;
        }
        void convercommonformat(BaseTriPolygon src)
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

            this.vertices = new VertexAttribute<Vector2f>(DeclarationUsage.Position, nverts);

            int iv = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                Face16 tris = triangles[i];
                this.vertices[iv++] = src.vertices[tris.x];
                this.vertices[iv++] = src.vertices[tris.y];
                this.vertices[iv++] = src.vertices[tris.z];
            }
            if (src.textures != null)
            {
                iv = 0;
                this.textures = new VertexAttribute<Vector2f>(DeclarationUsage.TexCoord, nverts);
                for (int i = 0; i < triangles.Count; i++)
                {
                    Face16 tris = triangles[i];
                    this.textures[iv++] = src.textures[tris.x];
                    this.textures[iv++] = src.textures[tris.y];
                    this.textures[iv++] = src.textures[tris.z];
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
    public class MeshListPolygon : BaseTriPolygon
    {
        public IndexAttribute<Face16> indices;

        /// <summary>
        /// Emptry
        /// </summary>
        public MeshListPolygon()
            : base()
        { }
        /// <summary>
        /// Copy attributes
        /// </summary>
        public MeshListPolygon(BaseTriPolygon src)
            : base(src)
        { }
        /// <summary>
        /// Duplicate same type
        /// </summary>
        public MeshListPolygon(MeshListPolygon src)
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

        public override void GetTriangle(int primitive, out int I, out int J, out int K)
        {
            I = indices[primitive].x;
            J = indices[primitive].y;
            K = indices[primitive].z;
        }
        public override int numIndices
        {
            get { return (indices != null) ? indices.Count * 3 : 0; }
        }
        /// <summary>
        /// Write all indices to unmanaged buffer (like the graphic buffer)
        /// </summary>
        /// <param name="buffer">the destination buffer</param>
        /// <param name="bufferSize">size in bytes of destination buffer</param>
        /// <param name="bufferOffset">offset in bytes of destination buffer where write beginning</param>
        /// <param name="bufferInfo">descriptor of buffer element to understand indices conversion</param>
        /// <param name="IndexOffset">an usefull value to sum for each indices when use batch algorithm</param>
        /// <returns>return false if found some error</returns>
        public bool WriteIndicesToBuffers(IntPtr buffer, IndexLayout bufferInfo, int bufferSize, int bufferOffset, int IndexOffset = 0)
        {
            if (indices != null) indices.WriteToBuffers(buffer, bufferInfo, bufferSize, bufferOffset, IndexOffset);
            return true;
        }

        #region Down casting of triangles primitives

        /// <summary>
        /// Can convert any type of triangles geometry in this, with indices preserve the
        /// conectivity
        /// </summary>
        public static implicit operator MeshListPolygon(TriListPolygon generic)
        {
            //copy vertexattribute
            MeshListPolygon mesh = new MeshListPolygon((BaseTriPolygon)generic);
            mesh.convercommonformat(generic);
            return mesh;
        }
        /// <summary>
        /// Can convert any type of triangles geometry in this, with indices preserve the
        /// conectivity
        /// </summary>
        public static implicit operator MeshListPolygon(TriStripPolygon generic)
        {
            //copy vertexattributes
            MeshListPolygon mesh = new MeshListPolygon((BaseTriPolygon)generic);
            mesh.convercommonformat(generic);
            return mesh;
        }

        /// <summary>
        /// Decode trianglestrip and trianglefan primitives in the default face's format, and remove
        /// degenerated case for a better conversion
        /// </summary>
        void convercommonformat(BaseTriPolygon src)
        {
            // build indices , the getTriangle methods return indices used for TriangleList
            if (src.numPrimitives > 0)
            {
                List<Face16> faces = new List<Face16>(src.numPrimitives);
                for (int i = 0; i < src.numPrimitives; i++)
                {
                    int I, J, K;
                    GetTriangle(i, out I, out J, out K);
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
        public void Concatenate(MeshListPolygon mesh)
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

        /// <summary>
        /// Tessellated plane mesh, tipical for terrain, the plane is alligned with Y direction
        /// </summary>
        /// <param name="xsuddivision">1 = no suddivision, rectangle have 4 vertices and 2 faces</param>
        public static MeshListPolygon TessellatedRectangle(float minx, float miny, float maxx, float maxy, int xsuddivision, int ysuddivision)
        {
            MeshListPolygon mesh = new MeshListPolygon();

            if (xsuddivision < 1 || ysuddivision < 1)
                throw new ArgumentOutOfRangeException("suddivision must be > 0");

            int numverts = (xsuddivision + 1) * (ysuddivision + 1);
            int numtris = xsuddivision * ysuddivision * 2;
            int row = xsuddivision + 1;

            if (numverts > ushort.MaxValue - 1)
                throw new OverflowException("too many vertices for Face16bit");

            mesh.vertices = new VertexAttribute<Vector2f>(DeclarationUsage.Position, numverts);
            mesh.indices = new IndexAttribute<Face16>(numtris);

            int n = 0;
            int i = 0;
            for (int y = 0; y <= ysuddivision; y++)
            {
                float dy = y / (float)ysuddivision;
                float Y = miny + dy * (maxy - miny);

                for (int x = 0; x <= xsuddivision; x++)
                {
                    float dx = x / (float)xsuddivision;
                    float X = minx + dx * (maxx - minx);
                    mesh.vertices[n++] = new Vector2f(X, Y);
                }
            }

            n = 0;
            i = 0;
            for (int y = 0; y < ysuddivision; ++y)
            {
                for (int x = 0; x < xsuddivision; ++x)
                {
                    mesh.indices[i++] = new Face16(n + 1, n, row + (n + 1));
                    mesh.indices[i++] = new Face16(row + (n + 1), n, row + n);
                    n += 1;
                }
                n += 1;
            }
            return mesh;
        }
    }
    #endregion
}


#endif