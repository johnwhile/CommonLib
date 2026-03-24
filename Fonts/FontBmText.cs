using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Fonts
{
    public class FontBmText
    {
        const int MAXCHARCOUNT = ushort.MaxValue;
        string[] REMOVECHAR = new string[] { "\"" };
        string[] SPLITCHARS = new string[] { " ", "\t", "=" };

        public BmTextInfo Info;
        public BmTextCommon Common;
        public List<string> Pages;

        public BmTextKerning[] Kernings;

        public FontBmText(FileStream file, FontDictionary dictionary)
        {
            using (var sr = new StreamReader(file, System.Text.Encoding.UTF8))
            {
                Pages = new List<string>();
                
                dictionary.IdToIndex = new Dictionary<ushort, int>();
                dictionary.RawGlyphs = new List<Glyph>();

                dictionary.RawGlyphs.Add(Glyph.Null);
                dictionary.IdToIndex.Add(0, 0);

                int line = 0;
                while (!sr.EndOfStream)
                {
                    line++;
                    string[] splitted = sr.ReadLine().Split(SPLITCHARS, StringSplitOptions.RemoveEmptyEntries);

                    if (splitted.Length < 1) continue;

                    string type = splitted[0].ToLower();

                    switch (type)
                    {
                        case "info":Info = new BmTextInfo(splitted);break;
                        case "common":Common = new BmTextCommon(splitted);break;
                        case "page": var pages = new BmTextPage(splitted); Pages.Add(pages.file); break;
                        case "chars": break;
                        case "char": 
                            var g = new BmTextGlyphs(splitted);
                            var glyph = g.ConvertToMyGlyph();
                            
                            if (glyph.IsValidGlyph)
                                if (dictionary.IdToIndex.ContainsKey(glyph.id))
                                {
                                    Debug.Print("Error, the char id: " + glyph.id + " already defined");
                                }
                                else
                                {
                                    dictionary.IdToIndex.Add(glyph.id, dictionary.RawGlyphs.Count);
                                    dictionary.RawGlyphs.Add(glyph);
                                }
                            break;

                        default: throw new BmFontException("unknow line type definition", line);
                    }
                }
            }

        }

        /// <summary>
        /// The font description
        /// </summary>
        public class BmTextInfo
        {
            //face="Arial" size=32 bold=0 italic=0 charset="" unicode=0 stretchH=100 smooth=1 aa=1 padding=8,8,8,8 spacing=0,0
            public string fontname;
            public ushort size;
            public bool bold;
            public bool italic;
            public string padding;

            public BmTextInfo(string[] segment)
            {
                for (int i = 1; i < segment.Length; i += 2)
                {
                    string name = segment[i].ToLower();
                    int boolean = 0;
                    switch (name)
                    {
                        case "Face": fontname = segment[i + 1].Replace("\"", string.Empty); break;
                        case "Size": if (!ushort.TryParse(segment[i + 1], out size)) size = 0; break;
                        case "bold": if (int.TryParse(segment[i + 1], out boolean)) bold = boolean > 0; break;
                        case "italic": if (int.TryParse(segment[i + 1], out boolean)) italic = boolean > 0; break;
                        case "padding": padding = segment[i + 1]; break;
                    }
                }
            }
        }
        /// <summary>
        /// Common textures values for each Glyphs. see "measures.png"
        /// </summary>
        public class BmTextCommon
        {
            //common lineHeight=53 base=29 scaleW=512 scaleH=512 pages=1 packed=0
            public ushort lineHeight;
            /// <summary>
            /// The base value is how far from the top of the cell height the base of the characters in the font should be placed.
            /// Characters can of course extend above or below this base line, which is entirely up to the font design.
            /// </summary>
            public ushort layoutBase;
            /// <summary>
            /// Textures width
            /// </summary>
            public ushort scaleW;
            /// <summary>
            /// Textures Height
            /// </summary>
            public ushort scaleH;
            public ushort pages;

            public BmTextCommon(string[] segment)
            {
                for (int i = 1; i < segment.Length; i += 2)
                {
                    string name = segment[i].ToLower();
                    switch (name)
                    {
                        case "lineheight": if (!ushort.TryParse(segment[i + 1], out lineHeight)) lineHeight = 0; break;
                        case "base": if (!ushort.TryParse(segment[i + 1], out layoutBase)) layoutBase = 0; break;
                        case "scalew": if (!ushort.TryParse(segment[i + 1], out scaleW)) scaleW = 0; break;
                        case "scaleh": if (!ushort.TryParse(segment[i + 1], out scaleH)) scaleH = 0; break;
                        case "pages": if (!ushort.TryParse(segment[i + 1], out pages)) pages = 0; break;
                    }
                }
            }
        }
        public class BmTextPage
        {
            
            public byte id;
            public string file;

            public BmTextPage(string[] segment)
            {
                //page id=0 file="Arial_distancefield.png"
                for (int i = 1; i < segment.Length; i += 2)
                {
                    string name = segment[i].ToLower();
                    switch (name)
                    {
                        case "id": if (!byte.TryParse(segment[i + 1], out id)) id = 0; break;
                        case "file": file = segment[i + 1].Replace("\"", string.Empty); break;
                    }
                }
            }
        }

        /// <summary>
        /// The char texture definition. Make sure that id match with Ascii table to avoid confusion
        /// </summary>
        public class BmTextGlyphs
        {
            public int id;
            /// <summary>
            /// The green rectangle illustrates the quad that should be copied from the texture to the screen when rendering the character.
            /// The width and height gives the size of this rectangle, and x and y gives the position of the rectangle in the texture.<br/>
            /// <b>I impose for convenience it can't be greater than <see cref="short.MaxValue"/></b>
            /// </summary>
            public short x;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public short y;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public short width;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public short height;
            /// <summary>
            /// The xoffset gives the horizontal offset that should be added to the cursor position to find the left position where the character should be drawn
            /// </summary>
            public short xoffset;
            /// <summary>
            /// The yoffset gives the distance from the top of the cell height to the top of the character
            /// </summary>
            public short yoffset;
            /// <summary>
            /// marks the position of the cursor after drawing the character
            /// </summary>
            public short xadvance;

            public BmTextGlyphs(string[] segment)
            {
                //char id=0   x=35   y=163  width=32   height=37   xoffset=-4   yoffset=0    xadvance=40   page=0    chnl=0 
                for (int i = 1; i < segment.Length; i += 2)
                {
                    string name = segment[i].ToLower();
                    switch (name)
                    {
                        case "id": if (!int.TryParse(segment[i + 1], out id)) id = 0; break;
                        case "x": if (!short.TryParse(segment[i + 1], out x)) x = 0; break;
                        case "y": if (!short.TryParse(segment[i + 1], out y)) y = 0; break;
                        case "width": if (!short.TryParse(segment[i + 1], out width)) width = 0; break;
                        case "height": if (!short.TryParse(segment[i + 1], out height)) height = 0; break;
                        case "xoffset": if (!short.TryParse(segment[i + 1], out xoffset)) xoffset = 0; break;
                        case "yoffset": if (!short.TryParse(segment[i + 1], out yoffset)) yoffset = 0; break;
                        case "xadvance": if (!short.TryParse(segment[i + 1], out xadvance)) xadvance = 0; break;
                    }
                }
            }

            public Glyph ConvertToMyGlyph()
            {
                if (id < -1) return Glyph.Null;
                Glyph glyph = new Glyph();
                glyph.id = id < 0 ? ushort.MinValue : (ushort)id;
                glyph.x = x;
                glyph.y = y;
                glyph.xadvance = xadvance;
                glyph.width = width;
                glyph.height = height;
                glyph.xoffset = xoffset;
                glyph.yoffset = yoffset;
                return glyph;
            }
        }
        /// <summary>
        /// I prefer not use the kerning pair
        /// </summary>
        public class BmTextKerning
        {
            public uint first;
            public uint second;
            public int amount;
        }
    }
}
