using System;
using System.Collections.Generic;
using System.Text;

namespace Quadtree
{
    class Program
    {
        static void Main(string[] args)
        {

            Tree tree = new Tree(4);
            
            tree.root = new Quad(tree);

            Console.WriteLine(uint.MaxValue);
            for (int i = 0; i < 32; i++)
            {

                Console.WriteLine(string.Format("{0} {1}", i, Tree.MaximumNodes(i)));
            }

            Console.ReadKey();
        }
    }
}
