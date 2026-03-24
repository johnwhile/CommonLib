using System;
using System.Diagnostics;
using System.Windows.Forms;
using Common.Maths;

namespace Common.Gui
{
    public delegate void GuiEventHandlerMouse(GuiControl sender, Vector2f mouse);
    public delegate void GuiEventHandlerKey(GuiControl sender, KeyEventArgs key);
    public delegate void GuiEventHandler(GuiControl sender);

    public abstract partial class GuiControl
    {
        GuiState States;

        /// <summary>
        /// Occurs when the control is clicked by the mouse : mouse down + mouse up but trigger before mouse up
        /// </summary>
        public event GuiEventHandler MouseClick;
        public event GuiEventHandler MouseDown;
        public event GuiEventHandler MouseUp;
        /// <summary>
        /// Occurs when Mouse move, pass the mouse position value. 
        /// </summary>
        public event GuiEventHandlerMouse MouseMove;
        /// <summary>
        /// Occurs when Mouse move, pass the mouse delta value
        /// </summary>
        public event GuiEventHandlerMouse MouseMoveDelta;
        public event GuiEventHandler MouseHover;
        public event GuiEventHandler Selecting;
        public event GuiEventHandler Deselecting;
        public event GuiEventHandlerKey KeyDown;

        protected Vector2f tmp_mousedown_position;
        protected Vector2f tmp_mousedown_offset;
        protected Vector2f tmp_mousedown_size;

        /// <summary>
        /// do something when mouse move
        /// </summary>
        internal GuiMovement m_currentMovement = GuiMovement.nothing;
        /// <summary>
        /// scaling type when mouse move
        /// </summary>
        public GuiEdge ScalingType = GuiEdge.Outside;

        /// <summary>
        /// the state of control after MouseDown and prev MouseUp
        /// </summary>
        public bool IsPressed
        {
            get => (States & GuiState.Pressed) != 0;
            set
            {
                if (value) States |= GuiState.Pressed;
                else States &= ~GuiState.Pressed;
            }
        }
        /// <summary>
        /// Focusing a element imply all children will be focused
        /// </summary>
        public virtual bool IsFocused
        {
            get => (States & GuiState.Focused) != 0;
            set
            {
                //disable IsFocused = false
                if (AlwaysFocused && !value) return;

                GuiControl root = this;

                //reset focused value for all controls
                if (value)
                {
                    //GuiControl par = HasParent ? parent : this;
                    //foreach (var other in GuiTreeTraversal.ForwardSkipNode(Manager.Root, par))
                    //    other.focused = false;
                    foreach (var dependent in GuiTreeTraversal.Forward(Manager.Root))
                        if (!dependent.alwaysfocused)
                            dependent.States &= ~GuiState.Focused; //set focused = false
                }

                //only when selecting mean select also parent
                if (value)
                    foreach (var dependent in GuiTreeTraversal.Backward(this))
                    {
                        root = dependent;
                        if (!dependent.CanParentFocus) break;
                    }

                foreach (var dependent in GuiTreeTraversal.Forward(root))
                {
                    if (value) dependent.States |= GuiState.Focused;
                    else dependent.States &= ~GuiState.Focused;
                }
            }
        }

        /// <summary>
        /// Check if mouse coordinate are inside <see cref="m_globalrect"/> area. An additional region can be used as test
        /// </summary>
        public virtual bool IsMouseOver
        {
            get { return ContainMouse(Manager.MousePosition); }
        }

        public virtual void OnMouseDown(Vector2f mouse)
        {
            IsPressed = true;
            tmp_mousedown_position = mouse;
            tmp_mousedown_offset = Offset;
            tmp_mousedown_size = Size;
            if (Translatable) m_currentMovement = GuiMovement.translation;
            if (IsEnabled) MouseDown?.Invoke(this);
        }
        public virtual void OnMouseUp()
        {
            IsPressed = false;
            m_currentMovement = GuiMovement.nothing;
            if (IsEnabled) MouseUp?.Invoke(this);
        }
        public virtual void OnMouseClick()
        {
            if (IsEnabled) MouseClick?.Invoke(this);
        }

        /// <summary>
        /// On mouse moving but called only for selected <see cref="GuiControl"/>
        /// </summary>
        /// <param name="delta">mouse delta</param>
        [Obsolete("Use OnMouseMove instead")]
        public virtual void OnMouseMoveByDelta(Vector2f delta)
        {
            if (m_currentMovement == GuiMovement.translation && Translatable)
            {
                MoveOffsetBy(delta);
            }
            else if (m_currentMovement == GuiMovement.scaling && Resizable)
            {
                MoveEdgeBy(delta, ScalingType);
            }
            if (IsEnabled) MouseMoveDelta?.Invoke(this, delta);
        }

        /// <summary>
        /// On mouse moving but called only for selected <see cref="GuiControl"/>
        /// </summary>
        /// <param name="mouse">mouse position</param>
        public virtual void OnMouseMove(Vector2f mouse)
        {
            if (m_currentMovement == GuiMovement.translation && Translatable)
            {
                Offset = tmp_mousedown_offset + mouse - tmp_mousedown_position;
            }
            else if (m_currentMovement == GuiMovement.scaling && Resizable)
            {
                Vector2i pos = tmp_mousedown_offset;
                Vector2i size = tmp_mousedown_size;
                Vector2i move = mouse - tmp_mousedown_position;

                if ((ScalingType & GuiEdge.Left) > 0)
                {
                    pos.x += Mathelp.MIN(move.x, (int)tmp_mousedown_size.x);
                    size.x += -move.x;
                }
                if ((ScalingType & GuiEdge.Top) > 0) 
                {
                    pos.y += Mathelp.MIN(move.y, (int)tmp_mousedown_size.y);
                    size.y += -move.y;
                }
                if ((ScalingType & GuiEdge.Right) > 0)
                {
                    size.x += move.x;
                }
                if ((ScalingType & GuiEdge.Bottom) > 0)
                {
                    size.y += move.y;
                }
                //avoid a continuos translation when size is too smaller... i don't like it
                size.x = Mathelp.MAX(1, size.x);
                size.y = Mathelp.MAX(1, size.y);

                Offset = pos;
                Size = size;
            }
            if (IsEnabled) MouseMove?.Invoke(this, mouse);
        }
        public virtual void OnMouseHover()
        {
            if (IsEnabled && IsFocused)
            {
                //Debugg.Print($"{Name} mouse hover");
                MouseHover?.Invoke(this);
                /*
                if (HasChildren)
                    foreach (var element in GuiTreeTraversal.ForwardSkipRoot(this))
                        if (element.IsMouseOver) element.OnMouseHover();
                */
            }

        }
        public virtual void OnSelecting()
        {
            //Debugg.Print($"{Name} selecting");
            if (IsEnabled) Selecting?.Invoke(this);
        }
        public virtual void OnDeselecting()
        {
            //Debugg.Print($"{Name} deselecting");
            if (IsEnabled) Deselecting?.Invoke(this);
        }

        public virtual void OnKeyDown(KeyEventArgs arg)
        {
            if (IsEnabled) KeyDown?.Invoke(this, arg);
        }
    }
}
