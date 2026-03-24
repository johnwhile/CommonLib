using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common.Maths;
using Common.Tools;


namespace Common.Partitions
{
    [Flags]
    public enum OctIdx : byte
    {
        None = 0,
        All = Q0 | Q1 | Q2 | Q3 | Q4 | Q5 | Q6 | Q7,
        Q0 = 1,
        Q1 = 2,
        Q2 = 4,
        Q3 = 8,
        Q4 = 16,
        Q5 = 32,
        Q6 = 64,
        Q7 = 128
    }


    public abstract class OctNode
    {
        public static int InstanceCounter = 0;
        public const int AvarageSizeInByte = 0;

        public const byte ALL = 0xFF;
        public const byte Q0 = 1 << 0;
        public const byte Q1 = 1 << 1;
        public const byte Q2 = 1 << 2;
        public const byte Q3 = 1 << 3;
        public const byte Q4 = 1 << 4;
        public const byte Q5 = 1 << 5;
        public const byte Q6 = 1 << 6;
        public const byte Q7 = 1 << 7;

        public sbyte level, index;
        public int nodeid;

        protected byte childrenFlag = 0;

        public bool IsLeaf
        {
            get { return level == 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public OctIdx childused
        {
            get { return (OctIdx)childrenFlag; }
        }

        /// <summary>
        /// Empty initialization, hide it from user
        /// </summary>
        public OctNode()
        {
            InstanceCounter++;
        }

        ~OctNode()
        {
            InstanceCounter--;
        }
        /// <summary>
        /// This formula return the nodeid of parent
        /// </summary>
        public static int GetParentById(int nodeid)
        {
            throw new NotImplementedException();
            //return (nodeid - 1) >> 3;
        }
        /// <summary>
        /// This formula return the index of nodeid
        /// </summary>
        public static int GetIndexById(int nodeid)
        {
            throw new NotImplementedException();
            //return (nodeid - 1) & 5;
        }

        public override string ToString()
        {
            return string.Format("Octant{0}_id{1}_l{2}", index, nodeid, level);
        }

        public abstract void Destroy();

    }


    /// <summary>
    /// Base linkable node
    /// </summary>
    public abstract class OctNode<T> : OctNode where T : OctNode<T>
    {
        public T parent;
        public T[] child;
        public Octree<T> main;

        protected void SetAsRoot(Octree<T> main)
        {
            this.main = main;
            this.parent = null;
            this.level = (sbyte)(main.Depth - 1);
            this.nodeid = 0;
            this.index = -1;
            this.main.NodeCount++;
        }

        protected void SetAsNode(T parent, int index)
        {
            this.parent = parent;
            this.main = parent.main;
            this.level = (sbyte)(parent.level - 1);
            this.index = (sbyte)index;
            this.parent.childrenFlag |= (byte)(1 << index);
            this.nodeid = (parent.nodeid << 3) + (index + 1);
            this.main.NodeCount++;
        }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public OctNode()
            : base()
        {
        }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public OctNode(Octree<T> main)
            : base()
        {
            SetAsRoot(main);
        }

        /// <summary>
        /// Intialization for node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public OctNode(T parent, int index)
            : base()
        {
            SetAsNode(parent, index);
        }


        /*
        /// <summary>
        /// Intialization for node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public QuadNode(T parent, sbyte index)
            : base()
        {
            Vector2 c = parent.size.center;
            Vector2 hs = parent.size.halfsize * 0.5f;

            c.x = ((index & 1) != 0) ? c.x - hs.x : c.x + hs.x;
            c.y = ((index & 2) != 0) ? c.y - hs.y : c.y + hs.y;

            SetParams(parent, index, size);
        }


        public QuadNode(T parent, sbyte index, float cx, float cy, float hx, float hy) :
            this(parent, index, new RectangleAA2(cx, cy, hx, hy))
        { }


        public static T New<T>(Quadtree main, RectangleAA2 size) where T : QuadNode, new()
        {
            T node = new T();
            node.SetParams(main, size);
            return node;
        }
        public static T New<T>(QuadNode parent, sbyte index, RectangleAA2 size) where T : QuadNode, new()
        {
            T node = new T();
            node.SetParams(parent, index, size);
            return node;
        }
        public static T New<T>(QuadNode parent, sbyte index, float cx, float cy, float hx, float hy) where T : QuadNode, new()
        {
            T node = new T();
            node.SetParams(parent, index, new RectangleAA2(cx, cy, hx, hy));
            return node;
        }
        */


        public override void Destroy()
        {
            if (child != null) foreach (T node in child) if (node != null) node.Destroy();
            child = null;
            main.NodeCount--;
        }

        /*
        /// <summary>
        /// Split node in 8 children. You can pass as parameter the type of Node because can use different node implementations,
        /// example you can use a custom implementation only for leaf nodes where parents require only the base implementations
        /// </summary>
        /// <typeparam name="T">the type of OctNode</typeparam>
        public void Split<T>() where T : QuadNode, new()
        {
            childrenFlag = ALL;
            float cx = size.center.x;
            float cy = size.center.y;
            float hx = size.halfsize.x * 0.5f;
            float hy = size.halfsize.y * 0.5f;

            if (child == null) child = new T[4];
            child[0] = QuadNode.New<T>(this, 0, new RectangleAA2( cx - hx, cy - hy, hx, hy));
            child[1] = QuadNode.New<T>(this, 1,new RectangleAA2( cx + hx, cy - hy, hx, hy));
            child[2] = QuadNode.New<T>(this, 2, new RectangleAA2(cx - hx, cy + hy, hx, hy));
            child[3] = QuadNode.New<T>(this, 3, new RectangleAA2(cx + hx, cy + hy, hx, hy));

        }
        /// <summary>
        /// The reasond of this implementation is to avoid the worst performance of generic's 'new()' when you are splitting 
        /// an intermediates node
        /// </summary>
        public void Split()
        {
            childrenFlag = ALL;
            float cx = size.center.x;
            float cy = size.center.y;
            float hx = size.halfsize.x * 0.5f;
            float hy = size.halfsize.y * 0.5f;

            if (child == null) child = new QuadNode[4];
            child[0] = new QuadNode(this, 0, cx - hx, cy - hy, hx, hy);
            child[1] = new QuadNode(this, 1, cx + hx, cy - hy, hx, hy);
            child[2] = new QuadNode(this, 2, cx - hx, cy + hy, hx, hy);
            child[3] = new QuadNode(this, 3, cx + hx, cy + hy, hx, hy);
        }

        /// <summary>
        /// Recursive splitting, T node are assigned only at last level , the param 'T' have a performance penality
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RecursiveSplit<T>(bool onlyleaf) where T : QuadNode, new()
        {
            if (level > 0)
            {
                if (level > 1 && onlyleaf) Split(); else Split<T>();
                foreach (QuadNode node in child) node.RecursiveSplit<T>(onlyleaf);
            }
        }
        /// <summary>
        /// Recursive splitting
        /// </summary>
        public void RecursiveSplit()
        {
            if (level > 0)
            {
                Split();
                foreach (QuadNode node in child) node.RecursiveSplit();
            }
        }
        */

        /// <summary>
        /// Only for debug purpose
        /// </summary>
        public TreeNode GetDebugView()
        {
            TreeNode node = new TreeNode(string.Format("ID {0} , level {1} , class {2}", nodeid, level, this.GetType()));

            for (int i = 0, mask = 1; i < 8; i++, mask <<= 1)
            {
                if ((childrenFlag & mask) != 0) node.Nodes.Add(child[i].GetDebugView());
            }
            return node;
        }
    }









    /*
    /// <summary>
    /// Base linkable node
    /// </summary>
    public class OctNode
    {
        public static int InstanceCounter = 0;
        public const int AvarageSizeInByte = 0;

        public const byte ALL = 0xFF;
        public const byte Q0 = 1 << 0;
        public const byte Q1 = 1 << 1;
        public const byte Q2 = 1 << 2;
        public const byte Q3 = 1 << 3;
        public const byte Q4 = 1 << 4;
        public const byte Q5 = 1 << 5;
        public const byte Q6 = 1 << 6;
        public const byte Q7 = 1 << 7;

        protected byte childrenFlag = 0;

        public BoxAA2 size;

        public OctTree main;
        public OctNode parent;
        public OctNode[] child;

        public sbyte level, index;

        public int nodeid;

        public bool IsLeaf
        {
            get { return level == 0; }
        }
        public OctIdx childused
        {
            get { return (OctIdx)childrenFlag; }
        }

        /// <summary>
        /// Empty initialization, hide it from user
        /// </summary>
        public OctNode()
        {
            InstanceCounter++;
        }

        ~OctNode()
        {
            InstanceCounter--;
        }

        private void SetParams(OctTree main, BoxAA2 size)
        {
            this.main = main;
            this.parent = null;
            this.level = (sbyte)(main.Depth - 1);
            this.size = size;
            this.nodeid = 0;
            this.index = -1;
            main.NodeCount++;
        }
        private void SetParams(OctNode parent, sbyte index, BoxAA2 size)
        {
            this.parent = parent;
            this.main = parent.main;
            this.level = (sbyte)(parent.level - 1);
            this.index = index;
            this.size = size;
            this.parent.childrenFlag |= (byte)(1 << index);
            this.nodeid = (parent.nodeid << 3) + (index + 1);
            main.NodeCount++;
        }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public OctNode(OctTree main, BoxAA2 size)
            : this()
        {
            SetParams(main, size);
        }

        /// <summary>
        /// Intialization for node
        /// </summary>
        /// <param name="index">from 0 to 7 it define the child id</param>
        public OctNode(OctNode parent, sbyte index, BoxAA2 size)
            : this()
        {
            SetParams(parent, index, size);
        }
        /// <summary>
        /// Intialization for node
        /// </summary>
        /// <param name="index">from 0 to 7 it define the child id</param>
        public OctNode(OctNode parent, sbyte index)
            : this()
        {
            Vector3 c = parent.size.center;
            Vector3 hs = parent.size.halfsize * 0.5f;

            c.x = ((index & 1) != 0) ? c.x - hs.x : c.x + hs.x;
            c.y = ((index & 2) != 0) ? c.y - hs.y : c.y + hs.y;
            c.z = ((index & 4) != 0) ? c.z - hs.z : c.z + hs.z;

            SetParams(parent, index, size);
        }

        public static T New<T>(OctTree main, BoxAA2 size) where T : OctNode, new()
        {
            T node = new T();
            node.SetParams(main, size);
            return node;
        }

        public static T New<T>(OctNode parent, sbyte index, BoxAA2 size) where T : OctNode, new()
        {
            T node = new T();
            node.SetParams(parent, index, size);
            return node;
        }
        public static T New<T>(OctNode parent, sbyte index, float cx, float cy, float cz, float hx, float hy, float hz) where T : OctNode, new()
        {
            T node = new T();
            node.SetParams(parent, index, new BoxAA2(cx, cy, cz, hx, hy, hz));
            return node;
        }

        public void Destroy()
        {
            if (child != null) foreach (OctNode node in child) if (node != null) node.Destroy();
            child = null;
            main.NodeCount--;
        }

        /// <summary>
        /// Split node in 8 children. You can pass as parameter the type of Node because can use different node implementations,
        /// example you can use a custom implementation only for leaf nodes where parents require only the base implementations
        /// </summary>
        /// <typeparam name="T">the type of OctNode</typeparam>
        public void Split<T>() where T : OctNode, new()
        {
            childrenFlag = ALL;
            float cx = size.center.x;
            float cy = size.center.y;
            float cz = size.center.z;
            float hx = size.halfsize.x * 0.5f;
            float hy = size.halfsize.y * 0.5f;
            float hz = size.halfsize.z * 0.5f;

            if (child == null) child = new T[8];
            child[0] = OctNode.New<T>(this, 0, cx - hx, cy - hy, cz - hz, hx, hy, hz);
            child[1] = OctNode.New<T>(this, 1, cx - hx, cy - hy, cz + hz, hx, hy, hz);
            child[2] = OctNode.New<T>(this, 2, cx - hx, cy + hy, cz - hz, hx, hy, hz);
            child[3] = OctNode.New<T>(this, 3, cx - hx, cy + hy, cz + hz, hx, hy, hz);
            child[4] = OctNode.New<T>(this, 4, cx + hx, cy - hy, cz - hz, hx, hy, hz);
            child[5] = OctNode.New<T>(this, 5, cx + hx, cy - hy, cz + hz, hx, hy, hz);
            child[6] = OctNode.New<T>(this, 6, cx + hx, cy + hy, cz - hz, hx, hy, hz);
            child[7] = OctNode.New<T>(this, 7, cx + hx, cy + hy, cz + hz, hx, hy, hz);
        }
        /// <summary>
        /// The reasond of this implementation is to avoid the worst performance of generic's 'new()' when you are splitting 
        /// an intermediates node
        /// </summary>
        public void Split()
        {
            childrenFlag = ALL;
            float cx = size.center.x;
            float cy = size.center.y;
            float cz = size.center.z;
            float hx = size.halfsize.x * 0.5f;
            float hy = size.halfsize.y * 0.5f;
            float hz = size.halfsize.z * 0.5f;
            if (child == null) child = new OctNode[8];
            child[0] = new OctNode(this, 0, new BoxAA2(cx - hx, cy - hy, cz - hz, hx, hy, hz));
            child[1] = new OctNode(this, 1, new BoxAA2(cx - hx, cy - hy, cz + hz, hx, hy, hz));
            child[2] = new OctNode(this, 2, new BoxAA2(cx - hx, cy + hy, cz - hz, hx, hy, hz));
            child[3] = new OctNode(this, 3, new BoxAA2(cx - hx, cy + hy, cz + hz, hx, hy, hz));
            child[4] = new OctNode(this, 4, new BoxAA2(cx + hx, cy - hy, cz - hz, hx, hy, hz));
            child[5] = new OctNode(this, 5, new BoxAA2(cx + hx, cy - hy, cz + hz, hx, hy, hz));
            child[6] = new OctNode(this, 6, new BoxAA2(cx + hx, cy + hy, cz - hz, hx, hy, hz));
            child[7] = new OctNode(this, 7, new BoxAA2(cx + hx, cy + hy, cz + hz, hx, hy, hz));
        }

        /// <summary>
        /// Recursive splitting, T node are assigned only at last level , the param 'T' have a performance penality
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RecursiveSplit<T>(bool onlyleaf) where T : OctNode, new()
        {
            if (level > 0)
            {
                if (level > 1 && onlyleaf) Split(); else Split<T>();
                foreach (OctNode node in child) node.RecursiveSplit<T>(onlyleaf);
            }
        }
        /// <summary>
        /// Recursive splitting
        /// </summary>
        public void RecursiveSplit()
        {
            if (level > 0)
            {
                Split();
                foreach (OctNode node in child) node.RecursiveSplit();
            }
        }

        /// <summary>
        /// Only for debug purpose
        /// </summary>
        public TreeNode GetDebugView()
        {
            TreeNode node = new TreeNode(string.Format("ID {0} , level {1} , class {2}", nodeid, level, this.GetType()));

            for (int i = 0, mask = 1; i < 8; i++, mask <<= 1)
            {
                if ((childrenFlag & mask) != 0) node.Nodes.Add(child[i].GetDebugView());
            }
            return node;
        }
        /// <summary>
        /// Remember what child size can be derived by its index
        /// </summary>
        public static Vector3 GetCenterByQuadIndex(Vector3 center, Vector3 halfsize, int index)
        {
            Vector3 c = center;
            Vector3 hs = halfsize;
            c.x = ((index & 1) != 0) ? c.x - hs.x * 0.5f : c.x + hs.x * 0.5f;
            c.y = ((index & 2) != 0) ? c.y - hs.y * 0.5f : c.y + hs.y * 0.5f;
            c.z = ((index & 4) != 0) ? c.z - hs.z * 0.5f : c.z + hs.z * 0.5f;
            return c;
        }

        /// <summary>
        /// This formula return the nodeid of parent
        /// </summary>
        public static int GetParentById(int nodeid)
        {
            return (nodeid - 1) >> 3;
        }
        /// <summary>
        /// This formula return the index of nodeid
        /// </summary>
        public static int GetIndexById(int nodeid)
        {
            return (nodeid - 1) & 7;
        }

        public override string ToString()
        {
            return string.Format("Oct{0}_{1}", index, nodeid);
        }
    }

    /// <summary>
    /// </summary>
    public class OctNode<T> : OctNode
    {
        public T obj;

        public OctNode()
            : base()
        { }

        public OctNode(OctTree main, BoxAA2 size)
            : base(main, size)
        { }
        public OctNode(OctNode parent, sbyte index, BoxAA2 size)
            : base(parent, index, size)
        { }
    }
     * */
}
