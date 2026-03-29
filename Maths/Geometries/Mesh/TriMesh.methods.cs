using System;
using System.Collections.Generic;

using Common.Tools;
using Common.IO.Wavefront;

namespace Common.Maths
{
    public partial class TriMesh
    {
        /// <summary>
        /// Remove unused vertices or vertices and triangles passed as bitarray, resort indices in submeshes
        /// </summary>
        /// <param name="addVertices">true if vertex will be add in the returned mesh</param>
        /// <param name="addTriangles">true if triangles will be add in the returned mesh</param>
        public TriMesh RemapMesh(BitArray1 addVertices = null, BitArray1 addTriangles = null)
        {
            int submeshcount = SubMeshCount;

            if (submeshcount == 0) return null;

            TriMesh mesh = new TriMesh(Topology, Name);
            mesh.SubMeshes = new List<SubMesh>(submeshcount);
            mesh.Transform = Transform;


            int[] vertexRemap = new int[VerticesCount];
            int vertCounter = 1;
            int triCounter = 0;


            for (int s = 0; s < submeshcount; s++)
            {
                var sub = SubMeshes[s];

                SubMesh newSub = new SubMesh(mesh, sub.IndincesCount / 3, IndexFormat.Index32bit, sub.Name);

                int numTriangles = 0;

                for (int f = 0; f < sub.IndincesCount / 3; f++)
                {
                    if (addTriangles != null && !addTriangles[triCounter++]) continue;

                    int i = sub.Indices[f * 3 + 0];
                    int j = sub.Indices[f * 3 + 1];
                    int k = sub.Indices[f * 3 + 2];
                    if (addVertices != null && (!addVertices[i] || !addVertices[j] || !addVertices[k])) continue;

                    if (vertexRemap[i] == 0) vertexRemap[i] = vertCounter++;
                    if (vertexRemap[j] == 0) vertexRemap[j] = vertCounter++;
                    if (vertexRemap[k] == 0) vertexRemap[k] = vertCounter++;

                    newSub.Indices.Add(vertexRemap[i] - 1);
                    newSub.Indices.Add(vertexRemap[j] - 1);
                    newSub.Indices.Add(vertexRemap[k] - 1);

                    numTriangles++;
                }

                if (numTriangles > 0)
                {
                    mesh.SubMeshes.Add(newSub);
                }

            }
            vertCounter--;

            mesh.Vertices = new StructBuffer<Vector3f>(vertCounter);
            for (int i = 0; i < vertCounter; i++) mesh.Vertices.Add(default);

            for (int i = 0; i < vertexRemap.Length; i++)
                if (vertexRemap[i] > 0)
                    mesh.Vertices[vertexRemap[i] - 1] = Vertices[i];

            if (mesh.HasNormals)
            {
                mesh.Normals = new StructBuffer<Vector3f>(vertCounter);
                for (int i = 0; i < vertCounter; i++) mesh.Normals.Add(default);

                for (int i = 0; i < vertexRemap.Length; i++)
                    if (vertexRemap[i] > 0)
                        mesh.Normals[vertexRemap[i] - 1] = Normals[i];
            }
            if (mesh.HasTexCoords)
            {
                mesh.TexCoords = new StructBuffer<Vector2f>(vertCounter);
                for (int i = 0; i < vertCounter; i++) mesh.TexCoords.Add(default(Vector2f));

                for (int i = 0; i < vertexRemap.Length; i++)
                    if (vertexRemap[i] > 0)
                        mesh.TexCoords[vertexRemap[i] - 1] = TexCoords[i];
            }

            if (vertCounter == 0 || mesh.SubMeshes.Count == 0) return null;

            return mesh;
        }

        /// <summary>
        /// Convert this mesh structure into simple wavefront file.
        /// </summary>
        /// <param name="merge">if true all submeshes are converted into one wavefront group</param>
        /// <param name="worldTRS">apply a local to global affine trasformation because wavefront doesn't contain a coord system reference</param>
        /// <remarks>
        /// The shared vertices between submesh will be lost and will be create a copy of vertex:
        /// submesh1 = (0,1,2) (3,4,5) -> (0,1,2) (3,4,5)
        /// submesh2 = (6,7,8) (0,1,2) -> (6,7,8) (9,10,11)
        /// </remarks>
        public WavefrontObj ConvertToWavefront(bool merge = false, Matrix4x4f? worldTRS = null)
        {
            bool useGlobalShared = true;
            WavefrontObj wavefile = new WavefrontObj(useGlobalShared);

            WavePrimitive primitive = WavePrimitive.Unknow;
            switch (Topology)
            {
                case Primitive.Point: primitive = WavePrimitive.Point; break;
                case Primitive.LineList: primitive = WavePrimitive.Line; break;
                case Primitive.TriangleList: primitive = WavePrimitive.Triangle; break;
                default: throw new NotImplementedException();
            }


            if (merge)
            {
                WaveObject waveObject = wavefile.Create(Name);
                waveObject.AddVertex(Vertices);

                if (HasTexCoords) waveObject.TexCoords.AddRange(TexCoords);
                if (HasNormals) waveObject.Normals.AddRange(Normals);

                if (worldTRS is Matrix4x4f worldtrs)
                    for (int i = 0; i < waveObject.Vertices.Count; i++)
                        waveObject.Vertices[i] = waveObject.Vertices[i].TransformCoordinate(in worldtrs);

                foreach (var submesh in SubMeshes)
                {
                    var waveGroup = waveObject.Create(primitive, submesh.Name);
                    waveGroup.indexV.AddRange(submesh.Indices);
                }
            }
            else
            {
                foreach (var submesh in SubMeshes)
                {
                    var waveObject = wavefile.Create(submesh.Name);
                    var waveGroup = waveObject.Create(primitive, submesh.Name);

                    (int min, int max) = Mathelp.GetMinMax(submesh.Indices);
                    int range = max - min;

                    //remap method
                    //BitArray1 indexMask = new BitArray1(range, false);
                    //foreach (var idx in submesh.indices) indexMask[idx - min] = true;

                    waveGroup.indexV.AddRange(submesh.Indices);

                    //ignore t and n index
                    waveObject.UpdateMinMaxIndexValue();

                    for (int v = waveObject.minIndexUsed.x; v <= waveObject.maxIndexUsed.x; v++)
                        waveObject.Vertices.Add(Vertices[v]);

                    if (HasTexCoords)
                        for (int v = waveObject.minIndexUsed.x; v <= waveObject.maxIndexUsed.x; v++)
                            waveObject.TexCoords.Add(TexCoords[v]);

                    if (HasNormals)
                        for (int v = waveObject.minIndexUsed.x; v <= waveObject.maxIndexUsed.x; v++)
                            waveObject.Normals.Add(Normals[v]);


                    if (worldTRS is Matrix4x4f worldtrs)
                        for (int i = 0; i < waveObject.Vertices.Count; i++)
                            waveObject.Vertices[i] = waveObject.Vertices[i].TransformCoordinate(in worldtrs);
                }
            }
            return wavefile;
        }

        /// <summary>
        /// ATTENTION : the multi normals and texcoord will be collaped to match with vertices count<br/>
        /// Wavefront format can contain more than one primitive but i will convert only first type
        /// </summary>
        public static TriMesh ConvertFrom(WavefrontObj file)
        {
            if (!file.UseGlobalShared) throw new NotImplementedException();

            WavePrimitive firstprimitive = WavePrimitive.Unknow;
            foreach (var obj in file.Objects)
            {
                foreach (var grp in obj.Groups)
                {
                    if (firstprimitive == WavePrimitive.Unknow) firstprimitive = grp.Topology;
                    break;
                }
            }

            Primitive meshprimitive = Primitive.Undefined;
            switch (firstprimitive)
            {
                case WavePrimitive.Point: meshprimitive = Primitive.Point; break;
                case WavePrimitive.Line: meshprimitive = Primitive.LineList; break;
                case WavePrimitive.LineStrip: meshprimitive = Primitive.LineStrip; break;
                case WavePrimitive.Triangle: meshprimitive = Primitive.TriangleList; break;
                case WavePrimitive.TriangleFan: meshprimitive = Primitive.TriangleFan; break;
                default: throw new NotImplementedException();
            }


            TriMesh mesh = new TriMesh(meshprimitive, System.IO.Path.GetFileNameWithoutExtension(file.Filename));
            mesh.Transform = Matrix4x4f.Identity;

            int vcount = file.GetTotalVerticesCount();
            mesh.Vertices = new StructBuffer<Vector3f>(new Vector3f[vcount]);

            int tcount = file.GetTotalTexCoordsCount();
            int[] t_map = null;
            if (tcount > 0)
            {
                if (tcount != vcount) Debugg.Warning("Multy wavefront TexCoords are not exported, will be collapsed to match the Vertices m_count");
                mesh.TexCoords = new StructBuffer<Vector2f>(new Vector2f[vcount]);
                t_map = new int[vcount];
            }


            int ncount = file.GetTotalNormalsCount();
            int[] n_map = null;

            if (ncount > 0)
            {
                if (ncount != vcount) Debugg.Warning("Multy wavefront Normals are not exported, will be collapsed to match the Vertices m_count");
                mesh.Normals = new StructBuffer<Vector3f>(new Vector3f[vcount]);
                n_map = new int[vcount];
            }

            int voffset = 0;
            int ioffset = 0;
            foreach (var waveobj in file.Objects)
            {
                vcount = waveobj.Vertices.Count;

                for (int i = 0; i < vcount; i++)
                    mesh.Vertices[voffset + i] = waveobj.Vertices[i];

                foreach (var group in waveobj.Groups)
                {
                    if (group.Topology == firstprimitive)
                    {
                        SubMesh subMesh = mesh.AddSubMesh(0, IndexFormat.Index32bit, group.Name);

                        subMesh.Indices.AddRange(group.indexV);

                        //assuming indexV.Count = indexT.Count = indexN.Count
                        for (int i = 0; i < group.indexT.Count; i++)
                        {
                            int idx = group.indexV[i];
                            mesh.TexCoords[idx] += waveobj.TexCoords[ioffset + group.indexT[i]];
                            t_map[idx]++;
                        }
                        for (int i = 0; i < group.indexN.Count; i++)
                        {
                            int idx = group.indexV[i];
                            mesh.Normals[idx] += waveobj.Normals[ioffset + group.indexN[i]];
                            n_map[idx]++;
                        }
                    }
                }
                if (!file.UseGlobalShared) ioffset += vcount;
                voffset += vcount;
            }

            if (t_map!=null)
            {
                for (int i = 0; i < mesh.VerticesCount; i++)
                    mesh.TexCoords[i] /= t_map[i];
            }
            if (n_map != null)
            {
                for (int i = 0; i < mesh.VerticesCount; i++)
                    mesh.Normals[i] = Vector3f.Normalize(mesh.Normals[i] / n_map[i]);
            }
            return mesh;
        }

        /// <summary>
        /// TODO: implement something better.
        /// </summary>
        /// <remarks>check if <see cref="Transform"/> is set</remarks>
        public static TriMesh MergeAllGeometries(IEnumerable<TriMesh> Meshes)
        {
            if (Meshes == null) return null;

            TriMesh bigOne = new TriMesh();
            bigOne.Vertices = new StructBuffer<Vector3f>();

            foreach (var mesh in Meshes)
            {
                int vbegin = bigOne.Vertices.Count;
                int vend = vbegin + mesh.Vertices.Count;
                int voffset = vbegin;

                bigOne.Vertices.AddRange(mesh.Vertices);

                for (int i = vbegin; i < vend; i++)
                    bigOne.Vertices[i] = bigOne.Vertices[i].TransformCoordinate(in mesh.Transform);
                

                SubMesh submesh = bigOne.AddSubMesh();
                submesh.Name = mesh.Name;

                mesh.SortSubMesh();

                foreach (var sub in mesh.SubMeshes)
                {
                    int ibegin = submesh.Indices.Count;
                    int iend = ibegin + sub.Indices.Count;

                    submesh.Indices.AddRange(sub.Indices);

                    for (int i = ibegin; i < iend; i++)
                    {
                        submesh.Indices[i] = submesh.Indices[i] + voffset;
                    }
                }
            }
            if (bigOne.Vertices.Count == 0) return null;
            return bigOne;
        }


        /// <summary>
        /// Calculate the bound of this submesh
        /// </summary>
        public static BoundingBoxMinMax GetSubBound(TriMesh mesh, int submesh)
        {
            BoundingBoxMinMax bound = new BoundingBoxMinMax();
            SubMesh sub = mesh.SubMeshes[submesh];
            foreach (int i in sub.Indices)
                bound.Merge(mesh.Vertices[i]);
            return bound;
        }

        /// <summary>
        /// Algorithm to convert submesh as mesh, so the returned value is a mesh with an unique submesh
        /// </summary>
        /// <returns></returns>
        public TriMesh ConvertToMesh(SubMesh submesh)
        {
            TriMesh new_mesh = new TriMesh(submesh.mesh.Topology);

            //build the indices map as bit array because is the best option for memory usage
            (int offset, int max) = Mathelp.GetMinMax(submesh.Indices);
            BitArray1 utilsMap = BitArray1.Create(submesh.Indices, offset, max - offset + 1);
            int vertsCount = utilsMap.GetCount1;


            //build new vertices list with only index found in indices list
            new_mesh.Vertices = new StructBuffer<Vector3f>(vertsCount);

            foreach (int iv in utilsMap.IndicesList)
            {
                int old_v = iv + offset;
                new_mesh.Vertices.Add(Vertices[old_v]);
            }

            //make a shallow copy of entire class
            //SubMesh new_submesh = MemberwiseClone() as SubMesh;

            SubMesh new_submesh = new_mesh.AddSubMesh(submesh);

            //remap indices list
            int[] subtractor = new int[max - offset + 1];
            int idx = 0;
            int subtract = 0;
            foreach (bool val in utilsMap.BooleanList)
            {
                if (!val) subtract++;
                else subtractor[idx] = subtract;
                idx++;
            }
            for (int i = 0; i < new_submesh.Indices.Count; i++)
            {
                idx = new_submesh.Indices[i] - offset;
                new_submesh.Indices[i] -= subtractor[idx] + offset;
            }

            //assign manualy the new references
            //new_mesh.SubMeshes = new List<SubMesh>(1);
            //new_submesh.mesh = new_mesh;
            //new_mesh.SubMeshes.Add(new_submesh);

            return new_mesh;
        }

        public void Tessellate(SubMesh submesh, int triangle)
        {
            int voffset = VerticesCount;
            int ioffset = submesh.Indices.Count;

            int i = submesh.Indices[triangle * 3 + 0];
            int j = submesh.Indices[triangle * 3 + 1];
            int k = submesh.Indices[triangle * 3 + 2];

            var vi = Vertices[i];
            var vj = Vertices[j];
            var vk = Vertices[k];

            var vij = (vi + vj) / 2;
            var vjk = (vj + vk) / 2;
            var vki = (vk + vi) / 2;

            Vertices.Add(vij);
            Vertices.Add(vjk);
            Vertices.Add(vki);

            //reuse this triangle
            submesh.Indices[triangle * 3 + 0] = i;
            submesh.Indices[triangle * 3 + 1] = voffset;
            submesh.Indices[triangle * 3 + 2] = voffset + 2;

            //add others
            int[] newindices = new int[]
            {
                i, voffset, voffset + 2,
                j, voffset + 1, voffset,
                k, voffset + 2, voffset + 1,
                voffset, voffset + 1, voffset + 2
            };
            submesh.Indices.AddRange(newindices);
        }

        public bool GenerateNormals()
        {
            if (VerticesCount < 3) return false;

            if (Topology == Primitive.TriangleList)
            {

                var normals = new Vector3f[VerticesCount];
                float epsilon = 1e-6f;

                foreach (var submesh in SubMeshes)
                {
                    if (submesh.IndincesCount < 3) throw new Exception("TriangleList can't have less than 2 indices");

                    for (int t = 0; t < submesh.IndincesCount; t += 3)
                    {
                        int i = submesh.Indices[t];
                        int j = submesh.Indices[t + 1];
                        int k = submesh.Indices[t + 2];

                        Vector3f v0 = Vertices[i];
                        Vector3f v1 = Vertices[j];
                        Vector3f v2 = Vertices[k];

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

                        normals[i] += n * (1.0f / (dot0 * dot1));
                        normals[j] += n * (1.0f / (dot2 * dot0));
                        normals[k] += n * (1.0f / (dot1 * dot2));
                    }
                }
                for (int i = 0; i < VerticesCount; i++)
                    normals[i].Normalize();

                Normals = new StructBuffer<Vector3f>(normals);

            }
            else
            {
                throw new NotImplementedException();
            }
            return true;
        }


        /// <summary>
        /// Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. 
        /// Terathon Software 3D Graphics Library, 2001. http://www.terathon.com/code/tangent.html
        /// </summary>
        /// <remarks>
        /// Remember to optain Bitangent vector as B = (NxT) * T.w
        /// </remarks>
        public bool GenerateTangents()
        {
            if (!HasTexCoords || !HasNormals) throw new Exception("require texcoords and normals");

            if (TexCoords.Count < VerticesCount || Normals.Count < VerticesCount)
                throw new ArgumentOutOfRangeException("texcoords or normals size smaller than vertices");

            if (Topology == Primitive.TriangleList)
            {
                Tangents = new StructBuffer<Vector4f>(new Vector4f[VerticesCount]);
                Vector3f[] tan1 = new Vector3f[VerticesCount];
                Vector3f[] tan2 = new Vector3f[VerticesCount];

                foreach (var submesh in SubMeshes)
                {
                    if (submesh.IndincesCount < 3) throw new Exception("TriangleList can't have less than 2 indices");
                    for (int t = 0; t < submesh.IndincesCount; t += 3)
                    {
                        int i = submesh.Indices[t];
                        int j = submesh.Indices[t + 1];
                        int k = submesh.Indices[t + 2];

                        Vector3f v1 = Vertices[i];
                        Vector3f v2 = Vertices[j];
                        Vector3f v3 = Vertices[k];
                        Vector2f w1 = TexCoords[i];
                        Vector2f w2 = TexCoords[j];
                        Vector2f w3 = TexCoords[k];

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

                        tan1[i] += sdir;
                        tan1[j] += sdir;
                        tan1[k] += sdir;

                        tan2[i] += tdir;
                        tan2[j] += tdir;
                        tan2[k] += tdir;
                    }
                }
                for (int i = 0; i < VerticesCount; i++)
                {
                    Vector3f n = Normals[i];
                    Vector3f t = tan1[i];

                    // Gram-Schmidt orthogonalize
                    Vector4f tangent = (t - n * Vector3f.Dot(n, t)).Normal;
                    // Calculate handedness
                    tangent.w = (Vector3f.Dot(Vector3f.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;

                    Tangents[i] = tangent;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return true;
        }



    }
}
