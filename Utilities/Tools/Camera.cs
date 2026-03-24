using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Tools
{
    /// <summary>
    /// The struct what contain all camera values necessary to update it
    /// </summary>
    public struct CameraStruct
    {
        #region View Components
        public enum eViewComponent { Right, Up, Look, Eye }

        public Vector3f up, eye, targhet;
        public Matrix4x4f view, iview;

        public Vector3f ViewRight
        {
            get { return new Vector3f(view.m00, view.m01, view.m02); }
        }
        public Vector3f ViewUp
        {
            get { return new Vector3f(view.m10, view.m11, view.m12); }
        }
        /// <summary>
        /// the direction from LooAt --> Eye, carefull
        /// </summary>
        public Vector3f ViewLook
        {
            get { return new Vector3f(view.m20, view.m21, view.m22); }
        }
        public Vector3f ViewEye
        {
            get { return new Vector3f(iview.m03, iview.m13, iview.m23); }
        }
        public Matrix4x4f CameraView
        {
            get { return iview; }
        }

        public static Vector3f GetViewComponent(Matrix4x4f View, eViewComponent component)
        {
            switch (component)
            {
                case eViewComponent.Right: return new Vector3f(View.m00, View.m01, View.m02);
                case eViewComponent.Up: return new Vector3f(View.m10, View.m11, View.m12);
                case eViewComponent.Look: return new Vector3f(View.m20, View.m21, View.m22);
                default: return Vector3f.Zero;
            }
        }


        public void UpdateView()
        {
            //Console.WriteLine("update <{0}> <{1}> <{2}>", eye.ToString(), targhet.ToString(), up.ToString());
            view = Matrix4x4f.MakeViewLH(eye, targhet, up);
            iview = view.Inverse();
            //Console.WriteLine("result <{0}> <{1}> <{2}>", view_look.ToString(), view_right.ToString(), view_up.ToString());
            up = ViewUp;
        }

        #endregion

        #region Project Components
        public enum ProjectionType
        {
            Orthogonal,
            Prospective
        }
        public ViewportClip viewport;
        public Matrix4x4f proj;
        public ProjectionType projType;
        public float fovy, near, far;

        public void UpdateProj()
        {
            switch (projType)
            {
                case ProjectionType.Orthogonal:
                    proj = Matrix4x4f.MakeOrthoLH(viewport.Width, viewport.Height, near, far);
                    break;
                case ProjectionType.Prospective:
                    proj = Matrix4x4f.MakeProjectionLHAFovY(near, far, viewport.Aspect, fovy);
                    break;
                default: throw new NotImplementedException();
            }
        }

        public static CameraStruct Default
        {
            get
            {
                CameraStruct cam = new CameraStruct();
                cam.up = Vector3f.UnitY;
                cam.eye = new Vector3f(10, 10, 10);
                cam.targhet = Vector3f.Zero;
                cam.viewport = new ViewportClip(800, 640);
                cam.projType = ProjectionType.Prospective;
                cam.fovy = Maths.Mathelp.Rad45;
                cam.near = 0.1f;
                cam.far = 1000.0f;
                cam.UpdateView();
                cam.UpdateProj();
                return cam;
            }
        }
        #endregion
    }

    /// <summary>
    /// A class to manage the control events
    /// </summary>
    public abstract class CameraController
    {
        protected Control panel;
        protected Mousing mousing;
        protected ChangeMethod moveaffect;
        protected Point mouseStart;

        public CameraController(Control owner)
        {
            Enabled = true;
            panel = owner;
            panel.MouseDown += new MouseEventHandler(OnMouseDown);
            panel.MouseMove += new MouseEventHandler(OnMouseMove);
            panel.MouseUp += new MouseEventHandler(OnMouseUp);
            panel.MouseWheel += new MouseEventHandler(OnMouseWheel);
            panel.Resize += new EventHandler(OnResize);
            panel.Paint += new PaintEventHandler(OnPaint);

            mousing = Mousing.None;
            moveaffect = ChangeMethod.Test;
            mouseStart = Point.Empty;

            owner.Focus();
        }

        ~CameraController()
        {
            if (panel != null)
            {
                panel.MouseDown -= OnMouseDown;
                panel.MouseMove -= OnMouseMove;
                panel.MouseUp -= OnMouseUp;
                panel.MouseWheel -= OnMouseWheel;
                panel.Resize -= OnResize;
            }
        }
        /// <summary>
        /// Enable or disable the mouse events, the OnResize work always to ensure a correct projection matrix
        /// </summary>
        public bool Enabled { get; set; }

        protected abstract void OnResize(object sender, EventArgs e);
        protected abstract void OnMouseDown(object sender, MouseEventArgs e);
        protected abstract void OnMouseMove(object sender, MouseEventArgs e);
        protected abstract void OnMouseUp(object sender, MouseEventArgs e);
        protected abstract void OnMouseWheel(object sender, MouseEventArgs e);
        protected abstract void OnPaint(object sender, PaintEventArgs e);


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
        protected enum ChangeMethod
        {
            // add constantly for each MouseMove call a movement.
            // Issue : the metric error are added each time so if mouse go in the previous position the value can be different
            Incremental,

            // store last postion or rotation, for each MouseMove calculate the rotation.
            // Pro : avoid metric error, the same previous mouse position return the same rotation
            PreservePrev,

            Test,
        }
    }

    /// <summary>
    /// My new implementation using arcball math, but i found a lot of difficult to adapt its
    /// </summary>
    public class TrackBallCamera_new : CameraController, ICamera
    {
        CameraStruct camera = CameraStruct.Default;

        Matrix4x4f rotation = Matrix4x4f.Identity;
        Matrix4x4f traslation = Matrix4x4f.Identity;
        Point start, end;
        float zoom;

        Matrix4x4f arcballWorld;

        public TrackBallCamera_new(Control panel, Vector3f CameraEye, Vector3f CameraTarghet, Vector3f CameraUp, float near, float far)
            : base(panel)
        {
            //update with new values
            camera.viewport = new ViewportClip(panel.ClientSize);
            camera.near = near;
            camera.far = far;
            camera.UpdateProj();

            this.zoom = Vector3f.Distance(CameraEye, CameraTarghet);

            camera.view = Matrix4x4f.MakeViewLH(CameraEye, CameraTarghet, CameraUp);

            // TODO : understand this corrrection because this it the only matrix that work correctly with rotation.
            // my idea to fix orientation is assign the defautl view matrix and rotate it to match with input data
            arcballWorld = Matrix4x4f.Identity;

            arcballWorld.m00 = camera.view.m00;
            arcballWorld.m01 = camera.view.m01;
            arcballWorld.m02 = camera.view.m02;

            arcballWorld.m10 = camera.view.m10;
            arcballWorld.m11 = camera.view.m11;
            arcballWorld.m12 = camera.view.m12;

            arcballWorld.m20 = camera.view.m20;
            arcballWorld.m21 = camera.view.m21;
            arcballWorld.m22 = camera.view.m22;

            // to match with virtual 2D ball of screen
            camera.eye = new Vector3f(0, 0, -zoom);
            camera.targhet = new Vector3f(0, 0, 0);
            camera.up = new Vector3f(0, 1, 0);
            camera.UpdateView();

            rotation = arcballWorld;
            traslation = Matrix4x4f.Translating( -CameraTarghet);
        }

        int min(int a, int b) { return a < b ? a : b; }


        int Width
        {
            get { return panel != null ? panel.ClientSize.Width : 0; }
        }
        int Height
        {
            get { return panel != null ? panel.ClientSize.Height : 0; }
        }

        public int ArcBallRadius
        {
            get { return (int)(min(Width, Height) / 2 * 0.8f); }
        }

        public Matrix4x4f View
        {
            get { return camera.view * rotation * traslation; }
        }
        public Matrix4x4f CameraView
        {
            get { return camera.iview * rotation * traslation; }
        }
        public Vector3f Eye
        {
            get { return camera.eye.TransformCoordinate(Matrix4x4f.Inverse(rotation * traslation)); }
        }
        public Vector3f Targhet
        {
            get { return camera.targhet.TransformCoordinate(Matrix4x4f.Inverse(rotation * traslation)); }
        }
        public Matrix4x4f Projection
        {
            get { return camera.proj; }
        }
        public ViewportClip Viewport
        {
            get { return camera.viewport; }
            set { camera.viewport = value; camera.UpdateProj(); }
        }
        public float Far
        {
            get { return camera.far; }
        }
        public float Near
        {
            get { return camera.near; }
        }
        public float Aspect
        {
            get { return camera.viewport.Aspect; }
        }
        protected override void OnPaint(object sender, PaintEventArgs e)
        {
            if (!Enabled) return;

            int centerx = Width / 2;
            int centery =Height / 2;
            int arcradius = ArcBallRadius;

            //e.Graphics.DrawEllipse(Pens.Yellow, centerx - arcradius, centery - arcradius, arcradius * 2, arcradius * 2);
        }
        protected override void OnResize(object sender, EventArgs e)
        {
            camera.viewport = new ViewportClip(panel.ClientSize);
            camera.UpdateProj();
        }

        protected override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            mousing = Mousing.None;
            if (e.Button == MouseButtons.Left)
            {
                mousing = Mousing.Traslation;
            }
            else if (e.Button == MouseButtons.Right)
            {
                mousing = Mousing.Rotating;
            }
            else
            {
                mousing = Mousing.None;
            }
            start = e.Location;
            // else wheel don't work
            panel.Focus();
        }
        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            if (mousing != Mousing.None)
            {
                end = e.Location;

                if (mousing == Mousing.Traslation)
                {
                    int dx = end.X - start.X;
                    int dy = start.Y - end.Y;

                    Matrix4x4f CurrView = View;
                    Vector3f vRight = new Vector3f(View.m00, View.m01, View.m02);
                    Vector3f vUp = new Vector3f(View.m10, View.m11, View.m12);
                    Vector3f move = (dx * vRight + dy * vUp) * (0.001f * zoom);

                    traslation = Matrix4x4f.Translating(move) * traslation;
                }
                else if (mousing == Mousing.Rotating)
                {
                    Vector3f p0 = arcballPoint(start);
                    Vector3f p1 = arcballPoint(end);

                    if ((p1 - p0).Length > 0.001f)
                    {
                        //Quaternion q0 = new Quaternion(p0.X, p0.Y, p0.Z, 0);
                        //Quaternion q1 = new Quaternion(p1.X, p1.Y, p1.Z, 0);
                        //rotation = Matrix.RotationQuaternion(q0 * q1) * rotation;
                        rotation = rotFromVectors(p0, p1) * rotation;
                    }
                }
                start = end;
            }
        }
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            mousing = Mousing.None;
        }
        protected override void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            mousing = Mousing.Zooming;

            if (e.Delta > 0) zoom *= 0.9f;
            else zoom *= 1.1f;

            camera.eye = new Vector3f(0, 0, -zoom);
            camera.UpdateView();

            mousing = Mousing.None;
        }

        /// <summary>
        /// Convert a screen point to the screen virtual hemisphere
        /// </summary>
        Vector3f arcballPoint(Point P)
        {
            Vector3f p = new Vector3f();

            // adjust to center screen 
            p.x = P.X - Width / 2;
            p.y = Height / 2 - P.Y;

            // distance from radius
            float radiusSq = ArcBallRadius * ArcBallRadius;
            float distSq = p.x * p.x + p.y * p.y;

            //project it to sphere surface
            if (distSq > radiusSq)
            {
                // the point is outside ball (on XY plane), is sufficent scale the vector
                p.z = 0;
            }
            else
            {
                // the point is inside ball, (on hemisphere surface) remember that z(x,y) : z^2 = r^2 - x^2 - y^2
                p.z = (float)System.Math.Sqrt(radiusSq - distSq);
            }

            // unknow operation
            //p.x *= -1;
            //p.y *= -1;
            p.z *= -1;

            p.Normalize();


            return p;
        }


        Matrix4x4f rotFromVectors(Vector3f from, Vector3f to)
        {
            float dot = Vector3f.Dot(from, to);
            Vector3f axe = Vector3f.Cross(from, to);
            return (Matrix4x4f)(new Quaternion4f(axe.x, axe.y, axe.z, dot));
            //return Matrix.RotationQuaternion(new Quaternion(axe.X, axe.Y, axe.Z, dot));
        }
    }
}
