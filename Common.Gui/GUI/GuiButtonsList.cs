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
    public class GuiButtonsList : GuiControl
    {
        protected GuiComboBox owner;

        public GuiButtonsList(GuiComboBox parent, Rectangle4i destination = default(Rectangle4i)) : base(parent, destination)
        {
            owner = parent;
            owner.ButtonsList = this;
            CanParentFocus = true;
            Resizable = false;
            Translatable = false;
            //set hide as initial state
            IsVisible = false;
            IsEnabled = false;
            //GuiComboBox can't be a good choice for clipping...
            m_clipDepth = 1;
        }

        public override void OnMouseClick()
        {
            if (owner.Items.Count > 0)
            {
                float y = Manager.MousePosition.y - m_globalrect.y;
                y /= m_globalrect.height;
                y *= owner.Items.Count;
                owner.SelectedItem = (int)y;
            }
            base.OnMouseClick();
        }

        public override void InitDefaultComponents()
        {

        }


        public override void Draw(GraphicsRenderer renderer, bool debugname = false)
        {
            if (renderer == null) return;
            int rows = owner.Items.Count;
            int dy = m_globalrect.height / rows;

            renderer.FillRectangle(m_globalrect, Color4b.White);
            renderer.DrawRectangle(m_globalrect, Color4b.Black);

            for (int i = 0; i < rows; i++)
            {
                string text = owner.Items[i].ToString();
                Rectangle4i rect = m_globalrect;
                rect.height = dy;
                rect.y = m_globalrect.y + dy * i;

                if (owner.SelectedItem == i)
                {
                    renderer.FillRectangle(rect, Color4b.Blue);
                    renderer.DrawString(text, rect, Color4b.White);
                }
                else
                {
                    renderer.DrawString(text, rect, Color4b.Black);
                }
                renderer.DrawRectangle(rect, Color4b.Gray);
            }
            //base.Draw(renderer, debugname);
        }
    }
}
