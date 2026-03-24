using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Base Enumerator implementation. Is inclusive so root node will be returned as first
    /// </summary>
    public abstract class OctEnumerator<T> : IEnumerator<T>, IEnumerable<T> where T : OctNode , IAABBox
    {
        protected T root;
        protected T current;
        protected int count = 0;

        protected OctEnumerator(T root)
        {
            this.root = root;
            this.current = root;
            this.count = 0;
        }

        public T Current { get { return current; } }

        /// <summary>
        /// keeping track of total nodes returned
        /// </summary>
        public int IterCounter { get { return count; } }

        public abstract int MaxStackSizeUsed { get; }

        public abstract bool MoveNext();

        public abstract void Reset();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (MoveNext())
                yield return current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    /// <summary>
    /// Return the collection of all children. Is inclusive so root node will be always returned first
    /// Not-recursive algorithm code from : http://www.cornelrat.ro/?p=56
    /// </summary>
    public class OctNodesEnumerator<T> : OctEnumerator<T> where T : OctNode<T>, IAABBox
    {
        protected MyStack<T> stack;

        public OctNodesEnumerator(T root)
            : base(root)
        {
            stack = new MyStack<T>(OptimalStackSize);
            Reset();
        }

        /// <summary>
        /// The maximum cacapity that stack use, this calculation is done considered that for each level 
        /// the algorithm accumulate 8-1 node from parent's level into the stack each MoveNext().
        /// Is not possible calculate for iterators what use a second interetors inside like QuadAreaEnumerator
        /// </summary>
        public int OptimalStackSize
        {
            get { return 7 * root.level; }
        }

        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;

            current = stack.Pop();

            if (!current.IsLeaf)
            {
                for (int i = 7; i >= 0; i--)
                    if (current.child[i] != null)
                        stack.Push(current.child[i]);
            }

            count++;
            return true;
        }

        public override void Reset()
        {
            count = 0;
            stack.Clear();
            stack.Push(root);
            current = root;
        }


    }
}
