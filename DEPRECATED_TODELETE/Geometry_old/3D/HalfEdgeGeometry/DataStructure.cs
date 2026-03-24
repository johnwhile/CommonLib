using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Geometry.HalfEdge
{
    public class HVertex
    {
        public Vector3f v;
        public int idx;
        public HEdge he;

        public HVertex(Vector3f point, int idx)
        {
            this.v = point;
            this.idx = idx;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("v {0}", idx);
        }
    }

    public class HFace
    {
        public HEdge he;

        public HFace()
        {
            HEdge he0 = new HEdge();
            HEdge he1 = new HEdge();
            HEdge he2 = new HEdge();

            he0.next = he1;
            he1.next = he2;
            he2.next = he0;

            he0.face = he1.face = he2.face = this;

            he0.next = he1;
            he1.next = he2;
            he2.next = he0;

            he = he0;
        }

        public bool Contain(HVertex vertex)
        {
            HEdge e = he;
            do
            {
                if (e.head == vertex) return true;
                e = e.next;
            }
            while (e != he);

            return false;
        }


        public override string ToString()
        {
            return string.Format("f {0} {1} {2}", he != null ? he.head.idx.ToString() : "-", he.next != null ? he.next.head.idx.ToString() : "-", he.next.next != null ? he.next.next.head.idx.ToString() : "-");
        }

    }

    public class HEdge
    {
        public HVertex head;
        public HFace face;
        public HEdge next;
        public HEdge opposite;
        
        // two half edge do one edge, the flag define the halfedge used as edge, the opposite is false
        public bool isEdge = true;


        public HEdge()
        {

        }

        public override string ToString()
        {
            return string.Format("e {0} {1}", head != null ? head.idx.ToString() : "-", next.head != null ? next.head.idx.ToString() : "-");
        }

        /// <summary>
        /// Generate a same hash code for two opposite halfedge
        /// </summary>
        public int GetEdgeHashCode()
        {
            int hash0 = head.GetHashCode();
            int hash1 = next.head.GetHashCode();
            return hash0 > hash1 ? hash0 + hash1 * ushort.MaxValue : hash1 + hash0 * ushort.MaxValue;
        }

    }

}
