using System;
using System.Collections.Generic;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// Virtual GuiController for tree hierarchy consistency.
    /// </summary>
    public class GuiRoot : GuiContainer
    {
        public GuiRoot(GuiManager manager, Rectangle4i viewport = default(Rectangle4i), string name = "GuiRoot") : base(null, viewport, name)
        {
            Resizable = false;
            Translatable = false;
            CanParentFocus = false;
#if !DEBUG
            IsVisible = false;
#endif
            Manager = manager;
        }

        public override void InitDefaultComponents()
        {
            Elements = new List<GuiElement>();
            Elements.Add(new GuiRectangle(this, Destination, "GuiRootRectangle")
            {
                Border = Color4b.Black,
                Background = Color4b.Trasparent,
                Radius = 0,
                Thickness = 1
            });
        }

        public override void Draw(GraphicsRenderer renderer, bool debug = false)
        {
            Elements[0].Draw(renderer, debug);
            if (debug) DrawDebugName(renderer);
        }

        
    }
}
