using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common.Maths;
using Common.Tools;

namespace Common.Partitions
{
    /// <summary>
    /// For example purpose
    /// </summary>
    public class BvhBoxNode : BvhNode<BvhBoxNode>, IAABBox
    {
        protected BoundingBoxCenter size;

        public Vector3f Max { get { return size.Max; } }
        public Vector3f Min { get { return size.Min; } }
        public Vector3f Center { get { return size.center; } }
        public Vector3f HalfSize { get { return size.extend; } }
        public Vector3f Size => Max - Min;

        /// <summary>
        /// Fake initialization
        /// </summary>
        public BvhBoxNode()
            : base() {}

        /// <summary>
        /// Initialization as root node
        /// </summary>
        public BvhBoxNode(BvhTree<BvhBoxNode> main, BoundingBoxCenter size)
            : base(main)
        {
            this.size = size;
        }

        /// <summary>
        /// Initialization as child node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public BvhBoxNode(BvhBoxNode parent, int index, BoundingBoxCenter size)
            : base()
        {
            SetAsNode(parent, index);
            this.size = size;
        }

        /// <summary>
        /// As child node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public BvhBoxNode(BvhBoxNode parent, sbyte index, float cx, float cy, float cz, float hx, float hy, float hz) :
            this(parent, index, new BoundingBoxCenter(cx, cy,cz, hx, hy,hz)) { }

    }


}
