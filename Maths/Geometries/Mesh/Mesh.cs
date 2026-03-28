using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;


namespace Common.Maths
{
    /// <summary>
    /// A simple mesh data structure with one list of vertices and a list of indices stored as one or more <see cref="SubMesh"/>
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract partial class Mesh
    {
        /// <summary>
        /// The signature define the class type, must be unique and it's used to stop reading if something wrong
        /// </summary>
        public readonly static long MeshSignature = BitConverterExt.ToInt64("MESH");

        /// <summary>
        /// Name of object
        /// </summary>
        public string Name;
        /// <summary>
        /// Base color of object
        /// </summary>
        public Color4b Diffuse;

        /// <summary>
        /// Define the type of geometry. A mesh class can be used also for Lines and Points geometry
        /// </summary>
        public Primitive Topology { get; protected set; } = Primitive.Undefined;

        public List<SubMesh> SubMeshes;

        public FileVersion Version = new FileVersion(1, 0, 1);

        /// <summary>
        /// For converting in Triangles or Lines counts it depends by <see cref="Topology"/>
        /// </summary>
        public int IndicesCount => SubMeshes?.Sum(x => x.IndincesCount) ?? 0;
        public int SubMeshCount => SubMeshes?.Count ?? 0;

        public int GetMaxIndex()
        {
            int max = int.MinValue;
            foreach (var submesh in SubMeshes)
                max = Math.Max(max, submesh.Indices.Max());
            return max;
        }
        public ushort[] GetIndexBuffer16()
        {
            ushort[] buffer = new ushort[IndicesCount];
            int offset = 0;
            unchecked
            {
                foreach (var submesh in SubMeshes)
                    foreach (var index in submesh.Indices)
                        buffer[offset++] = (ushort)index;
            }
            return buffer;
        }
        public int[] GetIndexBuffer32()
        {
            int[] buffer = new int[IndicesCount];
            int offset = 0;
            unchecked
            {
                foreach (var submesh in SubMeshes)
                    foreach (var index in submesh.Indices)
                        buffer[offset++] = index;
            }
            return buffer;
        }

        public Mesh(Primitive topology = Primitive.TriangleList, string name = "my_mesh")
        {
            Name = name;
            Topology = topology;
        }

        /// <summary>
        /// Submesh can be stored with a random order but it's better sort them by min-index value
        /// </summary>
        public void SortSubMesh()
        {
            if (SubMeshes == null) return;
            foreach (var sub in SubMeshes)
            {
                (int min, int max) = Mathelp.GetMinMax(sub.Indices);
                sub.FirstVertex = min;
            }
            SubMeshes.Sort((a, b) => b.FirstVertex.CompareTo(a.FirstVertex));
        }

        /// <summary>
        /// Create a <see cref="SubMesh"/> and add it into list
        /// </summary>
        public SubMesh AddSubMesh(int indexcapacity = 0, IndexFormat format = IndexFormat.Index32bit, string name = "my_submesh")
        {
            SubMesh sub = new SubMesh(this, indexcapacity, format, name);
            if (SubMeshes == null) SubMeshes = new List<SubMesh>();
            SubMeshes.Add(sub);
            return sub;
        }
        /// <summary>
        /// Copy a <see cref="SubMesh"/>
        /// </summary>
        public SubMesh AddSubMesh(SubMesh other)
        {
            SubMesh sub = new SubMesh(this, other);
            if (SubMeshes == null) SubMeshes = new List<SubMesh>();
            SubMeshes.Add(sub);
            return sub;
        }

        /// <summary>
        /// collapse all submeshes into one and return the class
        /// </summary>
        public SubMesh CollapseSubMeshes()
        {
            if (SubMeshCount > 0)
            {
                if (SubMeshCount > 1)
                {
                    int count = IndicesCount;
                    if (count <= 0) return null;

                    SortSubMesh();

                    SubMesh main = new SubMesh(this, count, IndexFormat.Index32bit, Name);

                    foreach (var sub in SubMeshes)
                        if (sub != null)
                            main.Indices.AddRange(sub.Indices);

                    SubMeshes = new List<SubMesh>(1) { main };
                }
                return SubMeshes[0];
            }
            return null;
        }


        #region Read Write
        /// <summary>
        /// Read <see cref="long"/> signature and <see cref="long"/> class size in byte, then restore the stream position.<br/>
        /// Can be use to check the data before start reading.
        /// </summary>
        public static (long signature, long bytesize) ReadHeaderAndBack(BinaryReader reader)
        {
            var pos = reader.BaseStream.Position;
            var signature = reader.ReadInt64();
            var bytesize = reader.ReadInt64();
            reader.BaseStream.Position = pos;
            return (signature, bytesize);
        }
        protected bool ReadSubMeshes(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count < 0) throw new Exception("SubMeshes count must be >= 0");
            if (count > 0)
            {
                SubMeshes = new List<SubMesh>(count);
                for (int i = 0; i < count; i++)
                {
                    var sub = SubMesh.Read(this, reader);
                    if (sub == null) return false;
                    SubMeshes.Add(sub);
                }
            }
            return true;
        }
        protected bool WriteSubMeshes(BinaryWriter writer, CompressionIndices compression_i)
        {
            writer.Write(SubMeshCount);
            foreach (var sub in SubMeshes)
                if (!sub.Write(writer, compression_i == CompressionIndices.Encoded7Bit)) return false;
            return true;
        }
        protected static class Packer
        {
            /// <summary>
            /// specified for a vector3 list
            /// </summary>
            public static void WriteVertices(BinaryWriter writer, IEnumerable<Vector3f> array, CompressionVertices compression)
            {
                int count = 0;
                if (array != null) count = array.Count();
                writer.Write(count);

                if (count < 1) return;
                writer.WriteByte((byte)compression);

                if (compression == CompressionVertices.HalfFloat)
                {
                    BoundingBoxMinMax bound = BoundingBoxMinMax.NaN;
                    foreach (var v in array) bound.Merge(v);
                    bound.Write(writer);

                    Vector3f size = bound.Size;
                    foreach (var v in array)
                    {
                        VectorHalf3 hv = (v - bound.min) / size;
                        hv.Write(writer);
                    }
                }
                else
                {
                    writer.Write(array);
                }
            }
            /// <summary>
            /// specified for a vector3 normalized vector
            /// </summary>
            public static void WriteNormals(BinaryWriter writer, IEnumerable<Vector3f> array, CompressionNormals compression)
            {
                int count = 0;
                if (array != null) count = array.Count();
                writer.Write(count);

                if (count < 1) return;
                writer.WriteByte((byte)compression);

                switch (compression)
                {
                    case CompressionNormals.Normals16:
                        foreach (var vector in array) writer.Write(UnitSphericalPacker16.Encode(vector)); break;
                    case CompressionNormals.Normals24:
                        foreach (var vector in array) writer.WriteUInt24(UnitSphericalPacker24.Encode(vector)); break;
                    case CompressionNormals.NormalsX15Y15Z1:
                        foreach (var vector in array) writer.Write(UnitVectorPacker32.EncodeX15Y15Z1(vector)); break;
                    default: writer.Write(array); break;
                }
            }
            /// <summary>
            /// specified for a vector2 uv
            /// </summary>
            public static void WriteTexCoords(BinaryWriter writer, IEnumerable<Vector2f> array, CompressionTexCoord compression)
            {
                int count = 0;
                if (array != null) count = array.Count();
                writer.Write(count);

                if (count < 1) return;
                writer.WriteByte((byte)compression);

                if (compression == CompressionTexCoord.HalfFloat)
                {
                    AABRminmax bound = AABRminmax.Empty;
                    foreach (var v in array) bound.Merge(v.x, v.y);
                    bound.Write(writer);
                    Vector2f size = bound.Size;

                    foreach (var v in array)
                    {
                        var hv = (v - bound.min) / size * ushort.MaxValue;
                        writer.Write((ushort)hv.x);
                        writer.Write((ushort)hv.y);
                    }
                }
                else writer.Write(array);
            }
            /// <summary>
            /// specified for rgba
            /// </summary>
            public static void WriteColor(BinaryWriter writer, IEnumerable<Color4b> array, CompressionColor compression)
            {
                int count = 0;
                if (array != null) count = array.Count();
                writer.Write(count);

                if (count < 1) return;
                writer.WriteByte((byte)compression);

                if (compression == CompressionColor.NoAlpha)
                {
                    foreach (var v in array)
                    {
                        writer.Write(v.r);
                        writer.Write(v.g);
                        writer.Write(v.b);
                    }
                }
                else writer.Write(array);
            }

            /// <summary>
            /// specified for a vector3 list
            /// </summary>
            public static StructBuffer<Vector3f> ReadVertices(BinaryReader reader)
            {
                int count = reader.ReadInt32();
                if (count < 1) return null;
                var comp = (CompressionVertices)reader.ReadByte();

                if (comp == CompressionVertices.HalfFloat)
                {
                    BoundingBoxMinMax bound = new BoundingBoxMinMax(reader);
                    Vector3f size = bound.Size;

                    StructBuffer<Vector3f> list = new StructBuffer<Vector3f>(count);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3f v = new VectorHalf3(reader);
                        v = v * size + bound.min;
                        list.Add(v);
                    }
                    return list;
                }
                else
                {
                    return new StructBuffer<Vector3f>(reader.ReadUnsafe<Vector3f>(count));
                }
            }
            /// <summary>
            /// specified for a vector2 uv
            /// </summary>
            public static StructBuffer<Vector2f> ReadTexCoords(BinaryReader reader)
            {
                int count = reader.ReadInt32();
                if (count < 1) return null;
                var comp = (CompressionTexCoord)reader.ReadByte();

                if (comp == CompressionTexCoord.HalfFloat)
                {
                    StructBuffer<Vector2f> list = new StructBuffer<Vector2f>(count);

                    AABRminmax bound = new AABRminmax(reader);
                    Vector2f size = bound.Size;

                    for (int i = 0; i < count; i++)
                    {
                        float x = (reader.ReadUInt16() * size.x / ushort.MaxValue) + bound.min.x;
                        float y = (reader.ReadUInt16() * size.y / ushort.MaxValue) + bound.min.y;
                        list.Add(new Vector2f(x, y));
                    }

                    return list;
                }
                else return new StructBuffer<Vector2f>(reader.ReadUnsafe<Vector2f>(count));
            }
            /// <summary>
            /// specified for a vector3 normalized vector
            /// </summary>
            public static StructBuffer<Vector3f> ReadNormals(BinaryReader reader)
            {
                int count = reader.ReadInt32();
                if (count < 1) return null;
                var comp = (CompressionNormals)reader.ReadByte();

                StructBuffer<Vector3f> array = new StructBuffer<Vector3f>(count);

                switch (comp)
                {
                    case CompressionNormals.Normals16:
                        for (int i = 0; i < count; i++) array.Add(UnitSphericalPacker16.Decode(reader.ReadUInt16()));
                        break;
                    case CompressionNormals.Normals24:
                        for (int i = 0; i < count; i++) array.Add(UnitSphericalPacker24.Decode(reader.ReadUInt24()));
                        break;
                    case CompressionNormals.NormalsX15Y15Z1:
                        for (int i = 0; i < count; i++) array.Add(UnitVectorPacker32.DecodeX15Y15Z1(reader.ReadUInt32()));
                        break;
                    default:
                        array.AddRange(reader.ReadUnsafe<Vector3f>(count));
                        break;
                }
                return array;
            }
            /// <summary>
            /// specified for rgba
            /// </summary>
            public static StructBuffer<Color4b> ReadColors(BinaryReader reader)
            {
                int count = reader.ReadInt32();
                if (count < 1) return null;
                var comp = (CompressionColor)reader.ReadByte();

                StructBuffer<Color4b> array = new StructBuffer<Color4b>(count);

                switch (comp)
                {
                    case CompressionColor.NoAlpha:
                        for (int i = 0; i < count; i++) array.Add(new Color4b(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255));
                        break;
                    default:
                        array.AddRange(reader.ReadUnsafe<Color4b>(count));
                        break;
                }

                return array;
            }




        }
        #endregion
    }
}
