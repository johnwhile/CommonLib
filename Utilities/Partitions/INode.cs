using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Common.Maths;
using Common.Tools;

namespace Common
{
    /// <summary>
    /// rappresent the base of base of base of base of any hierarchy implementations
    /// </summary>
    public interface IHierarchyNode<T>
    {
        T ParentNode { get;}
        T GetChild(int index);
    }

    public abstract class HierarchyNode
    {
        public static int InstanceCounter = 0;
        public const int AvarageSizeInByte = 0;

        /// <summary>
        /// the depth of current node where root is maximum, leaf is 0
        /// </summary>
        public sbyte level;
        /// <summary>
        /// child index, maximum value is 7 for octree
        /// </summary>
        public sbyte index;
        /// <summary>
        /// The unique number (calculated using depth and index) use same depth order
        /// <para>         A               </para>
        /// <para>      B     C            </para>
        /// <para>   D  E    F  G          </para>
        /// </summary>
        public int DepthId;
        /// <summary>
        /// The unique number (calculated using depth and index) use a relation order
        /// <para>         A               </para>
        /// <para>      B     E            </para>
        /// <para>   C  D    F  G          </para>
        /// </summary>
        public int ReletionId;

        /// <summary>
        /// a flag to to know what children exist
        /// </summary>
        protected byte childrenFlag = 0;

        /// <summary>
        /// IsLeaf mean the last node in tree, but can also interpreted in other way
        /// </summary>
        public virtual bool IsLeaf
        {
            //get { return level == 0; }
            get { return childrenFlag == 0; }
        }

        /// <summary>
        /// Empty initialization, hide it from user
        /// </summary>
        public HierarchyNode()
        {
            InstanceCounter++;
        }

        ~HierarchyNode()
        {
            InstanceCounter--;
        }

        public abstract void Destroy();

    }

}
