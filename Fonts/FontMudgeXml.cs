using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Common.Fonts
{
    /// <summary>
    /// http://www.larryhastings.com/programming/mudgefont/
    /// </summary>
    public class FontMudgeXml
    {
        public FontMudgeXml(FileStream file, FontDictionary dictionary)
        {
            dictionary.IdToIndex = new Dictionary<ushort, int>();
            dictionary.RawGlyphs = new List<Glyph>();

            using (XmlTextReader reader = new XmlTextReader(file))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "dictionary": break;
                                case "char":
                                    Glyph glyph = new Glyph();
                                    while (reader.MoveToNextAttribute())
                                    {
                                        switch (reader.Name)
                                        {
                                            case "id": if (!ushort.TryParse(reader.Value, out glyph.id)) glyph.id = 0; break;
                                            case "x": short.TryParse(reader.Value, out glyph.x); break;
                                            case "y": short.TryParse(reader.Value, out glyph.y); break;
                                            case "width": short.TryParse(reader.Value, out glyph.width); break;
                                            case "height": short.TryParse(reader.Value, out glyph.height); break;
                                        }
                                    }
                                    if (!dictionary.IdToIndex.ContainsKey(glyph.id))
                                    {
                                        //add missing informations
                                        glyph.xadvance = glyph.width;
                                        dictionary.LineHeight = glyph.height;
                                        dictionary.IdToIndex.Add(glyph.id, dictionary.RawGlyphs.Count);
                                        dictionary.RawGlyphs.Add(glyph);
                                    }
                                    break;

                                case "spacing": break; //not used
                            }
                            break;
                        case XmlNodeType.EndElement: break;
                    }
                }
            }
        }
    }
}
