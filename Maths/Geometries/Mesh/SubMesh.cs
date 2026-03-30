using Common.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Common.Maths
{
    /// <summary>
    /// Contains the indices list of shared vertices from main mesh class.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public partial class SubMesh
    {
        internal Mesh mesh;
        byte flags = 1;

        public string Name;
        /// <summary>
        /// The mininum index.<br/>Need to manualy update with <see cref="Mathelp.GetMinMax(IEnumerable{int})"/>
        /// </summary>
        public int FirstVertex;
        /// <summary>
        /// Raw indices, depend by topology type.
        /// </summary>
        public RawIndices Indices;

        /// <summary>
        /// ToDo : remove
        /// </summary>
        public bool Enable
        {
            get => (flags & 1) > 0;
            set
            {
                flags = (byte)(flags & 0xFE);
                if (value) flags |= 1;
            }
        }
        
        /// <summary>
        /// Triangles or Lines or Points count depend by topology
        /// </summary>
        public int IndincesCount => Indices?.Count ?? 0;


        private SubMesh (Mesh mesh)
        {
            this.mesh = mesh;
        }

        internal static SubMesh Read(Mesh mesh , BinaryReader reader)
        {
            SubMesh sub = new SubMesh(mesh);
            if (!sub.Read(reader)) return null;
            return sub;
        }

        /// <summary>
        /// </summary>
        /// <param name="indexCapacity">initialize capacity of Indices</param>
        internal SubMesh(Mesh mesh, int indexCapacity = 0, IndexFormat format = IndexFormat.Index32bit , string name = "my_submesh") : this(mesh)
        {
            Indices = new RawIndices(format, indexCapacity);
            Indices.owner = this;
            Name = name;
        }


        /// <summary>
        /// make a shallow copy
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="copyfrom"></param>
        public SubMesh(Mesh owner, SubMesh copyfrom) : this(owner, copyfrom.IndincesCount, copyfrom.Indices.Format, copyfrom.Name)
        {
            Indices.AddRange(copyfrom.Indices);
            FirstVertex = copyfrom.FirstVertex;
            Name = copyfrom.Name;
        }

        public bool Read(BinaryReader reader)
        {
            Name = reader.ReadString();
            Indices = RawIndices.Read(reader, out FirstVertex);
            Indices.owner = this;
            return true;
        }

        public bool Write(BinaryWriter writer, bool use7Bitencoder = false)
        {
            writer.Write(Name);
            Indices.Write(writer, use7Bitencoder);
            return true;
        }
        public bool Read(XmlReader reader)
        {
            throw new NotImplementedException();
        }
        public bool Write(XmlWriter writer)
        {
            writer.WriteStartElement("SubMesh");
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("IndicesCount", IndincesCount.ToString());
            
            switch(mesh.Topology)
            {
                case Primitive.Point:
                    foreach (var i in Indices) 
                        writer.WriteElementString("p", i.ToString());
                    break;

                case Primitive.LineList:
                case Primitive.LineStrip:
                    for (int i = 0; i < IndincesCount; i += 2)
                        writer.WriteElementString("l", $"{Indices[i]} , {Indices[i + 1]}"); 
                    break;

                case Primitive.TriangleList:
                case Primitive.TriangleStrip:
                    for (int i = 0; i < IndincesCount; i += 3)
                        writer.WriteElementString("f", $"{Indices[i]} , {Indices[i + 1]} , {Indices[i + 2]}");
                    break;

                default:
                    foreach (var i in Indices)
                        writer.WriteElementString("i", i.ToString());
                    break;
            }


            writer.WriteEndElement();
            return true;
        }



    }
}
