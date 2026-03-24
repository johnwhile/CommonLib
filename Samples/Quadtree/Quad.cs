
using System;
using System.Collections.Generic;
using System.Text;

using Engine.Quadtree;

namespace Quadtree
{

    public class Tree : QuadTree<Quad>
    {
        public Tree(int DepthCount)
            : base(DepthCount)
        {

        }
    }


    public class Quad : QuadNode<Quad, Tree>
    {
                /// <summary>
        /// Initialize as Root Node
        /// </summary>
        /// <param name="DepthCount">depth of tree, maximum 16 , minimum 1</param>
        public Quad(Tree tree):base(tree)
        {

        }
        
        /// <summary>
        /// Initialize as Child Node
        /// </summary>
        public Quad(Quad parent, byte index)
            : base(parent, index)
        {

        }
    }

}
