using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using Common.Tools;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

using Matrix = System.Drawing.Drawing2D.Matrix;


namespace Common.Diagnostics
{
    [Flags]
    public enum SerieScaleMode : byte
    {
        Fixed = 0,
        AbsoluteMax = 1,
        AbsoluteMin = 2,
        RelativeMax = 4,
        RelativeMin = 8
    }

    public abstract class SerieBase
    {
        protected const int CHUNCKSIZE = 50;
        internal bool show = true;

        /// <summary>
        /// Name of series
        /// </summary>
        public string Title = "SerieBase"; 
        /// <summary>
        /// Data range
        /// </summary>
        public AABRminmax range = AABRminmax.Empty;
        /// <summary>
        /// Data range fixed (a rectangle with zero area can't be used to draw)
        /// </summary>
        public AABRminmax fixrange = AABRminmax.Empty;
        /// <summary>
        /// The Min and Max from beginning
        /// </summary>
        public AABRminmax absolute = AABRminmax.Empty;

        protected SerieScaleMode ScaleMode;

        /// <summary>
        /// for SerieScaleMode.Fixed
        /// </summary>
        public float MaxYvalue
        {
            get;
            set;
        }

        /// <summary>
        /// for SerieScaleMode.Fixed
        /// </summary>
        public float MinYvalue
        {
            get;
            set;
        }

        /// <summary>
        /// Faster way to draw points without transform it manually
        /// </summary>
        protected Matrix screenTransform;
        protected Matrix invScaleTransform;
        protected Matrix previousTransform;

        protected SerieBase(string name, SerieScaleMode ScaleMode)
        {
            this.ScaleMode = ScaleMode;
            screenTransform = new Matrix();
            screenTransform.Reset();
            invScaleTransform = new Matrix();
            invScaleTransform.Reset();
            previousTransform = new Matrix();
            previousTransform.Reset();
            Title = name;
        }

        /// <summary>
        /// Calculate a copy of range rectangle valid for drawing
        /// </summary>
        /// <param name="round">Round algorithm</param>
        public void ComputeFixRangeValue(bool round)
        {
            fixrange = range;

            bool invalidmax = fixrange.max.IsNaN;
            bool invalidmin = fixrange.min.IsNaN;

            if (invalidmax && invalidmin)
            {
                fixrange.min = -Vector2f.One;
                fixrange.max = Vector2f.One;
            }
            else if (invalidmax)
            {
                fixrange.max = fixrange.min + Vector2f.One;
            }
            else if (invalidmin)
            {
                fixrange.min = fixrange.max - Vector2f.One;
            }

            if (fixrange.IsEmpty)
            {
                // the copy of range are used to maintain the original data
                if (Maths.Mathelp.AlmostEqual(fixrange.max.x, fixrange.min.x))
                {
                    fixrange.max.x = fixrange.min.x > 0 ?
                        fixrange.min.x * 1.01f :
                        fixrange.min.x * 0.99f;
                }
                if (Maths.Mathelp.AlmostEqual(fixrange.max.y, fixrange.min.y))
                {
                    fixrange.max.y = fixrange.min.y > 0 ?
                        fixrange.min.y * 1.01f :
                        fixrange.min.y * 0.99f;
                }

                if (fixrange.IsEmpty)
                {
                    //throw new ArgumentException("some problem with float precision");
                    fixrange = AABRminmax.UnitXY;
                }

            }
        }


        /// <summary>
        ///  Draw line using range rectangle to fill in screen rect
        /// </summary>
        public abstract void Draw(Graphics graphic, System.Drawing.Rectangle client , bool recalcRange = true);

        public override string ToString()
        {
            return Title;
        }
    }


    /// <summary>
    /// </summary>
    public class SerieLine : SerieBase
    {
        int knotSize = 2;
        int avaragecount = 0;
        bool drawed = false;
        float avaragey = 0;

        /// <summary>
        /// Refresh avarage calculation each time you call Draw.
        /// </summary>
        public float AvarageY
        {
            get; private set;
        }


        //Bitmap pointIcon = new Bitmap(4, 4);

        /// <summary>
        /// Data storage, queue array to discard old data
        /// </summary>
        public MyQueue<Vector2f> Data;

        /// <summary>
        /// Chuncked array to improve draw
        /// </summary>
        protected PointF[] Points = new PointF[CHUNCKSIZE];
        /// <summary>
        /// Color of line
        /// </summary>
        public Pen LinePen;
        public Brush LineSolidBrush;
        public Brush LineAlphaBrush;


        /// <summary>
        /// If True, the range Xmin,Xmax will be constantly update.
        /// </summary>
        public bool FillRangeX { get; set; }
        /// <summary>
        /// </summary>
        internal SerieLine(string name, int capacity, Color linecolor, SerieScaleMode ScaleMode)
            : base(name, ScaleMode)
        {
            range = AABRminmax.Zero;
            Data = new MyQueue<Vector2f>(capacity);
            LinePen = new Pen(linecolor, 1);
            LineSolidBrush = new SolidBrush(linecolor);
            LineAlphaBrush = new SolidBrush(Color.FromArgb(100, linecolor.R, linecolor.G, linecolor.B));
            
            
            //using (Graphics g = Graphics.FromImage(pointIcon))
            //{
            //    g.DrawRectangle(LinePen, 0, 0, 4, 4);
            //}
        }
        
        
        
        /// <summary>
        /// Add a new value, remove older value if you reach the serie capacity
        /// </summary>
        public void AddValue(Vector2f value)
        {
            if (Data.Count == Data.Capacity)
            {
                Data.RemoveHead();
            }
            Data.AddTail(value);

            absolute.Merge(ref value);

            if (drawed)
            {
                drawed = false;
                
                avaragecount = 0;
                avaragey = value.y;            
                AvarageY = avaragey / avaragecount;
            }
            else
            {
                avaragey += value.y;
                avaragecount++;
            }

        }
        
        /// <summary>
        /// Recalculate the range rectangle.
        /// </summary>
        public void ComputeRangeValue()
        {
            range.SetEmpty();
            foreach (Vector2f value in Data) range.Merge(value.x, value.y);
        }

        public float GetMinX()
        {
            float min = float.PositiveInfinity;
            foreach (Vector2f value in Data) if (min > value.x) min = value.x;
            return min;
        }

        /// <summary>
        /// Draw line using range rectangle to fill it in the screen rectangle.
        /// Point are not transformed in pixel coordinates.
        /// </summary>
        public void DrawUntrasformedV1(Graphics graphic, System.Drawing.Rectangle client)
        {
            if (Data.count < 2) return;

            Vector2f prev = Data.ItemList[0];

            //float x, y;
            Matrix prevtransform = graphic.Transform;
            //SysDrawingUtils.TransformPoint(prevtransform, prev.x, prev.y, out x, out y);
            //graphic.ResetTransform();
            //graphic.DrawString(prev.y.ToString(), SystemFonts.MenuFont, Brushes.Red, x, y);
            //graphic.Transform = prevtransform;

            for (int i = 0; i < Data.count; i++)
            {
                Vector2f value = Data.ItemList[i];
                graphic.DrawLine(this.LinePen, prev.x, prev.y, value.x, value.y);
                prev = value;
            }
        }
        /// <summary>
        /// new method, the idea is to reduce the DrawLine calls
        /// </summary>
        public void DrawUntrasformedV2(Graphics graphic, System.Drawing.Rectangle client)
        {
            if (Data.count < 2) return;

            Vector2f vect = Data.Tail;

            //float x, y;
            Matrix prevtransform = graphic.Transform;

            //SysDrawingUtils.TransformPoint(prevtransform, 0, fixrange.Center.y, out x, out y);
            //graphic.ResetTransform();
            //graphic.DrawString(vect.y.ToString(), SystemFonts.MenuFont, Brushes.Red, client.Width - 10, client.Height / 2);
            //graphic.Transform = prevtransform;

            int i = 0;

            while (i < Data.count)
            {
                int rest = Data.count - i;
                int j = rest < CHUNCKSIZE ? Data.count : i + CHUNCKSIZE;
                int k = 0;
                for (k = 0; i < j; k++, i++)
                {
                    vect = Data.ItemList[i];
                    Points[k].X = vect.x;
                    Points[k].Y = vect.y;
                }

                if (rest < CHUNCKSIZE)
                {
                    for (; k < CHUNCKSIZE; k++)
                    {
                        Points[k] = Points[k - 1];
                    }
                }
                // if isn't last loop, go back by one to add last previos point
                if (j < Data.count)
                {
                    i--;
                }
                graphic.DrawLines(LinePen, Points);
                
            }

            graphic.ResetTransform();

            foreach (Vector2f point in Data.ItemList)
            {
                Common.Windows.FormDrawingUtils.TransformPoint(prevtransform, point.x, point.y, out vect.x, out vect.y);
                //graphic.DrawImage(pointIcon, vect.x, vect.y);
                graphic.FillRectangle(LineSolidBrush, vect.x - knotSize, vect.y - knotSize, knotSize * 2, knotSize * 2);
            }

            graphic.Transform = prevtransform;
        }

        /// <summary>
        ///  Draw line using range rectangle to fill in screen rect
        /// </summary>
        public override void Draw(Graphics graphic, System.Drawing.Rectangle client, bool recalcRange = true)
        {
            drawed = true;

            ComputeRangeValue();
            ComputeFixRangeValue(true);

            Common.Windows.FormDrawingUtils.MakeScreenTransform(ref fixrange.min, ref fixrange.max, ref client, screenTransform, invScaleTransform);

            LinePen.Width = 1;
            LinePen.Transform = invScaleTransform;

            previousTransform = graphic.Transform;
            graphic.Transform = screenTransform;

            DrawUntrasformedV2(graphic, client);

            graphic.Transform = previousTransform;
        }


        public override string ToString()
        {
            return base.ToString();
        }
    }
}
