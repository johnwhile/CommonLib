using System;

namespace Common.Maths
{
    public static class TriMeshPrimitive
    {
        /// <summary>
        /// Spline for axis x,y,z
        /// </summary>
        /// <returns></returns>
        public static TriMesh AxisLine()
        {
            TriMesh obj = new TriMesh(Primitive.LineList, "Axis");
            obj.Vertices = new StructBuffer<Vector3f>()
            {
                Vector3f.Zero,
                Vector3f.UnitX,
                Vector3f.Zero,
                Vector3f.UnitY,
                Vector3f.Zero,
                Vector3f.UnitZ
            };
            obj.Colors = new StructBuffer<Color4b>()
            {
                Color4b.Red,
                Color4b.Red,
                Color4b.Green,
                Color4b.Green,
                Color4b.Blue,
                Color4b.Blue
            };

            var indices = obj.AddSubMesh();
            indices.Indices.AddRange(new int[] { 0, 1, 2, 3, 4, 5 });
            return obj;
        }

        /// <summary>
        /// Create the default Sphere
        /// </summary>
        /// <param name="horizontal">latitude</param>
        /// <param name="vertical">longitude</param>
        public static TriMesh Sphere(int horizontal = 3, int vertical = 3, float radius = 1f)
        {
            if (horizontal < 3 || vertical < 3) throw new Exception("impossible sphere");

            TriMesh sphere = new TriMesh(Primitive.TriangleList, "Sphere");

            float d_theta = Mathelp.Rad360 / vertical;
            float d_phi = Mathelp.Rad180 / (horizontal - 1);

            sphere.Vertices = new StructBuffer<Vector3f>(horizontal * vertical);

            //use 32bit to increase if necessary 
            var submesh = sphere.AddSubMesh(horizontal * vertical * 3);
            
            // Top vertex
            sphere.Vertices.Add(new Vector3f(0, radius, 0));

            // Intermediates vertices
            for (int i = 1; i < horizontal - 1; i++)
                for (int j = 0; j < vertical; j++)
                    sphere.Vertices.Add(Mathelp.SphericalToCartesian(radius, d_theta * j, d_phi * i));

            // Bottom vertex
            sphere.Vertices.Add(new Vector3f(0, -radius, 0));

            // Top faces
            for (int v = 0; v < vertical; v++)
                submesh.AddPrimitive(0, (v + 1) % vertical + 1, v + 1);

            // Intermediates faces
            for (int i = 0; i < horizontal - 3; i++)
            {
                int a = i * vertical + 1;
                int b = a + vertical;

                for (int j = 0; j < vertical; j++)
                    submesh.AddQuad(a + j, a + (j + 1) % horizontal, b + (j + 1) % horizontal, b + j);
            }

            // Borrom faces
            for (int i = 0; i < vertical; i++)
            {
                int a = vertical * (horizontal - 3) + 1;
                submesh.AddPrimitive(sphere.VerticesCount - 1, i + a, (i + 1) % vertical + a);
            }

            return sphere;
        }

        /// <summary>
        /// </summary>
        /// <param name="support">
        /// 1: vertex
        /// 2: edge
        /// 3: triangle
        /// </param>
        public static TriMesh Icosahedron(int support = 1)
        {
            TriMesh icosahedron = new TriMesh(Primitive.TriangleList, "Icosahedron");
            float rad5 = (float)Math.Sqrt(5);

            float[] vertices;
            byte[] indices;

            switch (support)
            {
                case 1:
                    {
                        
                        float a = 2f / rad5;
                        float b = 1f / rad5;
                        float c = (5 - rad5) / 10;
                        float d = (float)Math.Sqrt((5 + rad5) / 10);
                        float e = (5 + rad5) / 10;
                        float f = (float)Math.Sqrt((5 - rad5) / 10);

                        vertices = new float[]
                        {
                            0, 1, 0,
                            a, b, 0,
                            c, b, -d,
                            -e, b, -f,
                            -e, b, f,
                            c, b, d,
                            e, -b, f,
                            e, -b, -f,
                            -c, -b, -d,
                            -a, -b, 0,
                            -c, -b, d,
                            0, -1, 0
                        };
                        indices = new byte[] 
                        { 
                            0, 1, 2,
                            0, 2, 3,
                            0, 3, 4,
                            0, 4, 5,
                            0, 5, 1,
                            1, 6, 7,
                            2, 7, 8, 
                            3, 8, 9,
                            4, 9, 10,
                            5, 10, 6,
                            7, 2, 1,
                            8, 3, 2,
                            9, 4, 3,
                            10, 5, 4,
                            6, 1, 5,
                            11, 8, 7,
                            11, 9, 8,
                            11, 10, 9,
                            11, 6, 10, 
                            11, 7, 6 
                        };
                    }
                    break;
                case 2:
                default:
                    {
                        // rapporto aureo
                        float a = (1 + rad5) / 2;
                        vertices = new float[]
                        {
                            -1, a, 0,
                             1, a, 0,
                            -1,-a, 0,
                             1,-a, 0,
                             0,-1, a,
                             0, 1, a,
                             0,-1,-a,
                             0, 1,-a,
                             a, 0,-1,
                             a, 0, 1,
                            -a, 0,-1,
                            -a, 0, 1
                        };
                        indices = new byte[]
                        {
                             0, 11, 5,
                             0, 5,  1,
                             0, 1,  7,
                             0, 7,  10,
                             0, 10, 11,
                             1, 5,  9,
                             5, 11, 4,
                            11, 10, 2,
                            10, 7,  6,
                             7, 1,  8,
                             3, 9,  4,
                             3, 4,  2,
                             3, 2,  6,
                             3, 6,  8,
                             3, 8,  9,
                             4, 9,  5,
                             2, 4, 11,
                             6, 2, 10,
                             8, 6,  7,
                             9, 8,  1
                        };
                    }
                    break;
            }

            icosahedron.Vertices = StructBuffer<Vector3f>.Create(vertices);
            var submesh = icosahedron.AddSubMesh();
            submesh.Indices.AddRange(indices);


            return icosahedron;
        }

        /// <summary>
        /// Generate a base tetrahedron with lower face parallel to xy plane
        /// </summary>
        public static TriMesh Tetrahedron(float radius = 1)
        {
            TriMesh mesh = new TriMesh(Primitive.TriangleList, "Tetrahedron");

            float rad2 = (float)Math.Sqrt(2);
            float rad23 = (float)(rad2 / Math.Sqrt(3));
            float onethird = 1 / 3f;

            var vertices = new Vector3f[]
            {
                new Vector3f(2 *onethird * rad2, -onethird, 0) * radius,
                new Vector3f(-rad2 / 3f,  -onethird, rad23) * radius,
                new Vector3f(-rad2 / 3,-onethird, -rad23 ) * radius,
                Vector3f.UnitY * radius
            };

            mesh.Vertices = new StructBuffer<Vector3f>(vertices);

            var submesh = mesh.AddSubMesh();
            submesh.Indices.AddRange(new int[]
            {
                2,0,1,
                3,0,2,
                1,0,3,
                1,3,2
            });

            return mesh;
        }
    }
}
