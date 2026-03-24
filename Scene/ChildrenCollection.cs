using Common.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Common.Tools
{
    public partial class SceneNode
    {
        /// <summary>
        /// Simple implementation of linked list for children 
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(CollectionDebugView<SceneNode>))]
        public class ChildrenCollection : IEnumerable<SceneNode>
        {
            static bool HasChildren(SceneNode node) => node?.First != null;
            static bool HasNext(SceneNode node) => node?.Next != null;

            public int Count { get; private set; }

            readonly SceneNode parent;

            public ChildrenCollection(SceneNode parent)
            {
                this.parent = parent;
                Count = 0;
            }

            public void AddLast(SceneNode node)
            {
                //if (HasChildren(node)) throw new NotImplementedException("adding node with a hierarchy inside it is not implemented yet");

                if (node.Parent != null) throw new Exception("Possible node circular hierarchy");
                if (ReferenceEquals(this, node)) throw new Exception("Adding node to itself ?");

                if (parent.First == null) parent.First = node;
                else GetLast().Next = node;

                node.Parent = parent;
                Count++;

                if (!ReferenceEquals(parent.Tree, node.Tree)) throw new ArgumentException("Node must be in the same SceneTree");
            }

            public SceneNode GetLast()
            {
                SceneNode next = parent.First;
                if (next == null) return null;
                while (HasNext(next)) next = next.Next;
                return next;
            }

            public bool Remove(SceneNode node)
            {
                if (node == null || !ReferenceEquals(node.Parent, parent)) throw new ArgumentException("the node is null or it's not a child of this node");
                if (Count == 0) return false;

                if (ReferenceEquals(parent.First, node))
                {
                    parent.First = node.Next;
                }
                else
                {
                    SceneNode prev = parent.First;
                    while (!ReferenceEquals(prev.Next, node) && HasNext(prev)) prev = prev.Next;
                    //now prev.next is the node
                    prev.Next = node.Next;
                }
                node.Next = null;
                node.Parent = null;
                Count--;

                return true;
            }

            public void Clear()
            {
                parent.First = null;
            }


            public SceneNode[] ToArray()
            {
                SceneNode[] array = new SceneNode[Count];
                int i = 0;
                foreach (var node in this) array[i++] = node;
                return array;
            }


            public IEnumerator<SceneNode> GetEnumerator()
            {
                SceneNode node = parent.First;
                while (node != null)
                {
                    yield return node;
                    node = node.Next;
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Read(BinaryReader reader)
            {
                int numchildren = reader.ReadInt32();
                Clear();
                for (int i = 0; i < numchildren; i++)
                {
                    SceneNode node = new SceneNode(parent.Tree);
                    parent.Children.AddLast(node);
                    if (!node.Read(reader)) return false;
                }
                return true;
            }

            public bool Write(BinaryWriter writer)
            {
                writer.Write(Count);
                foreach (var child in this)
                    if (!child.Write(writer)) return false;
                return true;
            }
            
            public bool Write(XmlWriter writer)
            {
                if (Count <= 0) return true;
                writer.WriteStartElement("Children");
                writer.WriteAttributeString("Count", Count.ToString());
                foreach (var child in this) child.Write(writer);
                writer.WriteEndElement(); //end of "Children"
                return true;
            }

            public bool Read(XmlReader reader)
            {
                Clear();
                if (reader.IsEmptyElement) return false;

                int count = 0;
                if (reader.MoveToAttribute("Count")) int.TryParse(reader.Value, out count);

                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "SceneNode":
                            SceneNode node = new SceneNode(parent.Tree);
                            if (node.Read(reader)) AddLast(node);
                            break;

                        case "Children":
                            Debugg.Message("end children");
                            if (Count != count) Debugg.Error("Xml Children m_count doesn't match");
                            if (reader.NodeType != XmlNodeType.EndElement) throw new XmlException("ChildrenNode not exit correctly");
                            return true;
                    }
                }
                return true;
            }
        }
    }
}

