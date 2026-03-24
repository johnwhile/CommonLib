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
    /// root node is always first, after the children returned are 1,2,4 or 4,2,1 if ray direction is opposite
    /// 
    /// For line generalization we assume the origin always out of node boundary
    /// </remarks>
    public abstract class OctTraverseEnumerator<T> : OctEnumerator<T> where T : OctNode<T> , IAABBox
    {
        const float pINF = float.PositiveInfinity;
        const float nINF = float.NegativeInfinity;
        const byte X = 1;
        const byte Y = 2;
        const byte Z = 4;
        const byte XorY = X | Y; //3
        const byte XorZ = X | Z; //5
        const byte YorZ = Y | Z; //6
        const byte Exit = 8;
        //plane
        const byte YZ = 0;
        const byte XZ = 1;
        const byte XY = 2; 

        // where 8 mean Exit
        static byte[,] nextNodeTable = new byte[,]
        {
            { 4, 2, 1 },
            { 5, 3, 8 },
            { 6, 8, 3 }, 
            { 7, 8, 8 },
            { 8, 6, 5 },
            { 8, 7, 8 },
            { 8, 8, 7 },
            { 8, 8, 8 } 
        };

        MyStack<StackEntry> stack = new MyStack<StackEntry>(10);
        StackEntry currententry;

        byte raysign; // negative sign, make the mirrors operations, to work need to set X:100 Y:010 Z:001
        byte perp; // perpendicularity for degenerate direction

        delegate int AddVisibilityFunction(T parent, ref Ray3DParam param);
        delegate bool TestParamFunction(ref Ray3DParam param);
        TestParamFunction testparam = null;
        AddVisibilityFunction addvisibility = null;


        // all ray-line-segment implementation contain this generic values
        protected Vector3f origin, direction;
        protected float seglength;

       // Ray3DParam tmp_param;

        /// <summary>
        /// </summary>
        protected OctTraverseEnumerator(T root, Vector3f origin, Vector3f direction, float seglength)
            : base(root)
        {
            initialize(origin, direction, seglength);
        }

        /// <summary>
        /// re-initialize the ray parameters
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="seglength"></param>
        protected void initialize(Vector3f origin, Vector3f direction, float seglength)
        {
            this.perp = 0;
            this.raysign = 0;
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
                int childmarked = addvisibility(currententry.node, ref currententry.param);
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
            Ray3DParam param;
            if (processray(root, origin, direction, out param))
            {
                currententry = new StackEntry(root, ref param);
                stack.Push(currententry);
                addvisibility = gettraversed;
            }
        }
        /// <summary>
        /// return the bitflag of node's children traversed by ray and update the stack with them
        /// </summary>
        int gettraversed(T node, ref Ray3DParam par)
        {
            int childmarked = 0;

            eAxis flags = (eAxis)perp;

            // generalization for parallel ray
            float txm = (perp & X) != 0 ? (origin.x < node.Center.x) ? pINF : nINF : (par.tx1 + par.tx0) * 0.5f;
            float tym = (perp & Y) != 0 ? (origin.y < node.Center.y) ? pINF : nINF : (par.ty1 + par.ty0) * 0.5f;
            float tzm = (perp & Z) != 0 ? (origin.z < node.Center.z) ? pINF : nINF : (par.tz1 + par.tz0) * 0.5f;


            byte currnode = getFirstNode(ref par, txm, tym, tzm);

            do
            {
                Ray3DParam tmp_param;
                switch (currnode)
                {
                    case 0: tmp_param = new Ray3DParam(par.tx0, par.ty0, par.tz0, txm, tym, tzm); break;
                    case 1: tmp_param = new Ray3DParam(par.tx0, par.ty0, tzm, txm, tym, par.tz1); break;
                    case 2: tmp_param = new Ray3DParam(par.tx0, tym, par.tz0, txm, par.ty1, tzm); break;
                    case 3: tmp_param = new Ray3DParam(par.tx0, tym, tzm, txm, par.ty1, par.tz1); break;
                    case 4: tmp_param = new Ray3DParam(txm, par.ty0, par.tz0, par.tx1, tym, tzm); break;
                    case 5: tmp_param = new Ray3DParam(txm, par.ty0, tzm, par.tx1, tym, par.tz1); break;
                    case 6: tmp_param = new Ray3DParam(txm, tym, par.tz0, par.tx1, par.ty1, tzm); break;
                    case 7: tmp_param = new Ray3DParam(txm, tym, tzm, par.tx1, par.ty1, par.tz1); break;
                    default: throw new ArgumentException();
                }

                if (testparam(ref tmp_param))
                {
                    int qi = local2global(currnode);
                    stack.Push(new StackEntry(node.child[qi], ref tmp_param));
                }
                currnode = getNextNode(currnode, ref tmp_param);
            }
            while (currnode != Exit);
            
            return childmarked;
        }

        /// <summary>
        /// </summary>
        byte getNextNode(byte currentnode, ref Ray3DParam rparam)
        {
            byte plane = getExitPlane(rparam.tx1, rparam.ty1, rparam.tz1);
            return nextNodeTable[currentnode, plane];
        }

        /// <summary>
        /// return the index of first node, in local coordinate
        /// <para>XY 	txM &lt; tz0 : 2 tyM &lt; tz0 : 1</para>
        /// <para>YZ 	tyM &lt; tx0 : 1 tzM &lt; tx0 : 0</para>
        /// <para>XZ 	txM &lt; ty0 : 2 tzM &lt; ty0 : 0</para> 
        /// </summary>
        byte getFirstNode(ref Ray3DParam par, float txm, float tym, float tzm)
        {
            byte plane = getEntryPlane(ref par);
            
            byte bit = 0;
            
            switch (plane)
            {
                case XY:
                    if (txm < par.tz0) bit |= 4; // Z
                    if (tym < par.tz0) bit |= 2; // Y
                    break;
                case YZ:
                    if (tym < par.tx0) bit |= 2; // Y
                    if (tzm < par.tx0) bit |= 1; // X
                    break;
                case XZ:
                    if (txm < par.ty0) bit |= 4; // Z
                    if (tzm < par.ty0) bit |= 1; // X
                    break;
            }
            return bit;
        }

        /// <summary>
        /// Get the plane where ray entrer the box, 
        /// </summary>
        /// <remarks>
        /// according with paper the table for MAX(tx0,ty0,tz0)
        /// tx0 : YZ
        /// ty0 : XZ
        /// tz0 : YZ
        /// </remarks>
        byte getEntryPlane(ref Ray3DParam par)
        {
            if (par.tx0 > par.ty0)
            {
                if (par.tx0 > par.tz0) return YZ; // tx0
                else return XY; //tz0
            }
            else
            {
                if (par.ty0 > par.tz0) return XZ; // ty0
                else return XY; //tz0
            }
        }
        /// <summary>
        /// Get the plane where ray exit the box, 
        /// </summary>
        /// <remarks>
        /// tx1 : YZ
        /// ty1 : XZ
        /// tz1 : XY
        /// </remarks>
        byte getExitPlane(float tx1, float ty1, float tz1)
        {
            if (tx1 < ty1)
            {
                if (tx1 < tz1) return YZ; // tx1
                else return XY; //tz1
            }
            else
            {
                if (ty1 < tz1) return XZ; // ty1
                else return XY; //tz1
            }
        }

        /// <summary>
        /// Return the basic Rectangle-Ray intersection and store the result parameters.
        /// </summary>
        bool processray(T node, Vector3f orig, Vector3f dir, out Ray3DParam param)
        {
            // Parametric equation of ray = R(T) = O + t*D
            // t what intersect axis x0  tx0 = (x0-Ox)/Dx 
            float divx = 1.0f / dir.x;
            float divy = 1.0f / dir.y;
            float divz = 1.0f / dir.z;

            param = new Ray3DParam();

            Vector3f min = node.Min;
            Vector3f max = node.Max;

            // with negative sign, 0 is considered positive
            if (dir.x < 0)
            {
                param.tx0 = (max.x - orig.x) * divx;
                param.tx1 = (min.x - orig.x) * divx;
                raysign |= 4;
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
                raysign |= 2;
            }
            else
            {
                param.ty0 = (min.y - orig.y) * divy;
                param.ty1 = (max.y - orig.y) * divy;
            }
            if (dir.z < 0)
            {
                param.tz0 = (max.z - orig.z) * divz;
                param.tz1 = (min.z - orig.z) * divz;
                raysign |= 1;
            }
            else
            {
                param.tz0 = (min.z - orig.z) * divz;
                param.tz1 = (max.z - orig.z) * divz;
            }

            // the algorithm is robust because work also for div = -INF or +INF
            // only if computer use this definition:
            // (x/0) =  x<0: -INF ;   x>0: +INF

            
            // generalization for parallel ray, i noticed that work also without
            // this setting, but for safety i prefer set myself
            if (float.IsInfinity(param.tx0))
            {
                perp |= X;
                param.tx0 = nINF;
                param.tx1 = pINF;
            }
            if (float.IsInfinity(param.ty0))
            {
                perp |= Y;
                param.ty0 = nINF;
                param.ty1 = pINF;
            }
            if (float.IsInfinity(param.tz0))
            {
                perp |= Z;
                param.tz0 = nINF;
                param.tz1 = pINF;
            }

            switch (perp)
            {
                case 0: testparam = testParamXYZ; break;
                case X: testparam = testParamYZ; break;
                case Y: testparam = testParamXZ; break;
                case Z: testparam = testParamXY; break;
                case X | Y: testparam = testParamZ; break;
                case X | Z: testparam = testParamY; break;
                case Y | Z: testparam = testParamZ; break;
                default: throw new ArgumentException();
            }

            float tmin = Maths.Mathelp.MAX(param.tx0, param.ty0, param.tz0);
            float tmax = Maths.Mathelp.MIN(param.tx1, param.ty1, param.tz1);

            return testparam(ref param) && tmin < tmax;
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
        /// Test is ray or segment parameter, for line is always verified because is infinite
        /// </summary>
        protected virtual bool testParamXYZ(ref Ray3DParam param)
        {
            return testParamX(ref param) && testParamY(ref param) && testParamZ(ref param);
        }
        protected virtual bool testParamXY(ref Ray3DParam param)
        {
            return testParamX(ref param) && testParamY(ref param);
        }
        protected virtual bool testParamXZ(ref Ray3DParam param)
        {
            return testParamX(ref param) && testParamZ(ref param);
        }
        protected virtual bool testParamYZ(ref Ray3DParam param)
        {
            return testParamY(ref param) && testParamZ(ref param);
        }
        protected abstract bool testParamX(ref Ray3DParam param);
        protected abstract bool testParamY(ref Ray3DParam param);
        protected abstract bool testParamZ(ref Ray3DParam param);

        /// <summary>
        /// Is usefull store all 6 ray parameters (+ 3 middle values) in one struct
        /// </summary>
        public struct Ray3DParam
        {
            public const int sizeinbyte = sizeof(float) * 6;

            public float tx0, ty0, tz0, tx1, ty1, tz1;

            public Ray3DParam(float txmin, float tymin, float tzmin, float txmax, float tymax, float tzmax)
            {
                tx0 = txmin;
                ty0 = tymin;
                tz0 = tzmin;
                tx1 = txmax;
                ty1 = tymax;
                tz1 = tzmax;
            }

            public override string ToString()
            {
                string str = "";
                str += string.Format("x0:{0} x1:{1}", tx0, tx1);
                str += string.Format("y0:{0} y1:{1}", ty0, ty1);
                str += string.Format("z0:{0} z1:{1}", tz0, tz1);
                return str;
            }
        }

        /// <summary>
        /// Stack queue must contain also precomputed ray parameters
        /// </summary>
        struct StackEntry
        {
            public T node;
            public Ray3DParam param;
            public StackEntry(T node, ref Ray3DParam param)
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

    public class OctRayTraceEnumerator<T> : OctTraverseEnumerator<T> where T : OctNode<T> , IAABBox
    {
        /// <summary>
        /// You can set a new ray without create new instance, this generate a Reset()
        /// </summary>
        /// <param name="ray"></param>
        public void SetNewParameters(Ray ray)
        {
            base.initialize(ray.orig, ray.dir, -1);
        }

        public OctRayTraceEnumerator(T root, Ray ray)
            : base(root, ray.orig, ray.dir , -1)
        { }

        protected override bool testParamX(ref Ray3DParam param)
        {
            return param.tx1 > 0;
        }
        protected override bool testParamY(ref Ray3DParam param)
        {
            return param.ty1 > 0;
        }
        protected override bool testParamZ(ref Ray3DParam param)
        {
            return param.tz1 > 0;
        }
    }
    
    public class OctLineTraceEnumerator<T> : OctTraverseEnumerator<T> where T : OctNode<T> , IAABBox
    {
        /// <summary>
        /// You can set a new ray without create new instance, this generate a Reset()
        /// </summary>
        /// <param name="ray"></param>
        public void SetNewParameters(Line line)
        {
            base.initialize(line.orig, line.dir, -1);
        }

        public OctLineTraceEnumerator(T root, Line line)
            : base(root, line.orig, line.dir , -1)
        { }

        protected override bool testParamXYZ(ref Ray3DParam param)
        {
            return true;
        }
        protected override bool testParamX(ref Ray3DParam param)
        {
            return true;
        }
        protected override bool testParamY(ref Ray3DParam param)
        {
            return true;
        }
        protected override bool testParamZ(ref Ray3DParam param)
        {
            return true;
        }
    }
    
    public class OctSegTraceEnumerator<T> : OctTraverseEnumerator<T> where T : OctNode<T> , IAABBox
    {
        /// <summary>
        /// You can set a new ray without create new instance, this generate a Reset()
        /// </summary>
        /// <param name="ray"></param>
        public void SetNewParameters(Segment seg)
        {
            base.initialize(seg.orig, seg.dir, seg.length);
        }

        public OctSegTraceEnumerator(T root, Segment seg)
            : base(root, seg.orig, seg.dir, seg.length)
        { }

        protected override bool testParamX(ref Ray3DParam param)
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
        protected override bool testParamY(ref Ray3DParam param)
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
        protected override bool testParamZ(ref Ray3DParam param)
        {
            if (param.tz0 < 0)
            {
                return param.tz1 > 0;
            }
            else if (param.tz0 < seglength)
            {
                return true;
            }
            return false;
        }
    }
}
