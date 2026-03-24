using Common;
using Common.Maths;
using Common.Tools;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Matrix = System.Drawing.Drawing2D.Matrix;

namespace Test_GraphicMath
{
    public partial class MatrixFrom4Corners : Form
    {
        Font font;
        Point[] points = new Point[4];
        int selected = -1;

        Vector2f[] source;
        Vector2f[] destination;
        Matrix3x3f transform;
        Matrix3x3f transformInverse;

        Bitmap srcImage;
        Bitmap destImage;
        Rectangle4i bound;
        Point prevmouse;

        public MatrixFrom4Corners(int width = 600, int height = 400)
        {
            InitializeComponent();
            ClientSize = new Size(width, height);
            font = new Font("Calibri", 10);

            points[0] = new Point(50, 50);
            points[1] = new Point(50, 200);
            points[2] = new Point(200, 200);
            points[3] = new Point(200, 50);

            DoubleBuffered = true;

            srcImage = new Bitmap("image.bmp");

            source = new Vector2f[]
            {
                new Vector2f(0, 0),
                new Vector2f(0, srcImage.Height),
                new Vector2f(srcImage.Width, srcImage.Height),
                new Vector2f(srcImage.Width, 0)
            };

            destination = new Vector2f[]
            {
                points[0],
                points[1],
                points[2],
                points[3]
            };

            UpdateImage();
        }


        void UpdateImage()
        {
            bound = Rectangle4i.MakeBoundFromPoints(points);

            for (int i = 0; i < 4; i++) destination[i] = (Vector2i)points[i] - bound.position;

            transform = Matrix3x3f.MakeTransformFromTwoRectangle(source, destination);
            transformInverse = transform.Inverse();

            //destination bitmap are a rectangle image aligned to screen
            destImage = new Bitmap(bound.size.width, bound.size.height);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.Clear(Color.IndianRed);
                
            }

            //foreach pixel in the extended bitmap
            for (int x = 0; x < bound.width; x++)
                for (int y = 0; y < bound.height; y++)
                {
                    Vector2f coord = new Vector2f(x, y);
                    coord = transformInverse * coord;

                    if (coord.x >= 0 && coord.x < srcImage.Width && coord.y >= 0 && coord.y < srcImage.Height)
                    {
                        Color pixel = srcImage.GetPixel((int)coord.x, (int)coord.y);
                        destImage.SetPixel(x,y, pixel);
                    }
                }
        }
    



        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (selected >= 0)
            {
                prevmouse = e.Location.Subtract(prevmouse);
                if (selected < 4)
                    points[selected] = e.Location;
                else
                    for (int i = 0; i < 4; i++) points[i].Sum(prevmouse);

            }

            Vector2i local = (Vector2i)e.Location - bound.position;


            Vector2i p = transformInverse * local;

            Text = $"mouse: {local} transform: {p}";

            prevmouse = e.Location;
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            selected = -1;

            for (int i = 0; i < 4; i++)
                destination[i] = points[i];
            //destination[i] = new Vector2f(points[i].X - points[0].X, points[i].Y - points[0].Y);

            
            UpdateImage();

            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            prevmouse = e.Location;

            selected = -1;
            for (int i = 0; i < 4; i++)
            {
                var p = points[i];
                var m = e.Location;
                if (m.X >= p.X - 5 && m.X <= p.X + 5 &&
                    m.Y >= p.Y - 5 && m.Y <= p.Y + 5)
                    selected = i;
            }

            if (selected<0)
            {
                if (bound.Contain(e.Location)) selected = 4;
            }


            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Color.CornflowerBlue);

            //g.Transform = transform;
            g.DrawImage(destImage, bound.position);

            //g.DrawImage(image, points);

            g.ResetTransform();
            g.DrawPolygon(Pens.Black, points);

            foreach (var p in points)
                g.FillRectangle(Brushes.Red, p.X - 5, p.Y - 5, 10, 10);

            g.DrawRectangle(Pens.Gray, bound);

        }
    }
}
