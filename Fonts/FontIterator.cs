using Common.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Fonts
{
    /// <summary>
    /// Create a list of computed drawable glyph.
    /// </summary>
    public class FontIterator : IEnumerator<FontTypeSprite> , IEnumerable<FontTypeSprite>
    {
        static readonly char[] empty = new char[] { };
        FontDictionary font;
        char[] chars;
        int index;
        int xadvance;
        int yadvance;
        int lineHeight;

        FontTypeSprite current;

        /// <summary>
        /// Assign the string to iterate. A exception will be generate if you assign a string during foreach loop to prevent possible overflow.
        /// </summary>
        public string Text
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    chars = value.ToArray();
                else
                    chars = empty;

                if (index >= 0) throw new Exception("Can't change text during loop !");

                Reset();
            }
        }

        internal FontIterator(FontDictionary font)
        {
            this.font = font;
            Reset();
        }

        object IEnumerator.Current => Current;

        public FontTypeSprite Current => current;

        public bool MoveNext()
        {
            index++;

            if (chars == null || chars.Length <= index || !Advance(ref index, out var glyph))
            {
                Reset(); //to be sure you can start a new loop
                return false;
            }
            
            current.Info = glyph;

            current.Source.position.x = glyph.x;
            current.Source.position.y = glyph.y;
            current.Source.size.x = glyph.width;
            current.Source.size.y = glyph.height;

            current.Destination.position.x = glyph.xoffset + xadvance;
            current.Destination.position.y = glyph.yoffset + yadvance;
            //must be reinterpreted
            current.Destination.size = current.Source.size;

            //not sure the padding math is correct
            xadvance += glyph.xadvance - font.PaddingLeft - font.PaddingRight;

            return true;
        }

        public void Reset()
        {
            index = -1;
            xadvance = 0;
            yadvance = 0;
            lineHeight = font.LineHeightMinusPadding;
        }

        /// <summary>
        /// Get next drawable char
        /// </summary>
        /// <param name="index">current position in the chars array</param>
        bool Advance(ref int index, out Glyph glyph)
        {
            do
            {
                char c = chars[index];

                if (c=='\r')
                {

                }
                if (c == '\t' || c == '\0')
                {
                    xadvance += font.XAdvanceEmptyChar;
                }

                if (c == '\n')
                {
                    xadvance = 0;
                    yadvance += lineHeight;
                }
                else
                {
                    if (font.GetGlyph(c, out glyph))
                    {
                        return true;
                    }
                }
                index++;
            }
            while (index < chars.Length);

            glyph = Glyph.Null;
            return false;
        }

        public void Dispose()
        {
            Reset();
        }

        public IEnumerator<FontTypeSprite> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public struct FontTypeSprite
    {
        /// <summary>
        /// The glyph associated to current char
        /// </summary>
        public Glyph Info;
        /// <summary>
        /// pixel coordinate of texture (topleft coord system)
        /// </summary>
        public Rectangle4i Source;
        /// <summary>
        /// pixel coordinate of destination rectangle on the screen (topleft coord system)
        /// </summary>
        public Rectangle4i Destination;

        public override string ToString()
        {
            return string.Format("char \"{0}\"", Info.Char);
        }
    }
}
