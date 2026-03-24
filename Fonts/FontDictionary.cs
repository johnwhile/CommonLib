using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Fonts
{
    /// <summary>
    /// Utility class to read the font format.<br/>
    /// The current implementations:<br/>
    /// 1. bmfont from <b>http://www.angelcode.com/products/bmfont/</b><br/>
    /// 2. mudgefont from <b>http://www.larryhastings.com/programming/mudgefont/</b>
    /// </summary>
    public class FontDictionary
    {
        public Format FileVariant = Format.Unknow;
        
        static Dictionary<string, FontDictionary> fontcollection;
        /// <summary>
        /// The font name string used as key
        /// </summary>
        public string FontNameKey;
        /// <summary>
        /// The font description file
        /// </summary>
        public string Filename;
        /// <summary>
        /// The true font name stored in the font description file
        /// </summary>
        public string FontName;
        /// <summary>
        /// The full path of texture, currently i'll use only one
        /// </summary>
        public string Texture;
        /// <summary>
        /// number of pixel defines the new line
        /// </summary>
        public short LineHeight;
        /// <summary>
        /// </summary>
        public int LineHeightMinusPadding => LineHeight - PaddingUp - PaddingDown;
        /// <summary>
        /// size of atlas texture. For mudgefont must be estracted from texture
        /// </summary>
        /// <remarks>There must be no difference between the actual size of texture and that stored in the description</remarks>
        public short ScaleWidth;
        /// <summary>
        /// <inheritdoc cref="ScaleWidth"/>
        /// </summary>
        public short ScaleHeight;
        /// <summary>
        /// line where text lies. For mudgefont is zero
        /// </summary>
        public short Base;
        /// <summary>
        /// Font size in pixel, can match with <see cref="LineHeight"/>
        /// </summary>
        public short Size;

        public short XAdvanceEmptyChar;

        /// <summary>
        /// TODO: apply to glyph's math
        /// </summary>
        public byte PaddingUp;
        public byte PaddingRight;
        public byte PaddingDown;
        public byte PaddingLeft;

        /// <summary>
        /// ID to <see cref="RawGlyphs"/> index
        /// </summary>
        public Dictionary<ushort, int> IdToIndex;
        /// <summary>
        /// list of all glyph in file's order
        /// </summary>
        public List<Glyph> RawGlyphs;

        /// <summary>
        /// List of drawable sprites.
        /// <code>
        /// FontType.Sprites.Text = "mystring"
        /// foreach(var sprite in FontType.Sprites) { }
        /// </code>
        /// </summary>
        public FontIterator Sprites;

        /// <summary>
        /// All loaded font, sorted by fontname value (no case sensitive)
        /// </summary>
        public static PropertyIndexerGet<string, FontDictionary> Fonts;

        static FontDictionary()
        {
            fontcollection = new Dictionary<string, FontDictionary>();
            Fonts = new PropertyIndexerGet<string, FontDictionary>(i =>
            {
                if (!fontcollection.TryGetValue(i, out var font)) return null;
                return font;
            });
        }

        private FontDictionary(string filename)
        {
            Filename = Path.GetFullPath(filename);
            Sprites = new FontIterator(this);
        }
        /// <summary>
        /// Load a new font or return a previous loaded font
        /// </summary>
        /// <param name="fontName">It will be set to lowercase, can be any string. See also FontNameKey</param>
        /// <param name="filename">the font's description file, pass null if font was already loaded</param>
        /// <param name="filevariant">the bmfont variants are autodetected, the mudgefont must be set instead</param>
        public static FontDictionary GetOrLoad(string fontName, string filename = null, Format filevariant = Format.Unknow)
        {
            fontName = fontName.ToLower();
            
            if (fontcollection.TryGetValue(fontName, out var font)) return font;

            if (!File.Exists(filename)) throw new FileNotFoundException("file \"" + filename + "\" not found !");
            font = new FontDictionary(filename);
            font.FontNameKey = fontName;

            //check the file variant
            using (var file = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                string c;
                font.FileVariant = filevariant;

                if (filevariant == Format.Mudge_Xml)
                {
                    font.LoadMudgeXml(file);
                }
                else
                {
                    if (filevariant == Format.Unknow)
                    {
                        using (var br = new BinaryReader(file, Encoding.Default, true))
                            c = new string(br.ReadChars(5));
                        file.Position = 0;

                        if (c[0] == 'B' & c[1] == 'M' & c[2] == 'F')
                            font.FileVariant = Format.Bm_Binary;
                        else if (c[0] == 'i' & c[1] == 'n' & c[2] == 'f' & c[3] == 'o')
                            font.FileVariant = Format.Bm_Text;
                        else if (c[2] == 'x' & c[3] == 'm' & c[4] == 'l')
                            font.FileVariant = Format.Bm_Xml;
                    }

                    switch (font.FileVariant)
                    {
                        case Format.Bm_Xml: font.LoadXml(file); break;
                        case Format.Bm_Binary: font.LoadBinary(file); break;
                        case Format.Bm_Text: font.LoadText(file); break;
                        default: throw new NotImplementedException("file \"" + filename + "\" unknow type of file");
                    }
                }
            }

            if (font != null)
            {
                Glyph empty;
                fontcollection.Add(fontName, font);

                if (font.GetGlyph(' ', out empty))
                    font.XAdvanceEmptyChar = empty.xadvance;
                
                font.XAdvanceEmptyChar = font.Size;

                empty = font.RawGlyphs[0];
                empty.xadvance = font.XAdvanceEmptyChar;
                empty.height = font.LineHeight;


                font.RawGlyphs[0] = empty;
            }

            return font;
        }

        /// <summary>
        /// Return true if exist a glyph data.<br/>
        /// Return true If the char ins't found but exist the empty glyph (defined as zero unicode).<br/>
        /// Return false if not glyph definition exist or the glyph has a zero size.<br/>
        /// </summary>
        /// <param name="unicode"></param>
        public bool GetGlyph(char unicode, out Glyph glyph, out int index)
        {
            glyph = Glyph.Null;
            if (IdToIndex.TryGetValue(unicode, out index))
            {
                glyph = RawGlyphs[index];
                //return glyph.IsValidGlyph;
                return true;
            }
            return false;
        }

        public bool GetGlyph(char unicode, out Glyph glyph)
        {
            return GetGlyph(unicode, out glyph, out _);
        }


        #region Loading...
        bool LoadXml(FileStream file)
        {
            FontBmXml loader = new FontBmXml(file, this);

            if (loader.font.Common.Pages != 1) throw new NotSupportedException("Only one texture page are implemented");

            FontName = loader.font.Info.Face;
            LineHeight = Convert.ToInt16(loader.font.Common.LineHeight);
            ScaleWidth = Convert.ToInt16(loader.font.Common.ScaleW);
            ScaleHeight = Convert.ToInt16(loader.font.Common.ScaleH);
            Base = Convert.ToInt16(loader.font.Common.Base);
            Size = Convert.ToInt16(loader.font.Info.Size);

            /*
            TryParsePadding(loader.font.Info.Padding, 
                out PaddingUp, 
                out PaddingRight, 
                out PaddingDown, 
                out PaddingLeft);
            */
            PaddingUp = Convert.ToByte(loader.font.Info.Padding.x);
            PaddingRight = Convert.ToByte(loader.font.Info.Padding.y);
            PaddingDown = Convert.ToByte(loader.font.Info.Padding.z);
            PaddingLeft = Convert.ToByte(loader.font.Info.Padding.w);


            Texture = Path.Combine(Path.GetDirectoryName(Filename), loader.font.Pages[0].Filename);
            if (!File.Exists(Texture))
            {
                Debug.Print("The texture \"" + Texture + "\" not exist !");
            }

            return true;
        }
        bool LoadBinary(FileStream file)
        {
            FontBmBinary loader = new FontBmBinary(file, this);
            if (loader.Common.pages != 1) throw new NotSupportedException("Only one texture page are implemented");
            
            FontName = loader.Info.fontName;
            LineHeight = Convert.ToInt16(loader.Common.lineHeight);
            ScaleWidth = Convert.ToInt16(loader.Common.TextureWidth);
            ScaleHeight = Convert.ToInt16(loader.Common.TextureHeight);
            Base = Convert.ToInt16(loader.Common.layoutBase);
            Size = Convert.ToInt16(loader.Info.fontSize);
            PaddingUp = loader.Info.paddingUp;
            PaddingDown = loader.Info.paddingDown;
            PaddingLeft = loader.Info.paddingLeft;
            PaddingRight = loader.Info.paddingRight;

            Texture = Path.Combine(Path.GetDirectoryName(Filename), loader.PageNames[0]);

            return true;
        }
        bool LoadText(FileStream file)
        {
            FontBmText loader = new FontBmText(file, this);
            if (loader.Common.pages != 1) throw new NotSupportedException("Only one texture page are implemented");

            FontName = loader.Info.fontname;
            LineHeight = Convert.ToInt16(loader.Common.lineHeight);
            ScaleWidth = Convert.ToInt16(loader.Common.scaleW);
            ScaleHeight = Convert.ToInt16(loader.Common.scaleH);
            Base = Convert.ToInt16(loader.Common.layoutBase);
            Size = Convert.ToInt16(loader.Info.size);
            Texture = Path.Combine(Path.GetDirectoryName(Filename), loader.Pages[0]);
            TryParsePadding(loader.Info.padding,
                out PaddingUp,
                out PaddingRight,
                out PaddingDown,
                out PaddingLeft);

            return true;
        }
        bool LoadMudgeXml(FileStream file)
        {       
            FontMudgeXml loader = new FontMudgeXml(file, this);

            //mudgefont contain only basic glyph description, all other info must be calculated
            FontName = Path.GetFileNameWithoutExtension(Filename);
            Texture = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Filename)), FontName);

            string[] extension = new string[] { ".bmp", ".png", ".jpg", ".tga" };

            bool found = false;

            foreach (var ext in extension)
            {
                if (found = File.Exists(Texture + ext))
                {
                    Texture += ext;
                    break;
                }
            }
            if (!found) throw new FileNotFoundException("Tindex don't find the texture for mudgefont \"" + Texture + "\"");

            return found;
        }

        internal static bool TryParsePadding(string padstring, out byte up, out byte right, out byte down, out byte left)
        {
            up = right = down = left = 0;
            string[] pads = padstring.Split(',');
            return byte.TryParse(pads[0], out up) &&
                byte.TryParse(pads[1], out right) &&
                byte.TryParse(pads[2], out down) &&
                byte.TryParse(pads[3], out left);
        }
        public enum Format
        {
            Unknow,
            /// <summary>
            /// bmfont from <b>http://www.angelcode.com/products/bmfont/</b><br/>, binary
            /// </summary>
            Bm_Binary,
            Bm_Xml,
            Bm_Text,
            /// <summary>
            /// see http://www.larryhastings.com/programming/mudgefont/
            /// must be specified in the FontType constructor
            /// </summary>
            Mudge_Xml
        }
        #endregion
    }


}
