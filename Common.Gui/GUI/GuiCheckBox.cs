using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Common.Gui.SystemGraphic;
using Common.Maths;

using Common;

namespace Common.Gui
{
    /// <summary>
    /// It's only a sample, implement it directly by GuiButton derived class<br/>
    /// - Simple square button with check symbol<br/>
    /// - Not contain children.<br/>
    /// - Check value are changed by MouseClick event<br/>
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class GuiCheckBox : GuiButton
    {
        public bool IsChecked = false;
        
        public GuiCheckBox(GuiContainer parent, Rectangle4i destination = default(Rectangle4i), string name = "GuiCheckBox") : base(parent, destination, name)
        {
            MouseClick += delegate (GuiControl sender)
            {
                IsChecked = !IsChecked;
            };

        }

        public override void InitDefaultComponents()
        {
            base.InitDefaultComponents();

            var checkedShape = new GuiRectangle(this, Destination, "GuiShapeChecked")
            {
                Background = Color4b.Mixing(Color4b.CornflowerBlue, Color4b.Black, 0.8f),
                Border = Color4b.Mixing(Color4b.CornflowerBlue, Color4b.Gray, 0.8f),
                Thickness = 2,
                Radius = 5
            };
            Elements_.Checked.Add(checkedShape);

            cursor = Inputs.MouseCursor.Hand;
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            if (IsVisible)
            {
                renderer.ClipRectangle = UseClipping ? m_cliprectangle : Rectangle4i.Null;

                if (!IsEnabled || !IsFocused)
                {
                    foreach (var element in Elements_.Disabled) element.Draw(renderer, debug);
                }
                else
                {
                    if (IsPressed) foreach (var element in Elements_.Pressed) element.Draw(renderer, debug);
                    else if (IsMouseOver) foreach (var element in Elements_.MouseOver) element.Draw(renderer, debug);
                    else foreach (var element in Elements_.Focused) element.Draw(renderer, debug);
                }
                if (IsChecked)
                    foreach (var element in Elements_.Checked) element.Draw(renderer, debug);
            }
            if (debug) DrawDebugName(renderer);
        }

    }
}
