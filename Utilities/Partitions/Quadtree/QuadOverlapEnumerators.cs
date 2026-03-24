using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Return the collection of Nodes intersected by 2D area, is inclusive so also root node are returned if selected
    /// </summary>
    /// <remarks>
    /// Due the goal of algorithm, the time of execution is reduced in two cases :
    /// 1) if rectangle is small -> tree traversal reduced down to a minimum of nodes = quadtree level
    /// 2) if rectangle is bigger -> from first node completly inside the area the tree traversal use the default 
    ///    QuadBaseCollection that don't contain intersection tests 
    /// So the worst case in an average between these two cases
    /// </remarks>
    public abstract class QuadOverlapEnumerator<N,T> : QuadEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA , new() 
        where T : Quadtree<N,T>
    {
        MyStack<StackEntry> stack;
        StackEntry currentstack;
        bool completeinside = false;

        public QuadOverlapEnumerator(N root)
            : base(root)
        {
            stack = new MyStack<StackEntry>(10);
            Reset();
        }

        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        //Without "completeinside" optimization is x2 slower
        /*
        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;
            currentstack = stack.Pop();
            current = currentstack.node;

            if (!current.IsLeaf)
            {
                for (int i = 3; i >= 0; i--)
                {
                    QuadNode node = current.child[i];
                    if (current.child[i] != null && intersection(node, out completeinside))
                    {
                        stack.Push(new StackEntry(node,false));
                    }
                }
            }
            count++;
            return true;
        }
        */

        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;
            currentstack = stack.Pop();
            current = currentstack.node;

            if (!current.IsLeaf)
            {
                if (currentstack.alloverlap)
                {
                    for (int i = 3; i >= 0; i--)
                        if (current.child[i] != null)
                            stack.Push(new StackEntry(current.child[i], true));
                }
                else
                {
                    for (int i = 3; i >= 0; i--)
                        if (current.child[i] != null && intersection(current.child[i], out completeinside))
                            stack.Push(new StackEntry(current.child[i], completeinside));
                }
            }
            count++;
            return true;
        }

        public override void Reset()
        {
            count = 0;
            stack.Clear();

            completeinside = false;

            if (intersection(root, out completeinside))
            {
                current = root;
                currentstack = new StackEntry(root, completeinside);
                stack.Push(currentstack);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="completeinside">true if geometry contain completly the quadnode</param>
        protected abstract bool intersection(N qi, out bool completleinside);

        /// <summary>
        /// Stack queue contain also complete-inside flag
        /// </summary>
        struct StackEntry
        {
            public N node;
            public bool alloverlap;
            public StackEntry(N node, bool alloverlap)
            {
                this.node = node;
                this.alloverlap = alloverlap;
            }
            public override string ToString()
            {
                if (node == null) return "Undefined";
                return "Entry: " + node.ToString();
            }
        }
    }

    /// <summary>
    /// Return all node selected by a rectangle
    /// </summary>
    public class QuadRectangleEnumerator<N,T> : QuadOverlapEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA, new() 
        where T : Quadtree<N,T>
    {
        AABRminmax rectangle;

        public QuadRectangleEnumerator(N root, AABRminmax rectangle)
            : base(root)
        {
            this.rectangle = rectangle;
        }

        /// <summary>
        /// Remember that my QuadNode implementation use AABRectangle2 instead AABRectangle
        /// </summary>
        protected override bool intersection(N qi, out bool completeinside)
        {
            Vector2f qimax = qi.Max;
            Vector2f qimin = qi.Min;

            if (PrimitiveIntersections.Intersect_AABR_AABR(qimin, qimax, rectangle.min, rectangle.max))
            {
                completeinside =
                    qimin.x > rectangle.min.x && qimax.x < rectangle.max.x &&
                    qimin.y > rectangle.min.y && qimax.y < rectangle.max.y;

                return true;
            }
            completeinside = false;
            return false;

        }
    }
    /// <summary>
    /// Return all node selected by a circle
    /// </summary>
    public class QuadCircleEnumerator<N,T> : QuadOverlapEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA, new() 
        where T : Quadtree<N,T>
    {
        Circle circle;
        public QuadCircleEnumerator(N root, Circle circle)
            : base(root)
        {
            this.circle = circle;
        }



        protected override bool intersection(N qi, out bool completeinside)
        {
            Vector2f qimax = qi.Max;
            Vector2f qimin = qi.Min;

            completeinside = false;

            if (PrimitiveIntersections.Intersect_AABR_Circle_v2(qimin, qimax, circle.center, circle.radius))
            {
                Vector2f corner = Vector2f.Zero;
                float rr = circle.radius * circle.radius;

                completeinside = true;
                corner.x = qimax.x - circle.center.x;
                corner.y = qimax.y - circle.center.y;

                if (corner.LengthSq > rr)
                {
                    completeinside = false;
                }
                else
                {
                    corner.x = qimax.x - circle.center.x;
                    corner.y = qimin.y - circle.center.y;
                    if (corner.LengthSq > rr)
                    {
                        completeinside = false;
                    }
                    else
                    {
                        corner.x = qimin.x - circle.center.x;
                        corner.y = qimax.y - circle.center.y;
                        if (corner.LengthSq > rr)
                        {
                            completeinside = false;
                        }
                        else
                        {
                            corner.x = qimin.x - circle.center.x;
                            corner.y = qimin.y - circle.center.y;
                            if (corner.LengthSq > rr)
                            {
                                completeinside = false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;


        }
    }



}
