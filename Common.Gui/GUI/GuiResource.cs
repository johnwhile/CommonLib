using System;
using System.Drawing;
using System.Windows.Forms;

using Common;
using Common.Gui.SystemGraphic;
using Common.Maths;

namespace Common.Gui
{
    public abstract class GuiResource
    {
        static int instancescounter = 0;
        //Debugging purpose
        protected int instance = 0;
        public string Name;
        public object Tag;

        public GuiResource(GuiControl parent, Rectangle4i rectangle = default(Rectangle4i), string name = "GuiResource")
        {
            Parent = parent;
            Name = name;
            instance = instancescounter++;
            //parent is null for root's guicontrol's constructor
            Manager = parent?.Manager;
        }

        //use clip rectangle when drawing to cut the out-bound image.
        protected bool clipping = true;
        /// <summary>
        /// False by default
        /// </summary>
        public bool UseClipping
        {
            get
            {
                bool result = clipping;
                if (Manager != null) result &= Manager.UseClipping;
                return result;
            }
            set
            {
                if (Manager != null) Manager.updatePending |= GuiUpdate.Clip;
                clipping = value;
            }
        }
        //0 to use the immediate parent as "clipper", 1 for parent.parent etc...This is to resolve some drawing issue
        //like GuiSlider
        internal int m_clipDepth = 0;
        /// <summary>
        /// Return the correct Parent for clipping
        /// </summary>
        protected GuiControl ClipParent
        {
            get
            {
                int depth = m_clipDepth;
                GuiControl cliparent = Parent;
                while (depth-- > 0 && cliparent != null) cliparent = cliparent.Parent;

                if (cliparent == null) throw new NullReferenceException("Root can't has a ClipParent");

                return cliparent;
            }
        }

        /// <summary>
        /// The owner of any gui resources it's a control that define its actions
        /// </summary>
        public GuiControl Parent { get; internal set; }

        /// <summary>
        /// Only root don't contains parent
        /// </summary>
        public bool HasParent => Parent != null;

        /// <summary>
        /// gui's resources was assigned only to one Manager
        /// </summary>
        public GuiManager Manager { get; internal set; }


        /// <summary>
        /// All gui's resources are enclosed in a rectangle. This rectangle is in global coordinates (related to main root node) and it will be use for drawing.
        /// </summary>
        public abstract Rectangle4i Destination { get; internal set; }
        /// <summary>
        /// The dimension related to its parent.
        /// </summary>
        public abstract Rectangle4i Local { get; internal set; }   
        /// <summary>
        /// Draw or not
        /// </summary>
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Disable or not the events and iterations, but still draw
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        /// <summary>
        /// This resource may or not be resizable.<br/>
        /// Normally for <see cref="GuiControl"/> this is activated by holding down the mouse on the bottom-right corner
        /// </summary>
        public bool Resizable = false;
        /// <summary>
        /// defines the behavior of children's position during parent's size change, default is TopLeft corner
        /// because the <see cref="Offset"/> and <see cref="Size"/> implementation are referred to top-left corner. 
        /// </summary>
        public GuiEdge Anchor { get; set; } = GuiEdge.Top | GuiEdge.Left;


        public GuiEdge Dock { get; set; } = GuiEdge.Fill;
        /// <summary>
        /// Default drawing function, used to draw to <see cref="Control"/>. Tipically used for debugging
        /// </summary>
        public abstract void Draw(GraphicsRenderer renderer, bool debug = false);


        internal virtual void DrawDebugName(GraphicsRenderer renderer)
        {
            renderer.ClipRectangle = null;
            renderer.DrawString($"{Name}{instance}", Destination.position + 10, Color4b.Black);
        }
        
        /// <summary>
        /// Update some states before draw.
        /// </summary>
        public abstract void Update();

        public override string ToString() => $"{Name}{instance}";
        
    }
}
