using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// The main entry point of your Graphic User Interface
    /// </summary>
    public class GuiManager
    {
        enum MouseState
        {
            None = 0,
            Down = 1,
            Up = 2,
        }

        /// <summary>
        /// <inheritdoc cref="GuiRoot"/>
        /// </summary>
        public GuiRoot Root;

        internal GuiUpdate updatePending;

        MouseState mouseState;
        bool elementMoving;

        /// <summary>
        /// last onMove event
        /// </summary>
        Vector2f mousePrevMove;
        // when press down
        Vector2f mouseAtDownEvent;
        // location at prev frame during down event
        Vector2f mousePrevDuringDownEvent;
        
        
        GuiControl selected;
        Vector2f selectedOffset_mouseDown;
        
        GuiControl lastMouseOver;
        DepthOrdered depthOrdered;

        /// <summary>
        /// TODO: sometime not work
        /// Enable clipping rectangle calculations for all controls
        /// </summary>
        public bool UseClipping = false;

        /// <summary>
        /// Changing selected control do something
        /// </summary>
        public GuiControl Selected
        {
            get => selected;
            set
            {
                if (Root == null) return;
                //unfocused all other controllers
                if (selected != null)
                {
                    selected.IsFocused = false;
                    selected.OnDeselecting();
                }
                selected = value;
                if (selected == null)
                {
                    Root.IsFocused = false;
                    return;
                };
                
                //focuse all dependents
                selected.IsFocused = true;
                selected.OnSelecting();
                GuiControl.SetTopMost(selected);
            }
        }
        /// <summary>
        /// Return, after call <see cref="DepthOrderedControls"/>, the top-most guicontrol contains mouse, 
        /// usefull to get the current <see cref="GuiControl.GuiCursor"/> to use.<br/>
        /// The cursor must be set only after all GuiControls are drawed to avoid cursor flickering.<br/>
        /// <b>It can be null.</b>
        /// </summary>
        public GuiControl LastVisitedByMouse => lastMouseOver;
        /// <summary>
        /// Get current mouse position in absolute coordinates
        /// </summary>
        public Vector2f MousePosition { get; private set; }
        /// <summary>
        /// Get translation value since MouseDown event. Reset on MouseDown
        /// </summary>
        public Vector2f MouseMovement => MousePosition - mouseAtDownEvent;

        /// <summary>
        /// Iterator of children. It update all controls before return the enumerator.
        /// </summary>
        public DepthOrdered DepthOrderedControls 
        { 
            get 
            {
                DoUpdateJob();

                lastMouseOver = Root.IsMouseOver ? Root : null;

                foreach (var element in GuiTreeTraversal.Forward(Root))
                {
                    element.Update();
                    if (element.IsFocused && element.IsMouseOver)
                    {
                        if (lastMouseOver != null && lastMouseOver.Depth < element.Depth)
                            lastMouseOver = element;
                    }
                }

                depthOrdered.Root = Root;
                return depthOrdered;
            } 
        }

        public GuiManager()
        {
            elementMoving = false;
            updatePending = GuiUpdate.Depth;
            depthOrdered = new DepthOrdered();
        }

        /// <summary>
        /// Return true if selected isn't null
        /// </summary>
        public bool SelectTopMost(Vector2i absolute, out GuiControl select)
        {
            if (Root == null) { select = null; return false; }
            select = GuiControl.GetTopMost(absolute, Root);
            return select != null;
        }

        #region Events   
        public void MouseDown(Vector2f mouse, MouseButtons button)
        {
            mouseState = MouseState.Down;
            mousePrevDuringDownEvent = mouseAtDownEvent = MousePosition = mouse;

            if (SelectTopMost(MousePosition, out var _selected))
            {
                _selected.OnMouseDown(mouse);
                selectedOffset_mouseDown = mouse - _selected.Offset;
            }
            Selected = _selected;

        }

        public void MouseUp(Vector2f mouse, MouseButtons button)
        {
            mouseState = MouseState.Up;
            MousePosition = mouse;

            var delta = MousePosition - mouseAtDownEvent;

            //mouseclick also with a little movement
            if (!elementMoving || Math.Abs(delta.x * delta.y) <= 100)
                selected?.OnMouseClick();
            
            selected?.OnMouseUp();

            elementMoving = false;
            mouseAtDownEvent = MousePosition;
        }

        public void MouseMove(Vector2f mouse , MouseButtons button)
        {
            MousePosition = mouse;
            Vector2f delta = MousePosition - mousePrevMove;
            mousePrevMove = mouse;

            //no relevant movement
            if (delta.x == 0 && delta.y == 0) return;

            if (mouseState == MouseState.Down)
            {
                var move = MousePosition - mousePrevDuringDownEvent;
                //Debugg.Print($"gui delta {move}");
                mousePrevDuringDownEvent = MousePosition;

                if (move.x != 0 || move.y != 0)
                {
                    elementMoving = true;

                    if (selected != null)
                    {
                        //Debugg.Print($"{selected.Name} mouse move by delta {move}");
                        selected.OnMouseMove(mouse);
                        //selected.OnMouseMoveByDelta(move);
                    }
                }
            }
            else
            {
                var topmost = GuiControl.GetTopMost(MousePosition, Root);
                if (topmost != null) topmost.OnMouseHover();
                
            }
            if (selected != null)
            {
                //if (selected.IsMouseOver) selected.OnMouseHover();
            }
           
        }

        public void KeyDown(KeyEventArgs e)
        {
            selected?.OnKeyDown(e);
        }

        /// <summary>
        /// <inheritdoc cref="GuiControl.PreCalculate(GuiControl, ref GuiUpdate)"/>
        /// </summary>
        internal void DoUpdateJob()
        {
            if (updatePending != GuiUpdate.None) 
                GuiControl.PreCalculate(Root, ref updatePending);
        }


        #endregion


    }

}
