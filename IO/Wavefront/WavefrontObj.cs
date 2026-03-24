using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using System.Diagnostics;
using Common.Maths;
using System.Collections;

namespace Common.IO.Wavefront
{
    /// <summary>
    /// Simple Wavefront Object file format, only implemented simple meshes.
    /// The indices are store as 32bit integer.
    /// Ordering of the vertices is counterclockwise
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public partial class WavefrontObj : Wavefront, IEnumerable<WaveObject>
    {
        public static string Ext = ".obj";
        public override string Extension => Ext;

        internal List<WaveObject> Objects;
        WaveObject currentObject;
        WaveGroup currentGroup;

        BoundingBoxMinMax m_bound = BoundingBoxMinMax.NaN;
        public BoundingBoxMinMax Bound => m_bound;
        
        public IEnumerator<WaveObject> GetEnumerator()=> Objects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>GetEnumerator();


        public void Clear()
        {
            Objects.Clear();
            currentGroup = null;
            currentObject = null;
            m_bound = BoundingBoxMinMax.NaN;
        }

        public void UpdateBound()
        {
            m_bound = BoundingBoxMinMax.NaN;
            foreach (var obj in this)
            {
                foreach (var v in obj.Vertices)
                    m_bound.Merge(v);
            }
        }
        /// <summary>
        /// Some programm require same filename of wavefront object file
        /// </summary>
        public string MaterialFilename { get; set; }

        /// <summary>
        /// if =true the polygon indices refers to the global list and it's the most used version.
        /// if =false means the list of vertices is shared only for current object (local reference)
        /// but it generates incompatibility with many formats. 
        /// </summary>
        /// <remarks>
        /// the format can be inferred at the end of reading.
        /// </remarks>
        public bool UseGlobalShared { get; internal set; }


        public WavefrontObj() : this(true)
        {
        }

        public WaveObject Create(string name)
        {
            return new WaveObject(this, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UseGlobalShared">set to true to share vertices between all groups</param>
        /// <remarks>
        /// wavefront OBJ format tell that vertices are not shared between new groups, but some application
        /// like 3ds max's wavefront importer don't like face index separation between new group.
        /// </remarks>
        public WavefrontObj(bool UseGlobalShared = true)
        {
            Objects = new List<WaveObject>();
            this.UseGlobalShared = UseGlobalShared;

        }

        public int GetTotalIndicesCount() => Objects.Sum(x => x.GetTotalIndicesCount);
        public int GetTotalVerticesCount() => Objects.Sum(x => x.Vertices.Count);
        public int GetTotalNormalsCount() => Objects.Sum(x => x.Normals.Count);
        public int GetTotalTexCoordsCount() => Objects.Sum(x => x.TexCoords.Count);


        /// <summary>
        /// Load .obj from a filename
        /// </summary>
        public static WavefrontObj Load(string filename) => Load<WavefrontObj>(filename);
        
        /// <summary>
        /// Load .obj from a stream
        /// </summary>
        protected override bool Read(Stream stream)
        {
            if (!stream.CanRead)
            {
                Debugg.Error("> stream report can's read from it");
                return false;
            }
            using (WavefontReader reader = new WavefontReader(stream))
            {
                try
                {

                    UseGlobalShared = false;

                    //names can only be assigned after reading the vertices section
                    int ObjectCount = 0;
                    int GroupCount = 0;
                    currentObject = new WaveObject(this);
                    currentGroup = new WaveGroup(currentObject);

                    // this flag manage the creation of new groups
                    bool nextVertsMakeNew = false;
                    // this flag is necessary to avoid creation of a second group if read a new "g" in same waveObject
                    bool isFirstGroup = true;

                    while (reader.ReadNext(out var splitline))
                    {
                        if (splitline.Length < 2) throw new InconsistentDataException("wrong row m_format at", reader.LineCounter);

                        switch (splitline[0])
                        {
                            // the vertices section implies a new object
                            case "v":
                                if (nextVertsMakeNew)
                                {
                                    currentObject = new WaveObject(this);
                                    currentGroup = new WaveGroup(currentObject);
                                    isFirstGroup = true;
                                    nextVertsMakeNew = false;
                                    ObjectCount++;
                                    GroupCount++;
                                }
                                currentObject.Vertices.Add(ParseVector3(splitline));
                                break;

                            case "vn":
                                if (nextVertsMakeNew)
                                    throw new InconsistentDataException("the geometry vertex list missing", reader.LineCounter);
                                currentObject.Normals.Add(ParseVector3(splitline));
                                break;

                            case "vt":
                                if (nextVertsMakeNew)
                                    throw new InconsistentDataException("the geometry vertex list missing", reader.LineCounter);
                                currentObject.TexCoords.Add(ParseVector2(splitline));
                                break;

                            case "o":
                                nextVertsMakeNew = true;//can be omitted in my opition
                                // if data is null, object's statement will miss
                                for (int i = 1; i < splitline.Length; i++) currentObject.Name += (i > 2) ? " " : "" + splitline[i];
                                break;

                            case "g":
                                nextVertsMakeNew = true; //can be omitted in my opition
                                if (!isFirstGroup)
                                {
                                    currentGroup = new WaveGroup(currentObject);
                                    GroupCount++;
                                }
                                // at next "g" (if exist it) create a new group
                                isFirstGroup = false;
                                // if data is null, group's statement will miss
                                for (int i = 1; i < splitline.Length; i++) currentGroup.Name += (i > 2) ? " " : "" + splitline[i];
                                break;

                            case "p":
                                {
                                    nextVertsMakeNew = true;

                                    var format = ParseIndices(splitline,
                                        out var v, out var t, out var n,
                                        currentObject.VertsCount, currentObject.TexCoordsCount, currentObject.NormalsCount,
                                        currentGroup.Format);

                                    if (currentGroup.Format == WaveVertexFormat.None) currentGroup.Format = format;

                                    if (v != null && v.Length > 0) currentGroup.indexV.AddRange(v);
                                    if (t != null && t.Length > 0) currentGroup.indexT.AddRange(t);
                                    if (n != null && n.Length > 0) currentGroup.indexN.AddRange(n);

                                    if (v == null || v.Length < 1) throw new InconsistentDataException("Point polygon require at least one vertex", reader.LineCounter);

                                    //no conceptual difference between PointStrip and PointList
                                    //so i add indices like a PointList
                                    currentGroup.Topology = WavePrimitive.Point;
                                }
                                break;

                            case "l":
                                {
                                    nextVertsMakeNew = true;

                                    var format = ParseIndices(splitline,
                                        out var v, out var t, out var n,
                                        currentObject.VertsCount, currentObject.TexCoordsCount, currentObject.NormalsCount,
                                        currentGroup.Format);

                                    if (currentGroup.Format == WaveVertexFormat.None) currentGroup.Format = format;

                                    if (v != null && v.Length > 0) currentGroup.indexV.AddRange(v);
                                    if (t != null && t.Length > 0) currentGroup.indexT.AddRange(t);
                                    if (n != null && n.Length > 0) currentGroup.indexN.AddRange(n);

                                    if (v == null || v.Length < 2) throw new InconsistentDataException("Line polygon require at least two vertex", reader.LineCounter);

                                    var topology = v.Length > 2 ? WavePrimitive.LineStrip : WavePrimitive.Line;

                                    if (currentGroup.Topology == WavePrimitive.Unknow) currentGroup.Topology = topology;
                                    if (topology != currentGroup.Topology) throw new NotSupportedException("All WaveGroup must have the same Topology of current WaveObject");
                                    //currentGroup.AddPolygon(ParsePolygon(data));
                                }
                                break;


                            case "f":
                                {
                                    //define the end of vertex section
                                    nextVertsMakeNew = true;

                                    //i don't know if it's a triangle list or triangle strip yet
                                    var format = ParseIndices(splitline,
                                        out var v, out var t, out var n,
                                        currentObject.VertsCount, currentObject.TexCoordsCount, currentObject.NormalsCount,
                                        currentGroup.Format);

                                    if (currentGroup.Format == WaveVertexFormat.None) currentGroup.Format = format;

                                    if (v == null || v.Length < 3) throw new InconsistentDataException("Face polygon require at least three Vertices", reader.LineCounter);
                                    var topology = v.Length > 3 ? WavePrimitive.TriangleFan : WavePrimitive.Triangle;

                                    if (topology == WavePrimitive.TriangleFan)
                                    {
                                        if (v != null && v.Length > 0) for (int i = 0; i < v.Length - 1; i++) { currentGroup.indexV.Add(v[0]); currentGroup.indexV.Add(v[i]); currentGroup.indexV.Add(v[i + 1]); }
                                        if (t != null && t.Length > 0) for (int i = 0; i < t.Length - 1; i++) { currentGroup.indexT.Add(t[0]); currentGroup.indexT.Add(t[i]); currentGroup.indexT.Add(t[i + 1]); }
                                        if (n != null && n.Length > 0) for (int i = 0; i < n.Length - 1; i++) { currentGroup.indexN.Add(n[0]); currentGroup.indexN.Add(n[i]); currentGroup.indexN.Add(n[i + 1]); }
                                        topology = WavePrimitive.Triangle;
                                    }
                                    else
                                    {
                                        //index are set as local reference to avoid remap,
                                        //but i will know if they are in global reference only at the end of reading, i think...
                                        if (v != null && v.Length > 0) currentGroup.indexV.AddRange(v);
                                        if (t != null && t.Length > 0) currentGroup.indexT.AddRange(t);
                                        if (n != null && n.Length > 0) currentGroup.indexN.AddRange(n);
                                    }

                                    if (currentGroup.Topology == WavePrimitive.Unknow) currentGroup.Topology = topology;
                                    if (topology != currentGroup.Topology) throw new NotSupportedException("All WaveGroup must have the same Topology of current WaveObject");


                                }
                                break;

                            // Materials
                            case "mtllib":
                                MaterialFilename = splitline[1];
                                break;

                            case "usemtl":
                                currentGroup.Material = splitline[1];
                                break;

                            #region Not implemented
                            case "s":
                                // remeber that off is equal to 0, so scale it to common zero-based array indices
                                int smoothingroup = splitline[1] == "off" ? -1 : ParseInteger(splitline[1]) - 1;
                                break;

                            // Grouping
                            case "mg": // merging group
#if DEBUG
                            // Display/render attributes
                            case "usemap": // texture map name
                            case "bevel": // bevel interpolation
                            case "c_interp": // color interpolation
                            case "d_interp": // dissolve interpolation
                            case "lod": // level of detail
                            case "shadow_obj": // shadow casting
                            case "trace_obj": // ray tracing
                            case "ctech": // curve approximation technique
                            case "stech": // surface approximation technique

                            // Vertex data
                            case "vp": // parameter space vertices
                            case "cstype": // rational or non-rational forms of curve or surface type: basis matrix, Bezier, B-spline, Cardinal, Taylor
                            case "degree": // degree
                            case "bmat": // basis matrix
                            case "step": // step size

                            // Elements
                            case "curv": // curve
                            case "curv2": // 2D curve
                            case "surf": // surface

                            // Free-form curve/surface body statements
                            case "parm": // parameter name
                            case "trim": // outer trimming loop (trim)
                            case "hole": // inner trimming loop (hole)
                            case "scrv": // special curve (scrv)
                            case "sp":  // special point (sp)
                            case "end": // end statement (end)

                            // Connectivity between free-form surfaces
                            case "con": // connect
#endif
                                #endregion
                                break;
                        }
                    }

                    //check global or local indices's reference difference
                    foreach (var obj in Objects)
                    {
                        obj.UpdateMinMaxIndexValue();
                        UseGlobalShared |= obj.isMinIndexOutOfLocalRange;

                        if (string.IsNullOrEmpty(obj.Name)) obj.Name = "Object" + ObjectCount++;

                        foreach(var grp in obj.Groups)
                            if (string.IsNullOrEmpty(grp.Name)) grp.Name = "Group" + GroupCount++;

                    }
                    if (Objects.Count == 1) UseGlobalShared = true;


                    //found material file, try to load it. The file must be in the same directory
                    if (MaterialFilename != null)
                    {
                        MaterialFilename = Path.Combine(Path.GetDirectoryName(Filename), MaterialFilename);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR AT ROW " + reader.LineCounter);
                    Debug.WriteLine(e.Message);
                    return false;
                }
            }
            currentObject = null;
            currentGroup = null;
            return true;
        }


        #region ReadWriter
        protected override void WriteTitle(StreamWriter writer)
        {
            writer.WriteLine("# Johnwhile Wavefront OBJ exporter v.2");
            writer.WriteLine("# Created : " + DateTime.Now.ToString("dd-MM-yyyy", DotCulture));
        }
        protected override bool WriteRest(StreamWriter writer)
        {
            // if you assign a wave material file, is necessary to add the correct name

            if (MaterialFilename == null)
            {
                //it must have the same filename
                //MaterialFilename = ((FileStream)writer.BaseStream).Name;
                //MaterialFilename = Path.GetFileNameWithoutExtension(MaterialFilename) + MaterialLib.Extension;
            }
            
            if (MaterialFilename != null)
            {
                if (MaterialFilename.Contains(" "))
                {
                    Debugg.Warning("replace white space in material's filename with underscore because some programms dont accept it");
                    MaterialFilename = MaterialFilename.Replace(' ', '_');
                }
                MaterialFilename = Path.GetFileNameWithoutExtension(MaterialFilename) + WavefrontMat.Ext;

                writer.WriteLine("\nmtllib " + MaterialFilename);
            }
            foreach (var wobj in Objects)
                if (!wobj.Write(writer)) return false;
            return true;
        }
        
        #endregion

        public override string ToString() => $"{base.ToString()} Objects:{Objects.Count}";
        
    }
}
