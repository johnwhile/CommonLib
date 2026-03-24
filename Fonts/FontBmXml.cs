
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using Common.Maths;

namespace Common.Fonts
{
    /// <summary>
    /// <inheritdoc cref="BmFont"/>
    /// <para><b>Xml</b> format of *.fnt files</para>
    /// </summary>
    public class FontBmXml
    {
        //public xmlFont font;

        public Font font;

        public FontBmXml(FileStream file, FontDictionary dictionary)
        {
            //var serializer = XmlSerializer.FromTypes(new[] { typeof(xmlFont) })[0];
            //https://stackoverflow.com/questions/1127431/xmlserializer-giving-filenotfoundexception-at-constructor
            //var serializer = new XmlSerializer(typeof(xmlFont)); 
            //font = (xmlFont)serializer.Deserialize(file);

            using (XmlTextReader reader = new XmlTextReader(file))
            {
                while (reader.Read() && reader.IsStartElement())
                {
                    if (reader.Name == "font")
                    {
                        font = new Font();
                        if (!font.Read(reader, dictionary)) break;
                    }
                }
            }

        }

        public class Font
        {
            public Info Info;
            public Common Common;
            public List<Page> Pages;

            public bool Read(XmlTextReader reader, FontDictionary dictionary)
            {
                while (reader.Read() && reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "info":
                            Info = new Info(); if (!Info.Read(reader)) return false;
                            break;
                        case "common":
                            Common = new Common(); if (!Common.Read(reader)) return false;
                            break;
                        case "pages":
                            Pages = new List<Page>();
                            while (reader.Read() && reader.IsStartElement() && reader.Name == "page")
                            {
                                var page = new Page();
                                if (!page.Read(reader)) return false;
                                Pages.Add(page);
                            }
                            Pages.Sort((a, b) => a.Id.CompareTo(b.Id));
                            break;
                        case "chars":
                            //optional: we can get array capacity
                            if (int.TryParse(reader.GetAttribute("m_count"), out int count) && count > 1) { }

                            dictionary.IdToIndex = new Dictionary<ushort, int>(count);
                            dictionary.RawGlyphs = new List<Glyph>(count);
                            dictionary.RawGlyphs.Add(Glyph.Null);
                            dictionary.IdToIndex.Add(0, 0);

                            while (reader.Read() && reader.IsStartElement() && reader.Name == "char")
                            {
                                var chr = new Char();
                                if (!chr.Read(reader)) return false;

                                var glyph = chr.ConvertToMyGlyph();

                                if (glyph.IsValidGlyph)
                                    if (dictionary.IdToIndex.ContainsKey(glyph.id))
                                    {
                                        Debugg.Error("Duplicate char id in xml font file at row " + reader.LineNumber);
                                    }
                                    else
                                    {
                                        dictionary.IdToIndex.Add(glyph.id, dictionary.RawGlyphs.Count);
                                        dictionary.RawGlyphs.Add(glyph);
                                    }
                            }
                            break;
                    }
                }
                if (Pages == null) Pages = new List<Page>(0);

                return true;
            }
        }

        #region structures
        public struct Info
        {
            public string Face;
            public int Size;
            public int Bold;
            public int Italic;
            public string Charset;
            public int Unicode;
            public int StretchH;
            public int Smooth;
            public int Aa;
            public Vector4i Padding;
            public Vector2i Spacing;
            public int Outline;

            public bool Read(XmlTextReader reader)
            {
                Face = reader.GetAttribute("Face");
                Charset = reader.GetAttribute("charset");
                int.TryParse(reader.GetAttribute("Size"), out Size);
                int.TryParse(reader.GetAttribute("bold"), out Bold);
                int.TryParse(reader.GetAttribute("italic"), out Italic);
                int.TryParse(reader.GetAttribute("unicode"), out Unicode);
                int.TryParse(reader.GetAttribute("stretchH"), out StretchH);
                int.TryParse(reader.GetAttribute("smooth"), out Smooth);
                int.TryParse(reader.GetAttribute("aa"), out Aa);
                int.TryParse(reader.GetAttribute("outline"), out Outline);
                Vector4i.TryParse(reader.GetAttribute("padding"), out Padding);
                Vector2i.TryParse(reader.GetAttribute("spacing"), out Spacing);
                return true;
            }

            public override string ToString()
            {
                return Face;
            }
        }
        public struct Common
        {
            public int LineHeight;
            public int Base;
            public int ScaleW;
            public int ScaleH;
            public int Pages;
            public int Packed;
            public int AlphaChnl;
            public int RedChnl;
            public int GreenChnl;
            public int BlueChnl;
            public bool Read(XmlTextReader reader)
            {
                int.TryParse(reader.GetAttribute("lineHeight"), out LineHeight);
                int.TryParse(reader.GetAttribute("base"), out Base);
                int.TryParse(reader.GetAttribute("scaleW"), out ScaleW);
                int.TryParse(reader.GetAttribute("scaleH"), out ScaleH);
                int.TryParse(reader.GetAttribute("pages"), out Pages);
                int.TryParse(reader.GetAttribute("packed"), out Packed);
                int.TryParse(reader.GetAttribute("alphaChnl"), out AlphaChnl);
                int.TryParse(reader.GetAttribute("redChnl"), out RedChnl);
                int.TryParse(reader.GetAttribute("greenChnl"), out GreenChnl);
                int.TryParse(reader.GetAttribute("blueChnl"), out BlueChnl);
                return true;
            }

            public override string ToString()
            {
                return $"H:{LineHeight}, B{Base}";
            }
        }
        public struct Page
        {
            public int Id;
            public string Filename;
            public bool Read(XmlTextReader reader)
            {
                int.TryParse(reader.GetAttribute("id"), out Id);
                Filename = reader.GetAttribute("file");
                return true;
            }
            public override string ToString()
            {
                return Filename;
            }
        }
        public struct Char
        {
            public int Id;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int Xoffset;
            public int Yoffset;
            public int Xadvance;
            public int Page;
            public int Chnl;
            public bool Read(XmlTextReader reader)
            {
                int.TryParse(reader.GetAttribute("id"), out Id);
                int.TryParse(reader.GetAttribute("x"), out X);
                int.TryParse(reader.GetAttribute("y"), out Y);
                int.TryParse(reader.GetAttribute("width"), out Width);
                int.TryParse(reader.GetAttribute("height"), out Height);
                int.TryParse(reader.GetAttribute("xoffset"), out Xoffset);
                int.TryParse(reader.GetAttribute("yoffset"), out Yoffset);
                int.TryParse(reader.GetAttribute("xadvance"), out Xadvance);
                int.TryParse(reader.GetAttribute("page"), out Page);
                int.TryParse(reader.GetAttribute("chnl"), out Chnl);
                return true;
            }
            public override string ToString()
            {
                return char.ConvertFromUtf32(Id);
            }

            public Glyph ConvertToMyGlyph()
            {
                if (Id < -1) return Glyph.Null;
                Glyph glyph = new Glyph();
                glyph.id = Id < 0 ? ushort.MinValue : (ushort)Id;
                glyph.x = Convert.ToInt16(X);
                glyph.y = Convert.ToInt16(Y);
                glyph.xadvance = Convert.ToInt16(Xadvance);
                glyph.width = Convert.ToInt16(Width);
                glyph.height = Convert.ToInt16(Height);
                glyph.xoffset = Convert.ToInt16(Xoffset);
                glyph.yoffset = Convert.ToInt16(Yoffset);
                return glyph;
            }

        }
        #endregion

        #region xml serializer structures
        /*
        [XmlRoot("font")]
        public class xmlFont
        {
            [XmlElement("info")]
            public xmlInfo Info { get; set; }
            [XmlElement("common")]
            public xmlCommon Common { get; set; }
            [XmlArray("pages")]
            [XmlArrayItem("page")]
            public xmlPage[] Pages { get; set; }
            [XmlArray("chars")]
            [XmlArrayItem("char")]
            public xmlChar[] Chars { get; set; }
            [XmlArray("kernings")]
            [XmlArrayItem("kerning")]
            public xmlKerning[] Kernings { get; set; }
        }
        public class xmlInfo
        {
            [XmlAttribute("Face")]
            public string Face { get; set; }
            [XmlAttribute("Size")]
            public Int32 Size { get; set; }
            [XmlAttribute("bold")]
            public Int32 Bold { get; set; }
            [XmlAttribute("italic")]
            public Int32 Italic { get; set; }
            [XmlAttribute("charset")]
            public string CharSet { get; set; }
            [XmlAttribute("stretchH")]
            public Int32 StretchH { get; set; }
            [XmlAttribute("smooth")]
            public Int32 Smooth { get; set; }
            [XmlAttribute("aa")]
            public Int32 SuperSampling { get; set; }
            [XmlAttribute("padding")]
            public string Padding { get; set; }
            [XmlAttribute("spacing")]
            public string Spacing { get; set; }
            [XmlAttribute("outline")]
            public int Outline { get; set; }
        }
        public class xmlCommon
        {
            [XmlAttribute("lineHeight")]
            public Int32 LineHeight { get; set; }
            [XmlAttribute("base")]
            public Int32 Base { get; set; }
            [XmlAttribute("scaleW")]
            public Int32 ScaleW { get; set; }
            [XmlAttribute("scaleH")]
            public Int32 ScaleH { get; set; }
            [XmlAttribute("pages")]
            public Int32 Pages { get; set; }
            [XmlAttribute("packed")]
            public Int32 Packed { get; set; }
            [XmlAttribute("alphaChnl")]
            public Int32 AlphaChnl { get; set; }
            [XmlAttribute("redChnl")]
            public Int32 RedChnl { get; set; }
            [XmlAttribute("blueChnl")]
            public Int32 BlueChnl { get; set; }
        }
        public class xmlPage
        {
            [XmlAttribute("id")]
            public Int32 Id { get; set; }
            [XmlAttribute("file")]
            public string File { get; set; }
        }
        public class xmlChar
        {
            [XmlAttribute("id")]
            public Int32 Id { get; set; }
            [XmlAttribute("x")]
            public Int32 X { get; set; }

            [XmlAttribute("y")]
            public Int32 Y { get; set; }

            [XmlAttribute("width")]
            public Int32 Width { get; set; }

            [XmlAttribute("height")]
            public Int32 Height { get; set; }

            [XmlAttribute("xoffset")]
            public Int32 XOffset { get; set; }
            [XmlAttribute("yoffset")]
            public Int32 YOffset { get; set; }
            [XmlAttribute("xadvance")]
            public Int32 XAdvance { get; set; }
            [XmlAttribute("page")]
            public Int32 Page { get; set; }
            [XmlAttribute("chnl")]
            public Int32 Channel { get; set; }

            public Glyph ConvertToMyGlyph()
            {
                if (Id < -1) return Glyph.Null;
                Glyph glyph = new Glyph();
                glyph.id = Id < 0 ? ushort.MinValue : (ushort)Id;
                glyph.x = Convert.ToInt16(X);
                glyph.y = Convert.ToInt16(Y);
                glyph.xadvance = Convert.ToInt16(XAdvance);
                glyph.width = Convert.ToInt16(Width);
                glyph.height = Convert.ToInt16(Height);
                glyph.xoffset = Convert.ToInt16(XOffset);
                glyph.yoffset = Convert.ToInt16(YOffset);
                return glyph;
            }
        }
        public class xmlKerning
        {
            [XmlAttribute("first")]
            public Int32 First { get; set; }
            [XmlAttribute("second")]
            public Int32 Second { get; set; }
            [XmlAttribute("amount")]
            public Int32 Amount { get; set; }
        }
        */
        #endregion
    }
}
