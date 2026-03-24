using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Common.Maths;

namespace Common.Inputs
{
    /// <summary>
    /// Extension of <see cref="System.Windows.Forms.Cursor"/>
    /// </summary>
    public class MouseCursor : Disposable
    {
        public virtual Cursor Cursor { get; protected set; }

        /// <summary>
        /// Gets the default arrow cursor.
        /// </summary>
        public static MouseCursor Arrow { get; private set; }
        /// <summary>
        /// Gets the cursor that appears when the mouse is over text editing regions.
        /// </summary>
        public static MouseCursor IBeam { get; private set; }
        /// <summary>
        /// Gets the waiting cursor that appears while the application/system is busy.
        /// </summary>
        public static MouseCursor Wait { get; private set; }
        /// <summary>
        /// Gets the crosshair ("+") cursor.
        /// </summary>
        public static MouseCursor Crosshair { get; private set; }
        /// <summary>
        /// Gets the cross between Arrow and Wait cursors.
        /// </summary>
        public static MouseCursor WaitArrow { get; private set; }
        /// <summary>
        /// Gets the northwest/southeast ("\") cursor.
        /// </summary>
        public static MouseCursor SizeNWSE { get; private set; }
        /// <summary>
        /// Gets the northeast/southwest ("/") cursor.
        /// </summary>
        public static MouseCursor SizeNESW { get; private set; }
        /// <summary>
        /// Gets the horizontal west/east ("-") cursor.
        /// </summary>
        public static MouseCursor SizeWE { get; private set; }
        /// <summary>
        /// Gets the vertical north/south ("|") cursor.
        /// </summary>
        public static MouseCursor SizeNS { get; private set; }
        /// <summary>
        /// Gets the size all cursor which points in all directions.
        /// </summary>
        public static MouseCursor SizeAll { get; private set; }
        /// <summary>
        /// Gets the cursor that points that something is invalid, usually a cross.
        /// </summary>
        public static MouseCursor No { get; private set; }
        /// <summary>
        /// Gets the hand cursor, usually used for web links.
        /// </summary>
        public static MouseCursor Hand { get; private set; }
        public static MouseCursor HSplit { get; private set; }
        public static MouseCursor VSplit { get; private set; }
        public static MouseCursor WaitCursor { get; private set; }
        
        static MouseCursor()
        {
            Arrow = new MouseCursor(Cursors.Arrow);
            IBeam = new MouseCursor(Cursors.IBeam);
            Wait = new MouseCursor(Cursors.WaitCursor);
            Crosshair = new MouseCursor(Cursors.Cross);
            WaitArrow = new MouseCursor(Cursors.AppStarting);
            SizeNWSE = new MouseCursor(Cursors.SizeNWSE);
            SizeNESW = new MouseCursor(Cursors.SizeNESW);
            SizeWE = new MouseCursor(Cursors.SizeWE);
            SizeNS = new MouseCursor(Cursors.SizeNS);
            SizeAll = new MouseCursor(Cursors.SizeAll);
            No = new MouseCursor(Cursors.No);
            Hand = new MouseCursor(Cursors.Hand);
            HSplit = new MouseCursor(Cursors.HSplit);
            VSplit = new MouseCursor(Cursors.VSplit);
            WaitCursor = new MouseCursor(Cursors.WaitCursor);
        }

        protected MouseCursor()
        {
        }

        public MouseCursor(Cursor cursor)
        {
            Cursor = cursor;
        }

        /// <summary>
        /// </summary>
        /// <param name="bmp">small image to convert into icon</param>
        /// <param name="hotspot">the position of mouse point inside image</param>
        public static MouseCursor Create(Bitmap bmp, Vector2i hotspot = default(Vector2i))
        {
            return new MouseCursor(BmpToCursor(bmp, hotspot));
        }

        public static implicit operator Cursor(MouseCursor mouse)
        {
            if (mouse == null || mouse.IsDisposed) throw new Exception("Invalid casting for disposed MouseCursor");
            return mouse.Cursor;
        }

        public static void SetCursor(MouseCursor cursor, Control control)
        {
            if (cursor == null || control == null) return;
            control.Cursor = cursor.Cursor;
        }

        #region Native
        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            /// <summary> Specifies whether this structure defines an icon or a cursor. 
            /// A value of TRUE specifies an icon; FALSE specifies a cursor.
            /// </summary>
            public bool IsIcon;
            /// <summary>
            /// Specifies the coordinate of a cursor's hot spot. If this structure defines an icon,
            /// the hotspot is always in the center of the icon, and this member is ignored.
            /// </summary>
            public Vector2i HotSpot;
            /// <summary>
            /// (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon,
            /// this bitmask is formatted so that the upper half is the icon AND bitmask and the lower half is
            /// the icon XOR bitmask. Under this condition, the height should be an even multiple of two. If
            /// this structure defines a color icon, this mask only defines the AND bitmask of the icon.
            /// </summary>
            public IntPtr MaskBitmap;
            /// <summary>
            /// (HBITMAP) Handle to the icon color bitmap. This member can be optional if this
            /// structure defines a black and white icon. The AND bitmask of hbmMask is applied with the SRCAND
            /// flag to the destination; subsequently, the color bitmap is applied (using XOR) to the
            /// destination by using the SRCINVERT flag.
            /// </summary>
            public IntPtr ColorBitmap;
        };

        [DllImport("user32.dll")]
        static extern IntPtr CreateIconIndirect([In] ref ICONINFO iconInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetIconInfo(IntPtr hIcon, ref ICONINFO pIconInfo);

        /// <summary>
        /// </summary>
        /// <param name="bmp">small image to convert into icon</param>
        /// <param name="hotspot">the position of mouse point inside image</param>
        protected static Cursor BmpToCursor(Bitmap bmp, Vector2i hotspot = default(Vector2i))
        {
            try
            {
                ICONINFO icon_info = new ICONINFO();
                GetIconInfo(bmp.GetHicon(), ref icon_info);
                icon_info.HotSpot = hotspot;
                icon_info.IsIcon = false;    // Cursor, not icon.
                return new Cursor(CreateIconIndirect(ref icon_info));
            }
            catch (Exception e)
            {
                Debugg.Error("Cant create Cursor.\n" + e.Message);
                return null;
            }
        }
        #endregion

#if DEBUG
        /// <summary>
        /// Override to skip debug message
        /// </summary>
        ~MouseCursor() { if (!IsDisposed) Dispose(); }
#endif
        public override void Dispose()
        {
            Cursor?.Dispose();
            base.Dispose();
        }
    }

    /// <summary>
    /// Extension of <see cref="System.Windows.Forms.Cursor"/> but using differents cursors each frame.
    /// Call <see cref="UpdateElapsedTime"/> to update the currect frame, typically in the Update() loop.
    /// </summary>
    public class AnimatedCursor : MouseCursor
    {
        Cursor[] cursors;
        int framecount;
        int frame;
        float time;
        float frameTime;
        float maxtime;
        /// <summary>
        /// Return the currect cursor relative to time
        /// </summary>
        public override Cursor Cursor
        {
            get => cursors[frame];
            protected set => cursors[frame] = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="frameCount">number of cursors</param>
        /// <param name="frameMS">time interval between cursors</param>
        public AnimatedCursor(int frameCount = 1, float frameMS = 16.7f) : base()
        {
            framecount = frameCount;
            cursors = new Cursor[frameCount];
            FrameMS = frameMS;
        }

        public bool CreateFrame(Bitmap bmp, int frame, Vector2i hotspot = default(Vector2i))
        {
            if (frame < 0 || frame >= framecount) throw new ArgumentOutOfRangeException($"frame value must be in range [0,{framecount}]");
            if (cursors[frame] != null) cursors[frame].Dispose();
            cursors[frame] = BmpToCursor(bmp, hotspot);
            return cursors[frame] != null;
        }

        public static new AnimatedCursor WaitCursor { get; private set; }

        /// <summary>
        /// time interval between cursors
        /// </summary>
        public float FrameMS
        {
            get => frameTime;
            set
            {
                time = 0;
                frame = 0;
                frameTime = value;
                maxtime = framecount * frameTime;
            }
        }
        


        /// <summary>
        /// The current cursor are calculated using elapsed time, you need to call in Update() loop
        /// </summary>
        /// <param name="milliseconds">elapsed time</param>
        public void UpdateElapsedTime(float milliseconds)
        {
            time += milliseconds;
            time %= maxtime;
            frame = (int)(time / frameTime) % framecount;
        }

        static AnimatedCursor()
        {
            WaitCursor = new AnimatedCursor(8);
            WaitCursor.cursors[0] = Cursors.PanNorth;
            WaitCursor.cursors[1] = Cursors.PanNE;
            WaitCursor.cursors[2] = Cursors.PanEast;
            WaitCursor.cursors[3] = Cursors.PanSE;
            WaitCursor.cursors[4] = Cursors.PanSouth;
            WaitCursor.cursors[5] = Cursors.PanSW;
            WaitCursor.cursors[6] = Cursors.PanWest;
            WaitCursor.cursors[7] = Cursors.PanNW;
        }
        public override void Dispose()
        {
            for (int i = 0; i < cursors.Length; i++)
            {
                cursors[i]?.Dispose();
                cursors[i] = null;
            }
            base.Dispose();
        }


    }
}
