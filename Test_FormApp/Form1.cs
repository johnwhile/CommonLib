using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Common.Maths;
using Common.Fonts;

namespace Common
{
    public partial class Form1 : Form
    {
        FontDictionary font;
        Image texture;
        Vector2f origin = new Vector2f(100, 50);
        float scale = 2.0f;
        Pen red, green, blue, black, white;
        string text;

        public Form1()
        {

            InitializeComponent();

            font = FontDictionary.GetOrLoad("Arial", @"C:\Users\johnw\Projects\SharpDxEngine\Content\Fonts\Planer.fnt");
            //font = FontDictionary.GetOrLoad("ArialUnicodeMS", @"C:\Users\johnw\Projects\SharpDxEngine\Content\Fonts\ArialUnicodeMS.xml", FontType.Format.Mudge_Xml);

            texture = Image.FromFile(font.Texture);
            red = new Pen(Color.Red);
            green = new Pen(Color.Green);
            blue = new Pen(Color.Blue);
            black = new Pen(Color.Black);
            white = new Pen(Color.White);

            text = "abcd\nB";

            ClientSize = new Size(1200, 600);

            Paint += TextPainting;
            //Paint += AtlasPainting;
        }

        private void AtlasPainting(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);


            var src = new Rectangle4f(0, 0, texture.Width, texture.Height);
            var dst = src;
            dst.size *= scale;

            g.DrawImage(texture, dst, src, GraphicsUnit.Pixel);

            foreach (var glyph in font.RawGlyphs)
            {
                var rect = new Rectangle4f(glyph.x, glyph.y, glyph.width, glyph.height);

                g.DrawRectangle(green, rect * scale);
            }
        }

        private void TextPainting(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.CornflowerBlue);


            int linecount = text.Count(i => i == '\n') + 1;

            int LH = font.LineHeight - font.PaddingUp - font.PaddingDown;

            //base line
            Vector2f left = Vector2f.Zero;
            Vector2f right = new Vector2f(ClientSize.Width, 0);
            for (int i = 1; i <= linecount; i++)
            {
                right.y = left.y = i * LH * scale + origin.y;
                g.DrawLine(white, left, right);
                right.y = left.y = ((i - 1) * LH + font.Base) * scale + origin.y;
                g.DrawLine(red, left, right);
            }
            g.DrawLine(white, new Point((int)origin.x, 0), new Point((int)origin.x, ClientSize.Height));


            byte line = 0;
            ushort xadvance = 0;
            foreach (var unicode in text)
            {
                if (unicode == '\n')
                {
                    line++;
                    xadvance = 0;
                    continue;
                }

                float yadvance = line * LH;

                if (font.GetGlyph(unicode, out var glyph))
                {
                    int xoffset = glyph.xoffset - font.PaddingLeft;
                    int yoffset = glyph.yoffset - font.PaddingUp;

                    //texture source rectangle
                    var src = new Rectangle4f(glyph.x, glyph.y, glyph.width, glyph.height);

                    //screen rectangle of char
                    var dst = new Rectangle4f()
                    {
                        x = xoffset + xadvance,
                        y = yoffset + yadvance,
                        width = src.width,
                        height = src.height
                    };
                    dst.size *= scale;
                    dst.position *= scale;
                    dst.position += origin;

                    g.DrawImage(texture, dst, src, GraphicsUnit.Pixel);
                    g.DrawRectangle(black, dst);

                    //screen rectangle of char's place
                    var chr = new Rectangle4f()
                    {
                        x = xadvance,
                        y = yadvance,
                        width = glyph.xadvance,
                        height = font.LineHeight
                    };
                    chr.size *= scale;
                    chr.position *= scale;
                    chr.position += origin;

                    g.DrawRectangle(blue, chr);

                    xadvance += (ushort)(glyph.xadvance - font.PaddingLeft);
                }
            }
        }
    }
}
