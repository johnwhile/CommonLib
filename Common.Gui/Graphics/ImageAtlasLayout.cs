
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Common.Maths;

namespace Common.Gui
{
    /// <summary>
    /// Load the texture atlas definition, currently can store "only" 255 sources rectangles.<br/>
    /// The single source in the texture atlas is defined by a unique number, previously defined by the user.
    /// </summary>
    public class ImageAtlasLayout
    {
        /// <summary>
        /// path of this xml layout (full-path)
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// paths of textures (full-path), it's advisable to use only one texture for optimization
        /// </summary>
        public List<string> ImageFilename;


        public TypeCodeEnumerator TypeCodes=>new TypeCodeEnumerator(this);


        Rectangle4i[] sources;
        sbyte[] indices;
        string[] names;


        private ImageAtlasLayout()
        {
            sources = new Rectangle4i[255];
            indices = new sbyte[255];
            names = new string[255];
            for (int i = 0; i < 255; i++) indices[i] = -1;
        }

        /// <summary>
        /// Read the layout file with xml extension, return null if error
        /// </summary>
        public static ImageAtlasLayout Open(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"File: {filename} doesn't exit");
            var layout = new ImageAtlasLayout();
            layout.Filename = Path.GetFullPath(filename);
            if (!layout.Read(layout.Filename)) return null;
            return layout;
        }

        /// <summary>
        /// Try to get the source rectangle defined byte <paramref name="typecode"/> value in the texture atlas.<br/>
        /// The <paramref name="typecode"/> is a custom number defined by user.
        /// </summary>
        /// <param name="source">the texture coords</param>
        /// <param name="imageIndex">the texture as index</param>
        public bool TryGetSource(byte typecode, out Rectangle4i source, out int imageIndex, out string name)
        {
            source = sources[typecode];
            imageIndex = indices[typecode];
            name = names[typecode];
            return imageIndex >= 0 && !source.IsEmpty;
        }

        /// <summary>
        /// Save to file the texture atlas layout
        /// </summary>
        /// <param name="filename">extension will be set to *.xml</param>
        public bool Save(string filename)
        {
            filename = Path.Combine(
                Path.GetDirectoryName(filename),
                Path.GetFileNameWithoutExtension(filename) + ".xml");
            try
            {
                XmlWriterSettings setting = new XmlWriterSettings() { Indent = true };

                using (XmlWriter writer = XmlWriter.Create(filename, setting))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("layout");

                    bool missing = false;

                    for (int i = 0; i < ImageFilename.Count; i++)
                    {
                        writer.WriteStartElement("image");
                        string file = Path.GetFileName(ImageFilename[i]);
                        writer.WriteAttributeString("File", file);

                        for (byte typecode = 0; typecode < 255; typecode++)
                        {
                            if (indices[typecode] == i)
                            {
                                writer.WriteStartElement("coord");
                                writer.WriteAttributeString("source", sources[typecode].ToString());
                                writer.WriteAttributeString("type", typecode.ToString());
                                writer.WriteAttributeString("name", names[typecode]);
                                writer.WriteEndElement();
                            }
                            missing |= indices[typecode] < 0;
                        }
                        writer.WriteEndElement();
                    }

                    if (missing)
                    {
                        writer.WriteStartElement("image_missing");
                        for (byte typecode = 0; typecode < 255; typecode++)
                        {
                            if (indices[typecode] < 0)
                            {
                                writer.WriteStartElement("icon");
                                writer.WriteAttributeString("type", typecode.ToString());
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch(Exception e)
            {
                Debugg.Error("error saving xml layout: " + e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// read the xml file
        /// </summary>
        private bool Read(string filename)
        {
            try
            {
                string dir = Path.GetDirectoryName(filename);
                using (FileStream file = File.OpenRead(filename))
                using (XmlReader reader = XmlReader.Create(file))
                {
                    ImageFilename = new List<string>();

                    Rectangle4i source = default(Rectangle4i);
                    byte typecode = 0;
                    sbyte index = -1;
                    string name = "";

                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "image":
                                while (reader.MoveToNextAttribute())
                                    if (reader.Name == "File")
                                    {
                                        index = (sbyte)ImageFilename.IndexOf(reader.Value);
                                        if (index < 0)
                                        {
                                            index = (sbyte)ImageFilename.Count;
                                            ImageFilename.Add(Path.Combine(dir, reader.Value));
                                        }
                                    }
                                break;

                            case "coord":
                                bool result = false;
                                while (reader.MoveToNextAttribute())
                                {
                                    switch (reader.Name)
                                    {
                                        case "source": result |= Rectangle4i.TryParse(reader.Value, out source); break;
                                        case "type": result |= byte.TryParse(reader.Value, out typecode); break;
                                        case "name": name = reader.Value; break;
                                    }
                                }
                                if (result && typecode > 0)
                                {
                                    sources[typecode] = source;
                                    indices[typecode] = index;
                                    names[typecode] = name;
                                }
                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debugg.Message("error reading xml layout: " + e.Message);
                return false;
            }
            return true;
        }


        public class TypeCodeEnumerator
        {
            ImageAtlasLayout owner;

            internal TypeCodeEnumerator(ImageAtlasLayout owner)
            {
                this.owner = owner;
            }
            public IEnumerator<byte> GetEnumerator() => new Enumerator(owner);
            
            public struct Enumerator : IEnumerator<byte>, IEnumerator
            {
                ImageAtlasLayout owner;
                int typecode;
                byte current;

                internal Enumerator(ImageAtlasLayout owner)
                {
                    this.owner = owner;
                    typecode = -1;
                    current = 0;
                }

                public byte Current => current;
                object IEnumerator.Current => current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    while (++typecode < owner.sources.Length)
                    {
                        if (owner.indices[typecode] >= 0)
                        {
                            current = (byte)typecode;
                            return true;
                        }
                    }
                    return false;
                }

                public void Reset()
                {
                    typecode = -1;
                }
            }
        }


    }
}
