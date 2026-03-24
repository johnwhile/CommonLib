using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// tree traversal methods
    /// <code>
    /// root─┬─p1─┬─c1
    ///      |    └─c2
    ///      ├─p2
    ///      └─p3
    /// </code>
    /// return sequence for visible nodes:
    /// <code>
    ///  p1, c1, c2, p2, p3 ...
    /// </code>
    /// </summary>
    public static class GuiTreeTraversal
    {
        /// <summary>
        /// <inheritdoc cref="GuiTreeTraversal"/>
        /// <b>root isn't returned</b>
        /// </summary>
        public static IEnumerable<GuiControl> ForwardSkipRoot(GuiControl root)
        {
            if (root.HasChildren)
                foreach (var child in root.ChildrenDepthSorted)
                    foreach (var element in Forward(child))
                        if (element.IsVisible) yield return element;
        }
        /// <summary>
        /// <inheritdoc cref="GuiTreeTraversal"/>
        /// <b>skipnode is returned</b>
        /// </summary>
        public static IEnumerable<GuiControl> ForwardSkipNode(GuiControl root, GuiControl skipnode)
        {
            if (root.IsVisible) yield return root;
            if (root == skipnode) yield break;
            
            if (root.HasChildren)
                foreach (var child in root.ChildrenDepthSorted)
                    foreach (var element in ForwardSkipNode(child, skipnode))
                        if (element.IsVisible) yield return element;
            
        }
        /// <summary>
        /// <inheritdoc cref="GuiTreeTraversal"/>
        /// <b>root is returned</b>
        /// </summary>
        public static IEnumerable<GuiControl> Forward(GuiControl root)
        {
            if (root.IsVisible) yield return root;
            if (root.HasChildren)
                foreach (var child in root.ChildrenDepthSorted)
                    foreach (var element in Forward(child))
                        if (element.IsVisible) yield return element;
        }
        /// <summary>
        /// <inheritdoc cref="GuiTreeTraversal"/>
        /// </summary>
        public static IEnumerable<GuiControl> ForwardSkipOutOfParentBound(GuiControl root, Vector2i absolute)
        {
            //mouse inside destination rectangle
            bool insidedest = root.ContainMouse(absolute);
            if (insidedest)
                if (root.IsVisible) yield return root;
            
            //if inside continue searching, else continue only if no clipping mode
            if (root.IsVisible && root.HasChildren && (insidedest || !(insidedest || root.UseClipping)))
                foreach (var child in root.ChildrenDepthSorted)
                    foreach (var element in ForwardSkipOutOfParentBound(child, absolute))
                        if (root.IsVisible)
                            yield return element;

        }

        /// <summary>
        /// </summary>
        public static IEnumerable<GuiControl> Backward(GuiControl root, bool skipRoot = false)
        {
            yield return root;
            while (root.HasParent)
            {
                yield return root.Parent;
                root = root.Parent;
            }
        }


    }
}
