using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Maths
{
    public partial class SubMesh
    {
        public void AddQuad(int i, int j, int k, int l)
        {
            //Debug.Print($"{i} {j} {k} {l}");
            AddPrimitive(i, j, k);
            AddPrimitive(i, k, l);
        }
        public void AddPrimitive(int i, int j = -1, int k = -1)
        {
            //Debug.Print($"{i} {j} {k}");
            switch (mesh.Topology)
            {
                case Primitive.TriangleList:
                    Indices.Add(i);
                    Indices.Add(j);
                    Indices.Add(k);
                    break;
                case Primitive.LineList:
                    Indices.Add(i);
                    Indices.Add(j);
                    break;
                case Primitive.Point:
                    Indices.Add(i);
                    break;
            }
        }

        public void AddOffset(int offset)
        {
            for (int i = 0; i < Indices.Count; i++) Indices[i] += offset;
        }

        public void InvertTranglesOrder()
        {
            for(int t=0;t<IndincesCount/3;t++)
            {
                int j = Indices[t * 3 + 1];
                Indices[t * 3 + 1] = Indices[t * 3 + 2];
                Indices[t * 3 + 2] = j;
            }
        }


    }
}
