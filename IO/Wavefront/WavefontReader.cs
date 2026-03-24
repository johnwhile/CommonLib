using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Common.Maths;

namespace Common.IO.Wavefront
{
    public class WavefontReader : BinaryReader
    {
        const int BUFFER = 1024; //1KB

        static char[] spaceSeparator = new char[] { ' ', '\t' };
        
        char[] buffer = new char[BUFFER]; //1KB buffer;

        public int LineCounter { get; private set; }

        public WavefontReader(Stream input) : base(input, Encoding.UTF8, false)
        {
            LineCounter = 0;
        }

        /// <summary>
        /// Return a valid line, null if reach EOF
        /// </summary>
        /// <returns></returns>
        public bool ReadNext(out string[] splitline)
        {
            splitline = null;
            long length = BaseStream.Length;
            if (BaseStream.Position >= length) return false;

            char c = '\0';
            bool isEOF = false;
            int size = 0;
            int count = 0;

            ///////// READ UNTIL NEXT VALID LINE
            do
            {
                ///////// READ LINE
                bool iscomment = false;
                bool isvalid = false;
                do
                {
                    c = ReadChar();

                    isEOF = BaseStream.Position >= length;
                    if (c == '#') iscomment |= true;

                    if (c >= 33 && c <= 126) isvalid |= true;

                    if (!iscomment && isvalid)
                    {
                        if (size >= buffer.Length)
                        {
                            Debugg.Warning("increase m_buffer size of 1KB");
                            Array.Resize(ref buffer, buffer.Length + BUFFER);
                        }
                        buffer[size++] = c < 33 || c > 126 ? ' ' : c;
                    }
                    count++;
                }
                while (!isEOF && c != '\n' && c != '\r');

                //\n\r sequence but it the same line in a text editor
                c = (char)PeekChar();
                if (c == '\n' || c == '\r')
                {
                    BaseStream.Position += 1;
                    LineCounter++;
                }
                count = 0;
                isEOF = BaseStream.Position >= length;
            }
            while (size == 0 && !isEOF);

            if (size == 0) return false;

            string line = new string(buffer, 0, size);

            splitline = line.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);

            return true;
        }
    }
}
