
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Common.Maths;

namespace Common.Tools
{
    [DebuggerDisplay("{Name}")]
    public partial class SceneNode
    {
        int m_elementIndex = -1;

        #region Data
        /// <summary>
        /// Name of node
        /// </summary>
        public string Name;
        /// <summary>
        /// A tag of node
        /// </summary>
        public object Element
        {
            get => Tree.Element[m_elementIndex];
            set
            {
                if (value == null) return;
                m_elementIndex = Tree?.m_elements?.IndexOf(value) ?? -1;
                if (m_elementIndex < 0)
                {
                    m_elementIndex = Tree.ElementsCount;
                    Tree.Element[m_elementIndex] = value;
                }
            }
        }
        /// <summary>
        /// Local transform
        /// </summary>
        protected Matrix4x4f transform = Matrix4x4f.Identity;
        /// <summary>
        /// Children nodes of this node
        /// </summary>
        public readonly ChildrenCollection Children;
        #endregion

        public readonly SceneTree Tree;
        /// <summary>
        /// Parent node, null if it's root
        /// </summary>
        public SceneNode Parent { get; internal set; }
        public SceneNode Previous { get; internal set; }
        public SceneNode Next { get; internal set; }
        public SceneNode First { get; internal set; }


        /// <summary>
        /// </summary>
        public SceneNode(SceneTree tree, string name = "Node")
        {
            Name = name;
            Children = new ChildrenCollection(this);
            Tree = tree;
        }

        /// <summary>
        /// It's the affine transformation of this node. <seealso cref="Matrix4x4f.ComposeTRS(Vector3f, Quaternion4f, Vector3f)"/>
        /// </summary>
        public Matrix4x4f LocalTransform
        {
            get => transform;
            set => transform = value;
        }

        /// <summary>
        /// Calculate the global transform relative to root 
        /// </summary>
        public Matrix4x4f GetGlobalTransform()
        {
            Matrix4x4f matrix = transform;
            SceneNode node = Parent;

            while (node != null) { matrix = node.transform * matrix; node = node.Parent; }
            return matrix;
        }
        
        
        
        #region Binary Serialization
        public bool Write(BinaryWriter writer, TransformVersion matversion = TransformVersion.Float16)
        {
            writer.Write(Name);
            writer.Write(m_elementIndex);

            bool isIdentity = transform.IsIdentity;
            if (isIdentity)
            {
                writer.Write(false);
            }
            else if (matversion == TransformVersion.Float16)
            {
                writer.Write((byte)matversion);
                transform.Write(writer);
            }
            else if (matversion == TransformVersion.Decomposed)
            {
                writer.Write((byte)matversion);
                transform.Decompose(out var t, out var s, out var q);
                t.Write(writer);
                s.Write(writer);
                q.Write(writer);
            }
            
            if (!Children.Write(writer)) return false;

            return true;
        }
        public bool Read(BinaryReader reader)
        {
            Name = reader.ReadString();
            m_elementIndex = reader.ReadInt32();

            var matrixversion = (TransformVersion)reader.ReadByte();
            switch (matrixversion)
            {
                case TransformVersion.Float16:
                    transform = new Matrix4x4f(reader);
                    break;
                case TransformVersion.Decomposed:
                    var t = new Vector3f(reader);
                    var r = new Vector4f(reader);
                    var s = new Vector3f(reader);
                    transform = Matrix4x4f.ComposeTRS(t, r, s); break;
                default:
                    transform = Matrix4x4f.Identity;
                    break;
            }

            if (!Children.Read(reader)) return false;

            return true;
        }
        #endregion

        #region Xml Serialization
        public bool Write(XmlWriter writer, TransformVersion matversion = TransformVersion.Float16)
        {
            writer.WriteStartElement("SceneNode");
            writer.WriteAttributeString("Name", Name);
            if (m_elementIndex>=0) writer.WriteAttributeString("ElementRef", m_elementIndex.ToString());

            if (!transform.IsIdentity)
            {
                writer.WriteStartElement("LocalTransform");
                if (matversion == TransformVersion.Decomposed)
                {
                    transform.Decompose(out var t, out var r, out var s);
                    if (t != Vector3f.Zero) writer.WriteAttributeString("pos", t.ToString());
                    if (r != Quaternion4f.Identity) writer.WriteAttributeString("rot", r.ToString());
                    if (s != Vector3f.One) writer.WriteAttributeString("scale", s.ToString());
                }
                else
                {
                    writer.WriteString(transform.ToString());
                }

                writer.WriteEndElement();
            }

            if (!Children.Write(writer)) return false;
            
            writer.WriteEndElement(); //end of "SceneNode"
            return true;
        }
        public bool Read(XmlReader reader)
        {
            if (!reader.IsStartElement() || reader.Name != "SceneNode") return false;

            // a node can be empty : <SceneNode Name="####" /> but must be checked before
            // call MoveToAttribyte()
            bool isEmpty = reader.IsEmptyElement; 
            if (reader.MoveToAttribute("Name"))  Name = reader.Value;
            if (reader.MoveToAttribute("ElementRef")) if (!int.TryParse(reader.Value, out m_elementIndex)) m_elementIndex = -1;

            while (!isEmpty && reader.Read())
            {
                switch (reader.Name)
                {
                    case "LocalTransform": ReadXmlTransform(reader); break;
                    case "Children": if (!Children.Read(reader)) return false; break;
                    //check correct exit
                    case "SceneNode": if (reader.NodeType != XmlNodeType.EndElement) throw new XmlException("SceneNode not exit correctly"); return true;
                }
            }
            return true;
        }
        private void ReadXmlTransform(XmlReader reader)
        {
            //if (reader.IsEmptyElement) return;

            bool isDecomposed = true;
            Vector3f pos = default(Vector3f);
            Vector3f scale = default(Vector3f);
            Quaternion4f rot = default(Quaternion4f);

            if (isDecomposed && reader.MoveToAttribute("scale"))
            {
                if (!Vector3f.TryParse(reader.Value, out scale)) scale = Vector3f.One;
                isDecomposed = true;
            }
            if (isDecomposed && reader.MoveToAttribute("pos"))
            {
                if (!Vector3f.TryParse(reader.Value, out pos)) pos = Vector3f.Zero;
                isDecomposed = true;
            }
            if (isDecomposed && reader.MoveToAttribute("rot"))
            {
                if (!Quaternion4f.TryParse(reader.Value, out rot)) rot = Quaternion4f.Identity;
                isDecomposed = true;
            }
            if (isDecomposed)
            {
                LocalTransform = Matrix4x4f.ComposeTRS(pos, rot, scale);
            }
            else
            {
                if (!reader.HasValue || !Matrix4x4f.TryParse(reader.Value, out transform))
                {
                    transform = Matrix4x4f.Identity;
                }
            }
        }      
        #endregion
        
        public override string ToString()
        {
            return Name;
        }
    }
}
