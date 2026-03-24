using Common.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Common;
using Common.IO;
using System.Diagnostics;

namespace Common.Tools
{
    public enum TransformVersion : byte
    {
        Float16 = 1,
        Decomposed = 2
    }
    [DebuggerDisplay("{Name} Elements = {ElementsCount}")]
    public class SceneTree : IBinarySerializable , IXmlSerializable
    {
        /// <summary>
        /// The signature of SceneTree class. Derive class must implement own signature overriding <see cref="Signature"/> property.
        /// </summary>
        public readonly static long BaseSignature = BitConverterExt.ToInt64("TREE");

        /// <summary>
        /// Access to <see cref="m_elements"/> array using a controlled indexer implementation
        /// </summary>
        public IndexedProperty<int, object> Element;

        /// <summary>
        /// The signature define the class type, must be unique and it's used when reading 
        /// </summary>
        public virtual long Signature => BaseSignature;

        public string Name;

        public SceneNode Root;

        /// <summary>
        /// the elements must be custom written or readed
        /// </summary>
        internal protected List<object> m_elements;

        /// <summary>
        /// Return the list of Element with same type of T
        /// </summary>
        public ElementCollection<T> GetElementCollection<T>() => new ElementCollection<T>(m_elements);
        
        /// <summary>
        /// Get the number of elements used by nodes of this tree
        /// </summary>
        public int ElementsCount
        {
            get => m_elements?.Count ?? 0;
        }

        public SceneTree(string name = "Tree")
        {
            Name = name;
            Root = new SceneNode(this, "Root");

            Element = new IndexedProperty<int, object>(
                delegate(int index) 
                { 
                    return index > -1 && m_elements?.Count > index ? m_elements[index] : null;
                }, 
                delegate(int index, object value)
                {
                    if (index > -1)
                    {
                        if (m_elements == null) m_elements = new List<object>(index + 1);
                        
                        if (m_elements.Count <= index)
                            for (int i = m_elements.Count; i <= index; i++) m_elements.Add(null);
                        
                        m_elements[index] = value;
                    }
                });
        }

        #region Serialized
        /// <summary>
        /// The stream position are restored to begin.
        /// </summary>
        public static (long signature,long bytesize) ReadHeader(BinaryReader reader)
        {
            var pos = reader.BaseStream.Position;
            var signature = reader.ReadInt64();
            var bytesize = reader.ReadInt64();
            reader.BaseStream.Position = pos;
            return (signature, bytesize);
        }

        public virtual bool Read(BinaryReader reader)
        {
            //header
            long begin = reader.BaseStream.Position;
            var signature = reader.ReadInt64();
            var bytesize = reader.ReadInt64();

            if (Signature != signature) throw new Exception($"signature not match for class {GetType()}");

            Name = reader.ReadString();
            if (!Root.Read(reader)) return false;

            int count = reader.ReadInt32();

            m_elements = new List<object>(count);
            Element[count - 1] = null;
            
            long end = reader.BaseStream.Position;
            if (bytesize != end - begin) Debugg.Error($"Possible wrong byte size for {GetType()} class");

            return true;
        }
        public virtual bool Write(BinaryWriter writer) => Write(writer, TransformVersion.Float16);
        public virtual bool Write(BinaryWriter writer, TransformVersion matversion)
        {
            long begin = writer.BaseStream.Position;
            writer.WriteLong(Signature);
            writer.WriteLong(); //filesize
            writer.Write(Name);
            Root.Write(writer, matversion);

            writer.Write(ElementsCount);

            //write filesize
            long end = writer.BaseStream.Position;
            writer.BaseStream.Position = begin + 8;
            writer.Write(end - begin);
            writer.BaseStream.Position = end;
            return true;
        }

        public virtual bool Read(XmlReader reader)
        {
            bool success = false;
            if (!reader.IsStartElement() || reader.Name != "SceneTree") return false;
                if (reader.MoveToAttribute("Name")) Name = reader.Value;

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "SceneNode": success |= Root.Read(reader); break; //only one scenenode exists
                    case "SceneTree": return success;
                    case "Elements":
                        if (reader.MoveToAttribute("Count"))
                        {
                            if (!int.TryParse(reader.Value, out int count)) count = 0;
                            m_elements = new List<object>(count);
                        }
                        break;
                }
            }
            return success;
        }
        public virtual bool Write(XmlWriter writer) => Write(writer, TransformVersion.Float16);
        public virtual bool Write(XmlWriter writer, TransformVersion matversion)
        {
            writer.WriteStartElement("SceneTree");
            writer.WriteAttributeString("Name", Name);
            
            Root.Write(writer, matversion);

            writer.WriteStartElement("Elements");
            writer.WriteAttributeString("Count", ElementsCount.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
            return true;
        }

        public class ElementCollection<T> : IEnumerable<T>
        {
            List<object> elements;
            public ElementCollection(List<object> elements)
            {
                this.elements = elements;
            }
            public IEnumerator<T> GetEnumerator()
            {
                if (elements == null || elements.Count == 0) yield break;
                foreach (var element in elements)
                    if (element is T castelement)
                        yield return castelement;
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
        #endregion
    }
}
