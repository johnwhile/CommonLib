using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Common.Maths;
using Common.Gui;
using Common.Gui.SystemGraphic;
using Common.Inputs;

namespace Common
{
    public partial class GuiForm : Form
    {
        GuiManager manager;
        GraphicsRenderer renderer;
        ImageAtlasLayout layout;
        Image guiTexture;

        SysGuiImage getIcon(byte typecode)
        {
            layout.TryGetSource(typecode, out var source, out _, out _);
            return new SysGuiImage(guiTexture, source);
        }

        public GuiForm()
        {
            InitializeComponent();

            layout = ImageAtlasLayout.Open(@"Graphics\gui.xml");
            guiTexture = Image.FromFile(layout.ImageFilename[0]);

            manager = new GuiManager();
            renderer = new GraphicsRenderer(manager);

            manager.Root = new GuiRoot(manager);
            manager.Root.IsVisible = true;
            manager.UseClipping = true;

           
            //link control's painting event to renderer
            renderer.PaintController = renderPanel;
            
            renderPanel.ClientSizeChanged += RenderPanelClientSizeChanged;
            
            renderPanel.MouseDown += delegate (object sender, MouseEventArgs arg)
            {
                manager.MouseDown(arg.Location, arg.Button);
                if (sender is Control control) control.Refresh();
            };
            renderPanel.MouseUp += delegate (object sender, MouseEventArgs arg)
            {
                manager.MouseUp(arg.Location, arg.Button);
                if (sender is Control control) control.Refresh();
            };
            renderPanel.MouseMove += delegate (object sender, MouseEventArgs arg)
            {
                manager.MouseMove(arg.Location, arg.Button);
                if (sender is Control control) control.Refresh();
            };

            KeyDown += delegate (object sender, KeyEventArgs arg) 
            {
                manager.KeyDown(arg);
                if (sender is Control control)
                {
                    control.Refresh();
                }
            };

            KeyPreview = true;

            BuildGui1();

            RenderPanelClientSizeChanged(renderPanel, null);
        }



        private void RenderPanelClientSizeChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            manager.Root.Size = control.ClientSize - new Size(20, 20);
            manager.Root.Offset = new Vector2i(10, 10);
            control.Refresh();
        }

        void BuildGui2()
        {

        }

        void BuildGui1()
        {
            var root = manager.Root;
            Image image = Image.FromFile(layout.ImageFilename[0]);

            root.GuiCursor = MouseCursor.No;
            root.UseClipping = true;

            var p1 = new GuiPanel(root, new Rectangle4i(50, 50, 400, 400))
            { UseClipping = true };

            //p1.iconPanel = getIcon(1);
            //p1.srcBorderpx = 10;
            //p1.dstBorderpx = 10;


            var b1 = new GuiButton(p1, new Rectangle4i(50, 20, 200, 50))
            { 
                UseClipping = true
            };
            b1.InitDefaultComponents();

            var testo = new GuiText(b1, "Testo", DefaultFont, Color4b.Black, new Rectangle4i(10, 10, 10, 10));

            b1.Elements_.Focused.Add(testo);
            b1.Elements_.Disabled.Add(testo);
            b1.Elements_.MouseOver.Add(testo);
            b1.Elements_.Pressed.Add(testo);


            b1.MouseClick += delegate (GuiControl sender) { Debugg.Info($"{sender.Name} CLICK"); };
            //b1.iconPressed = getIcon(2);
            //b1.iconRelease = getIcon(1);
            //b1.srcBorderpx = 20;
            //b1.dstBorderpx = 20;

            var c1 = new GuiCheckBox(p1, new Rectangle4i(50,80,30,30))
            { UseClipping = true };

            //c1.iconPressed = getIcon(5);
            //c1.iconRelease = getIcon(16);
            //c1.iconSymbol = getIcon(4);
            //c1.iconSymbol.SetRemapColor(Color4b.Black, Color4b.Blue);

            var p2 = new GuiPanel(null, new Rectangle4i(400, 100, 400, 400))
            { 
                UseClipping = false ,
                CanParentFocus = false
            };


            /// VERTICAL SLIDERBAR

            int h = 10;

            var sbar1 = new GuiScrollBar(p2, GuiScrollType.Vertical, new Rectangle4i(50, 50, 10, 200))
            { UseClipping = true };

            var slider1 = new GuiSlider(sbar1, new Rectangle4i(-2, 0, 14, 150))
            { UseClipping = true };

            
            var s1up = new GuiButton(sbar1, new Rectangle4i(0, -(h+2), h, h))
            {
                UseClipping = true,
                Name = "UpBtn1",
                GuiCursor = new MouseCursor(Cursors.PanNorth)
            };

            var s1down = new GuiButton(sbar1, new Rectangle4i(0, sbar1.Size.height+2, h, h))
            {
                UseClipping = true,
                Name = "DownBtn1",
                GuiCursor = new MouseCursor(Cursors.PanSouth)
            };

            sbar1.ButtonDown = s1down;
            sbar1.ButtonUp = s1up;

            /// HORIZONTAL SLIDERBAR

            var sbar2 = new GuiScrollBar(p2, GuiScrollType.Horizontal, new Rectangle4i(20, 300, 300, 10))
            { UseClipping = true };
            var slider2 = new GuiSlider(sbar2, new Rectangle4i(10, -2, 50, 14))
            { UseClipping = true };

            /// COMBO BOX

            var combo = new GuiComboBox(p2, new Rectangle4i(200, 50, 100, 20))
            { UseClipping = true };

            combo.Items.Add("Row1");
            combo.Items.Add("Row2");
            combo.Items.Add("Row3");

            var combobtn = new GuiButton(combo, new Rectangle4i(combo.Size.width, 0, combo.Size.height, combo.Size.height))
            { UseClipping = true };

            combo.ScrollDownBtn = combobtn;

            var combolist = new GuiButtonsList(combo, new Rectangle4i(0, combo.Size.height, combo.Size.width, 20 * combo.Items.Count))
            { UseClipping = true };


            foreach (var control in GuiTreeTraversal.Forward(root))
            {
                if (control!= b1)
                control.InitDefaultComponents();
                control.AlwaysFocused = false;
            }

        }

        void generatetree(GuiContainer root, ref int count, int level = 3)
        {
            if (level < 0) return;
            int childs = 5;// Mathelp.Rnd.Next(0, 8);
            for (int i = 0; i < childs; i++)
            {
                int w = Mathelp.Rnd.Next(100, 200);
                int h = Mathelp.Rnd.Next(100, 200);
                int x = Mathelp.Rnd.Next(0, 100);
                int y = Mathelp.Rnd.Next(0, 100);
               
                var p = new GuiPanel(root);
                p.UseClipping = false;
                p.Offset = new Vector2i(x, y);
                p.Size = new Vector2i(w, h);

                count++;
                generatetree(p, ref count, level - 1);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            renderPanel.Refresh();
        }
    }
}
