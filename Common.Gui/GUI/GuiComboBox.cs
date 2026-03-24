using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// </summary>
    public class GuiComboBox : GuiContainer
    {
        GuiButton scrolldownbtn;
        GuiEditText label;
        GuiButtonsList buttonslist;
        
        public List<object> Items;
        int index;

        public GuiComboBox(GuiContainer parent, Rectangle4i rectangle = default(Rectangle4i), string name = "GuiComboBox") : base(parent, rectangle, name)
        {
            Items = new List<object>();
            index = 0;
            CanParentFocus = true;
        }


        public GuiButton ScrollDownBtn
        {
            get => scrolldownbtn;
            set
            {
                if (scrolldownbtn != null)
                {
                    Remove(scrolldownbtn);
                    scrolldownbtn.m_clipDepth = 0;
                    scrolldownbtn.MouseClick -= Scrolldownbtn_MouseClick;
                }
                scrolldownbtn = value;
                scrolldownbtn.Translatable = false;
                scrolldownbtn.Resizable = false;
                scrolldownbtn.CanParentFocus = true;
                scrolldownbtn.MouseClick += Scrolldownbtn_MouseClick;
                //GuiComboBox can't be a good choice for clipping...
                scrolldownbtn.m_clipDepth = 1;
            }
        }

        void Scrolldownbtn_MouseClick(GuiControl sender)
        {
            buttonslist.IsEnabled = !buttonslist.IsEnabled;
            buttonslist.IsVisible = buttonslist.IsEnabled;

        }

        public GuiEditText Label
        {
            get => label;
            set => label = value;
        }

        public GuiButtonsList ButtonsList
        {
            get => buttonslist;
            internal set
            {
                buttonslist = value;
            }
        }


        public int SelectedItem
        {
            get => index;
            set
            {
                if (Items.Count > value) index = value;
                else throw new IndexOutOfRangeException("invalid index for current items list");
            }
        }
        public override void InitDefaultComponents() 
        {

        }

        public override void Draw(GraphicsRenderer renderer, bool debugname = false)
        {

            if (SelectedItem >= 0)
            {
                renderer.DrawString(Items[SelectedItem].ToString(), m_globalrect, Color4b.Black);
            }
        }

        public override void Update()
        {
            if (!IsFocused)
            {
                buttonslist.IsVisible = false;
                buttonslist.IsEnabled = false;
            }
        }

        
    }
}
