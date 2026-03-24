using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// Base Controller class from which all controllable objects must derive.<br/>
    /// The implemented hierarchy is a tree where the first element is considered the invisible root.<br/>
    /// Children can be added at any time, the order of insertion defines the drawing priority witch change when selecting.<br/>
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract partial class GuiControl : GuiResource
    {
        /// <summary>
        /// Create a new Controller and add to children list of parent.<br/>
        /// <b>Parent can be null</b>, see <see cref="GuiContainer.AddChild(GuiControl)"/>
        /// </summary>
        /// <param name="parent">All controls are in a tree hierarchy, except root that is auto-generated</param>
        /// <param name="rectangle">offset is in local coordinates</param>
        public GuiControl(GuiContainer parent, Rectangle4i rectangle, string name = "GuiControl") : base(parent, rectangle, name)
        {
            depth = 0;
            m_globalrect = rectangle;
            
            //parent is null for root's constructor
            if (Manager != null)
            {
                Manager.updatePending |= GuiUpdate.Depth;
                //destination will be converted into absolute position.
                if (Parent is GuiContainer container)
                    container.AddChild(this);
            }
        }

        /// <summary>
        /// If true, children need a sorting call by its depth priority
        /// </summary>
        protected bool needChildSort;
        /// <summary>
        /// unsorted list, used only for <see cref="GuiContainer"/>
        /// </summary>
        protected List<GuiControl> children = null;

        /// <summary>
        /// Global destination is the control's size and location relative to first root node
        /// </summary>
        /// <remarks>
        /// <i>For <see cref="GuiControl"/> class I preferred the global destination instead of the local one to avoid continuous iterating every time I call the draw.</i>
        /// </remarks>
        protected Rectangle4i m_globalrect;
        /// <summary>
        /// <inheritdoc/><br/>
        /// <b>To avoid errors, use only <see cref="Offset"/> and <see cref="Size"/> properties</b><br/><br/>
        /// </summary>
        /// <remarks><inheritdoc cref="m_globalrect"/></remarks>
        public override Rectangle4i Destination
        {
            get => m_globalrect;
            internal set
            {
                m_globalrect = value;
                if (Manager != null) Manager.updatePending |= GuiUpdate.Clip;
            }
        }

        /// <summary>
        /// Position in absolute coordinates. Changing position calls <see cref="MoveOffsetBy(Vector2i)"/>
        /// </summary>
        public Vector2i Position
        {
            get => m_globalrect.position;
            internal set { MoveOffsetBy(value - m_globalrect.position); }
        }
        /// <summary>
        /// <inheritdoc/> 
        /// </summary>
        public override Rectangle4i Local
        {
            get
            {
                var rect = m_globalrect;
                rect.position = Offset;
                return rect;
            }
            internal set
            {
                Offset = value.position;
                Size = value.size;
            }
        }
        /// <summary>
        /// Position relative to its parent. Changing position calls <see cref="MoveOffsetBy(Vector2i)"/>,
        /// all children will be moved with same value
        /// </summary>
        public Vector2i Offset
        {
            get
            {
                if (HasParent) return m_globalrect.position - Parent.m_globalrect.position;
                return m_globalrect.position;
            }
            set { MoveOffsetBy(value - Offset); }
        }
        /// <summary>
        /// Size of rectangle. Changing size affect position for all children with 
        /// <see cref="GuiAnchor.Bottom"/> and <see cref="GuiAnchor.Right"/> anchors
        /// </summary>
        public Vector2i Size
        {
            get => m_globalrect.size;
            set { MoveBRCornedBy(value - m_globalrect.size); }
        }

        /// <summary>
        /// <inheritdoc cref="Depth"/>
        /// </summary>
        protected int depth;
        /// <summary>
        /// Depth define the draw order from 0 to greater, theoretically it's equal to total elements in the tree. 
        /// All children must be always on top of its parent. Internally it can have any value but the <see cref="GuiTreeTraversal"/> method 
        /// return the children in a sorted order using <see cref="ChildrenDepthSorted"/>.<br/>
        /// To work with <see cref="GetTopMost"/> method you have to call <b><see cref="PreCalculate(GuiControl, ref GuiUpdate)"/></b> to assign a unique depth value.
        /// </summary>
        public int Depth
        {
            get => depth;
            internal set
            {
                depth = value;
                if (HasParent) Parent.needChildSort = true;
            }
        }



        protected bool alwaysfocused = false;
        /// <summary>
        /// Disable get/set focused
        /// </summary>
        public bool AlwaysFocused
        {
            get => alwaysfocused;
            set
            {
                alwaysfocused = value;
                if (value) IsFocused = true;
            }
        }
        /// <summary>
        /// this control may or not be movable, normally this is activated by holding down the mouse
        /// </summary>
        public bool Translatable = true;
        /// <summary>
        /// valid only for <see cref="GuiContainer"/> return false for base class <see cref="GuiControl"/>
        /// </summary>
        public virtual bool HasChildren => false;
        /// <summary>
        /// When a child get focus, focus also its parent. True by default.
        /// </summary>
        public bool CanParentFocus = true;

        /// <summary>
        /// Use this instead <see cref="children"/>, return the children list sorted by its depth's priority<br/>
        /// <b>if it's not a <see cref="GuiContainer"/> return null.</b> 
        /// </summary>
        /// <remarks>
        /// <i>All children implementation are for both <see cref="GuiContainer"/> and <see cref="GuiControl"/> for avoid
        /// casting in <see cref="GuiTreeTraversal"/></i>
        /// </remarks>
        public virtual List<GuiControl> ChildrenDepthSorted => null;
        /// <summary>
        /// Calculate the sum of all children node including itself.<br/>
        /// <b>it's preferable not use it intensively, example per frame</b>
        /// </summary>
        public virtual int GetNodeTotalCount => 1;

        /// <summary>
        /// <b>Translation</b><br/>
        /// Changing position affect only relatives and not require to recalculate all element of tree, 
        /// but clip rectangles must be updated
        /// </summary>
        public void MoveOffsetBy(Vector2i move) => MoveOffsetBy(move.x, move.y);
        
        /// <summary>
        /// <inheritdoc cref="MoveOffsetBy(Vector2i)"/>
        /// </summary>
        public virtual void MoveOffsetBy(int x, int y)
        {
            if (x == 0 && y == 0) return;
            //changing position require to recalculate all clip rectangles
            if (Manager.UseClipping) Manager.updatePending |= GuiUpdate.Clip;

            //changing position affect all children absolute position
            foreach (var dependent in GuiTreeTraversal.Forward(this))
            {
                //traversal return also this element
                dependent.m_globalrect.position.x += x;
                dependent.m_globalrect.position.y += y;
            }
        }

        /// <summary>
        /// <b>Scaling (using edge)</b><br/>
        /// Changing edge position performs both translation and scaling
        /// </summary>
        public void MoveEdgeBy(Vector2i move, GuiEdge edge) => MoveEdgeBy(move.x, move.y, edge);
        
        /// <summary>
        /// <inheritdoc cref="MoveEdgeBy(Vector2i, GuiEdge)"/>
        /// </summary>
        public void MoveEdgeBy(int x, int y, GuiEdge edge)
        {
            int tx = 0;
            int ty = 0;
            int sx = 0;
            int sy = 0;

            if ((edge & GuiEdge.Left) > 0) { tx = x; sx = -x; }
            if ((edge & GuiEdge.Top) > 0) { ty = y; sy = -y; }
            if ((edge & GuiEdge.Right) > 0) sx = x;
            if ((edge & GuiEdge.Bottom) > 0) sy = y;

            //avoid a continuos translation when size is too smaller... i don't like it
            if (m_globalrect.size.width <= 1 && sx < 0) { tx = 0; sx = 0; }
            if (m_globalrect.size.height <= 1 && sy < 0) { ty = 0; sy = 0; }

            MoveOffsetBy(tx, ty);
            MoveBRCornedBy(sx, sy);
        }

        /// <summary>
        /// <b>Scaling (using bottom right corner)</b><br/>
        /// Changing scale affect only its direct children position with defined anchor.
        /// but clip rectangles must be updated
        /// </summary>
        public void MoveBRCornedBy(Vector2i move) => MoveBRCornedBy(move.x, move.y);
        
        /// <summary>
        /// <inheritdoc cref="MoveBRCornedBy(Vector2i)"/>
        /// </summary>
        public virtual void MoveBRCornedBy(int x, int y)
        {
            if (x == 0 && y == 0) return;
            //Debug.Print("move BR corner by " + x + " " + y);
            //changing scale require to recalculate all clip rectangles
            if (Manager.UseClipping) Manager.updatePending |= GuiUpdate.Clip;

            //changing scale affect only its direct children
            if (HasChildren)
                foreach (var child in children)
                {
                    int cx = (child.Anchor & GuiEdge.Right) > 0 ? x : 0;
                    int cy = (child.Anchor & GuiEdge.Bottom) > 0 ? y : 0;
                    child.MoveOffsetBy(cx, cy);
                }

            m_globalrect.size.x += x;
            m_globalrect.size.y += y;
            if (m_globalrect.size.x <= 0) m_globalrect.size.x = 1;
            if (m_globalrect.size.y <= 0) m_globalrect.size.y = 1;
        }



        public override string ToString() => $"{Name}{instance} d:{depth}";
        
    }
}
