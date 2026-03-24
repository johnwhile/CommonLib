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
    public enum BvhIdx : byte
    {
        None = 0,    
        Left = 1,
        Right = 2,
        All = 3,
    }

    public abstract class BvhNode
    {
        public int subnodecount = 0;

        public static int InstanceCounter = 0;
        public const int AvarageSizeInByte = 0;

        public sbyte level, index;
        public int nodeid;


        public bool IsLeaf
        {
            get { return level == 0; }
        }
        /// <summary>
        /// Return the total count of nodes tree that this node contain
        /// </summary>
        public int SubNodeCount
        {
            get { return subnodecount; }
        }

        /// <summary>
        /// Empty initialization, hide it from user
        /// </summary>
        public BvhNode()
        {
            InstanceCounter++;
        }

        ~BvhNode()
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
            return string.Format("Bvhnode{0}_id{1}_l{2}", index, nodeid, level);
        }

        public abstract void Destroy();

    }


    /// <summary>
    /// Base linkable node
    /// </summary>
    public abstract class BvhNode<T> : BvhNode where T : BvhNode<T>
    {
        public T parent;
        public T left, right;
        public BvhTree<T> main;

        protected void SetAsRoot(BvhTree<T> main)
        {
            this.main = main;
            this.parent = null;
            this.level = (sbyte)(main.Depth - 1);
            this.subnodecount = 0;
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
            this.nodeid = (parent.nodeid << 3) + (index + 1);
            this.main.NodeCount++;


            T current = parent;
            while (current != null) { current.subnodecount += this.subnodecount; current = current.parent; }
        }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public BvhNode()
            : base()
        {
        }

        /// <summary>
        /// Initialization for root
        /// </summary>
        public BvhNode(BvhTree<T> main)
            : base()
        {
            SetAsRoot(main);
        }

        /// <summary>
        /// Intialization for node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public BvhNode(T parent, int index)
            : base()
        {
            SetAsNode(parent, index);
        }


        public override void Destroy()
        {
            if (left != null) left.Destroy();
            if (right != null) right.Destroy();
            left = null;
            right = null;
            main.NodeCount--;
        }

        /// <summary>
        /// Only for debug purpose
        /// </summary>
        public TreeNode GetDebugView()
        {
            TreeNode node = new TreeNode(string.Format("ID {0} , level {1} , class {2}", nodeid, level, this.GetType()));
            if (left != null) node.Nodes.Add(left.GetDebugView());
            if (right != null) node.Nodes.Add(right.GetDebugView());  
            return node;
        }
    }
}
