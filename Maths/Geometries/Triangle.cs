using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Maths
{
    /// <summary>
    /// Triangle, in clock wire order of course
    /// </summary>
    public struct Triangle
    {
        internal Vector3f p0, p1, p2;

        public Vector3f Normal => Vector3f.Cross(p1 - p0, p2 - p0).Normal;
        public Vector3f Center { get { return (p0 + p1 + p2) / 3.0f; } }
        public Vector3f P0 { get { return p0; } set { p0 = value; } }
        public Vector3f P1 { get { return p1; } set { p1 = value; } }
        public Vector3f P2 { get { return p2; } set { p2 = value; } }

        public Triangle(Vector3f p0, Vector3f p1, Vector3f p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
