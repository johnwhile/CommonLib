using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Common.Tools;


namespace Common.Maths
{
    /// <summary>
    /// Oriented Bounding Rectangle
    /// </summary>
    public struct RectangleO
    {
        private const float pi2 = (float)(System.Math.PI * 2);

        private float angle;

        public Vector2f origin, axisX, halfsize;


        public Vector2f Center
        {
            get { return origin; }
        }
        public Vector2f HalfSize
        {
            get { return halfsize; }
        }
        public Vector2f AxisX
        {
            get { return axisX; }
            set { axisX = value; angle = (float)System.Math.Acos(Vector2f.Dot(axisX, Vector2f.UnitX)); }
        }
        public Vector2f AxisY
        {
            get { return new Vector2f(-axisX.y, axisX.x); }
            set { axisX = new Vector2f(value.y, -value.x); }
        }
        public float Angle
        {
            get { return angle; }
            set { angle = value % pi2; axisX = new Vector2f(System.Math.Cos(angle), -System.Math.Sin(angle)); }
        }

        public Matrix3x3f Transform
        {
            get { return Matrix3x3f.TRS(origin.x, origin.y, halfsize.x, halfsize.y, angle); }
        }

        public Vector2f Corner0
        {
            get 
            {
                Vector2f p = origin;
                p -= AxisX * HalfSize.x;
                p -= AxisY * HalfSize.y;
                return p;
            }
        }
        public Vector2f Corner1
        {
            get
            {
                Vector2f p = origin;
                p += AxisX * HalfSize.x;
                p -= AxisY * HalfSize.y;
                return p;
            }
        }
        public Vector2f Corner2
        {
            get
            {
                Vector2f p = origin;
                p += AxisX * HalfSize.x;
                p += AxisY * HalfSize.y;
                return p;
            }
        }
        public Vector2f Corner3
        {
            get
            {
                Vector2f p = origin;
                p -= AxisX * HalfSize.x;
                p += AxisY * HalfSize.y;
                return p;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="Angle">clockwise rotation around center, where Vector2.UnitX is zero</param>
        public RectangleO(Vector2f Center, Vector2f HalfSize, float Angle)
        {
            this.origin = Center;
            this.halfsize = HalfSize;
            this.axisX = new Vector2f(System.Math.Cos(Angle), -System.Math.Sin(Angle));
            this.angle = Angle;
        }

        public RectangleO(float cx, float cy, float hx, float hy, float angle) :
            this(new Vector2f(cx, cy), new Vector2f(hx, hy), angle)
        {
        }

        /// <summary>
        /// center = Zero , halfsize = 1 , rotation = 0 -> the corners are in range [-1,1]
        /// </summary>
        public static RectangleO Unit()
        {
            return new RectangleO(0, 0, 1, 1, 0);
        }


        public override string ToString()
        {
            return string.Format("C {0} , Angle {1}", origin.ToString(), Mathelp.RadianToDegree(angle));
        }
    }
}
