using System;

namespace Common.Fonts
{
    /// <summary>
    /// The green rectangle illustrates the quad that should be copied from the texture to the screen when rendering the character.
    /// The width and height gives the size of this rectangle, and x and y gives the position of the rectangle in the texture.
    /// </summary>
    public struct Glyph
    {
        /// <summary>
        /// unicode UTF-16 id
        /// </summary>
        public ushort id;
        /// <summary>
        /// x and y gives the position of the source rectangle in the texture (topleft [x,y]).<br/>
        /// <b>I impose for convenience it can't be greater than <see cref="short.MaxValue"/></b>
        /// </summary>
        public short x;
        /// <summary>
        /// <inheritdoc cref="x"/>
        /// </summary>
        public short y;
        /// <summary>
        /// The width and height gives the size of the source rectangle in the texture.<br/>
        /// <b>I impose for convenience it can't be greater than <see cref="short.MaxValue"/></b>
        /// </summary>
        public short width;
        /// <summary>
        /// <inheritdoc cref="width"/>
        /// </summary>
        public short height;
        /// <summary>
        /// It gives the horizontal offset that should be added to the cursor position to find
        /// the left position where the character should be drawn.<br/>
        /// <b>I impose for convenience it can't be greater than <see cref="short.MaxValue"/></b>
        /// </summary>
        public short xadvance;
        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen.<br/>
        /// <b>I impose for convenience it can't be greater than <see cref="short.MaxValue"/></b>
        /// </summary>
        public short xoffset;
        /// <summary>
        /// <inheritdoc cref="xoffset"/> 
        /// </summary>
        public short yoffset;

        public char Char
        {
            get { return char.ConvertFromUtf32(id)[0]; }
        }

        public override string ToString()
        {
            return string.Format("{0}", Char.ToString());
        }

        /// <summary>
        /// not need to draw a whitespace char
        /// </summary>
        public bool IsSizeZero => width * height <= 0;

        public bool IsValidGlyph => xadvance > 0;

        public static Glyph Null => new Glyph() { id = 0, x = 0, y = 0, width = 0, height = 0, xadvance = 0 };
    }
}
