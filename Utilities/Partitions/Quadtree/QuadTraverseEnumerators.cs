using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    /// <summary>
    /// Return the collection of all children traversed by a ray.
    /// Is inclusive so also root node are returned if selected
    /// </summary>
    /// <remarks>
    /// The traverse algorithm return the nodes sorted by first intersection, 
    /// parent have the priority.
    /// Example with tree's depth = 3
    /// if ray is Ray2D.FromStartEnd({0,0} , {1.1,1})  the returned nodes are 0,1,5,6,8,2,11,4,17,18,20 
    /// if ray is Ray2D.FromStartEnd({1.1,1} , {0,0})  the returned nodes are 0,4,20,18,17,2,11,1,8,6,5 
    /// root node is always first, after the children returned are 1,2,4 or 4,2,1 because ray direction is opposite
    /// 
    /// For line generalization we assume the origin always out of node boundary
    /// </remarks>
    public abstract class QuadTraverseEnumerator<N,T> : QuadEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA , new() 
        where T : Quadtree<N,T>
    {
        const float pINF = float.PositiveInfinity;
        const float nINF = float.NegativeInfinity;

        MyStack<StackEntry> stack = new MyStack<StackEntry>(10);
        StackEntry currententry;

        // computed only at begining
        byte raysign = 0;
        eAxis parallel = eAxis.None;

        delegate int AddVisibilityFunction(N parent, Ray2DParam param);
        delegate bool TestParamFunction(Ray2DParam param);
        TestParamFunction testparam = null;
        AddVisibilityFunction addvisibility = null;


        // all ray-line-segment implementation contain this generic values
        protected Vector2f origin, direction;
        protected float seglength;

        Ray2DParam rayparam;
        int idx;

        /// <summary>
        /// </summary>
        protected QuadTraverseEnumerator(N root, Vector2f origin, Vector2f direction)
            : this(root, origin, direction, -1)
        { }

        /// <summary>
        /// </summary>
        protected QuadTraverseEnumerator(N root, Vector2f origin, Vector2f direction, float seglength)
            : base(root)
        {
            this.origin = origin;
            this.direction = direction;
            this.seglength = seglength;
            Reset();
        }

        /// <summary>
        /// Used to debug the stack memory used.
        /// </summary>
        public override int MaxStackSizeUsed
        {
            get { return stack.MaxStackSizeUsed; }
        }

        /// <summary>
        /// first add visible children or current node to stack, when return the first added
        /// </summary>
        public override bool MoveNext()
        {
            if (stack.Count == 0) return false;

            currententry = stack.Pop();
            current = currententry.node;

            // if current node contain children, find the children traversed by ray
            if (!currententry.node.IsLeaf)
            {
                int childmarked = addvisibility(currententry.node, currententry.param);
            }

            count++;
            return true;
        }

        /// <summary>
        /// Recompute first intersection ray's parameters. All children ray's parameter
        /// depend to it with a very small computation.
        /// </summary>
        public override void Reset()
        {
            current = null;
            stack.Clear();
            count = 0;
            if (processray(root, origin, direction, out rayparam))
            {
                currententry = new StackEntry(root, rayparam);
                stack.Push(currententry);
                addvisibility = gettraversed;
            }
        }
        /// <summary>
        /// NOT-PARALLEL version
        /// return the bitflag of node's children traversed by ray and update the stack with them
        /// </summary>
        int gettraversed(N node, Ray2DParam par)
        {
            int childmarked = 0;

            // generalization for parallel ray
            Vector2f center = node.Center;

            float txm = parallel != eAxis.Y ? (par.tx1 + par.tx0) * 0.5f : (origin.x < center.x) ? pINF : nINF;
            float tym = parallel != eAxis.X ? (par.ty1 + par.ty0) * 0.5f : (origin.y < center.y) ? pINF : nINF;

            // add visible children from q3 to q0 order because stack is LIFO
            if (tym > txm)
            {
                // Add Q3
                if (par.tx1 > tym)
                {
                    rayparam = new Ray2DParam(txm, tym, par.tx1, par.ty1);
                    if (testparam(rayparam))
                    {
                        idx = local2global(3);
                        childmarked |= 1 << idx; // of course must match with q3.index
                        stack.Push(new StackEntry(node.child[idx], rayparam));
                    }

                }

                // Add Q1
                rayparam = new Ray2DParam(txm, par.ty0, par.tx1, tym);
                if (testparam(rayparam))
                {
                    idx = local2global(1);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }
                // Add Q0
                if (txm > par.ty0)
                {
                    rayparam = new Ray2DParam(par.tx0, par.ty0, txm, tym);
                    if (testparam(rayparam))
                    {
                        idx = local2global(0);
                        childmarked |= 1 << idx;
                        stack.Push(new StackEntry(node.child[idx], rayparam));
                    }
                }
            }
            else
            {
                // Add Q3
                if (par.ty1 > txm)
                {
                    rayparam = new Ray2DParam(txm, tym, par.tx1, par.ty1);
                    if (testparam(rayparam))
                    {
                        idx = local2global(3);
                        childmarked |= 1 << idx;
                        stack.Push(new StackEntry(node.child[idx], rayparam));
                    }
                }

                // Add Q2
                rayparam = new Ray2DParam(par.tx0, tym, txm, par.ty1);
                if (testparam(rayparam))
                {
                    idx = local2global(2);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

                // Add Q0
                if (tym > par.tx0)
                {
                    rayparam = new Ray2DParam(par.tx0, par.ty0, txm, tym);
                    if (testparam(rayparam))
                    {
                        idx = local2global(0);
                        childmarked |= 1 << idx;
                        stack.Push(new StackEntry(node.child[idx], rayparam));
                    }
                }
            }
            return childmarked;
        }

        /// <summary>
        /// X-PARALLEL version more simple and efficent than paper generalization,
        /// but not implemented in octree
        /// </summary>
        int gettraversed_x(N node, Ray2DParam par)
        {
            int childmarked = 0;
            Vector2f center = node.Center;

            float txm = (par.tx1 + par.tx0) * 0.5f;

            // ray traverse bottom side
            if (origin.y < center.y)
            {
                // Add Q1
                rayparam = new Ray2DParam(txm, 0, par.tx1, 0);
                if (testParamX(rayparam))
                {
                    idx = local2global(1);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

                // Add Q0
                rayparam = new Ray2DParam(par.tx0, 0, txm, 0);
                if (testParamX(rayparam))
                {
                    idx = local2global(0);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }
            }
            // ray traverse top side
            else
            {
                // Add Q3
                rayparam = new Ray2DParam(txm, 0, par.tx1, 0);
                if (testParamX(rayparam))
                {
                    idx = local2global(3);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

                // Add Q2
                rayparam = new Ray2DParam(par.tx0, 0, txm, 0);
                if (testParamX(rayparam))
                {
                    idx = local2global(2);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }
            }
            return childmarked;
        }
        /// <summary>
        /// Y-PARALLEL version
        /// </summary>
        int gettraversed_y(N node, Ray2DParam par)
        {
            int childmarked = 0;
            Vector2f center = node.Center;

            float tym = (par.ty1 + par.ty0) * 0.5f;

            // ray traverse left side
            if (origin.x < center.x)
            {
                // but q0 and q2 are localized to ray direction and not match with real quads if direction is negative
                // so use XOR function
                // Add Q2
                rayparam = new Ray2DParam(0, tym, 0, par.ty1);
                if (testParamY(rayparam))
                {
                    idx = local2global(2);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

                // Add Q0
                rayparam = new Ray2DParam(0, par.ty0, 0, tym);
                if (testParamY(rayparam))
                {
                    idx = local2global(0);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

            }
            // ray traverse right side
            else
            {
                // Add Q3
                rayparam = new Ray2DParam(0, tym, 0, par.ty1);
                if (testParamY(rayparam))
                {
                    idx = local2global(3);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }

                // Add Q1
                rayparam = new Ray2DParam(0, par.ty0, 0, tym);
                if (testParamY(rayparam))
                {
                    idx = local2global(1);
                    childmarked |= 1 << idx;
                    stack.Push(new StackEntry(node.child[idx], rayparam));
                }
            }
            return childmarked;
        }

        /// <summary>
        /// Return the basic Rectangle-Ray intersection and store the result parameters.
        /// </summary>
        bool processray(N node, Vector2f orig, Vector2f dir, out Ray2DParam param)
        {
            // Parametric equation of ray = R(T) = O + t*D
            // t what intersect axis x0  tx0 = (x0-Ox)/Dx 
            float divx = 1.0f / dir.x;
            float divy = 1.0f / dir.y;

            param = new Ray2DParam();

            Vector2f min = node.Min;
            Vector2f max = node.Max;

            // with negative sign, 0 is considered positive
            if (dir.x < 0)
            {
                param.tx0 = (max.x - orig.x) * divx;
                param.tx1 = (min.x - orig.x) * divx;
                raysign |= 0x1;
            }
            // with positive sign
            else
            {
                param.tx0 = (min.x - orig.x) * divx;
                param.tx1 = (max.x - orig.x) * divx;
            }

            if (dir.y < 0)
            {
                param.ty0 = (max.y - orig.y) * divy;
                param.ty1 = (min.y - orig.y) * divy;
                raysign |= 0x2;
            }
            else
            {
                param.ty0 = (min.y - orig.y) * divy;
                param.ty1 = (max.y - orig.y) * divy;
            }

            // the algorithm is robust because work also for div = -INF or +INF
            // only if computer use this definition:
            // (x/0) =  x<0: -INF ;   x>0: +INF

            testparam = this.testParamXY;

            if (float.IsInfinity(param.tx0))
            {
                parallel |= eAxis.Y;
                // generalization for parallel ray, i noticed that work also without
                // this setting, but for safety i prefer set myself
                param.tx0 = nINF;
                param.tx1 = pINF;
                testparam = this.testParamY;
            }
            if (float.IsInfinity(param.ty0))
            {
                parallel |= eAxis.X;
                param.ty0 = nINF;
                param.ty1 = pINF;
                testparam = this.testParamX;
            }

            float tmin = Maths.Mathelp.MAX(param.tx0, param.ty0);
            float tmax = Maths.Mathelp.MIN(param.tx1, param.ty1);

            if (parallel == eAxis.XY)
                throw new ArithmeticException("a ray can not be parallel to both x and y axis");

            return testparam(param) && tmin < tmax;
        }

        /// <summary>
        /// Very important function, the ray-rectangle intersection algorithm orient the quads to ray direction and
        /// match only when direction's x and y are positive
        /// </summary>
        int local2global(byte qi)
        {
            //return signTable[qi];
            return qi ^ raysign;
        }

        /// <summary>
        /// Test it's ray or segment parameter, for line is always verified because is infinite
        /// </summary>
        protected virtual bool testParamXY(Ray2DParam param)
        {
            return testParamX(param) && testParamY(param);
        }
        protected abstract bool testParamX(Ray2DParam param);
        protected abstract bool testParamY(Ray2DParam param);

        /// <summary>
        /// Is usefull store all 4 ray parameters in one struct
        /// </summary>
        protected struct Ray2DParam
        {
            public const int sizeinbyte = sizeof(float) * 4;

            public float tx0, ty0, tx1, ty1;

            public Ray2DParam(float txmin, float tymin, float txmax, float tymax)
            {
                tx0 = txmin;
                ty0 = tymin;
                tx1 = txmax;
                ty1 = tymax;
            }

            public override string ToString()
            {
                return string.Format("x0:{0} x1:{1} y0:{0} y1:{1}", tx0, tx1, ty0, ty1);
            }
        }

        /// <summary>
        /// Stack queue must contain also precomputed ray parameters
        /// </summary>
        struct StackEntry
        {
            public N node;
            public Ray2DParam param;
            public StackEntry(N node, Ray2DParam param)
            {
                this.node = node;
                this.param = param;
            }
            public override string ToString()
            {
                if (node == null) return "Undefined";

                return "Entry: " + node.ToString();
            }
        }
    }

    public class QuadRayTraceEnumerator<N,T> : QuadTraverseEnumerator<N,T> 
        where N : QuadNode<N,T>, IRectangleAA, new()
        where T : Quadtree<N,T>

    {
        public QuadRayTraceEnumerator(N root, Ray2D ray)
            : base(root, ray.orig, ray.dir)
        { }

        protected override bool testParamX(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            return param.tx1 > 0;
        }
        protected override bool testParamY(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            return param.ty1 > 0;
        }
    }
    public class QuadLineTraceEnumerator<N,T> : QuadTraverseEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA, new()
        where T : Quadtree<N, T>
    {
        public QuadLineTraceEnumerator(N root, Line2D line)
            : base(root, line.orig, line.dir)
        { }

        protected override bool testParamXY(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            return true;
        }
        protected override bool testParamX(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            return true;
        }
        protected override bool testParamY(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            return true;
        }
    }
    public class QuadSegTraceEnumerator<N,T> : QuadTraverseEnumerator<N,T>
        where N : QuadNode<N, T>, IRectangleAA, new()
        where T : Quadtree<N, T>
    {
        public QuadSegTraceEnumerator(N root, Segment2D seg)
            : base(root, seg.orig, seg.dir, seg.length)
        { }

        protected override bool testParamX(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            if (param.tx0 < 0)
            {
                return param.tx1 > 0;
            }
            else if (param.tx0 < seglength)
            {
                return true;
            }
            return false;
        }
        protected override bool testParamY(QuadTraverseEnumerator<N,T>.Ray2DParam param)
        {
            if (param.ty0 < 0)
            {
                return param.ty1 > 0;
            }
            else if (param.ty0 < seglength)
            {
                return true;
            }
            return false;
        }
    }
}
