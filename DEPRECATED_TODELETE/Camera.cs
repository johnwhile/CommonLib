using Common.Maths;
using System;

namespace Common.Tools
{
    /// <summary>
    /// interface to access to main camera's values, Viewport is independent from camera value
    /// in fact in the view matrix there is only aspect ratio.
    /// </summary>
    public interface ICamera
    {
        Vector3f Eye { get; }
        Matrix4x4f View { get; }
        Matrix4x4f CameraView { get; }
        Matrix4x4f Projection { get; }
        float Far { get; }
        float Near { get; }
        float Aspect { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Remember that if Camera = View^-1, when apply a camera transformation
    /// V' = (C*T)^-1 = T^-1 * C^-1 = T^-1 * V
    /// T^-1 can be simplified if contain only rotation, only traslation, only scale
    /// 
    /// </remarks>
    public class Camera_old : ICamera
    {
        protected Frustum frustum;
        protected Matrix4x4f cameraview;  // camera transform (inverse of view)
        protected Matrix4x4f view;        // inverse of camera
        protected Matrix4x4f projection;  // camera projection
        protected bool frustumNeedUpdate = false;


        public Camera_old()
        {
            projection = view = cameraview = Matrix4x4f.Identity;
            this.frustum = new Frustum(projection * view);
            frustumNeedUpdate = true;
        }

        public Camera_old(Matrix4x4f projection, Matrix4x4f view)
        {
            this.projection = projection;
            this.view = view;
            this.cameraview = view.Inverse();
            this.frustum = new Frustum(projection * view);
            frustumNeedUpdate = false;
        }

        public Frustum Frustum
        {
            get
            {
                if (frustumNeedUpdate)
                {
                    Matrix4x4f projview = projection;
                    projview.Multiply(in view);
                    frustum.MakeFrustum(in projview);
                }
                return frustum;
            }
        }

        #region View

        public Vector3f Eye
        {
            get { return CameraView.Position; }
        }

        public virtual Matrix4x4f View
        {
            get { return view; }
            set { view = value; cameraview = view.Inverse(); frustumNeedUpdate = true; }
        }

        public virtual Matrix4x4f CameraView
        {
            get { return cameraview; }
            set { cameraview = value; view = cameraview.Inverse(); frustumNeedUpdate = true; }
        }


        /// <summary>
        /// The UnitX vector if view matrix is Identity
        /// </summary>
        public Vector3f Left
        {
            get { return new Vector3f(view.m00, view.m01, view.m02); }
        }
        /// <summary>
        /// The UnitY vector if view matrix is Identity
        /// </summary>
        public Vector3f Up
        {
            get { return new Vector3f(view.m10, view.m11, view.m12); }
        }
        /// <summary>
        /// The UnitZ vector if view matrix is Identity
        /// </summary>
        public Vector3f Forward
        {
            get { return new Vector3f(view.m20, view.m21, view.m22); }
        }

        #endregion

        #region Projection

        public Matrix4x4f Projection
        {
            get { return projection; }
            set { projection = value; frustumNeedUpdate = true; }
        }
        /// <summary>
        /// ORTHOGONAL version : Update the camera Width where Width is in world coordinate
        /// </summary>
        public float OrthoWidth
        {
            get { return CameraHelp.GetWidth(in projection); }
            set { CameraHelp.SetWidth(ref projection, value); }
        }
        /// <summary>
        /// ORTHOGONAL version : Update the camera Height where Height is in world coordinate
        /// </summary>
        public float OrthoHeight
        {
            get { return CameraHelp.GetHeight(in projection); }
            set { CameraHelp.SetHeight(ref projection, value); }
        }

        /// <summary>
        /// PROJECTION version : Update projection with minimum calculations, FovX will be derived from aspectratio
        /// </summary>
        public float Aspect
        {
            get { return CameraHelp.GetAspectRatio(in projection); }
            set { CameraHelp.SetAspectRatio(ref projection, value); }
        }

        /// <summary>
        /// PROJECTION version : Update projection with minimum calculations, FovX will be derived from aspectratio
        /// </summary>
        public float FovY
        {
            get { return CameraHelp.GetFovY(in projection); }
            set { CameraHelp.SetFovY(ref projection, value); }
        }

        /// <summary>
        /// Update projection with minimum calculations
        /// </summary>
        public float Near
        {
            get { return CameraHelp.GetNear(in projection); }
            set { CameraHelp.SetNear(ref projection, value); }
        }

        public float Far
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }

}
