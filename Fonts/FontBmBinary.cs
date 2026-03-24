using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Common.Fonts
{
    public class FontBmBinary
    {
        const int MAXCHARCOUNT = ushort.MaxValue;
        /// <summary>
        /// Currently is 3
        /// </summary>
        private byte Version;
        /// <summary>
        /// </summary>
        public bmInfo Info;
        /// <summary>
        /// Contain the size of texture
        /// </summary>
        public bmCommon Common;

        //public bmGlyphs[] Chars;
        /// <summary>
        /// Not implemented
        /// </summary>
        private bmKerning[] Kernings;
        /// <summary>
        /// The textures filename. Usualy the best choice is to use only one texture. Must be in the same folder of *.fnt file
        /// </summary>
        public string[] PageNames;


        public FontBmBinary(FileStream file, FontDictionary dictionary)
        {
            using (var br = new BinaryReader(file))
            {
                string identifier = new string(br.ReadChars(3));
                if (identifier != "BMF") throw new BmFontException("It isn't a BMF file m_format");

                Version = br.ReadByte();

                while (br.BaseStream.Position < br.BaseStream.Length - 6) // where 6 is size of blockType + blockSize + 1
                {
                    byte blockType = br.ReadByte();
                    long blockSize = br.ReadUInt32();
                    long begin = br.BaseStream.Position;

                    if (begin + blockSize > br.BaseStream.Length)
                        throw new BmFontException("Can't read this block because pass ad end of file", begin);

                    try
                    {
                        switch (blockType)
                        {
                            case 1:
                                Info = new bmInfo(br, Version);
                                break;

                            case 2:
                                Common = br.ReadSafe<bmCommon>();
                                break;

                            case 3:
                                //can exist multiple textures with same length's name example from "font_0.png" to "font_9.png"
                                string firstpage = br.ReadStringToNull();
                                int numPages = (int)(blockSize / (firstpage.Length + 1));
                                PageNames = new string[numPages];
                                PageNames[0] = firstpage;
                                for (int i = 1; i < numPages; i++)
                                    PageNames[i] = br.ReadStringToNull();
                                break;

                            case 4:
                                int numChars = (int)(blockSize / bmGlyphs.SizeOf);
                                if (numChars < 1 || numChars > MAXCHARCOUNT) throw new ArgumentOutOfRangeException("BlockChars Items Size seems wrong");

                                dictionary.IdToIndex = new Dictionary<ushort, int>(numChars);
                                dictionary.RawGlyphs = new List<Glyph>(numChars);
                                
                                dictionary.RawGlyphs.Add(Glyph.Null);
                                dictionary.IdToIndex.Add(0, 0);

                                for (int i = 0; i < numChars; i++)
                                {
                                    var glyph = br.ReadSafe<bmGlyphs>().ConvertToMyGlyph();

                                    if (glyph.IsValidGlyph)
                                        if (dictionary.IdToIndex.ContainsKey(glyph.id))
                                        {
                                            Debugg.Error("Duplicate char id in binary font file");
                                        }
                                        else
                                        {
                                            dictionary.IdToIndex.Add(glyph.id, dictionary.RawGlyphs.Count);
                                            dictionary.RawGlyphs.Add(glyph);
                                        }
                                }
     
                                break;

                            case 5:
                                int numPair = (int)(blockSize / bmKerning.SizeOf);
                                if (numPair < 0 || numPair > MAXCHARCOUNT) throw new BmFontException("BlockKerning Items Size seems wrong");
                                Kernings = new bmKerning[numPair];
                                for (int i = 0; i < numPair; i++)
                                    Kernings[i] = br.ReadSafe<bmKerning>();
                                break;

                            default: break; //unknow type, try to continue
                        }
                    }
                    catch
                    {
                        throw new BmFontException("exeption when reading block type " + blockType.ToString(), br.BaseStream.Position);
                    }

                    br.BaseStream.Position = begin + blockSize;
                }
            }
        }

        #region structures
        /// <summary>
        ///  bits 6 7 8: reserved
        /// </summary>
        [Flags]
        public enum bmField : byte
        {
            smooth = 128,
            unicode = 64,
            italic = 32,
            bold = 16,
            fixedHeight = 8
        }

        /// <summary>
        /// The font description
        /// </summary>
        public class bmInfo
        {
            public short fontSize; //why is negative ?
            public bmField field;
            public byte charSet;
            public ushort stretchH;
            public byte aa;
            public byte paddingUp;
            public byte paddingRight;
            public byte paddingDown;
            public byte paddingLeft;
            public byte spacingHoriz;
            public byte spacingVert;
            public sbyte outline; //version >= 2
            public string fontName; //null-terminated string


            public bmInfo(BinaryReader br, int version)
            {
                fontSize = Maths.Mathelp.ABS(br.ReadInt16());
                field = (bmField)br.ReadByte();
                charSet = br.ReadByte();
                stretchH = br.ReadUInt16();
                aa = br.ReadByte();
                paddingUp = br.ReadByte();
                paddingRight = br.ReadByte();
                paddingDown = br.ReadByte();
                paddingLeft = br.ReadByte();
                spacingHoriz = br.ReadByte();
                spacingVert = br.ReadByte();
                outline = version >= 2 ? br.ReadSByte() : (sbyte)0;
                fontName = br.ReadStringToNull();
            }
        }
        /// <summary>
        /// Common textures values for each Glyphs. see "measures.png"
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct bmCommon
        {
            public ushort lineHeight;
            /// <summary>
            /// The base value is how far from the top of the cell height the base of the characters in the font should be placed.
            /// Characters can of course extend above or below this base line, which is entirely up to the font design.
            /// </summary>
            public ushort layoutBase;
            /// <summary>
            /// Textures width
            /// </summary>
            ushort scaleW;
            /// <summary>
            /// Textures Height
            /// </summary>
            ushort scaleH;
            /// <summary>
            /// reference to <see cref="PageNames"/> index
            /// </summary>
            public ushort pages;
            byte packed; //bits 1-7: reserved, bit 8: packed
            public byte alphaChnl;
            public byte redChnl;
            public byte greenChnl;
            public byte blueChnl;
            /// <summary>
            /// ushort to float
            /// </summary>
            public float TextureWidth => scaleW;
            /// <summary>
            /// ushort to float
            /// </summary>
            public float TextureHeight => scaleH;
            public bool IsPacked => packed > 0;
            public static int SizeOf => sizeof(ushort) * 5 + sizeof(byte) * 5;
        }

        /// <summary>
        /// The char texture definition. Make sure that id match with Ascii table to avoid confusion
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct bmGlyphs
        {
            public int id;
            /// <summary>
            /// The green rectangle illustrates the quad that should be copied from the texture to the screen when rendering the character.
            /// The width and height gives the size of this rectangle, and x and y gives the position of the rectangle in the texture.
            /// </summary>
            public ushort x;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public ushort y;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public ushort width;
            /// <summary><inheritdoc cref="x"></inheritdoc></summary>
            public ushort height;
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
            public ushort xadvance;

            public byte page;
            public byte chnl;

            public static int SizeOf => sizeof(ushort) * 7 + sizeof(byte) * 2 + sizeof(int);

            public char Char => Convert.ToChar(id);

            /// <summary>
            /// The ID = -1 mean an invalid char definition
            /// </summary>
            public bool IsInvalidChar => id == -1;

            /// <summary>
            /// I use only ascii number from 32 to 255
            /// </summary>
            public bool IsAscii => id >= 32 && id <= 255;

            public static bmGlyphs InvalidChar => new bmGlyphs() { id = -1 };

            public override string ToString()
            {
                string c = IsInvalidChar ? "null" : Char.ToString();
                return string.Format("char: \'{0}\' x{1} y{2}", c, x, y);
            }

            public Glyph ConvertToMyGlyph()
            {
                if (id < -1) return Glyph.Null;
                Glyph glyph = new Glyph();
                glyph.id = id < 0 ? ushort.MinValue : (ushort)id;
                glyph.x = Convert.ToInt16(x);
                glyph.y = Convert.ToInt16(y);
                glyph.xadvance = Convert.ToInt16(xadvance);
                glyph.width = Convert.ToInt16(width);
                glyph.height = Convert.ToInt16(height);
                glyph.xoffset = xoffset;
                glyph.yoffset = yoffset;
                return glyph;
            }
        }
        /// <summary>
        /// I prefer not use the kerning pair
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct bmKerning
        {
            public uint first;
            public uint second;
            public int amount;
            public static int SizeOf => sizeof(int) * 3;
        }
        #endregion
    }
    /// <summary>
    /// The exeption contain the last binary file position, useful for debugging
    /// </summary>
    public class BmFontException : Exception
    {
        public BmFontException() { }
        public BmFontException(string message) : base(message) { }
        public BmFontException(string message, long atFilePos)
            : base(string.Format("Exeption data at byte: {0} message: {1}", atFilePos, message)) { }
    }
}
