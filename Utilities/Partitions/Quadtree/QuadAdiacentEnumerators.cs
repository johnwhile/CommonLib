using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Return the collection of all children that are neighbour of node setted
    /// </summary>
    public class QuadAdjacentEnumerator<N,T> : QuadEnumerator<N,T>
        where N : QuadNode<N, T>, new()
        where T : Quadtree<N,T>
    {
        protected MyStack<N> stack;

        public QuadAdjacentEnumerator(T tree)
            : base(tree)
        {
            SetCenterNode(root);
            stack = new MyStack<N>(OptimalStackSize);
            Reset();
        }

        public void SetCenterNode(N node)
        {
            this.root = node;
        }


        public int OptimalStackSize
        {
            get { return 3 * (tree.Depth - 1); }
        }

        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        public override bool MoveNext()
        {
            return false;
 
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
