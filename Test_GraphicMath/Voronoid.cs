

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Common;
using Common.Maths;
using Common.Maths.Geometries2D;

namespace Test_GraphicMath
{
    public class Voronoi
    {
        List<Vector2f> vertices;
        List<Vector3us> triangles;

        Rectangle4i bound;
        Polygon2d poly;

        public Voronoi()
        {


        }



        int compare(Vector2f a, Vector2f b)
        {
            if (a.y < b.y) return -1;
            if (a.y > b.y) return 1;
            if (a.x < b.x) return -1;
            if (a.x > b.x) return 1;
            return 0;
        }


        public void Generate(List<Vector2i> Points)
        {

            poly = new Polygon2d();

            vertices = new List<Vector2f>(Points.Count);
            foreach (var p in Points) vertices.Add(p);

            triangles = new List<Vector3us>();


            bound = Rectangle4f.MakeBoundFromPoints(vertices);
            vertices.Sort((Vector2f a, Vector2f b) => compare(a, b));


            int nv = vertices.Count;

            if (vertices.Count < 3) return;

            //create super-triangle
            Vector2f mid = new Vector2f(
                bound.x + bound.width * 0.5f,
                bound.y + bound.height * 0.5f);

            int max = Mathelp.MAX(bound.width, bound.height);

            vertices.Add(new Vector2f(mid.x - 2 * max, mid.y - max));
            vertices.Add(new Vector2f(mid.x, mid.y + 2 * max));
            vertices.Add(new Vector2f(mid.x + 2 * max, mid.y - max));
            triangles.Add(new Vector3us(nv, nv + 1, nv + 2));

            poly.AddVertices(vertices);
            poly.AddTriangle(nv, nv + 1, nv + 2);


            for (int i=0;i< nv; i++) 
            {
                for (int t = 0; t < poly.TrianglesCount; t++)
                {
                    int a, b, c;
                    poly.GetVerticesByTriangle(t, out a, out b, out c);


                    if (Utilities2d.IsPointInsideTriangle(poly.GetVertices(i), poly.GetVertices(a), poly.GetVertices(b), poly.GetVertices(c)))
                    //if (InCircle( vertices[i], vertices[a], vertices[b], vertices[c]))
                    {
                        poly.RemoveTriangle(t);


                        var t0 = new Vector3us(i, a, b);
                        var t1 = new Vector3us(i, b, c);
                        var t2 = new Vector3us(i, c, a);
                        triangles.RemoveAt(t);

                        triangles.Add(t0);
                        triangles.Add(t1);
                        triangles.Add(t2);
                        break;
                    }
                }

            }
        }



        static bool InCircle(Vector2i p, Vector2i p0, Vector2i p1, Vector2i p2) => Circle.FromPoints(p0, p1, p2).Contains(p);


        public void Draw(Graphics g)
        {
            poly.DrawByTriangle(g);
            poly.DrawByEdge(g);
            return;

            #region
            #endregion
            if (vertices != null && vertices.Count > 0)
            {
                //add some padding
                Rectangle4i border = bound;
                //border.position -= 20;
                //border.size += 40;
                //g.DrawRectangle(Pens.DarkBlue, border);

                int i = 0;
                foreach (Vector2i p in vertices)
                {
                    g.FillRectangle(Brushes.Black, p.x - 2, p.y - 2, 4, 4);
                    g.DrawString((i++).ToString(), SystemFonts.DefaultFont, Brushes.Black, p.x - 3, p.y - 3);
                }

                foreach (Vector3us t in triangles)
                {
                    g.DrawLine(Pens.Blue, vertices[t.x], vertices[t.y]);
                    g.DrawLine(Pens.Blue, vertices[t.y], vertices[t.z]);
                    g.DrawLine(Pens.Blue, vertices[t.z], vertices[t.x]);

                    //var circle = Circle.FromPoints(vertices[t.x], vertices[t.y], vertices[t.z]);
                    //g.DrawEllipse(Pens.Green, circle.center.x - circle.radius, circle.center.y - circle.radius, circle.radius * 2, circle.radius * 2);
                }
            }
        }
    }

    public partial class VoronoidForm : Form
    {
        Polygon2d tripolygon;
        
        Voronoi voronoi;
        List<Vector2i> Points = new List<Vector2i>();

        public void Add(ref Vector3f v, int i)
        {
            v.x += i;
            v.y += i;
            v.z += i;
        }


        public VoronoidForm()
        {
            InitializeComponent();


            TriMesh tri = new TriMesh(Primitive.TriangleList, "mesh");
            var sub = tri.AddSubMesh(0, IndexFormat.Index32bit, "sub");
            sub.AddPrimitive(0, 1, 2);
            sub.AddPrimitive(3, 4, 5);
            sub.AddPrimitive(6, 7, 8);

            tri.Vertices = new StructBuffer<Vector3f>
            {
                new Vector3f(0, 1, 2),
                new Vector3f(3, 4, 5),
                new Vector3f(6, 7, 8)
            };

            ref Vector4f v = ref tri.Vertices.GetGenericByRef<Vector4f>(0);
            v.x = 9;
            v.y = 9;
            v.z = 9;
            v.w = 9;

            tri.Normals = new StructBuffer<Vector3f>
            {
                new Vector3f(0, 1, 2),
                new Vector3f(3, 4, 5),
                new Vector3f(6, 7, 8)
            };

            tri.TexCoords = new StructBuffer<Vector2f>
            {
                new Vector2f(0, 1),
                new Vector2f(2, 3)
            };

            using (FileStream file = new FileStream("C:\\Users\\johnw\\Desktop\\test.mesh", FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(file))
                tri.Write(writer, 0, 0, 0,0, 0, 0);


            TriMesh tri2 = new TriMesh();
            using (FileStream file = new FileStream("C:\\Users\\johnw\\Desktop\\test.mesh", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(file))
                tri2.Read(reader);



            return;
            Points = new List<Vector2i>()
            {
                new Vector2i(1, 3) * 100,
                new Vector2i(2, 2) * 100,
                new Vector2i(3, 4) * 100,
                new Vector2i(2, 1) * 100,
            };
            Points.Clear();
            Points = new List<Vector2i>()
            {
                new Vector2i(153, 157),
                new Vector2i(211, 127),
                new Vector2i(216, 180),
            };

            CreatePolygon();

            //voronoi = new Voronoi();
            //voronoi.Generate(Points);
        }



       void CreatePolygon()
        {
            tripolygon = new Polygon2d();


            tripolygon.AddVertex(new Vector2f(100, 100));
            tripolygon.AddVertex(new Vector2f(400, 100));
            tripolygon.AddVertex(new Vector2f(100, 400));
            tripolygon.AddVertex(new Vector2f(400, 400));
            tripolygon.AddVertex(new Vector2f(250, 250));


            tripolygon.AddTriangle(0, 1, 4);
            tripolygon.AddTriangle(1, 3, 4);
            tripolygon.AddTriangle(4, 3, 2);
            tripolygon.AddTriangle(4, 2, 0);

            tripolygon.RemoveEdge(7);
            tripolygon.CheckHashMap();

            //tripolygon.AddTriangle(0, 1, 4);

            //tripolygon.RemoveEdge(0);
        }



        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Points.Add(e.Location);
                Debugg.Success(e.Location.ToString());
                voronoi.Generate(Points);
            }
            else
            {
                Points.Clear();
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            voronoi?.Draw(e.Graphics);
            tripolygon?.Draw(e.Graphics);
        }
    }
}
