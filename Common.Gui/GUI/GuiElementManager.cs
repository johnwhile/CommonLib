using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Gui
{
    public class GuiElementManager<T> where T : GuiElement
    {
        GuiControl control;

        public List<T> Disabled = new List<T>(0);
        public List<T> Focused = new List<T>(0);
        public List<T> Pressed = new List<T>(0);
        public List<T> Checked = new List<T>(0);
        public List<T> MouseOver = new List<T>(0);

        public GuiElementManager(GuiControl control)
        {
            this.control = control;
        }

        public void Update()
        {
            foreach (var item in Disabled) item.Update();
            foreach (var item in Focused) item.Update();
            foreach (var item in Pressed) item.Update();
            foreach (var item in Checked) item.Update();
            foreach (var item in MouseOver) item.Update();
        }

    }
}
