using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Gui
{
    public class DepthOrdered : IEnumerable<GuiControl>
    {
        GuiControl root;
        public GuiControl Root { set => root = value; }

        public IEnumerator<GuiControl> GetEnumerator()
        {
            foreach (GuiControl control in GuiTreeTraversal.Forward(root))
                if (control.IsVisible)
                    yield return control;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
