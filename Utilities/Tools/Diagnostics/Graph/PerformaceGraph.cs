
using System;
using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Drawing2D;

using Common.Maths;
using Common.Windows;

namespace Common.Diagnostics
{
    /// <summary>
    /// A line graph to show a value in the time
    /// </summary>
    public class PerformaceGraph : UserControl
    {

        bool isInDesignMode = false;

        Random rnd = new Random();
 
        bool PlotterConstX = true;
        public bool useAbsoluteMinMax = true;
        public bool useDoubleBuffer = true;
        public SmoothingMode smoothing = SmoothingMode.None;

        //SerieCollector Plotter;

        DoubleBuffered memGraphics;
        Bitmap TmpBackBuffer;

        Font font;
        int drawcount = 0;

        bool candraw = true;
        bool mousehold = false;

        Brush background;
        LinearGradientBrush gradientbackground;

        public readonly SerieCollector Plotter;


        #region Constructors

        public PerformaceGraph()
            : this(false, SerieScaleMode.RelativeMax | SerieScaleMode.RelativeMin)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PlotterConstX">The X coordinate is an integer relative of insert order, all series in the same plotter must be coerent</param>
        /// <param name="ScaleMode">Not implemented</param>
        public PerformaceGraph(bool PlotterConstX, SerieScaleMode ScaleMode)
        {

            this.isInDesignMode = FormUtils.IsInDesignMode(this);

            this.PlotterConstX = PlotterConstX;

            font = new Font("Currier New", 8);
            background = new SolidBrush(BackColor);

            if (isInDesignMode)
            {
                useDoubleBuffer = false;
                Plotter = new SerieCollectorConstX(Name, ScaleMode);

                SerieLine first = Plotter.AddSerie("FIRST", 50);
                SerieLine second = Plotter.AddSerie("SECOND", 50);
            }
            else
            {
                if (PlotterConstX)
                    Plotter = new SerieCollectorConstX(Name, ScaleMode);
                else
                    Plotter = new SerieCollectorXY(Name, ScaleMode);
            } 
            
            Title = "MyPlotterControl";
        }


        /// <summary>
        /// Name of plotter control
        /// </summary>
        public string Title
        {
            get { return Plotter.Title; }
            set { Plotter.Title = value; }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            updateSize();
            memGraphics = new DoubleBuffered(ClientSize);
        }
        #endregion


        void updateSize()
        {
            gradientbackground = new LinearGradientBrush(new Point(0, 0), new Point(0, ClientSize.Height), Color.White, Color.LightBlue);
        }

        private void PaintBackground(Graphics graphic, System.Drawing.Rectangle client)
        {
            //graphic.Clear(Color.White);
            graphic.FillRectangle(gradientbackground, this.ClientRectangle);
        }


        private void PaintControl(Graphics graphic, System.Drawing.Rectangle client)
        {
            Plotter.Draw(graphic, client);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            TmpBackBuffer = memGraphics.BackBuffer;
            mousehold = true;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
 	         base.OnMouseUp(e);
             mousehold = false;
        }
        

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!candraw) return;

            System.Drawing.Rectangle client = ClientRectangle;
            client.X += 5;
            client.Y += 5;
            client.Width -= 10;
            client.Height -= 10;

            if (isInDesignMode)
            {
                Graphics CurGraphics = e.Graphics;
                drawcount++;

                int serie = Maths.Mathelp.GetRandomInt(0, 2);
                if (serie > 1) serie = 1;
                Plotter.AddValue(serie, Maths.Mathelp.GetRandomFloat() * 100.0f);
 
                PaintBackground(CurGraphics, client);
                PaintControl(CurGraphics, client);
            }
            else
            {
                if (ParentForm != null)
                {
                    if (mousehold)
                    {
                        e.Graphics.DrawImage(TmpBackBuffer, 0, 0);
                    }
                    else
                    {
                        Graphics CurGraphics = useDoubleBuffer ? memGraphics.BufferGraphics : e.Graphics;

                        CurGraphics.SmoothingMode = smoothing;

                        PaintBackground(CurGraphics, client);
                        PaintControl(CurGraphics, client);

                        if (useDoubleBuffer)
                        {
                            memGraphics.Render(e.Graphics);
                        }
                    }
                }
                else
                {
                    PaintBackground(e.Graphics, client);
                }
            }
            
        }

        protected override void OnResize(EventArgs e)
        {
            if (memGraphics != null)
            {
                if (ClientSize.Width<=0 || ClientSize.Height<=0)
                {
                    candraw = false;
                }
                else
                {
                    candraw = true;
                    memGraphics.Resize(ClientSize);
                    updateSize();

                    Invalidate();
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PerformaceGraph
            // 
            this.Name = "PerformaceGraph";
            this.ResumeLayout(false);
        }

    }
}
