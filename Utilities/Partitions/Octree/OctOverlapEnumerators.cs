using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Return the collection of Nodes intersected by 3D volume, is inclusive so also root node are returned if selected
    /// </summary>
    public abstract class OctOverlapEnumerator<T> : OctEnumerator<T> where T : OctNode<T> , IAABBox
    {
        MyStack<StackEntry> stack;
        StackEntry currentstack;
        bool completeinside = false;

        public OctOverlapEnumerator(T root)
            : base(root)
        {
            stack = new MyStack<StackEntry>(10);
            Reset();
        }

        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;
            currentstack = stack.Pop();
            current = currentstack.node;

            if (!current.IsLeaf)
            {
                if (currentstack.alloverlap)
                {
                    for (int i = 7; i >= 0; i--)
                        if (current.child[i] != null)
                            stack.Push(new StackEntry(current.child[i], true));
                }
                else
                {
                    for (int i = 7; i >= 0; i--)
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
        protected abstract bool intersection(T qi, out bool completleinside);

        /// <summary>
        /// Stack queue contain also complete-inside flag
        /// </summary>
        struct StackEntry
        {
            public T node;
            public bool alloverlap;
            public StackEntry(T node, bool alloverlap)
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
    /// Return all node selected by a AABBox
    /// </summary>
    public class OctBoxEnumerator<T> : OctOverlapEnumerator<T> where T : OctNode<T> , IAABBox
    {
        BoundingBoxCenter box;
        // precomputed
        Vector3f boxmin, boxmax;


        public OctBoxEnumerator(T root, BoundingBoxCenter box)
            : base(root)
        {
            this.box = box;
            this.boxmin = box.Min;
            this.boxmax = box.Max;
        }

        /// <summary>
        /// Remember that my QuadNode implementation use AABRectangle2 instead AABRectangle
        /// </summary>
        protected override bool intersection(T qi, out bool completeinside)
        {
            Vector3f qimax = qi.Max;
            Vector3f qimin = qi.Min;

            if (PrimitiveIntersections.Intersect_AABB_AABB(qimin, qimax, boxmin, boxmax))
            {
                completeinside =
                    qimin.x > boxmin.x && qimax.x < boxmax.x &&
                    qimin.y > boxmin.y && qimax.y < boxmax.y &&
                    qimin.z > boxmin.z && qimax.z < boxmax.z;

                return true;
            }
            completeinside = false;
            return false;

        }
    }
    /// <summary>
    /// Return all node selected by a Sphere
    /// </summary>
    public class OctSphereEnumerator<T> : OctOverlapEnumerator<T> where T : OctNode<T> , IAABBox
    {
        Sphere sphere;

        public OctSphereEnumerator(T root, Sphere sphere)
            : base(root)
        {
            this.sphere = sphere;
        }



        protected override bool intersection(T qi, out bool completeinside)
        {
            Vector3f qimax = qi.Max;
            Vector3f qimin = qi.Min;

            completeinside = false;

            if (PrimitiveIntersections.IntersectAABBSphere(qimin, qimax, sphere.center, sphere.radius))
            {
                Vector3f corner = Vector3f.Zero;
                float rr = sphere.radius * sphere.radius;

                completeinside = true;
                corner.x = qimax.x - sphere.center.x;
                corner.y = qimax.y - sphere.center.y;
                corner.z = qimax.z - sphere.center.z;

                if (corner.LengthSq > rr)
                {
                    completeinside = false;
                }
                else
                {
                    corner.x = qimax.x - sphere.center.x;
                    corner.y = qimin.y - sphere.center.y;
                    corner.z = qimin.z - sphere.center.z;
                    if (corner.LengthSq > rr)
                    {
                        completeinside = false;
                    }
                    else
                    {
                        corner.x = qimin.x - sphere.center.x;
                        corner.y = qimax.y - sphere.center.y;
                        corner.z = qimax.z - sphere.center.z;
                        if (corner.LengthSq > rr)
                        {
                            completeinside = false;
                        }
                        else
                        {
                            corner.x = qimin.x - sphere.center.x;
                            corner.y = qimin.y - sphere.center.y;
                            corner.z = qimin.z - sphere.center.z;
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
