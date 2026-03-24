using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    public delegate void GuiScrollEventHandler(GuiScrollBar sender, float value);
    
    public enum GuiScrollType : byte
    {
        Vertical = 0,
        Horizontal = 1
    }

    /// <summary>
    /// Simple control witch contain only 3 children slider 
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class GuiScrollBar : GuiContainer
    {
        GuiButton buttonup;
        GuiButton buttondown;
        GuiSlider slider;
        public readonly GuiScrollType scrolltype;

        public GuiScrollBar(GuiContainer parent, GuiScrollType type, Rectangle4i rectangle = default(Rectangle4i)) : base(parent, rectangle)
        {
            scrolltype = type;
            CanParentFocus = true;
            
        }

        public override void InitDefaultComponents()
        {
            cursor = scrolltype == GuiScrollType.Vertical ? Inputs.MouseCursor.HSplit : Inputs.MouseCursor.VSplit;

            var focusedShape = new GuiRectangle(this, Destination, "GuiShapeFocused")
            {
                Background = Color4b.Mixing(Color4b.Green, Color4b.CornflowerBlue, 0.5f),
                Border = Color4b.Green,
                Thickness = 1,
                Radius = 5
            };
            var disableShape = new GuiRectangle(this, Destination, "GuiShapeDisable")
            {
                Background = Color4b.Mixing(Color4b.Green, Color4b.Gray, 0.5f),
                Border = Color4b.Gray,
                Thickness = 1,
                Radius = 5
            };

            Elements = new List<GuiElement>() { focusedShape, disableShape };
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            if (IsVisible && Elements != null && Elements.Count > 1)
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

        /// <summary>
        /// value in the range [0,1]
        /// </summary>
        public float Value
        {
            get => GetValue();
            private set { throw new NotImplementedException(); }
        }

        public GuiButton ButtonUp
        {
            get => buttonup;
            set
            {
                if (buttonup != null)
                {
                    Remove(buttonup);
                    buttonup.MouseDown -= ButtonUpPressed;
                    buttonup.m_clipDepth = 0;
                }
                if (value == null) return;
                buttonup = value;
                buttonup.Resizable = false;
                buttonup.Translatable = false;
                buttonup.CanParentFocus = true;
                buttonup.MouseDown -= ButtonUpPressed;
                buttonup.MouseDown += ButtonUpPressed;
                buttonup.m_clipDepth = 1;//GuiScrollBar can't be a good choice for clipping...
            }
        }
        public GuiButton ButtonDown
        {
            get => buttondown;
            set
            {
                if (buttondown != null)
                {
                    Remove(buttondown);
                    buttondown.m_clipDepth = 0;
                    buttondown.MouseDown -= ButtonDownPressed;
                }
                if (value == null) return;
                buttondown = value;
                buttondown.Resizable = false;
                buttondown.Translatable = false;
                buttondown.CanParentFocus = true;
                buttondown.MouseDown -= ButtonDownPressed;
                buttondown.MouseDown += ButtonDownPressed;
                buttondown.m_clipDepth = 1;//GuiScrollBar can't be a good choice for clipping...
            }
        }     
        public GuiSlider Slider
        {
            get => slider;
            internal set
            {
                if (slider != null)
                {
                    Remove(slider);
                }
                slider = value;
                slider.Resizable = false;
                slider.Translatable = true;
                slider.CanParentFocus = true;
            }
        }
        void ButtonUpPressed(GuiControl sender)
        {
            if (slider != null)
            {
                float v0 = Value;
                slider.MoveOffsetBy(0, -5);
                slider.FixOffset();
                float v1 = Value;
                if (v0 != v1) OnValueChanged(v1);
            }
        }
        void ButtonDownPressed(GuiControl sender)
        {
            if (slider != null)
            {
                float v0 = Value;
                slider.MoveOffsetBy(0, 5);
                slider.FixOffset();
                float v1 = Value;
                if (v0 != v1) OnValueChanged(v1);
            }
        }

        float GetValue()
        {
            if (slider == null) return 0;
            if (scrolltype == GuiScrollType.Vertical)
                return slider.Offset.y / (float)(Size.height - slider.Size.height);
            else
                return slider.Offset.x / (float)(Size.width - slider.Size.width);
        }

        public event GuiScrollEventHandler ValueChanged;

        internal void OnValueChanged(float value)
        {
            Debugg.Message("ScrollBar Value = " + value);
            ValueChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// It's a simple fixed button whose movements are dependent from <see cref="GuiScrollBar"/> controller
    /// </summary>
    public class GuiSlider : GuiButton
    {
        public readonly GuiScrollBar ScrollOwner;

        public GuiSlider(GuiScrollBar parent, Rectangle4i rectangle = default(Rectangle4i), string name = "GuiSlider") : base(parent, rectangle, name)
        {
            ScrollOwner = parent;
            Resizable = false;
            Translatable = true;
            CanParentFocus = true;
            ScrollOwner.Slider = this;
            m_clipDepth = 1; //GuiScrollBar can't be a good choice for clipping...
        }

        public override void InitDefaultComponents()
        {
            base.InitDefaultComponents();

            if (Parent is GuiScrollBar owner)
                cursor = owner.scrolltype == GuiScrollType.Vertical ? Inputs.MouseCursor.SizeNS : Inputs.MouseCursor.SizeWE;
            else throw new InvalidCastException();
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            var element = Elements;

            base.Draw(renderer, debug);
        }


        public override void OnMouseMove(Vector2f mouse)
        {
            if (ScrollOwner == null) throw new NullReferenceException("where is my scroller");

            float v0 = ScrollOwner.Value;
            //only vertical movement

            if (ScrollOwner.scrolltype == GuiScrollType.Vertical) 
                mouse.x = tmp_mousedown_position.x;
            else 
                mouse.y = tmp_mousedown_position.y;

            base.OnMouseMove(mouse);

            FixOffset();

            float v1 = ScrollOwner.Value;

            if (v1 != v0) ScrollOwner.OnValueChanged(v1);
        }

        [Obsolete("Use OnMouseMove instead")]
        public override void OnMouseMoveByDelta(Vector2f delta)
        {
            if (ScrollOwner == null) throw new NullReferenceException("where is my scroller");

            float v0 = ScrollOwner.Value;
            //only vertical movement

            if (ScrollOwner.scrolltype == GuiScrollType.Vertical) delta.x = 0;
            else delta.y = 0;

            base.OnMouseMoveByDelta(delta);

            FixOffset();

            float v1 = ScrollOwner.Value;

            if (v1 != v0) ScrollOwner.OnValueChanged(v1);
        }

        /// <summary>
        /// clamp position inside slider height or width
        /// </summary>
        internal void FixOffset()
        {
            //only if linked to a scroll control
            if (ScrollOwner != null)
            {
                var offset = Offset;
                if (ScrollOwner.scrolltype == GuiScrollType.Vertical)
                    offset.y = Mathelp.CLAMP(offset.y, 0, Parent.Size.y - Size.y);
                else
                    offset.x = Mathelp.CLAMP(offset.x, 0, Parent.Size.x - Size.x);
                Offset = offset;
            }
        }

    }
}
