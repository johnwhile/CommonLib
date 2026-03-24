
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Common.Maths;
using Common.Tools;

namespace Common
{
    /// <summary>
    /// Common math for all trackball implementations, need to link the mouse events
    /// </summary>
    public abstract class TrackBallCameraMath : Camera_old
    {
        protected enum Mousing
        {
            // mouse was not pressed : view or proj matrix is fixed -> avoid constanly update
            None,
            // left mouse pressed
            Traslation,
            // right mouse pressed
            Rotating,
            // wheel mouse rotating
            Zooming,
            // middle (wheel) mouse pressed
            Turning
        }
        protected Mousing mousing;

        ViewportClip viewport;
       
        float zoom = 1.0f;
        float radius = 1.0f;
        Vector2i start, end;

        // if use accumulation matrix, cameraStart is the initial cameraview value,
        // after mouse release, the accumulations will be applied to camera view. 
        Matrix4x4f rotation;
        Matrix4x4f traslation;
        Matrix4x4f cameraStart;

        public TrackBallCameraMath(ViewportClip viewport, Matrix4x4f view, Matrix4x4f proj) : base(proj,view)
        {
            this.zoom = 1.0f;
            cameraStart = cameraview;
            this.viewport = viewport;
            this.mousing = Mousing.None;
            this.rotation = Matrix4x4f.Identity;
            this.traslation = Matrix4x4f.Identity;
            start = new Vector2i(0, 0);
            end = new Vector2i(0, 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="control">The ball require information about size of panel, at this moment viewport match with clientrectangle</param>
        public TrackBallCameraMath(ViewportClip viewport, Vector3f Eye, Vector3f Targhet, float near, float far)
            : base( Matrix4x4f.MakeProjectionLHAFovY(near, far, viewport.Aspect),Matrix4x4f.MakeViewLH(Eye, Targhet, Vector3f.UnitY))
        {
            this.zoom = Vector3f.Distance(Eye, Targhet);
            cameraStart = cameraview;
        }


        protected abstract void GetMouseCoord(out Vector2i pos);
        

        protected void ViewportResize(ViewportClip newviewport)
        {
            viewport = newviewport;
            Aspect = viewport.Aspect;
        }
        
        protected void MouseRotateDown()
        {
            mousing = Mousing.Rotating;
            cameraStart = cameraview;
            rotation = Matrix4x4f.Identity;
            GetMouseCoord(out start);
        }
        
        protected void MouseTraslateDown()
        {
            mousing = Mousing.Traslation;
            GetMouseCoord(out start);
        }
        
        protected void MouseUp()
        {
            mousing = Mousing.None;
            start = end;
           
            CameraView = cameraStart * rotation;
            rotation = Matrix4x4f.Identity;
            
        }
        
        protected void MouseWheeling(int Z)
        {
            Console.WriteLine("TrackBall Zooming");

            mousing = Mousing.Zooming;

            if (Z > 0)
                zoom *= 0.9f;
            else
                zoom *= 1.1f;

            //camera.eye = new Vector3(0, 0, -zoom);
            //camera.UpdateView();

            mousing = Mousing.None;
        }
        
        protected void MouseMoving()
        {
            GetMouseCoord(out end);

            if (mousing != Mousing.None)
            {
                int dx = end.x - start.x;
                int dy = start.y - end.y;

                if (mousing == Mousing.Traslation)
                {

                    if (dx != 0 || dy != 0)
                    {
                        /*
                        Console.WriteLine(string.Format("TrackBall Traslation {0} {1}", dx, dy));
                        

                        Matrix4 view = Camera.View;
                        Vector3 vRight = new Vector3(view.m00, view.m01, view.m02);
                        Vector3 vUp = new Vector3(view.m10, view.m11, view.m12);
                        Vector3 move = (dx * vRight + dy * vUp) * (0.001f * zoom);

                        Matrix4 tras = Matrix4.Translating(ref move);
                        traslation = tras * traslation;

                        //cameraview.Position -= move;
                        //view = cameraview.Inverse();
                       */
                    }
                }
                else if (mousing == Mousing.Rotating)
                {
                    Console.WriteLine("TrackBall Rotating " + dx.ToString() + " " + dy.ToString());

                    Vector3f p0 = ProjectOnSphere(start.x, start.y);
                    Vector3f p1 = ProjectOnSphere(end.x, end.y);
                    Vector3f dir = p0 - p1;

                    if (dir.LengthSq > 0)
                    {
                        rotation = RotationFromVectors(p0, p1);
                        CameraView = cameraStart * rotation;
                    }
                }
            }
        }

        protected float GetMaxArcBallRadius()
        {
            return (int)(Mathelp.MIN(viewport.Width, viewport.Height) / 2 * radius);
        }

        /// <summary>
        /// Square viewport
        /// adjust to center screen in [-1,1] range and flip Y
        /// </summary>
        protected void ToCenter(ref float x, ref float y)
        {
            x = 2.0f * x / viewport.Width - 1.0f;
            y = 1.0f - 2.0f * y / viewport.Height;
        }

        /// <summary>
        /// Convert a screen point to the screen virtual hemisphere, the result is always a normalized vector
        /// </summary>
        protected Vector3f ProjectOnSphere(int X, int Y)
        {
            Vector3f p = new Vector3f(X, Y, 0);

            // adjust to [-1,1] range with y inverted
            ToCenter(ref p.x, ref p.y);

            // distance from radius, z=0
            float distSq = p.x * p.x + p.y * p.y;
            float radiusSq = radius * radius;

            //project it to sphere surface
            if (distSq < radiusSq)
            {
                // the point is inside ball, (on hemisphere surface) then z(x,y) : z^2 = r^2 - x^2 - y^2
                p.z = (float)System.Math.Sqrt(radiusSq - distSq);
            }
            else
            {
                // the point is outside ball (on XY plane), is sufficent scale the vector
                p.z = 0;
                float mag = (float)System.Math.Sqrt(radiusSq);
                p.x /= mag;
                p.y /= mag;
            }

            p.Normalize();
            return p;
        }


        protected Matrix4x4f RotationFromVectors(Vector3f p0, Vector3f p1)
        {
            float dot = Vector3f.Dot(p0, p1);
            float angle = (float)System.Math.Acos(Maths.Mathelp.MIN(1, dot)) * 0.5f;

            Vector3f axe = Vector3f.Cross(p1, p0);
            axe.Normalize();

            Console.WriteLine(string.Format("p0 {0}\np1 {1}", p0.ToStringRounded(), p1.ToStringRounded()));
            Console.WriteLine("Ax " + axe.ToStringRounded());
            Console.WriteLine("ang " + angle);

            return Matrix4x4f.RotateAxis(in axe, angle);

        }

        public void Paint(System.Drawing.Graphics g)
        {
            int arcradius = (int)GetMaxArcBallRadius();
            int centerx = viewport.Width / 2;
            int centery = viewport.Height / 2;
            g.DrawEllipse(System.Drawing.Pens.Yellow, centerx - arcradius, centery - arcradius, arcradius * 2, arcradius * 2);
        }

    }

}
