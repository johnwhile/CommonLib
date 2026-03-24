using System;
using System.Diagnostics;

using Common.Maths;

namespace Common.IO.Wavefront
{
    [DebuggerDisplay("{name}")]
    public class WaveMaterial
    {
        WavefrontMat file;
        string name = "";

        public bool IsUsed = true;

        /// <summary>
        /// Names may be any length but cannot include blanks, underscores may be used.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (file.m_nameToIndex.ContainsKey(value))
                    Debug.WriteLine("> name already in use");
                else
                {
                    WaveMaterial mat = file.TryGetByName(name);
                    if (mat!=null)
                    {
                        if (mat == this)
                        {
                            if (file.m_nameToIndex.TryGetValue(name, out int position))
                            {
                                file.m_nameToIndex.Remove(name);
                                mat.name = value;
                                file.m_nameToIndex.Add(value, position);
                            }
                        }
                        else
                            Debug.WriteLine("> this material reference is inconsistent");
                    }
                    else
                        Debug.WriteLine("> current material doesn't exit in file");
                }
            }
        }
        /// <summary>
        /// No alpha channel
        /// </summary>
        public Vector3f Ambient { get; set; }
        public Vector3f Diffuse { get; set; }
        public Vector3f Specular { get; set; }
        public float Dissolve { get; set; } = 1.0f;


        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="name">must be unique for currect WaveMatFile, if null the method generate one using GetHashCode</param>
        internal WaveMaterial(WavefrontMat file, string name, Vector3f? diffuse = null)
        {
            this.file = file;
            if (file == null) throw new ArgumentNullException("file can not be null");
            if (string.IsNullOrWhiteSpace(name))
                name = "material_" + GetHashCode().ToString();
            this.name = name;

            if (diffuse != null) Diffuse = (Vector3f)diffuse;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
