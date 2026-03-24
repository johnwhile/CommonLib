using System;
using System.Windows.Forms;

namespace Common
{
    /// <summary>
    /// Extension methods for the <see cref="Control"/> base class.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Function to retrieve the form in which this control is contained.
        /// </summary>
        /// <typeparam name="T">Type of form. Must inherit from <see cref="Form"/>.</typeparam>
        /// <param name="control">The control to start searching from.</param>
        /// <returns>The <see cref="Form"/> of type <typeparamref name="T"/> if found, <b>null</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
        public static Form GetMainForm(this Control control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            //findform return same control for main form
            var parent = control.FindForm();
            if (parent == control || parent == null) return parent;
            return GetMainForm(parent);
        }

        /// <summary>
        /// ClientSize.Width / ClientSize.Height
        /// </summary>
        public static float Aspect(this Control control) => control.ClientSize.Width / (float)control.ClientSize.Height;

    }
}
