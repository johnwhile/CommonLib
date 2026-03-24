using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using Common.Gui.SystemGraphic;
using Common.Inputs;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// Simple controls container. Can be resaizable
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class GuiPanel : GuiContainer
    {
        /// <summary>
        /// size in pixel of border, used to triggers scaling
        /// </summary>
        public int ScalingThreshold = 4;

        MouseCursor m_currentCursor = MouseCursor.Arrow;

        public MouseCursor CursorMiddle = MouseCursor.Arrow;
        public MouseCursor CursorWE = MouseCursor.SizeWE;
        public MouseCursor CursorNS = MouseCursor.SizeNS;
        public MouseCursor CursorNWSE = MouseCursor.SizeNWSE;
        public MouseCursor CursorNESW = MouseCursor.SizeNESW;


        public GuiPanel(GuiContainer parent, Rectangle4i rectangle = default(Rectangle4i), string name = "GuiPanel") : base(parent, rectangle, name)
        {
            CanParentFocus = false;
            Resizable = true;
            Translatable = true;
        }

        public override MouseCursor GuiCursor 
        { 
            get => m_currentCursor; 
            set => base.GuiCursor = value;
        }

        public override void InitDefaultComponents()
        {
            var focusedShape = new GuiRectangle(this, Destination, "GuiShapeFocused")
            {
                Background = Color4b.Mixing(Color4b.Red, Color4b.Gray, 0.8f),
                Border = Color4b.Red,
                Thickness = 1,
                Radius = 5
            };
            var disableShape = new GuiRectangle(this, Destination, "GuiShapeDisable")
            {
                Background = Color4b.Mixing(Color4b.Red , Color4b.Gray, 0.5f),
                Border = Color4b.Gray,
                Thickness = 1,
                Radius = 5
            };
            Elements = new List<GuiElement>() { focusedShape, disableShape };

        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            if (IsVisible && Elements!=null && Elements.Count > 1)
            {
                if (IsFocused)
                {
                    Elements[0].Draw(renderer, debug);
                }
                else
                {
                    Elements[1].Draw(renderer, debug);
                }
            }
            if (debug) DrawDebugName(renderer);
        }




        public override void Update()
        {
            base.Update();

            if (IsEnabled && IsFocused && IsVisible)
            {
                var result = GetMouseBorder(Manager.MousePosition);

                switch (result)
                {
                    case GuiEdge.Left:
                    case GuiEdge.Right:
                        m_currentCursor = CursorWE;
                        break;

                    case GuiEdge.Top:
                    case GuiEdge.Bottom:
                        m_currentCursor = CursorNS;
                        break;

                    case GuiEdge.BottomRight:
                    case GuiEdge.TopLeft:
                        m_currentCursor = CursorNWSE;
                        break;

                    case GuiEdge.BottomLeft:
                    case GuiEdge.TopRight:
                        m_currentCursor = CursorNESW;
                        break;

                    case GuiEdge.Inside:
                        m_currentCursor = CursorMiddle;
                        break;

                    default:
                        m_currentCursor = cursor;
                        break;
                }
            }
        }


        /// <summary>
        /// Check if mouse is over the control, also considering the edge
        /// </summary>
        public override bool ContainMouse(int x, int y)
        {
            if (ScalingThreshold <= 1) return m_globalrect.Contain(x, y);
            return Rectangle4i.Contain(x, y, m_globalrect.x - ScalingThreshold, m_globalrect.y - ScalingThreshold, m_globalrect.width + ScalingThreshold * 2, m_globalrect.height + ScalingThreshold * 2);
        }

        public override void OnMouseDown(Vector2f mouse)
        {
            // MovementAction now is GuiMovement.translation
            base.OnMouseDown(mouse);

            ScalingType = GetMouseBorder(Manager.MousePosition);

            if ((ScalingType & GuiEdge.Fill) > 0)
            {
                m_currentMovement = GuiMovement.scaling;
            }
        }

        /// <summary>
        /// Get mouse position on border using a threshold
        /// </summary>
        protected GuiEdge GetMouseBorder(Vector2i mouse)
        {
            var inside = GuiEdge.Outside;
            var rect = m_globalrect;

            if (Rectangle4i.Contain(ref mouse, m_globalrect.Left - ScalingThreshold, m_globalrect.Top - ScalingThreshold, ScalingThreshold * 2, m_globalrect.height + ScalingThreshold * 2)) 
                inside |= GuiEdge.Left;
            if (Rectangle4i.Contain(ref mouse, m_globalrect.Left - ScalingThreshold, m_globalrect.Top - ScalingThreshold, m_globalrect.width + ScalingThreshold * 2, ScalingThreshold * 2))
                inside |= GuiEdge.Top;
            if (Rectangle4i.Contain(ref mouse, m_globalrect.Right - ScalingThreshold, m_globalrect.Top - ScalingThreshold, ScalingThreshold * 2, m_globalrect.height + ScalingThreshold * 2))
                inside |= GuiEdge.Right;
            if (Rectangle4i.Contain(ref mouse, m_globalrect.Left - ScalingThreshold, m_globalrect.Bottom - ScalingThreshold, m_globalrect.width + ScalingThreshold * 2, ScalingThreshold * 2))
                inside |= GuiEdge.Bottom;

            //inside but not touch borders
            if (inside == GuiEdge.Outside && Rectangle4i.Contain(ref mouse, ref m_globalrect)) inside = GuiEdge.Inside;

            return inside;
        }
    }
}
