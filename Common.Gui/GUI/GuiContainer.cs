using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// A GuiControl with childrens
    /// </summary>
    public abstract class GuiContainer : GuiControl
    {
        public override bool HasChildren => children != null && children.Count > 0;

        protected GuiContainer(GuiContainer parent, Rectangle4i localDestination = default(Rectangle4i), string name = "GuiContainer") :
            base(parent, localDestination, name)
        {
            children = null;
            needChildSort = false;
            CanParentFocus = false;
        }

        /// <summary>
        /// Add a child element, valid only for <see cref="GuiContainer"/>.
        /// A child will be always on top of its parent when drawing
        /// </summary>
        /// <remarks>
        /// <i>The implementation ensures that a child can be added in any order in the code:
        /// <code>
        /// A = new Gui();
        /// B = new Gui();
        /// C = new Gui();
        /// B.AddChild(C);
        /// A.AddChild(B);
        /// </code>
        /// If fact the localRectangle passed in the constructor are converted in globalRectangle</i>
        /// </remarks>
        internal bool AddChild(GuiControl child)
        {
            if (children == null) children = new List<GuiControl>();

            //child absolute position become the offset relative to this parent
            child.MoveOffsetBy(Position);

            needChildSort = true;
            children.Add(child);

            child.Parent = this;
            child.Manager = Manager;

            if (Manager!=null) Manager.updatePending |= GuiUpdate.Depth;
            return true;
        }

        /// <summary>
        /// Use this instead <see cref="children"/>, return the children list sorted by its priority with depth value
        /// </summary>
        public override List<GuiControl> ChildrenDepthSorted
        {
            get
            {
                if (HasChildren && needChildSort) children.Sort((a, b) => a.Depth - b.Depth);
                //for (int i = 0; i < children.Count; i++) children[i].indexInParent = i;
                needChildSort = false;
                return children;
            }
        }

        public override int GetNodeTotalCount
        {
            get
            {
                int count = 0;
                foreach (var child in GuiTreeTraversal.Forward(this)) count++;
                return count;
            }
        }
        public override string ToString()
        {
            int c = HasChildren ? children.Count : 0;
            return $"{Name}{instance} d:{depth} c:{c}";
        }
    }
}
