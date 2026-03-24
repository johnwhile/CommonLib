using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using Common.Maths;

namespace Common
{
    public struct CameraStruct
    {
        // Transforms homogeneous coordinates from world space to view space.
        public Matrix4x4f m_view;
        public Matrix4x4f m_view_inv;
        // The projection matrix transforms view space to clip space
        public Matrix4x4f m_proj;
        public Matrix4x4f m_proj_inv;


        public float far;
        public float near;
        public float aspect;
        public Vector3f eye;
        public Vector3f target;

        public Matrix4x4f ProjView => m_proj * m_view;
        
        // Remember that (AB)−1=B−1*A−1,
        public Matrix4x4f ProjViewInverse => m_view_inv * m_proj_inv;



        public CameraStruct (Vector3f eye, Vector3f target, float near, float far, float aspect) : this()
        {
            this.eye = eye;
            this.target = target;
            this.near = near;
            this.far = far;
            this.aspect = aspect;
            UpdateView();
            UpdateProj();
        }
        public void UpdateView()
        {
            m_view = Matrix4x4f.MakeViewLH(eye, target, Vector3f.UnitY);
            m_view_inv = Matrix4x4f.InvertAffineTransform(m_view);
        }
        public void UpdateProj()
        {
            m_proj = Matrix4x4f.MakeProjectionLHAFovY(near, far, aspect);
            m_proj_inv = m_proj.Inverse();
        }

        /// <summary>
        /// Tested: ok
        /// </summary>
        public Ray GetRayFromClipSpace(float clipx, float clipy) => new Ray(eye, clipx, clipy, ProjViewInverse);

        /// <summary>
        /// Get the world point from clip space's point. clipz = 1 for far plane, clipz = 0 for near plane.
        /// </summary>
        public Vector3f ClipToWorldSpace(Vector3f clipPoint) 
            => clipPoint.TransformCoordinate(ProjViewInverse);

        /// <summary>
        /// Get the clipx and clipy value from screen point (clipz = 0)
        /// </summary>
        public Vector3f ScreenToClipSpace(float screen_x, float screen_y, float screen_width, float screen_height)
            => new Vector3f(2 * screen_x / screen_width - 1, 1 - 2 * screen_y / screen_height, 0);

    }




    public class Camera
    {
        public CameraStruct camera;

        public Camera(Vector3f eye, Vector3f target, float near, float far, float aspect)
        {
            camera = new CameraStruct()
            {
                eye = eye,
                target = target,
                near = near,
                far = far,
                aspect = aspect
            };

            Update();
        }

        public void Update()
        {
            camera.UpdateView();
            camera.UpdateProj();
        }
    }



    public class TrackBallCamera : Camera
    {
        ViewportClip viewport;
        Vector3f traslation;


        [Flags]
        enum MouseMoving : byte
        {
            None = 0,
            Translating = 1,
            Rotating = 2,
            Zooming = 4
        }

        Vector2i mousedown;
        Vector2i prevmouse;

        MouseMoving moving;

        public TrackBallCamera(Vector3f eye, Vector3f target, float near, float far, float aspect) :
            base (eye, target, near, far, aspect)
        {
            
        }

        public ViewportClip Viewport
        {
            set => viewport = value;
        }


        public void MouseDown(Vector2f position, MouseButtons button)
        {
            mousedown = position;
            prevmouse = mousedown;

            if (button.HasFlag(MouseButtons.Left)) moving |= MouseMoving.Translating;
            if (button.HasFlag(MouseButtons.Right)) moving |= MouseMoving.Rotating;


            var clip = Mathelp.ScreenToClipSpace(position.x, position.y, 0, 0, viewport.Width, viewport.Height);


            var ray = camera.GetRayFromClipSpace(clip.x, clip.y);
            Console.WriteLine("ray : " + ray);

            var eyespace = new Vector3f(clip.x, clip.y, -1);
            eyespace = eyespace.TransformCoordinate(in camera.m_proj_inv);
            eyespace.z = -1;
            var worldspace = eyespace.TransformNormal(in camera.m_view_inv);
            worldspace.Normalize();

            Console.WriteLine("dir2 : " + worldspace);
        }

        public void MouseUp(Vector2f position, MouseButtons button)
        {
            if (button.HasFlag(MouseButtons.Left)) moving &= ~MouseMoving.Translating;
            if (button.HasFlag(MouseButtons.Right)) moving &= ~MouseMoving.Rotating;
        }

        public void MouseMove(Vector2f position, MouseButtons button)
        {
            if (moving.HasFlag(MouseMoving.Translating))
            {
                //var p0_a = Vector3f.TransformCoordinate(p0.x, p0.y, targetDepth, inverse);
                //var p1_a = Vector3f.TransformCoordinate(p1.x, p1.y, targetDepth, inverse);

                //p0_a = p0_a - p1_a;

                //Eye += move;
                //Target += move;

                //view = view * Matrix4x4f.Translating(-p0_a);

                prevmouse = position;
            }
        }
    }
}
