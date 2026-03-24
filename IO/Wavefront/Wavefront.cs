using Common.Maths;
using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace Common.IO.Wavefront
{

    public abstract class Wavefront
    {
        protected static CultureInfo DotCulture = new CultureInfo("en-US");
        protected static char[] spaceSeparator = new char[] { ' ', '\t' };
        protected static char[] slashSeparator = new char[] { '/' };

        static int fdigit = -1;
        static string fformat = "g";

        /// <summary>
        /// since the file is a text file, in some cases it's necessary to display floats numbers with low precision
        /// </summary>
        public static int NumberDecimalDigits
        {
            get { return fdigit; }
            set
            { 
                fdigit = value;
                DotCulture.NumberFormat.NumberDecimalDigits = Mathelp.CLAMP(fdigit, 0, 99);

                if (fdigit < 0) fformat = "g";
                else if (fdigit == 0) fformat = "0";
                else
                {
                    fformat = "0.";
                    for (int i = 0; i < fdigit; i++) fformat += "0";
                }
            }
        }

        public string Status = "OK";

        /// <summary>
        /// Is full filename
        /// </summary>
        public string Filename { get; internal set; }
        public abstract string Extension { get; }

        internal static float ParseFloat(string value) => float.TryParse(value, NumberStyles.Float, DotCulture, out float f) ? f : float.NaN;
        /// <summary>
        /// Wavefront format use no-zero based indices so zero mean not found
        /// </summary>
        internal static int ParseInteger(string value) => int.TryParse(value, NumberStyles.Float, DotCulture, out int i) ? i : 0;
        internal static string StringVector3(Vector3f v)
        {
            if (fdigit > 0)
            {
                v.x = (float)Math.Round(v.x, fdigit);
                v.y = (float)Math.Round(v.y, fdigit);
                v.z = (float)Math.Round(v.z, fdigit);
            }
            return $"{v.x.ToString(fformat, DotCulture)}\t{v.y.ToString(fformat, DotCulture)}\t{v.z.ToString(fformat, DotCulture)}";
        }
        internal static string StringVector2(Vector2f v)
        {
            if (fdigit > 0)
            {
                v.x = (float)Math.Round(v.x, fdigit);
                v.y = (float)Math.Round(v.y, fdigit);
            }
            return $"{v.x.ToString(fformat, DotCulture)}\t{v.y.ToString(fformat, DotCulture)}";
        }
        internal static string StringColor(Color4b c, bool alpha = true)
        {
            return 
                $"{c.fR.ToString(fformat, DotCulture)}\t" +
                $"{c.fG.ToString(fformat, DotCulture)}\t" +
                $"{c.fB.ToString(fformat, DotCulture)}" + 
                (alpha ? $"\t{c.fA.ToString(fformat, DotCulture)}" : "");
        }
        internal static Vector4f ParseVector4(string[] line)
            => new Vector4f(ParseFloat(line[1]), ParseFloat(line[2]), ParseFloat(line[3]), ParseFloat(line[4]));
        internal static Vector3f ParseVector3(string[] line)
            => new Vector3f(ParseFloat(line[1]), ParseFloat(line[2]), ParseFloat(line[3]));
        internal static Vector2f ParseVector2(string[] line)
            => new Vector2f(ParseFloat(line[1]), ParseFloat(line[2]));

        /// <summary>
        /// Parse the indinces, the negative index mean not found.
        /// </summary>
        /// <param name="Vcount">require not-shared count for negative index support</param>
        /// <param name="Tcount">require not-shared count for negative index support</param>
        /// <param name="Ncount">require not-shared count for negative index support</param>
        /// <param name="mustmatch">the first index parsing of <see cref="WaveGroup"/> define the format for all other indices</param>
        /// <remarks>
        /// <i>The negative indices are re-remapped.</i>
        /// </remarks>
        protected static WaveVertexFormat ParseIndices(
            string[] line, 
            out int[] vertices, out int[] texcoords, out int[] normals, 
            int Vcount = 0, int Tcount = 0, int Ncount = 0,
            WaveVertexFormat mustmatch = WaveVertexFormat.None)
        {
            vertices = null;
            texcoords = null;
            normals = null;
            int size = line.Length - 1;
            string[] slashed;

            if (mustmatch == WaveVertexFormat.None)
            {
                //make a test sample
                slashed = line[1].Split(slashSeparator, StringSplitOptions.None);
                mustmatch = WaveVertexFormat.Vertex;
                if (slashed.Length > 1 && !string.IsNullOrWhiteSpace(slashed[1])) mustmatch |= WaveVertexFormat.TexCoord; //case v\vt
                if (slashed.Length > 2 && !string.IsNullOrWhiteSpace(slashed[2])) mustmatch |= WaveVertexFormat.Normal; //case v\vt\vn and v\\vn
            }

            switch(mustmatch)
            {
                case WaveVertexFormat.Vertex:
                    vertices = new int[size];
                    for (int i = 0; i < size; i++)
                        vertices[i] = Remap(ParseInteger(line[i + 1]), Vcount);
                    break;

                case WaveVertexFormat.VertexTexcoord:
                    vertices = new int[size];
                    texcoords = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        slashed = line[i+1].Split(slashSeparator, StringSplitOptions.None);
                        vertices[i] = Remap(ParseInteger(slashed[0]), Vcount);
                        texcoords[i] = Remap(ParseInteger(slashed[1]), Tcount);
                    } 
                    break;

                case WaveVertexFormat.VertexTexcoordNormal:
                    vertices = new int[size];
                    texcoords = new int[size];
                    normals = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        slashed = line[i + 1].Split(slashSeparator, StringSplitOptions.None);
                        vertices[i] = Remap(ParseInteger(slashed[0]), Vcount);
                        texcoords[i] = Remap(ParseInteger(slashed[1]), Tcount);
                        normals[i] = Remap(ParseInteger(slashed[2]), Ncount);
                    }
                    break;

                case WaveVertexFormat.VertexNormal:
                    vertices = new int[size];
                    normals = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        slashed = line[i + 1].Split(slashSeparator, StringSplitOptions.None);
                        vertices[i] = Remap(ParseInteger(slashed[0]), Vcount);
                        normals[i] = Remap(ParseInteger(slashed[2]), Ncount);
                    }
                    break;
            }

            return mustmatch;
        }

        /// <summary>
        /// Convert to most common not-zero base index. -1 for not valid index
        /// </summary>
        /// <remarks><i>Wavefront can use negative: the polygon
        /// <b>f -4 -3 -2 -1</b> will be remapped to <b>f 1 2 3 4</b></i> 
        /// </remarks>
        /// <param name="count">number of not shared vertices used to calculate negative index</param>
        static int Remap(int idx, int count)
        {
            if (idx > 0) return idx - 1;
            else if (idx < 0) return count + idx;
            return -1;
        }

        /// <summary>
        /// Load data from a file, return null if something wrong.
        /// If wavefront OBJ contain a material lib, it load also the relative file and it store the material
        /// in <see cref="WavefrontObj.MaterialLib"/>
        /// </summary>
        protected static T Load<T>(string filename) where T : Wavefront , new()
        {
            // isGlobalRef is false because i don't know before completly read
            T wave = new T();

            if (!File.Exists(filename))
                throw new FileNotFoundException(string.Format("the file \"{0}\" not exists", filename));

            wave.Filename = Path.GetFullPath(filename);

            using (var file = File.OpenRead(filename))
            {
                Debugg.Message("Wavefront : loading " + Path.GetFileName(filename));
                if (!wave.Read(file)) return null;
            }

            return wave;
        }
        /// <summary>
        /// <see cref="WavefrontObj.Read(Stream)"/><br/>
        /// <see cref="WavefrontMat.Read(Stream)"/>
        /// </summary>
        protected abstract bool Read(Stream stream);
        protected abstract void WriteTitle(StreamWriter writer);
        protected abstract bool WriteRest(StreamWriter writer);

        /// <summary>
        /// Save .obj to a stream
        /// </summary>
        public bool Save(Stream stream)
        {
            if (!stream.CanWrite)
            {
                Debug.WriteLine("Can not write to steam");
                return false;
            }

            using (StreamWriter writer = new StreamWriter(stream))
            {
                WriteTitle(writer);
                if (!WriteRest(writer)) return false;
            }
            Debugg.Message("Wavefront : saved " + Path.GetFileName(Filename));
            return true;
        }
        /// <summary>
        /// Save or overwrite .obj to a file. File extension will be set = *.obj.
        /// </summary>
        public virtual bool Save(string filename)
        {
            string fixedname = Path.GetFileNameWithoutExtension(filename) + Extension;
            filename = Path.Combine(Path.GetDirectoryName(filename), fixedname);
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                Filename = fixedname;
                return Save(file);
            }
        }

        public override string ToString()
        {
            return Path.GetFileName(Filename);
        }

    }
}
