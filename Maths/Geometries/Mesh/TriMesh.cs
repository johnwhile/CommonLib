using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Common.IO;

namespace Common.Maths
{
    /// <summary>
    /// 3D triangle implementation
    /// </summary>
    public partial class TriMesh : Mesh, IBinarySerializable, IXmlSerializable
    {
        public CompressionTransform PreferedTransformComp;
        public CompressionIndices PreferedIndicesComp;
        public CompressionVertices PreferedVertsComp;
        public CompressionNormals PreferedNormsComp;
        public CompressionTexCoord PreferedTexCoordComp;
        public CompressionTangents PreferedTangentsComp;


        /// <summary>
        /// The signature of Mesh class. Derive class must implement own signature overriding <see cref="Signature"/> property.
        /// </summary>
        public readonly static long TriMeshSignature = BitConverterExt.ToInt64("TRIMESH");
        

        public TriMesh(Primitive topology = Primitive.TriangleList, string name = "my_mesh") : base(topology, name) 
        {
            /*
            Vertices = new StructBuffer<Vector3f>();
            TexCoords = new StructBuffer<Vector2f>();
            Normals = new StructBuffer<Vector3f>();
            Colors = new StructBuffer<Color4b>();
            Tangents = new StructBuffer<Vector4f>();
            BoneIds = new StructBuffer<Vector4b>();
            BoneWeights = new StructBuffer<Vector4b>();
            */
        }

        /// <summary>
        /// The geometry transform, all vertices must be converted using this transform.
        /// </summary>
        public Matrix4x4f Transform = Matrix4x4f.Identity;
        /// <summary>
        /// Vertices list in local world, can't be empty. Other attributes must have the same size
        /// </summary
        public StructBuffer<Vector3f> Vertices;
        public StructBuffer<Vector2f> TexCoords;
        public StructBuffer<Vector3f> Normals;
        public StructBuffer<Color4b> Colors;
        public StructBuffer<Vector4f> Tangents;
        public StructBuffer<Vector4b> BoneIds;
        public StructBuffer<Vector4b> BoneWeights;

        public int VerticesCount => Vertices?.Count ?? 0;
        public bool HasNormals => Normals?.Count > 0;
        public bool HasTexCoords => TexCoords?.Count > 0;
        public bool HasColors => Colors?.Count > 0;
        public bool HasTangents => Tangents?.Count > 0;
        public bool HasSkins => BoneIds?.Count > 0 && BoneWeights?.Count > 0;

        /// <summary>
        /// Calculate the vertices bound
        /// </summary>
        public BoundingBoxMinMax GetBound()
        {
            BoundingBoxMinMax bound = new BoundingBoxMinMax();
            if (VerticesCount > 0)
                foreach (var v in Vertices)
                    bound.Merge(v);
            return bound;
        }
        /// <summary>
        /// TODO : add normals and texcoords.
        /// Fuse two meshes
        /// </summary>
        public void Merge(TriMesh mesh)
        {
            int vcount = mesh.VerticesCount;
            int offset = VerticesCount;

            if (SubMeshes == null) SubMeshes = new List<SubMesh>(mesh.SubMeshes.Count);
            if (Vertices == null) Vertices = new StructBuffer<Vector3f>(mesh.VerticesCount);

            foreach (var sub in mesh.SubMeshes)
            {
                SubMesh newsub = new SubMesh(this, sub);
                newsub.AddOffset(offset);
                SubMeshes.Add(newsub);
            }
            Vertices.AddRange(mesh.Vertices);
            Matrix4x4f transform = mesh.Transform * Transform.Inverse();
            for (int i = 0; i < vcount; i++)
                Vertices[offset + i] = mesh.Vertices[i].TransformCoordinate(in transform);
        }

        public TriMesh Copy()
        {
            var mesh = new TriMesh(Topology, Name);

            if (VerticesCount > 0)
                mesh.Vertices = new StructBuffer<Vector3f>(Vertices);

            if (HasNormals)
                mesh.Normals = new StructBuffer<Vector3f>(Normals);

            if (HasTexCoords)
                mesh.TexCoords = new StructBuffer<Vector2f>(TexCoords);

            if (HasColors)
                mesh.Colors = new StructBuffer<Color4b>(Colors);

            if (HasTangents)
                mesh.Tangents = new StructBuffer<Vector4f>(Tangents);

            if (HasSkins)
            {
                mesh.BoneIds = new StructBuffer<Vector4b>(BoneIds);
                mesh.BoneWeights = new StructBuffer<Vector4b>(BoneWeights);
            }


            if (SubMeshCount > 0)
            {
                mesh.SubMeshes = new List<SubMesh>(SubMeshCount);
                for (int i = 0; i < SubMeshCount; i++)
                    mesh.AddSubMesh(SubMeshes[i]);
            }

            return mesh;
        }


        public virtual bool Read(BinaryReader reader)
        {
            //header
            long begin = reader.BaseStream.Position;
            var signature = reader.ReadInt64();
            var bytesize = reader.ReadInt64();
            
            if (TriMeshSignature != signature && MeshSignature != signature)
                throw new Exception($"signature not match for class {GetType()}");
            
            Version = new FileVersion(reader);
            Name = reader.ReadString();
            Topology = (Primitive)reader.ReadByte();
            Diffuse = reader.ReadColor4b();

            //transform
            Transform = Matrix4x4f.Identity;
            if (reader.ReadBoolean())
                switch ((CompressionTransform)reader.ReadByte())
                {
                    case CompressionTransform.MatrixTRS: Transform = Matrix4x4f.ComposeTRS(new Vector3f(reader), new Vector4f(reader), new Vector3f(reader)); break;
                    default: Transform = new Matrix4x4f(reader); break;
                }

            //indices
            if (!ReadSubMeshes(reader)) return false;

            //vertices
            Vertices = Packer.ReadVertices(reader);
            //texcoord
            TexCoords = Packer.ReadTexCoords(reader);
            //normals
            Normals = Packer.ReadNormals(reader);
            //Colors
            Colors = Packer.ReadColors(reader);
            
            
            
            //tangents
            if (Version.Major<2)
                Tangents = new StructBuffer<Vector4f>(reader.ReadUnsafe<Vector4f>(reader.ReadInt32()));
            else
                Tangents = Packer.ReadTangents(reader);
           
            //bones
            BoneIds = new StructBuffer<Vector4b>(reader.ReadUnsafe<Vector4b>(reader.ReadInt32()));
            BoneWeights = new StructBuffer<Vector4b>(reader.ReadUnsafe<Vector4b>(reader.ReadInt32()));
            

            long end = reader.BaseStream.Position;
            if (bytesize != end - begin) Debugg.Error($"Possible wrong byte size for {GetType()} class");
            return true;
        }
        public virtual bool Write(BinaryWriter writer) => Write(writer, 0, 0, 0, 0, 0, 0,0);
        
        public virtual bool Write(
            BinaryWriter writer,
            CompressionTransform comp_m = 0,
            CompressionIndices comp_i = 0,
            CompressionVertices comp_v = 0,
            CompressionNormals comp_n = 0,
            CompressionTexCoord comp_u = 0,
            CompressionColor comp_c = 0,
            CompressionTangents comp_t = 0)
        {
            long begin = writer.BaseStream.Position;
            writer.WriteLong(TriMeshSignature);
            writer.WriteLong(0); //filesize
            Version.Write(writer);
            writer.Write(Name);
            writer.Write((byte)Topology);
            writer.Write(Diffuse);

            //transform
            if (!Transform.IsIdentity)
            {
                writer.Write(true);
                writer.WriteByte((byte)comp_m);

                if (comp_m == CompressionTransform.MatrixTRS)
                {
                    Transform.Decompose(out var t, out var r, out var s);
                    t.Write(writer);
                    r.Write(writer);
                    s.Write(writer);
                }
                else Transform.Write(writer);
            }
            else writer.Write(false);

            //indices
            if (!WriteSubMeshes(writer, comp_i)) return false;

            // vertices
            Packer.WriteVertices(writer, Vertices, comp_v);

            //texcoords
            Packer.WriteTexCoords(writer, TexCoords, comp_u);

            //normals
            Packer.WriteNormals(writer, Normals, comp_n);

            //colors
            Packer.WriteColor(writer, Colors, comp_c);

            //tangents
            Packer.WriteTangents(writer, Tangents, comp_t);

            //bones
            if (HasSkins)
            {
                writer.Write(BoneIds.Count);
                BoneIds.Write(writer);
                writer.Write(BoneWeights.Count);
                BoneWeights.Write(writer);
            }
            else
            {
                writer.Write(0);
                writer.Write(0);
            }

            //write filesize
            long end = writer.BaseStream.Position;
            writer.BaseStream.Position = begin + 8;
            writer.Write(end - begin);
            writer.BaseStream.Position = end;

            return true;
        }
        public virtual bool Read(XmlReader reader)
        {
            throw new NotImplementedException();
        }
        public virtual bool Write(XmlWriter writer) => Write(writer, 0);
        public virtual bool Write(XmlWriter writer, CompressionTransform compression_m = 0)
        {
            writer.WriteStartElement("Mesh");
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Topology", Topology.ToString());
            writer.WriteAttributeString("Diffuse", Diffuse.ToHexString());

            if (!Transform.IsIdentity)
            {
                writer.WriteStartElement("LocalTransform");
                if (compression_m== CompressionTransform.MatrixTRS)
                {
                    Transform.Decompose(out var t, out var r, out var s);
                    if (t != Vector3f.Zero) writer.WriteAttributeString("pos", t.ToString());
                    if (r != Quaternion4f.Identity) writer.WriteAttributeString("rot", r.ToString());
                    if (s != Vector3f.One) writer.WriteAttributeString("scale", s.ToString());
                }
                else
                {
                    writer.WriteString(Transform.ToString());
                }
                writer.WriteEndElement(); //end of "LocalTransform"
            }

            writer.WriteStartElement("SubMesh");
            writer.WriteAttributeString("Count", SubMeshCount.ToString());
            foreach (var sub in SubMeshes)
                sub.Write(writer);
            writer.WriteEndElement();


            writer.WriteStartElement("Vertices");
            writer.WriteAttributeString("Count", VerticesCount.ToString());
            if (VerticesCount > 0)
                foreach (var v in Vertices)
                    writer.WriteElementString("v", v.ToString());

            writer.WriteEndElement();

            if (HasTexCoords)
            {
                writer.WriteStartElement("TexCoords");
                writer.WriteAttributeString("Count", VerticesCount.ToString());
                if (VerticesCount > 0)
                    foreach (var t in TexCoords)
                        writer.WriteElementString("t", t.ToString());
                writer.WriteEndElement();
            }
            if (HasNormals)
            {
                writer.WriteStartElement("Normals");
                writer.WriteAttributeString("Count", VerticesCount.ToString());
                if (VerticesCount > 0)
                    foreach (var n in Normals)
                        writer.WriteElementString("n", n.ToString());
                writer.WriteEndElement();
            }
            if (HasColors)
            {
                writer.WriteStartElement("Colors");
                writer.WriteAttributeString("Count", VerticesCount.ToString());
                if (VerticesCount > 0)
                    foreach (var c in Colors)
                        writer.WriteElementString("c", c.ToHexString());
                writer.WriteEndElement();
            }
            if (HasTangents)
            {
                writer.WriteStartElement("Tangents");
                writer.WriteAttributeString("Count", VerticesCount.ToString());
                if (VerticesCount > 0)
                    foreach (var t in Tangents)
                        writer.WriteElementString("tg", t.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); //end of "Mesh"
            return true;
        }

    }
}
