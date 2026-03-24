using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tools
{
    public partial class SceneNode
    {
        static DepthFirstCollection treetraversal = new DepthFirstCollection();

        public DepthFirstCollection TreeHierarchy
        {
            get 
            {
                treetraversal.Root = this;
                return treetraversal;
            }
        }

        /// <summary>
        /// Pre-order <seealso cref="https://en.wikipedia.org/wiki/Tree_traversal"/><br/>
        /// Attention: the root node is not returned by iteration. 
        /// </summary>
        public class DepthFirstCollection : IEnumerable<SceneNode>
        {
            DepthFirstEnumerator enumerator = new DepthFirstEnumerator();
            
            public SceneNode Root
            {
                get => enumerator.Root;
                set => enumerator.Root = value;
            }

            public IEnumerator<SceneNode> GetEnumerator() => enumerator;

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public class DepthFirstEnumerator : IEnumerator<SceneNode>
            {
                static bool HasChildren(SceneNode node) => node?.First != null;
                static bool HasNext(SceneNode node) => node?.Next != null;

                SceneNode root;
                SceneNode current;
                bool is_first_iteration;

                public SceneNode Root
                {
                    get => root;
                    set
                    {
                        root = value;
                        current = root;
                        Reset();
                    }
                }
                public SceneNode Current => current;
                object IEnumerator.Current => current;
                public void Dispose() { }
                public bool MoveNext()
                {
                    //the first case to check is if there is at least one child to continue
                    if (is_first_iteration && !HasChildren(current)) return false;
                    is_first_iteration = false;

                    //priority for first child
                    if (HasChildren(current))
                    {
                        current = current.First;
                        return true;
                    }
                    //if no children go to next
                    if (HasNext(current))
                    {
                        current = current.Next;
                        return true;
                    }

                    //if not children and no next, go up untill parent next
                    if (current.Parent != null && ReferenceEquals(current.Parent, root))
                    {
                        while (current != null)
                        {
                            current = current.Parent;

                            //reach the end, the last child of root
                            if (ReferenceEquals(current, root)) return false;

                            //the parent is not the last child
                            if (HasNext(current)) { current = current.Next; return true; }
                        }
                    }
                    return false;
                }
                public void Reset()
                {
                    current = root;
                    is_first_iteration = true;
                }
            }
        }

    }
}

