using Common.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Common.IO.Wavefront
{
    [DebuggerDisplay("{ToString()}")]
    public class WavefrontMat : Wavefront, IEnumerable<WaveMaterial>
    {
        public static string Ext = ".mtl";
        public override string Extension => Ext;

        internal List<WaveMaterial> m_materials;
        internal Dictionary<string, int> m_nameToIndex;

        
        public WavefrontMat()
        {
            m_materials = new List<WaveMaterial>();
            m_nameToIndex = new Dictionary<string, int>();
        }

        public WaveMaterial TryGetByName(string name)
        {
            if (m_nameToIndex.TryGetValue(name, out int position))
                if (position < m_materials.Count)
                    return m_materials[position];
            return null;
        }
        public WaveMaterial TryGetByIndex(int index)
        {
            if (index < m_materials.Count)
                return m_materials[index];
            return null;
        }
        public string TryGetNameByIndex(int index)
        {
            var mat = TryGetByIndex(index);
            return mat != null ? mat.Name : null;
        }

        /// <summary>
        /// Name must be unique
        /// </summary>
        public WaveMaterial this[string name]
        {
            get => TryGetByName(name);
            set
            {
                if (!m_nameToIndex.ContainsKey(name))
                {
                    m_nameToIndex.Add(name, m_materials.Count);
                    m_materials.Add(value);
                }
                else throw new ArgumentException("material name already in use");
            }
        }

        public void Remove(string name)
        {
            if (m_nameToIndex.TryGetValue(name, out int position))
            {
                m_nameToIndex.Remove(name);
                m_materials.RemoveAt(position);
            }
        }

        public WaveMaterial Create(string name = null, Vector3f? diffuse = null)
        {
            WaveMaterial material = new WaveMaterial(this, name, diffuse);
            this[name ?? material.Name] = material;
            return material;
        }

        /// <summary>
        /// Save or overwrite .mtl to a file. Attention : some programs do not accept spaces to filename
        /// </summary>
        public override bool Save(string filename)
        {
            string fixedname = Path.GetFileNameWithoutExtension(filename).Replace(' ', '_');
            filename = Path.Combine(Path.GetDirectoryName(filename), fixedname + Extension);
            return base.Save(filename);
        }


        protected override void WriteTitle(StreamWriter writer)
        {
            writer.WriteLine("# Johnwhile Wavefront MTL exporter v.2");
            writer.WriteLine("# Created : " + DateTime.Now.ToString("dd-MM-yyyy", DotCulture));
        }

        protected override bool WriteRest(StreamWriter writer)
        {
            foreach (var mat in this)
            {
                if (!mat.IsUsed) continue;

                string name = mat.Name;
                if (mat.Name.Split(spaceSeparator).Length > 1)
                {
                    Debug.WriteLine("> the material name can't contain white space, use underscores instead");
                    name.Replace(' ', '_');
                    name.Replace('\t', '_');
                }

                writer.WriteLine("");
                writer.WriteLine("newmtl " + name);

                writer.Write("\tKa\t");
                writer.WriteLine(StringVector3(mat.Ambient));

                writer.Write("\tKd\t");
                writer.WriteLine(StringVector3(mat.Diffuse));

                writer.Write("\tKs\t");
                writer.WriteLine(StringVector3(mat.Specular));

                writer.Write("\td\t" + mat.Dissolve.ToString(DotCulture));
            }
            return true;
        }

        /// <summary>
        /// Load .obj from a filename
        /// </summary>
        public static WavefrontMat Load(string filename) => Load<WavefrontMat>(filename);
        protected override bool Read(Stream stream)
        {
            if (!stream.CanRead)
            {
                Debug.WriteLine("> stream report can's read from it");
                return false;
            }
            try
            {
                using (WavefontReader reader = new WavefontReader(stream))
                {
                    WaveMaterial currentMat = null;

                    while (reader.ReadNext(out var line))
                    {
                        switch (line[0])
                        {
                            case "newmtl":
                                currentMat = new WaveMaterial(this, line[1]);
                                break;

                            //Material color and illumination statements:
                            case "Ka": currentMat.Ambient= ParseVector3(line); break;
                            case "Kd": currentMat.Diffuse = ParseVector3(line); break;
                            case "Ks": currentMat.Specular = ParseVector3(line); break;
                            case "d": currentMat.Dissolve = ParseFloat(line[1]); break;
                            case "Tr": //int
                            case "Tf": // vector3i
                            case "illum"://int
                            case "sharpness":
                            case "Ns": //int
                            case "Ni":
                                break;
                        }
                    }
                }
            }
            catch
            {

            }
            return true;
        }

        public IEnumerator<WaveMaterial> GetEnumerator() => m_materials.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
