using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Base Enumerator implementation. Is inclusive so root node will always be returned as first
    /// </summary>
    public abstract class QuadEnumerator<N,T> : IEnumerator<N>, IEnumerable<N>
        where N : QuadNode<N,T> , new()
        where T : Quadtree<N,T>
    {
        protected T tree;
        protected N root;
        protected N current;
        protected int count = 0;

        protected QuadEnumerator(N root)
        {
            this.tree = root.main;
            this.root = root;
            this.current = root;
            this.count = 0;
        }
        protected QuadEnumerator(T tree)
        {
            this.tree = tree;
            this.root = tree.root;
            this.current = root;
            this.count = 0;
        }

        public N Current { get { return current; } }
        
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

        public IEnumerator<N> GetEnumerator()
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
    public class QuadNodesEnumerator<N,T> : QuadEnumerator<N,T>
        where N : QuadNode<N, T>, new()
        where T : Quadtree<N,T>
    {
        protected MyStack<N> stack;

        public QuadNodesEnumerator(N root) : base(root)
        {
            stack = new MyStack<N>(OptimalStackSize);
            Reset();
        }

        /// <summary>
        /// The maximum cacapity that stack use, this calculation is done considering that for each level 
        /// the algorithm accumulate 4-1 node from parent's level into the stack each MoveNext().
        /// It's not possible calculate for iterators what use a second interetors inside like QuadAreaEnumerator
        /// </summary>
        public int OptimalStackSize
        {
            get { return 3 * root.level; }
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
                for (int i = 3; i >=0; i--)
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
