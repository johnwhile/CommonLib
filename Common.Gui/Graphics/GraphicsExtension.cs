using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Common.Gui
{
    public static class GraphicsExtensions
    {


        /// <summary>
        /// Draws a rounded rectangle specified by a bounding <see cref="Rectangle"/> and four corner radius values.
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Draws a rounded rectangle specified by a bounding <see cref="Rectangle"/> and a common corner radius value for each corners.
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle specified by a bounding <see cref="Rectangle"/> and four custom corner radius values.
        /// </summary>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle specified by a bounding <see cref="Rectangle"/> and a common corner radius value for each corners.
        /// </summary>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectangle(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }


        /// <summary>
        /// Returns the path for a rounded rectangle specified by a bounding <see cref="Rectangle"/> structure and four corner radius values.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that bounds the rounded rectangle.</param>
        /// <param name="radiusTopLeft">Size of the top-left radius.</param>
        /// <param name="radiusTopRight">Size of the top-right radius.</param>
        /// <param name="radiusBottomRight">Size of the bottom-right radius.</param>
        /// <param name="radiusBottomLeft">Size of the bottom-left radius.</param>
        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            var size = new Size(radiusTopLeft << 1, radiusTopLeft << 1);
            var arc = new Rectangle(bounds.Location, size);
            var path = new GraphicsPath();

            // top left arc
            if (radiusTopLeft == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 180, 90);

            // top right arc
            if (radiusTopRight != radiusTopLeft)
            {
                size = new Size(radiusTopRight << 1, radiusTopRight << 1);
                arc.Size = size;
            }

            arc.X = bounds.Right - size.Width;
            if (radiusTopRight == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 270, 90);

            // bottom right arc
            if (radiusTopRight != radiusBottomRight)
            {
                size = new Size(radiusBottomRight << 1, radiusBottomRight << 1);
                arc.X = bounds.Right - size.Width;
                arc.Size = size;
            }

            arc.Y = bounds.Bottom - size.Height;
            if (radiusBottomRight == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 0, 90);

            // bottom left arc
            if (radiusBottomRight != radiusBottomLeft)
            {
                arc.Size = new Size(radiusBottomLeft << 1, radiusBottomLeft << 1);
                arc.Y = bounds.Bottom - arc.Height;
            }

            arc.X = bounds.Left;
            if (radiusBottomLeft == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Returns the path for a rounded rectangle specified by a bounding <see cref="Rectangle"/> structure and a common corner radius value for each corners.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that bounds the rounded rectangle.</param>
        /// <param name="radius">Size of the corner radius for each corners.</param>
        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new Rectangle(bounds.Location, size);

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
