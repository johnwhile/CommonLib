using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{

    /// <summary>
    /// Simple control without children, can draw into 2 mode (pressed and not)
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class GuiButton : GuiControl
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public GuiButton(GuiContainer parent, Rectangle4i rectangle = default(Rectangle4i), string name = "GuiButton") : base(parent, rectangle, name)
        {
            CanParentFocus = true;
        }

        public override void InitDefaultComponents()
        {
            Debugg.Message($"{ToString()} init components");

            Elements_ = new GuiElementManager<GuiElement>(this);

            var focusedShape = new GuiRectangle(this, Local, "GuiShapeFocused")
            {
                Background = Color4b.Mixing(Color4b.CornflowerBlue, Color4b.Gray, 0.8f),
                Border = Color4b.Mixing(Color4b.CornflowerBlue, Color4b.Gray, 0.8f),
                Thickness = 1,
                Radius = 5
            };
            var disableShape = new GuiRectangle(focusedShape)
            {
                Background = Color4b.Mixing(Color4b.CornflowerBlue, Color4b.Gray, 0.5f),
                Border = Color4b.Gray,
            };
            var pressedShape = new GuiRectangle(focusedShape)
            {
                Background = Color4b.CornflowerBlue,
                Border = Color4b.Green
            };
            var mouseoverShape = new GuiRectangle(focusedShape)
            {
                Background = Color4b.CornflowerBlue,
                Border = Color4b.Blue
            };

            Elements_.Focused.Add(focusedShape);
            Elements_.Disabled.Add(disableShape);
            Elements_.Pressed.Add(pressedShape);
            Elements_.MouseOver.Add(mouseoverShape);
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            if (IsVisible)
            {
                renderer.ClipRectangle = UseClipping ? m_cliprectangle : Rectangle4i.Null;

                if (!IsEnabled || !IsFocused)
                {
                    foreach(var element in Elements_.Disabled) element.Draw(renderer, debug);
                }
                else
                {
                    
                    if (IsPressed) foreach (var element in Elements_.Pressed) element.Draw(renderer, debug);
                    else if (IsMouseOver) foreach (var element in Elements_.MouseOver) element.Draw(renderer, debug);
                    else foreach (var element in Elements_.Focused) element.Draw(renderer, debug);
                }
            }
            if (debug) DrawDebugName(renderer);
        }

    }
}
