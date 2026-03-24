using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Common.Maths;


namespace Common.Gui.SystemGraphic
{
    /// <summary>
    /// The example renderer using System's <see cref="Graphics"/>
    /// </summary>
    public class GraphicsRenderer
    {
        GuiManager manager;
        Control painter;
        Graphics graphics;
        SolidBrush brush;
        Pen pen;
        Font debugfont;

        Font font;

        public Font Font
        {
            get => font;
            set => font = value;
        }


        public GraphicsRenderer(GuiManager guiInterface)
        {
            pen = new Pen(Color4b.White);
            brush = new SolidBrush(pen.Color);
            manager = guiInterface;
            debugfont = new Font("Calibri", 8);
        }

        public Control PaintController
        {
            get => painter;
            set
            {
                if (ReferenceEquals(painter, value)) return;

                if (painter != null)
                {
                    painter.Paint -= OnControlPaint;
                    painter.Disposed -= OnControlDisposing;
                }
                if (value != null)
                {
                    painter = value;
                    painter.Paint += OnControlPaint;
                    painter.Disposed += OnControlDisposing;
                }

            }
        }



        Rectangle4i? m_clipregion = null;
        /// <summary>
        /// Use a clip rectangle to cut the drawing area. Set null if you want disable
        /// </summary>
        public Rectangle4i? ClipRectangle
        {
            get => m_clipregion;
            set
            {
                if (value.HasValue)
                {
                    //change clipregion only if different
                    if (m_clipregion == null || m_clipregion.Value != value.Value)
                    {
                        graphics?.ResetClip();
                        var region = value.Value;
                        //need to add one more pixel on left and bottom borders 
                        region.size += 1;
                        graphics?.IntersectClip(region);
                    }
                }
                //set null it means disable clipping
                else
                {
                    graphics?.ResetClip();
                }
                m_clipregion = value;
            }
        }

        void OnControlDisposing(object sender, EventArgs arg)
        {
            PaintController = null;
        }

        void OnControlPaint(object sender, PaintEventArgs arg)
        {
            graphics = arg.Graphics;

            foreach (GuiControl ctrl in manager.DepthOrderedControls)
            {
                ctrl.Update();
                ctrl.Draw(this, true);
            }

            if (manager.LastVisitedByMouse != null)
            {
                //Debugg.Info("Last visited byte mouse " + manager.LastVisitedByMouse.Name);
                //The cursor must be set only after all GuiControls are drawed to avoid cursor flickering
                painter.Cursor = manager.LastVisitedByMouse.GuiCursor ?? Cursors.Default;
            }
            else
            {
                painter.Cursor = (sender as Control).Cursor;
            }
        }


        public Vector2i MeasureString(Font font, string text)
        {
            return graphics.MeasureString(text, font);
        }

        public void DrawShape(SysGuiImage shape, Rectangle4i destination)
        {
            shape.Draw(this, destination);
        }
        public void DrawRectangle(Rectangle4i rectangle, Color4b color, int width = 1, int radius = 0)
        {
            if (color.a == 0 || width <= 0) return;
            pen.Color = color;
            pen.Width = width;
            if (radius > 0)
                graphics.DrawRoundedRectangle(pen, rectangle, radius);
            else graphics.DrawRectangle(pen, rectangle);
        }
        public void DrawEllipse(Rectangle4i rectangle, Color4b color, int width = 1)
        {
            if (color.a == 0) return;
            pen.Color = color;
            pen.Width = width;
            graphics.DrawEllipse(pen, rectangle);
        }
        public void FillRectangle(Rectangle4i rectangle, Color4b color, int radius = 0)
        {
            if (color.a == 0) return;
            brush.Color = color;
            if (radius > 0) graphics.FillRoundedRectangle(brush, rectangle, radius);
            else graphics.FillRectangle(brush, rectangle);
        }
        public void FillEllipse(Rectangle4i rectangle, Color4b color)
        {
            if (color.a == 0) return;
            brush.Color = color;
            graphics.FillEllipse(brush, rectangle);
        }
        public void DrawImage(Image image, Rectangle4i source, Rectangle4i destination, ImageAttributes attribute = null)
        {
            graphics.DrawImage(image, destination, source.x, source.y, source.width, source.height, GraphicsUnit.Pixel, attribute);
        }
        public void DrawString(string text, Rectangle4i destination, Color4b color, Font font = null)
        {
            if (color.a == 0) return;
            brush.Color = color;
            graphics.DrawString(text, font ?? Font ?? debugfont, brush, destination);
        }
        public void DrawString(string text, Vector2i pos, Color4b color, Font font = null)
        {
            DrawString(text, pos.x, pos.y, color, font);
        }
        public void DrawString(string text, int x, int y, Color4b color, Font font = null)
        {
            if (color.a == 0) return;
            brush.Color = color;
            graphics.DrawString(text, font ?? Font ?? debugfont, brush, x, y);
        }
    }
}
