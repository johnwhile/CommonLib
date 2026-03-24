using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Common.Maths;

namespace Common.IO.Wavefront
{

    [DebuggerDisplay("{Name}")]
    /// <summary>
    ///  Group name statements are used to organize collections of elements
    ///  and simplify data manipulation. uses vertices referring to its own waveObject,
    ///  so use waveObject to make saparate mesh.
    /// </summary>
    public class WaveGroup
    {
        //mantain linked list reference
        internal WaveGroup prev;
        internal WaveGroup next;
        internal WaveObject owner;

        public WaveVertexFormat Format { get; internal set; } = WaveVertexFormat.None;

        /// <summary>
        /// The Element type
        /// </summary>
        public WavePrimitive Topology { get; internal set; } = WavePrimitive.Unknow;


        public int PolygonDim = 0;

        /// <summary>
        /// classic primitive indices
        /// </summary>
        public List<int> indexV = new List<int>();
        /// <summary>
        /// wavefront can contain a different index for special geometry, but usually match with <see cref="indexV"/><br/>
        /// <b>Must be zero or equal to <see cref="indexV"/>.Count</b>
        /// </summary>
        public List<int> indexT = new List<int>();
        /// <summary>
        /// <inheritdoc cref="indexT"/>
        /// </summary>
        public List<int> indexN = new List<int>();


        /// <summary>
        /// since it doesn't matter, the name is a unique string instead of an array of strings
        /// ATTENTION, if name is not assigned, will be not write the group's statements "g group_name"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public string Material { get; set; }


        /// <summary>
        /// Will be automatically added to group list
        /// </summary>
        internal WaveGroup(WaveObject owner, WavePrimitive topology, string name = null, string material = null)
        {
            Name = name;
            Material = material;
            this.owner = owner;
            //polygons = new List<WavePolygon>();

            if (owner.Groups.Count > 0)
            {
                prev = owner.Groups[owner.Groups.Count - 1];
                prev.next = this;
            }
            owner.Groups.Add(this);
            Format = WaveVertexFormat.None;
            Topology = topology;
        }

        /// <summary>
        /// Will be automatically added to group list.
        /// </summary>
        /// <remarks>Only for internal use, a user must know the topology</remarks>
        internal WaveGroup(WaveObject owner, string name = null, string material = null) : this(owner, WavePrimitive.Unknow, name, material) { }

        [Obsolete("do it manualy")]
        /// <summary>
        /// All indices are refers to WaveObject vertex table. the Min Max table are not updated
        /// </summary>
        public void AddTriangle(int i, int j, int k)
        {
            if (Topology != WavePrimitive.Triangle) throw new Exception("This object isn't a triangle type");
            indexV.Add(i);
            indexV.Add(j);
            indexV.Add(k);
        }

        [Obsolete("do it manualy")]
        /// <summary>
        /// </summary>
        /// <param name="isSharedIndex">if false, to each indices will be add the offset refered to all <see cref="WaveObject"/></param>
        public void AddTriangles(IEnumerable<Vector3i> triangles, bool isSharedIndex)
        {
            if (Topology != WavePrimitive.Triangle) throw new Exception("This object isn't a triangle type");

            int voffset = isSharedIndex ? 0 : owner.GetVertexIndexOffset();

            foreach (var t in triangles)
                AddTriangle(t.x + voffset, t.y + voffset, t.z + voffset);

        }


        delegate int GetFakeIndex(int index);
        int GetIndexV(int index) => indexV[index] + 1;
        int GetIndexT(int index) => indexT[index] + 1;
        int GetIndexN(int index) => indexN[index] + 1;


        /// <summary>
        /// Save this section to file
        /// </summary>
        /// <param name="format">it must contain Vertex</param>
        public bool Write(StreamWriter writer, WaveVertexFormat format = WaveVertexFormat.Vertex)
        {
            if (Name != null) writer.WriteLine($"g {Name}");


            if (Material != null)
            {
                string lastMaterial = null;

                if (owner.prev != null && owner.prev.Groups.Count > 0)
                {
                    lastMaterial = owner.prev.Groups[owner.prev.Groups.Count - 1].Material;
                }

                // this improve a lot the performance
                if ((prev == null && lastMaterial != Material) || (prev != null && prev.Material != Material))
                {
                    writer.WriteLine($"usemtl {Material}");
                }
            }

            string signature = WaveObject.topololyName[(int)Topology];

            int dim = 0;
            switch (Topology)
            {
                case WavePrimitive.Line: dim = 2; break;
                case WavePrimitive.Triangle: dim = 3; break;

                case WavePrimitive.Point:
                case WavePrimitive.LineStrip:
                case WavePrimitive.TriangleFan: dim = indexV.Count; break;

                default: throw new NotImplementedException($"The topology {Topology} is not implemented");
            }

            int polygons = dim == 0 ? 1 : indexV.Count / dim;


            GetFakeIndex GetT = GetIndexT;
            GetFakeIndex GetN = GetIndexN;

            if (indexT.Count != indexV.Count) GetT = GetIndexV;
            if (indexN.Count != indexV.Count) GetN = GetIndexV;


            for (int p = 0; p < polygons; p++)
            {
                writer.Write(signature);

                switch (format)
                {
                    //v
                    case WaveVertexFormat.Vertex:
                        for (int d = 0; d < dim; d++)
                            writer.Write($" {GetIndexV(p * dim + d)}");
                        break;

                    // v/vt
                    case WaveVertexFormat.VertexTexcoord:
                        for (int d = 0; d < dim; d++)
                        {
                            int i = p * dim + d;
                            writer.Write($" {GetIndexV(i)}/{GetT(i)}");
                        }

                        break;

                    // v/vt/vn
                    case WaveVertexFormat.VertexTexcoordNormal:
                        for (int d = 0; d < dim; d++)
                        {
                            int i = p * dim + d;
                            writer.Write($" {GetIndexV(i)}/{GetT(i)}/{GetN(i)}");
                        }
                        break;

                    // v//vn
                    case WaveVertexFormat.VertexNormal:
                        for (int d = 0; d < dim; d++)
                        {
                            int i = p * dim + d;
                            writer.Write($" {GetIndexV(i)}//{GetN(i)}");
                        }
                        break;

                    default:
                        throw new ArgumentException("the vertex m_format \"" + format.ToString() + "\" is not correct");
                }

                writer.WriteLine();

            }

            if (Name != null) writer.WriteLine($"# {polygons} polygons");
            return true;
        }

        public override string ToString() => $"{Name} indices:{indexV.Count}";
    }
}
