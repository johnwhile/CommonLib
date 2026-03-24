using System;
using System.Collections.Generic;
using Common.Maths;

namespace Common.Geometry
{

    public static class GeometryTools
    {
        /// <summary>
        /// TODO : i don't like IList because if V is a struct i can't access value by reference
        /// Calculate default normal using faces , vertices is a reference class
        /// </summary>
        /// <typeparam name="V">composed array list, for optimization</typeparam>
        public static void CalculateNormals(IList<Vector3f> position, IList<Vector3f> normals, IList<Vector3us> faces)
        {
            int numVertices = position.Count;
            int numTriangles = faces.Count;

            float epsilon = 1e-6f;

            for (int t = 0; t < numTriangles; t++)
            {
                Vector3us face = faces[t];

                var v0 = position[face.x];
                var v1 = position[face.y];
                var v2 = position[face.z];

                Vector3f e0 = v1 - v0;
                Vector3f e1 = v2 - v0;
                Vector3f e2 = v2 - v1;

                Vector3f n = Vector3f.Cross(e0, e1);
                float dot0 = e0.LengthSq;
                float dot1 = e1.LengthSq;
                float dot2 = e2.LengthSq;

                if (dot0 < epsilon) dot0 = 1.0f;
                if (dot1 < epsilon) dot1 = 1.0f;
                if (dot2 < epsilon) dot2 = 1.0f;

                normals[face.x] += n * (1.0f / (dot0 * dot1)) * 10;
                normals[face.y] += n * (1.0f / (dot2 * dot0)) * 10;
                normals[face.z] += n * (1.0f / (dot1 * dot2)) * 10;

                position[face.x] = v0;
                position[face.y] = v1;
                position[face.z] = v2;

            }
            for (int i = 0; i < numVertices; i++)
                position[i].Normalize();
        }

        /// <summary>
        /// Calculate default normal using faces
        /// </summary>
        public static Vector3f[] CalculateNormals(IList<Vector3f> vertices, IList<Vector3us> faces)
        {
            int numVertices = vertices.Count;
            int numTriangles = faces.Count;

            Vector3f[] normals = new Vector3f[numVertices];

            float epsilon = 1e-6f;

            for (int t = 0; t < numTriangles; t++)
            {
                Vector3us face = faces[t];

                Vector3f v0 = vertices[face.x];
                Vector3f v1 = vertices[face.y];
                Vector3f v2 = vertices[face.z];

                Vector3f e0 = v1 - v0;
                Vector3f e1 = v2 - v0;
                Vector3f e2 = v2 - v1;

                Vector3f n = Vector3f.Cross(e0, e1);
                float dot0 = e0.LengthSq;
                float dot1 = e1.LengthSq;
                float dot2 = e2.LengthSq;

                if (dot0 < epsilon) dot0 = 1.0f;
                if (dot1 < epsilon) dot1 = 1.0f;
                if (dot2 < epsilon) dot2 = 1.0f;

                normals[face.x] += n * (1.0f / (dot0 * dot1));
                normals[face.y] += n * (1.0f / (dot2 * dot0));
                normals[face.z] += n * (1.0f / (dot1 * dot2));
            }
            for (int i = 0; i < numVertices; i++)
                normals[i].Normalize();

            return normals;
        }

        /// <summary>
        /// Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. 
        /// Terathon Software 3D Graphics Library, 2001. http://www.terathon.com/code/tangent.html
        /// </summary>
        /// <remarks>
        /// Remember to optain Bitangent vector as B = (NxT) * T.w
        /// </remarks>
        public static Vector4f[] CalculateTangents(IList<Vector3f> vertices, IList<Vector2f> texcood ,IList<Vector3f> normals, IList<Vector3us> faces)
        {
            int numVertices = vertices.Count;
            int numTriangles = faces.Count;

            if (texcood.Count < numVertices) throw new ArgumentOutOfRangeException("texcoord Items smaller than position ?");

            Vector3f[] tan1 = new Vector3f[numVertices];
            Vector3f[] tan2 = new Vector3f[numVertices];
            Vector4f[] tangents = new Vector4f[numVertices];

            for (int t = 0; t < numTriangles; t++)
            {
                Vector3us face = faces[t];

                Vector3f v1 = vertices[face.x];
                Vector3f v2 = vertices[face.y];
                Vector3f v3 = vertices[face.z];

                Vector2f w1 = texcood[face.x];
                Vector2f w2 = texcood[face.y];
                Vector2f w3 = texcood[face.z];

                //Vector3 e1 = v2 - v1;
                //Vector3 e2 = v3 - v1;

        
                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;
        
                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;
        
                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3f sdir = new Vector3f((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3f tdir = new Vector3f((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
        
                tan1[face.x] += sdir;
                tan1[face.y] += sdir;
                tan1[face.z] += sdir;
        
                tan2[face.x] += tdir;
                tan2[face.y] += tdir;
                tan2[face.z] += tdir;
            }


            for (int i = 0; i < numVertices; i++)
            {
                Vector3f n = normals[i];
                Vector3f t = tan1[i];
        
                // Gram-Schmidt orthogonalize
                tangents[i] = (t - n * Vector3f.Dot(n, t)).Normal;
                // Calculate handedness
                tangents[i].w = (Vector3f.Dot(Vector3f.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }

            return tangents;
        }



        /// <summary>
        /// Calculate default texture using an alligned plane
        /// </summary>
        /// <param name="minBound">minimum corner</param>
        /// <param name="maxBound">maximum corner</param>
        /// <param name="plane">plane to project the texture coordinates, identity matrix is alligned with plane XY</param>
        public static Vector2f[] GetPlanarProjection(IList<Vector3f> vertices, Vector2f minBound, Vector2f maxBound, Matrix4x4f plane)
        {
            int numVertices = vertices.Count;

            Vector2f[] texture = new Vector2f[numVertices];

            Vector2f min = new Vector2f(float.MaxValue, float.MaxValue);
            Vector2f max = new Vector2f(float.MinValue, float.MinValue);

            for (int i = 0; i < numVertices; i++)
            {
                Vector3f proj = Vector3f.Project(vertices[i], plane);
                texture[i] = new Vector2f(proj.x, proj.y);

                if (proj.x > max.x) max.x = proj.x;
                if (proj.y > max.y) max.y = proj.y;
                if (proj.x < min.x) min.x = proj.x;
                if (proj.y < min.y) min.y = proj.y;
            }
            // interpolate the bound condition y = (x-x0)(y1-y0)/(x1-x0) + y0
            // where x are the local coordinate , y the final bounding rectangle
            // y = (x-x0) * m + y0
            float mU = (maxBound.x - minBound.x) / (max.x - min.x);
            float mV = (maxBound.y - minBound.y) / (max.y - min.y);

            for (int i = 0; i < numVertices; i++)
            {
                texture[i].x = (texture[i].x - min.x) * mU + minBound.x;
                texture[i].y = (texture[i].y - min.y) * mV + minBound.y;
            }
            return texture;
        }

        /// <summary>
        /// Calculate default texture using 
        /// </summary>
        /// <param name="cilindral">cilindral transformation</param>
        /// <returns></returns>
        public static Vector2f[] GetCilindralProjection(IList<Vector3f> vertices, Matrix4x4f cilindral)
        {
            int numvertices = vertices.Count;
            Vector2f[] textures = new Vector2f[numvertices];

            float minh = float.MaxValue;
            float maxh = float.MinValue;

            for (int i = 0; i < numvertices; i++)
            {
                Vector3f v = vertices[i].TransformCoordinate(in cilindral);
                float r = (float)Math.Sqrt(v.x * v.x + v.z * v.z);
                float t = (float)Math.Asin(v.z / r);
                float h = v.y;

                if (v.x < 0) t = (float)Math.PI - t;
                if (v.y < minh) minh = v.y;
                if (v.y > maxh) maxh = v.y;

                textures[i] = new Vector2f((float)(t / Math.PI / 2.0), h);
            }

            for (int i = 0; i < numvertices; i++)
            {
                textures[i].y = (textures[i].y - minh) / (maxh - minh);
            }

            return textures;
        }


        public static Vector4f GetTangentVector(Vector3f p0, Vector3f p1, Vector3f p2, Vector2f uv0, Vector2f uv1, Vector2f uv2, Vector3f n)
        {
            // Given the 3 vertices (position and texture coordinates) of a triangle
            // calculate and return the triangle's tangent vector. The handedness of
            // the local coordinate system is stored in tangent.w. The bitangent is
            // then: float3 bitangent = cross(normal, tangent.xyz) * tangent.w.
            Vector3f edge0 = (p1 - p0).Normal;
            Vector3f edge1 = (p2 - p0).Normal;

            // Create 2 vectors in tangent (texture) space that point in the same
            // direction as edge1 and edge2 (in object space).
            Vector2f tedge0 = Vector2f.GetNormal(uv1 - uv0);
            Vector2f tedge1 = Vector2f.GetNormal(uv2 - uv0);

            // These 2 sets of vectors form the following system of equations:
            //
            //  edge1 = (texEdge1.x * tangent) + (texEdge1.y * bitangent)
            //  edge2 = (texEdge2.x * tangent) + (texEdge2.y * bitangent)
            //
            // Using matrix notation this system looks like:
            //
            //  [ edge1 ]     [ texEdge1.x  texEdge1.y ]  [ tangent   ]
            //  [       ]  =  [                        ]  [           ]
            //  [ edge2 ]     [ texEdge2.x  texEdge2.y ]  [ bitangent ]
            //
            // The solution is:
            //
            //  [ tangent   ]        1     [ texEdge2.y  -texEdge1.y ]  [ edge1 ]
            //  [           ]  =  -------  [                         ]  [       ]
            //  [ bitangent ]      det A   [-texEdge2.x   texEdge1.x ]  [ edge2 ]
            //
            //  where:
            //        [ texEdge1.x  texEdge1.y ]
            //    A = [                        ]
            //        [ texEdge2.x  texEdge2.y ]
            //
            //    det A = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)
            //
            // From this solution the tangent space basis vectors are:
            //
            //    tangent = (1 / det A) * ( texEdge2.y * edge1 - texEdge1.y * edge2)
            //  bitangent = (1 / det A) * (-texEdge2.x * edge1 + texEdge1.x * edge2)
            //     normal = cross(tangent, bitangent)

            Vector3f bitangent = new Vector3f();
            Vector3f tangent = new Vector3f();

            float det = (tedge0.x * tedge1.y) - (tedge0.y * tedge1.x);

            if (Maths.Mathelp.ABS(det) < 1e-6f)    // almost equal to zero
            {
                tangent.x = 1.0f;
                tangent.y = 0.0f;
                tangent.z = 0.0f;
                bitangent.x = 0.0f;
                bitangent.y = 1.0f;
                bitangent.z = 0.0f;
            }
            else
            {
                det = 1.0f / det;

                tangent.x = (tedge1.y * edge0.x - tedge0.y * edge1.x) * det;
                tangent.y = (tedge1.y * edge0.y - tedge0.y * edge1.y) * det;
                tangent.z = (tedge1.y * edge0.z - tedge0.y * edge1.z) * det;

                bitangent.x = (-tedge1.x * edge0.x + tedge0.x * edge1.x) * det;
                bitangent.y = (-tedge1.x * edge0.y + tedge0.x * edge1.y) * det;
                bitangent.z = (-tedge1.x * edge0.z + tedge0.x * edge1.z) * det;

                tangent.Normalize();
                bitangent.Normalize();
            }
            // Calculate the handedness of the local tangent space.
            // The bitangent vector is the cross product between the triangle face
            // normal vector and the calculated tangent vector. The resulting bitangent
            // vector should be the same as the bitangent vector calculated from the
            // set of linear equations above. If they point in different directions
            // then we need to invert the cross product calculated bitangent vector. We
            // store this scalar multiplier in the tangent vector's 'w' component so
            // that the correct bitangent vector can be generated in the normal mapping
            // shader's vertex shader.
            Vector3f b = Vector3f.Cross(n, tangent);
            float w = (Vector3f.Dot(b, bitangent) < 0.0f) ? -1.0f : 1.0f;
            Vector4f t = new Vector4f(tangent.x, tangent.y, tangent.z, w);

            return t;
        }

    }

}
