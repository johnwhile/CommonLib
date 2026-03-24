
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Common.Maths.Geometries2D
{

    public class Polygon2d : Manifold
    {
        Pen pen;
        StructBuffer<Vector2f> verts;

        public int VertsCount => verts.Count;


        public Polygon2d(int vertCapacity = 0, int triCapacity = 0) : base(vertCapacity, triCapacity)
        {
            verts = new StructBuffer<Vector2f>(vertCapacity);
            pen = new Pen(Brushes.LightBlue, 4);
        }

        public void AddVertices(IEnumerable<Vector2f> vertices)
        {
            int index = verts.Count;
            foreach (Vector2f v in vertices)
                AddVertex(v);
        }
        public void AddVertex(Vector2f vertex)
        {
            verts.Add(vertex);
        }
        public Vector2f GetVertices(int v) => verts[v];


        Vector2f getbaricentric(Triangle tri)
        {
            return (verts[tri.v0] + verts[tri.v1] + verts[tri.v2]) / 3.0f;
        }
        Vector2f getbaricentric(Edge edge)
        {
            return (verts[edge.v0] + verts[edge.v1]) / 2.0f;
        }



        public void Draw(Graphics g)
        {
            int i = 0;


            pen.Color = Color.DarkGreen;
            pen.Width = 1;

            for (i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                g.DrawLine(pen, verts[t.v0], verts[t.v1]);
                g.DrawLine(pen, verts[t.v1], verts[t.v2]);
                g.DrawLine(pen, verts[t.v2], verts[t.v0]);

                g.DrawString($"t{i}", SystemFonts.DefaultFont, Brushes.Red, getbaricentric(t));
            }

            for (i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                g.DrawString($"e{i}", SystemFonts.DefaultFont, Brushes.DarkGreen, getbaricentric(e));
            }

            i = 0;
            foreach (Vector2f v in verts)
            {
                g.FillRectangle(Brushes.Blue, v.x - 2, v.y - 2, 4, 4);
                g.DrawString((i++).ToString(), SystemFont.Arial, Brushes.Blue, v.x - 3, v.y - 3);
            }
        }


        public void DrawByTriangle(Graphics g)
        {
            int i = 0;
            foreach (Vector2f v in verts)
            {
                g.FillRectangle(Brushes.Black, v.x - 2, v.y - 2, 4, 4);
                g.DrawString((i++).ToString(), SystemFont.Arial, Brushes.Black, v.x - 3, v.y - 3);
            }

            for (i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                g.DrawLine(pen, verts[t.v0], verts[t.v1]);
                g.DrawLine(pen, verts[t.v1], verts[t.v2]);
                g.DrawLine(pen, verts[t.v2], verts[t.v0]);

                g.DrawString($"t{i}", SystemFonts.DefaultFont, Brushes.Red, getbaricentric(t));
            }

        }
        public void DrawByEdge(Graphics g)
        {
            int i = 0;
            foreach (Vector2f v in verts)
            {
                g.FillRectangle(Brushes.Black, v.x - 2, v.y - 2, 4, 4);
                g.DrawString((i++).ToString(), SystemFont.Arial, Brushes.Black, v.x - 3, v.y - 3);
            }
            pen.Color = Color.Green;
            pen.Width = 1;

            for (i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                g.DrawLine(pen, verts[e.v0], verts[e.v1]);
                g.DrawString($"e{i}", SystemFonts.DefaultFont, Brushes.DarkGreen, getbaricentric(e));

            }

        }
    }
}
