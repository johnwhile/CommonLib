using Common.Maths;
using Common.Partitions;

namespace Common.Tools
{
    class BitPartitioned
    {
    }

    public class BitNode : QuadRectNode<BitNode, BitTree>
    {
        int I, J, Width, Heigth;

        public BitNode()
            : base()
        {
        }


        public BitNode(BitTree main, int i, int j, int width, int heigth)
            : base(main)
        {
            this.I = i;
            this.J = j;
            this.Width = width;
            this.Heigth = heigth;

        }

        public BitNode(BitNode parent, int index, int i, int j, int width, int heigth)
            : base(parent, index)
        {
            this.I = i;
            this.J = j;
            this.Width = width;
            this.Heigth = heigth;
        }

        public override void Split()
        {
            int W2 = Width / 2;
            int H2 = Heigth / 2;
            int Im = I + W2;
            int Jm = J + H2;


            if (level + 1 < main.Depth)
            {
                child = new BitNode[4];
                child[0] = new BitNode(this, 0, I, J, W2, H2);
                child[1] = new BitNode(this, 1, I, Jm, W2, H2);
                child[2] = new BitNode(this, 2, Im, J, W2, H2);
                child[3] = new BitNode(this, 3, Im, Jm, W2, H2);
            }
            else
            {
                child = new BitLeaf[4];
                child[0] = new BitLeaf(this, 0, I, J, W2, H2);
                child[1] = new BitLeaf(this, 1, I, Jm, W2, H2);
                child[2] = new BitLeaf(this, 2, Im, J, W2, H2);
                child[3] = new BitLeaf(this, 3, Im, Jm, W2, H2);
            }
        }
        public override string ToString()
        {
            return string.Format("i{0} j{2}; w{3} h{4}", I, J, Width, Heigth);
        }
    }


    public class BitLeaf : BitNode
    {
        public BitArray2 bits = null;

        public BitLeaf(BitTree main, int i, int j, int width, int heigth)
            : base(main, i, j, width, heigth)
        {
        }

        public BitLeaf(BitNode parent, int index, int i, int j, int width, int heigth)
            : base(parent, index, i, j, width, heigth)
        {
        }
    }



    public class BitTree : QuadRectree<BitNode, BitTree>
    {
        /// <summary>
        /// </summary>
        /// <param name="Depth">Number of levels, level(N-1) is root, level 0 is leaf, safety value = maximum 127</param>
        public BitTree(int Depth, IRectangleAA size)
            : base(Depth, size)
        { }

        public BitTree(int Depth)
            : base(Depth, AABRminmax.UnitXY)
        { }
    }
    /// <summary>
    /// Bit array stored in a Quadtree structure to compress the data
    /// </summary>
    public class BitSurface
    {
        int width, heigth;

        BitTree quadtree;

        public BitSurface(int width, int heigth, bool initialvalue = false)
        {
            this.width = width;
            this.heigth = heigth;

            // the optimized level of tree is found with some test
            int depth = Maths.Mathelp.MIN(getmaximumdepth(width), getmaximumdepth(heigth));

            float leadW = (float)width / (1 << depth);
            float leadH = (float)heigth / (1 << depth);

            quadtree = new BitTree(depth, AABRminmax.Empty);
            quadtree.root = new BitNode(quadtree, 0, 0, width, heigth);

        }

        int getmaximumdepth(int size)
        {
            int i = 0;
            while ((size /= 2) > 4) i++;
            return ++i;
        }



    }

    /// <summary>
    /// Bit array stored in a Octree structure to compress the data
    /// </summary>
    public class BitVolume
    {

    }
}
