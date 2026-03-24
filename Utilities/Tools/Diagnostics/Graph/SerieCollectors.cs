using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Drawing.Drawing2D;

using Common.Maths;
using System.Windows.Forms;


namespace Common.Diagnostics
{
    public interface ISerieCollector
    {
        SerieLine AddSerie(string name, int pointcapacity);
        /// <summary>
        /// Add a new series, return the reference of class
        /// </summary>
        /// <param name="name"></param>
        SerieLine AddSerie(string name, int pointcapacity, Color linecolor);

        void AddValue(SerieLine serie, float y, float x = 0);
        void AddValue(int serie, float y, float x = 0);

        void SetBoundRange(AABRminmax bound);
    }

    /// <summary>
    /// Graph with more than one curve, the problem is fix the X range to include all curves without increase queue capacity
    /// </summary>
    public abstract class SerieCollector : SerieBase, ISerieCollector
    {
        #region AutoColor
        Color[] precolorlist = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Magenta,
            Color.Yellow,
            Color.Cyan
        };
        protected Color GetCurveColor(int i)
        {
            return i < precolorlist.Length ? precolorlist[i] :
                Color.FromArgb(
                Maths.Mathelp.GetRandomInt(0, 255),
                Maths.Mathelp.GetRandomInt(0, 255),
                Maths.Mathelp.GetRandomInt(0, 255));
        }
        #endregion


        /// <summary>
        /// The list of all lines
        /// </summary>
        protected List<SerieLine> series = new List<SerieLine>();

        public SerieCollector(string GraphName, SerieScaleMode ScaleMode)
            : base(GraphName, ScaleMode)
        {
        }

        public Pen AxisPen = new Pen(Brushes.Gray);
        public Font AxisFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point);
        public Brush AxisBrush = Brushes.Black;
        public Brush TrasparentBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 255));

        public int AxisSubdivisions = 5;


        public int SerieCount
        {
            get { return series.Count; }
        }

        public SerieLine AddSerie(string name, int pointcapacity)
        {
            int count = series.Count;
            return AddSerie(name, pointcapacity, GetCurveColor(count));
        }
        /// <summary>
        /// Add a new series, return the reference of class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pointcapacity"></param>
        public SerieLine AddSerie(string name, int pointcapacity, Color linecolor)
        {
            SerieLine serie = new SerieLine(name, pointcapacity, linecolor, this.ScaleMode);
            series.Add(serie);
            return serie;
        }

        public void SetBoundRange(AABRminmax bound)
        {
            foreach (SerieLine line in series)
            {
                line.range = bound;
            }
        }

        public virtual void AddValue(SerieLine serie, float y, float x=0)
        {
            if (float.IsInfinity(y) || float.IsNaN(y)) y = 0;
            if (float.IsInfinity(x) || float.IsNaN(x)) x = 0;

            //Console.WriteLine(string.Format("{0} {1}", x, y));

            serie.AddValue(new Vector2f(x,y));
        }

        public void AddValue(int serie, float y, float x=0)
        {
            if (serie < 0 || serie >= series.Count) return;
            this.AddValue(series[serie], y, x);
        }

        /// <summary>
        /// Calculate the bound of X and Y ranges
        /// </summary>
        public abstract void ComputeRangeValue();

        private void DrawLabels(Graphics graphic, System.Drawing.Rectangle client)
        {
            graphic.ResetTransform();

            SizeF stringSize = graphic.MeasureString(Title, AxisFont);

            graphic.ScaleTransform(client.Width / stringSize.Width, client.Height / stringSize.Height);
            graphic.DrawString(Title, AxisFont, TrasparentBrush, 0, 0);
            graphic.ResetTransform();

            int count = series.Count;
            int i = 1;
            foreach (SerieLine line in series)
            {
                if (line.Data.count > 0)
                {
                    float y = line.Data.Tail.y;

                    string text = string.Format("{1} {0}", line.Title, y < 99 && y > -99 ? y.ToString("G2") : ((int)y).ToString());

                    stringSize = graphic.MeasureString(text, AxisFont);
                    int d = client.Height / (count + 1) * i;
                    i++;

                    int x0 = (int)(client.Width - stringSize.Width);
                    int y0 = (int)(d - stringSize.Height / 2.0f);

                    graphic.FillRectangle(line.LineAlphaBrush, x0, y0, stringSize.Width, stringSize.Height);
                    graphic.DrawString(text, AxisFont, AxisBrush, x0, y0);
                }
            }

        }
        private void DrawAxis(Graphics graphic)
        {
            AxisPen.Width = 1;
            AxisPen.Transform = invScaleTransform;

            Vector2f min = fixrange.min;
            Vector2f max = fixrange.max;

            graphic.DrawLine(AxisPen, min.x, min.y, max.x, min.y);
            graphic.DrawLine(AxisPen, min.x, min.y, min.x, max.y);

            float dx = (max.x - min.x) / AxisSubdivisions;
            float dy = (max.y - min.y) / AxisSubdivisions;
            float p0;
            int i;
            float x, y;


            for (i = 0; i <= AxisSubdivisions; i++)
            {
                p0 = min.x + dx * i;

                graphic.Transform = screenTransform;
                graphic.DrawLine(AxisPen, p0, min.y, p0, max.y);

                //graphic.Transform = previousTransform;
                //SysDrawingUtils.TransformPoint(screenTransform, p0, fixrange.min.y, out x, out y);
                //graphic.DrawString(p0.ToString(), SystemFonts.DefaultFont, Brushes.Gray, x-5, y-10);

            }
            for (i = 0; i <= AxisSubdivisions; i++)
            {
                p0 = min.y + dy * i;

                graphic.Transform = screenTransform;
                graphic.DrawLine(AxisPen, min.x, p0, max.x, p0);

                graphic.Transform = previousTransform;
                Common.Windows.FormDrawingUtils.TransformPoint(screenTransform, min.x, p0, out x, out y);
                graphic.DrawString(p0.ToString(), AxisFont, AxisBrush, x, y - 5);
            }

            graphic.Transform = screenTransform;
        }

        public override void Draw(Graphics graphic, System.Drawing.Rectangle client, bool recalcRange = true)
        {
            // compute graph bounds and fix it if not valid.
            if (recalcRange) ComputeRangeValue();

            ComputeFixRangeValue(true);

            // compute the 2D transformation matrix to convert graph's coordinate system to windows's coordinate system.
            // This way to draw is usefull becuase i not need to transform manually the points to pixels coordinate.
            Common.Windows.FormDrawingUtils.MakeScreenTransform(ref fixrange.min, ref fixrange.max, ref client, screenTransform, invScaleTransform);

            // save original transformation.
            previousTransform = graphic.Transform;
            
            graphic.Transform = screenTransform;

            // draw all untrasformed data.
            DrawAxis(graphic);

            foreach (SerieLine line in series)
            {
                if (line.Data.count > 0)
                {
                    line.LinePen.Width = 2;
                    line.LinePen.Transform = invScaleTransform;
                    line.DrawUntrasformedV2(graphic, client);
                }
            }

            //this function reset the tranformation, so call at last
            DrawLabels(graphic, client);

            // restore original transformation.
            graphic.Transform = previousTransform;
        }
    }

    /// <summary>
    /// The X coordinate is an integer relative of insert order,
    /// </summary>
    public class SerieCollectorConstX : SerieCollector
    {
        int currentX = 0;

        public SerieCollectorConstX(string GraphName, SerieScaleMode ScaleMode)
            : base(GraphName, ScaleMode)
        {
        }
        /// <summary>
        /// Compute bound of graph and scale X values for all series
        /// </summary>
        public override void ComputeRangeValue()
        {
            range.SetEmpty();
            foreach (SerieLine line in series)
            {
                line.ComputeRangeValue();
                range.Merge(line.range);
            }

            // scale the x range
            float minx = range.min.x;
            range.max.x -= minx;
            foreach (SerieLine line in series)
            {
                for (int i = 0; i < line.Data.count; i++)
                {
                    int j = line.Data.calcIndex(i);
                    line.Data.elements[j].x -= minx;
                }
            }
        }

        public override void AddValue(SerieLine serie, float y,float x=0)
        {
            int prev = serie.Data.IsEmpty ? 0 : (int)serie.Data.Tail.x;
            if (prev == currentX) currentX++;
            x = currentX;
            serie.AddValue(new Vector2f(x, y));
        }
    }
    /// <summary>
    /// </summary>
    public class SerieCollectorXY : SerieCollector
    {
        AABRminmax bound;
        bool stopupdate;
        float width;

        public SerieCollectorXY(string GraphName, SerieScaleMode ScaleMode)
            : base(GraphName, ScaleMode)
        {
            bound = AABRminmax.Empty;
            stopupdate = false;
            width = 0;
        }

        public override void AddValue(SerieLine serie, float y, float x=0)
        {
            //Console.WriteLine(String.Format("{0}  {1}", x, y));

            base.AddValue(serie, y, x);

            bound.Merge(x, y);


            float r, d;
            // MathUtils.LooseLabel(bound.min.x, bound.max.x, 10, out bound.min.x, out bound.max.x, out r, out d);

            if (bound.Sizey > 0)
                Maths.Mathelp.LooseLabel(bound.min.y, bound.max.y, AxisSubdivisions, out bound.min.y, out bound.max.y, out r, out d);

            if (!stopupdate)
            {
                if (serie.Data.count == serie.Data.capacity)
                {
                    stopupdate = true;
                    width = bound.Sizex;
                }
            }
            else
            {
                bound.min.x = bound.max.x - width;
            }
        }

        /// <summary>
        /// Compute bound of graph and scale X values for all series
        /// </summary>
        public override void ComputeRangeValue()
        {
            range.SetEmpty();

            foreach (SerieLine line in series)
            {
                line.ComputeRangeValue();
                range.Merge(line.range);
            }

            //float r, d;
            //MathUtils.LooseLabel(bound.min.x, bound.max.x, 10, out bound.min.x, out bound.max.x, out r, out d);
            //MathUtils.LooseLabel(range.min.y, range.max.y, AxisSubdivisions, out range.min.y, out range.max.y, out r, out d);
        }

        public override void Draw(Graphics graphic, System.Drawing.Rectangle client, bool recalcRange = true)
        {
            base.range = bound;
            base.Draw(graphic, client, recalcRange);
        }
    }
}
