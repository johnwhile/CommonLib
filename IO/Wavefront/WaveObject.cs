using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Common.Maths;

namespace Common.IO.Wavefront
{
    public enum WavePrimitive : byte
    {
        Unknow = 0,
        /// <summary>
        /// <inheritdoc cref="Primitive.Point"/>
        /// PointList will be converted in PointStrip
        /// </summary>
        Point = 1,
        /// <summary><inheritdoc cref="Primitive.LineList"/></summary>
        Line = 2,
        /// <summary><inheritdoc cref="Primitive.LineStrip"/></summary>
        LineStrip = 3,
        /// <summary><inheritdoc cref="Primitive.TriangleList"/></summary>
        Triangle = 4,
        [Obsolete("Need to test if Wavefront support it")]
        /// <summary><inheritdoc cref="Primitive.TriangleStrip"/></summary>
        TriangleStrip = 5,
        /// <summary><inheritdoc cref="Primitive.TriangleFan"/></summary>
        TriangleFan = 6,
        [Obsolete("not implemented")]
        Curve = 7,
        [Obsolete("not implemented")]
        Curve2d = 8,
        [Obsolete("not implemented")]
        Surface = 9
    }

    [Flags]
    public enum WaveVertexFormat : byte
    {
        None = 0,
        Vertex = 1,
        Normal = 2,
        TexCoord = 4,
        VertexNormal = Vertex | Normal,
        VertexTexcoord = Vertex | TexCoord,
        VertexTexcoordNormal = Vertex | Normal | TexCoord,
    }


    [DebuggerDisplay("{Name}")]
    /// <summary>
    /// Represents an independent object, with its own list of vertices, polygons, and groups
    /// </summary>
    public class WaveObject
    {
        internal static string[] topololyName = new string[] { "?", "p", "l", "l", "f", "f", "f", "curv", "curv2", "surf" };
        internal static int[] topologyDim = new int[] { 0, 1, 2, 2, 3, 3, 3, 0, 0, 0 };

        //mantain linked list reference
        internal WaveObject prev;
        internal WaveObject next;

        public readonly WavefrontObj File;
        public List<Vector3f> Vertices;
        public List<Vector3f> Normals;
        public List<Vector2f> TexCoords;
        public List<WaveGroup> Groups;

        public int VertsCount => Vertices?.Count ?? 0;
        public int TexCoordsCount => TexCoords?.Count ?? 0;
        public int NormalsCount => Normals?.Count ?? 0;

        public int GetTotalIndicesCount => Groups.Sum(x => x.indexV.Count);

        /// <summary>
        /// since it doesn't matter, the name is a unique string instead of an array of strings
        /// ATTENTION, if name is not assigned, will be not write the object's statements "o object_name"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name write in the comment section
        /// </summary>
        public string CommentName { get; set; }

        public readonly BoundingBoxMinMax Bound;


        internal Vector3i maxIndexUsed;
        internal Vector3i minIndexUsed;

        /// <summary>
        /// Offset of maximum index used, must be less or equal to vertices.count
        /// </summary>
        /// <remarks> x y z = v t n</remarks>
        public Vector3i MaxIndexUsed => maxIndexUsed;
        public Vector3i MinIndexUsed => minIndexUsed;
        
        internal bool isMinIndexOutOfLocalRange
        {
            get
            {
                return
                    minIndexUsed.x >= Vertices.Count ||
                    minIndexUsed.y >= TexCoords.Count ||
                    minIndexUsed.z >= Normals.Count;
            }
        }

        public void UpdateMinMaxIndexValue()
        {
            minIndexUsed = new Vector3i(int.MaxValue);
            maxIndexUsed = new Vector3i(-1);

            foreach (var g in Groups)
            {
                foreach(var v in g.indexV)
                {
                    minIndexUsed.x = Mathelp.MIN(minIndexUsed.x, v);
                    maxIndexUsed.x = Mathelp.MAX(maxIndexUsed.x, v);
                }
                foreach (var t in g.indexT)
                {
                    minIndexUsed.y = Mathelp.MIN(minIndexUsed.y, t);
                    maxIndexUsed.y = Mathelp.MAX(maxIndexUsed.y, t);
                }
                foreach (var n in g.indexN)
                {
                    minIndexUsed.z = Mathelp.MIN(minIndexUsed.z, n);
                    maxIndexUsed.z = Mathelp.MAX(maxIndexUsed.z, n);
                }
            }
        }

        public WaveGroup Create(WavePrimitive topology = WavePrimitive.Triangle, string name = null, string material = null)
        {
           return new WaveGroup(this, topology, name, material);
        }

        /// <summary>
        /// will be automatically added to waveObject list
        /// </summary>
        /// <param name="owner"></param>
        internal WaveObject(WavefrontObj owner, string name = null)
        {
            File = owner;
            Vertices = new List<Vector3f>();
            Normals = new List<Vector3f>();
            TexCoords = new List<Vector2f>();
            Groups = new List<WaveGroup>();
            Bound = new BoundingBoxMinMax();
            Name = name;

            if (owner.Objects.Count > 0)
            {
                prev = owner.Objects[owner.Objects.Count - 1];
                prev.next = this;
            }
            owner.Objects.Add(this);

            maxIndexUsed = new Vector3i(-1);
            minIndexUsed = new Vector3i(int.MaxValue);
        }

        /// <summary>
        /// Return the sum of vertices count for all previous <see cref="WaveObject"/>.
        /// It's used to fix the difference between <b>shared</b> and <b>not-shared</b> vertices indices version, <br/>
        /// see <see cref="WavefrontObj.UseGlobalShared"/>
        /// </summary>
        public int GetVertexIndexOffset()
        {
            int offset = 0;
            WaveObject current = prev;
            while (current!=null)
            {
                offset += current.Vertices.Count;
                current = current.prev;
            }
            return offset;
        }

        public void AddVertex(Vector3f vertex)
        {
            Vertices.Add(vertex);
            Bound.Merge(vertex);
        }
        public void AddVertex(IEnumerable<Vector3f> vertex)
        {
            Vertices.AddRange(vertex);
            foreach (var v in vertex) Bound.Merge(v);
        }



        /// <summary>
        /// Save this section to file
        /// </summary>
        public bool Write(StreamWriter writer)
        {
            string name = Name;
            if (name == null) name = "NULL";

            writer.WriteLine();
            writer.Write('#'); for (int i = 0; i < name.Length + 10; i++) writer.Write('-');
            writer.WriteLine(string.Format("\n# object {0}", CommentName !=null ? CommentName : name));
            writer.Write('#'); for (int i = 0; i < name.Length + 10; i++) writer.Write('-');
            writer.WriteLine('\n');
            
            if (Groups.Count == 0)
            {
                writer.WriteLine("# ERROR, THE GROUPS LIST IS EMPTY");
            }

            WaveVertexFormat format = WaveVertexFormat.None;

            if (VertsCount > 0)
            {
                foreach (Vector3f v in Vertices)
                    writer.WriteLine($"v {Wavefront.StringVector3(v)}");
                writer.WriteLine($"# {VertsCount} geometry Vertices");
                writer.WriteLine();
                format = WaveVertexFormat.Vertex;
            }
            else
            {
                File.Status = "ERROR Vertices list is empty";
                writer.WriteLine("# ERROR, THE VERTICES LIST IS EMPTY");
                return false;
            }

            if (TexCoordsCount > 0)
            {
                foreach (Vector2f t in TexCoords)
                    writer.WriteLine($"vt {Wavefront.StringVector2(t)}");
                writer.WriteLine($"# {TexCoordsCount} texture coords");
                writer.WriteLine();
                format |= WaveVertexFormat.TexCoord; //v + vt
            }

            if (NormalsCount > 0)
            {
                foreach (Vector3f n in Normals)
                    writer.WriteLine($"vn {Wavefront.StringVector3(n)}");
                writer.WriteLine($"# {NormalsCount} vertex Normals");
                writer.WriteLine();
                format |= WaveVertexFormat.Normal; //v + vn
            }

            if (Name != null) writer.WriteLine($"o {Name}");

            if (Groups.Count > 0)
            {
                foreach (WaveGroup g in Groups)
                    if (!g.Write(writer, format)) return false;
            }

            return true;
        }

        public override string ToString() => $"{Name} Groups:{Groups.Count}";
    }
}
