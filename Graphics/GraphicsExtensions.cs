using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class GraphicsExtensions
    {
        public static void Sum(this ref Point p, Point a)
        {
            p.X += a.X;
            p.Y += a.Y;
        }
        public static void Sub(this ref Point p, Point a)
        {
            p.X -= a.X;
            p.Y -= a.Y;
        }
        public static Point Subtract(this Point p, Point a)
        {
            return new Point(p.X - a.X, p.Y - a.Y);
        }
    }
}
