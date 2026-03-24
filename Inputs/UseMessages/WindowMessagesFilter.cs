
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Common.WinNative;
using Common.Maths;

namespace Common.Inputs
{
    /// <summary>
    /// A message filter can be added to main application. Filtering messages work only when form is focused and mouse is inside client area
    /// </summary>
    public abstract class WinMessagesFilter : IInputDevice
    {
        bool acquired = false;
        protected abstract IMessageFilter MyFilter { get; }

        public WinMessagesFilter(Form app)
        {
        }

        public abstract bool Update();

        public bool IsAcquired
        {
            get { return acquired; }
        }

        public void StopAcquiring()
        {
            Application.RemoveMessageFilter(MyFilter);
            acquired = false;
        }

        public void EnableAcquiring()
        {
            StopAcquiring();
            Application.AddMessageFilter(MyFilter);
            acquired = true;
        }
        ~WinMessagesFilter()
        {
            StopAcquiring();
        }
    }
}
