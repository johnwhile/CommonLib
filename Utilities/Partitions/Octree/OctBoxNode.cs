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
    public class OctBoxNode : OctNode<OctBoxNode>, IAABBox
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
        public OctBoxNode()
            : base() {}

        /// <summary>
        /// Initialization as root node
        /// </summary>
        public OctBoxNode(Octree<OctBoxNode> main, BoundingBoxCenter size)
            : base(main)
        {
            this.size = size;
        }

        /// <summary>
        /// Initialization as child node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public OctBoxNode(OctBoxNode parent, int index, BoundingBoxCenter size)
            : base()
        {
            SetAsNode(parent, index);
            this.size = size;
        }

        /// <summary>
        /// As child node
        /// </summary>
        /// <param name="index">from 0 to 3 it define the child id</param>
        public OctBoxNode(OctBoxNode parent, sbyte index, float cx, float cy, float cz,float hx, float hy,float hz) :
            this(parent, index, new BoundingBoxCenter(cx, cy,cz, hx, hy,hz)) { }


        /// <summary>
        /// Recursive splitting
        /// </summary>
        public void RecursiveSplit()
        {
            if (level > 0)
            {
                Split();
                foreach (OctBoxNode node in child) node.RecursiveSplit();
            }
        }

        public void Split()
        {
            childrenFlag = ALL;
            float cx = size.center.x;
            float cy = size.center.y;
            float cz = size.center.z;
            float hx = size.extend.x * 0.5f;
            float hy = size.extend.y * 0.5f;
            float hz = size.extend.z * 0.5f;

            if (child == null) child = new OctBoxNode[8];
            child[0] = new OctBoxNode(this, 0, cx - hx, cy - hy, cz - hz, hx, hy, hz);
            child[1] = new OctBoxNode(this, 1, cx + hx, cy - hy, cz - hz, hx, hy, hz);
            child[2] = new OctBoxNode(this, 2, cx - hx, cy + hy, cz - hz, hx, hy, hz);
            child[3] = new OctBoxNode(this, 3, cx + hx, cy + hy, cz - hz, hx, hy, hz);
            child[4] = new OctBoxNode(this, 4, cx - hx, cy - hy, cz + hz, hx, hy, hz);
            child[5] = new OctBoxNode(this, 5, cx + hx, cy - hy, cz + hz, hx, hy, hz);
            child[6] = new OctBoxNode(this, 6, cx - hx, cy + hy, cz + hz, hx, hy, hz);
            child[7] = new OctBoxNode(this, 7, cx + hx, cy + hy, cz + hz, hx, hy, hz);
        }


        /// <summary>
        /// Child size can be derived by its index
        /// </summary>
        public static Vector3f GetCenterByQuadIndex(Vector3f center, Vector3f halfsize, int index)
        {
            Vector3f c = center;
            Vector3f hs = halfsize;
            c.x = ((index & 1) != 0) ? c.x - hs.x * 0.5f : c.x + hs.x * 0.5f;
            c.y = ((index & 2) != 0) ? c.y - hs.y * 0.5f : c.y + hs.y * 0.5f;
            c.z = ((index & 4) != 0) ? c.z - hs.z * 0.5f : c.z + hs.z * 0.5f;
            return c;
        }

    }


}
