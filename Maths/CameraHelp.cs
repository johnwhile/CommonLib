using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common
{
    /// <summary>
    /// Some math with projection matrix
    /// </summary>
    public static class CameraHelp
    {
        #region PROSPECTIVE
        public static float GetAspectRatio(in Matrix4x4f prospective)
        {
            return prospective.m11 / prospective.m00;
        }
        public static void SetAspectRatio(ref Matrix4x4f prospective, float aspect)
        {
            prospective.m00 = prospective.m11 / aspect;
        }
        public static float GetFovY(in Matrix4x4f prospective)
        {
            return (float)(Math.Atan(1.0 / prospective.m11) * 2.0);
        }
        public static void SetFovY(ref Matrix4x4f prospective, float fovy)
        {
            float m11 = (float)(1.0 / Math.Tan(fovy / 2.0));
            prospective.m00 = m11 / prospective.m11 * prospective.m00;
            prospective.m11 = m11;
        }

        public static float GetNear(in Matrix4x4f prospective)
        {
            return -prospective.m23 / prospective.m22;
        }
        public static void SetNear(ref Matrix4x4f prospective, float near)
        {
            float far = prospective.m23 / (1 - prospective.m22);
            prospective.m22 = far / (far - near);
            prospective.m23 = -near * prospective.m22;
        }
        #endregion

        #region ORTHOGONAL
        public static float GetWidth(in Matrix4x4f ortho)
        {
            return 2.0f / ortho.m00;
        }
        public static void SetWidth(ref Matrix4x4f ortho, float width)
        {
            float l = -width * (ortho.m03 + 1) / 2.0f;
            float r = width + l;
            ortho.m00 = 2.0f / (r - l);
            ortho.m03 = (l + r) / (l - r);
        }
        public static float GetHeight(in Matrix4x4f ortho)
        {
            return 2.0f / ortho.m11;
        }
        public static void SetHeight(ref Matrix4x4f ortho, float height)
        {
            float b = -height * (ortho.m13 + 1) / 2.0f;
            float t = height + b;
            ortho.m11 = 2.0f / (t - b);
            ortho.m13 = (t + b) / (b - t);
        }
        #endregion


    }
}
