using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Quadtree
{
    /// <summary>
    /// Base Enumerator implementation. Is inclusive so root node will always be returned as first
    /// The Not-Recursive algorithm code from : http://www.cornelrat.ro/?p=56 increase a lot the performace
    /// for a big depth tree.
    /// </summary>
    public abstract class QuadEnumerator<N,T> : IEnumerator<N>, IEnumerable<N>
        where N : QuadNode<N,T>
        where T : QuadTree<N>
    {
        protected T tree;
        protected N root;
        protected N current;
        protected int count = 0;

        protected QuadEnumerator(N root)
        {
            this.tree = root.tree;
            this.root = root;
            this.current = root;
            this.count = 0;
        }

        /// <summary>
        /// The maximum capacity that stack use, this calculation is done considered that for each level 
        /// the algorithm accumulate 4-1 node from parent's level into the stack each MoveNext().
        /// Is not possible calculate for iterators what use a second interetors inside like QuadAreaEnumerator
        /// </summary>
        /// <remarks>
        /// the greater size occur when walking from root to one leaf node
        /// +1 (1°pop) -1(pop) +4(push) -1(pop) +4(push) -1(pop) +4(push) -1(pop) +4(push) ....
        /// size of stack at the end of each MoveNext() : 4,7,10,... +3 for each depth. 
        /// if calculation is correct OptimalStackSize == MaxStackSizeUsed
        /// for a tree with maximum depth 16 (from level 15 to level 0) max stack used is 46, just initialize once the enumerator
        /// to preallocate space.
        /// </remarks>
        public int OptimalStackSize
        {
            get { return 3 * root.Level + 1; }
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
    /// Return the collection of all children.
    /// </summary>
    public class QuadNodesEnumerator<N,T> : QuadEnumerator<N,T>
        where N : QuadNode<N, T>
        where T : QuadTree<N>
    {
        protected MyStack<N> stack;

        /// <summary>
        /// Preallocate necessary space for walking to leaf node from root
        /// </summary>
        public QuadNodesEnumerator(N root) : base(root)
        {
            stack = new MyStack<N>(OptimalStackSize);
            Reset();
        }
        /// <summary>
        /// Change root without initialize a new instance, internal stack size can be increased
        /// </summary>
        public void ChangeRoot(N root, bool preallocatestacksize = false)
        {
            if (preallocatestacksize && this.root.Level < root.Level)
            {
                int optimal = OptimalStackSize;
                if (stack.Capacity < optimal) stack.Capacity = optimal;
            }
            this.root = root;
        }


        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;
   
            current = stack.Pop();

            if (current.ChildrenFlag != 0)
            {
                for (int i = 3; i >= 0; i--)
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
