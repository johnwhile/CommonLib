using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;


using Common.Maths;

namespace Common.Gui
{

    public abstract class GuiEditText : GuiControl
    {
        bool editing = false;
        bool highlighting = false;

        public Color4b FontColor;
        public Color4b FontColorSelected;
        public Color4b HighlightedColor;

        Vector2i startSelectionPoint;
        Vector2i endSelectionPoint;
   
        GuiStringBuilder builder;
        public string Text => builder.Text;

        public GuiEditText(GuiContainer parent, string text = "") : base(parent, default(Rectangle4i))
        {
            CanParentFocus = true;
            FontColor = Color4b.Black;
            FontColorSelected = Color4b.White;
            HighlightedColor = Color4b.Blue;

            Resizable = false;

            builder = new GuiStringBuilder(text);

            MouseClick += delegate (GuiControl sender)
            {

            };
            Selecting += delegate (GuiControl sender)
            { 
                editing = true;
            };
            Deselecting += delegate (GuiControl sender) 
            { 
                editing = false;
            };


            KeyDown += EditingText;
        }

        public override void OnMouseDown(Vector2f mouse)
        {
            if (editing)
            {
                highlighting = true;
                startSelectionPoint = endSelectionPoint = Manager.MousePosition;
            }
            base.OnMouseDown(mouse);
        }

        public override void OnMouseMove(Vector2f mouse)
        {
            if (highlighting)
            {
                endSelectionPoint = Manager.MousePosition;
            }
            base.OnMouseMove(mouse);
        }

        public override void OnMouseUp()
        {
            if (highlighting)
            {
                highlighting = false;
                endSelectionPoint = Manager.MousePosition;
            }
            base.OnMouseUp();
        }

        private void EditingText(GuiControl sender, KeyEventArgs arg)
        {
            if (!editing) return;

            if (arg.KeyCode == Keys.Enter)
            {
                //deselecting
                if (Manager.Selected == this) Manager.Selected = null;
                return;
            }
            if (arg.KeyCode == Keys.Back)
            {
                //remove at cursor position
                builder.Remove();
                return;
            }
            if (arg.KeyCode== Keys.Delete)
            {
                builder.Clear();
                return;
            }
            var c = Tools.CharHelp.GetAsciiFromKeys(arg);
            
            if (c == '\0')
            {
                Debugg.Warning("not valid " + arg.KeyCode.ToString());
                return;
            }

            Debugg.Warning("add char : " + c);
            builder.AddChar(c);

        }

        struct GuiChar
        {
            public char chr;
            public Rectangle4i dest;

            public GuiChar(char chr, Rectangle4i dest)
            {
                this.chr = chr;
                this.dest = dest;
            }
        }

        class GuiStringBuilder
        {
            string lastbuilded;
            bool ischanged;
            List<GuiChar> chars;
            //cursor position. zero mean before first char
            int Cursor;

            /// <summary>
            /// builded string
            /// </summary>
            public string Text
            {
                get
                {
                    if (chars.Count == 0)
                    {
                        lastbuilded = "";
                        ischanged = false;
                    }
                    if (ischanged)
                    {
                        char[] buffer = new char[chars.Count];
                        for (int i = 0; i < chars.Count; i++) buffer[i] = chars[i].chr;
                        lastbuilded = new string(buffer);
                        ischanged = false;
                    }
                    return lastbuilded;
                }
                set
                {
                    Clear();
                    foreach (char c in value)
                        if (c != '\n' && c != '\r')
                            AddChar(c);
                    lastbuilded = value;
                    ischanged = false;
                }
            }

            public GuiStringBuilder(int capacity)
            {
                ischanged = false;
                lastbuilded = "";
                chars = new List<GuiChar>(capacity);
                Cursor = 0;
            }
            public GuiStringBuilder(string initial) : this(initial.Length)
            {
                Text = initial;
            }


            public void Remove()
            {
                if (Cursor == 0) return;
                Debug.WriteLine("remove char index " + (Cursor - 1));

                if (Cursor > chars.Count) throw new ArgumentOutOfRangeException("cursor can't be greater than string lenght");
                if (Cursor > 0) chars.RemoveAt(Cursor - 1);
                Cursor--;
                ischanged = true;
            }

            public void AddChar(char chr)
            {
                if (chr == '\0' || chr == '\n' || chr == '\r') return;
                
                if (Cursor > chars.Count) throw new ArgumentOutOfRangeException("cursor can't be greater than string lenght");

                //cursor is after last char
                if (Cursor == chars.Count)
                    chars.Add(new GuiChar() { chr = chr });
                else
                    chars.Insert(Cursor, new GuiChar() { chr = chr });

                Cursor++;
                ischanged = true;
            }

            public void Clear()
            {
                Cursor = 0;
                ischanged = false;
                lastbuilded = "";
                chars.Clear();
            }

            public void SetRectangleToLastChar(Vector2i charSize, int interlineHeight)
            {
                if (chars.Count <= 0) return;
            }

            public override string ToString()
            {
                return Text;
            }


        }

    }



}
