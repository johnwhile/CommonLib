using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common.Tools
{
    [AttributeUsage(AttributeTargets.All)]
    [ComVisible(true)]
    public sealed class NotImplementedAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        /// <param name="message">The text string that describes alternative workarounds.</param>
        /// <param name="error">true if the element usage generates a compiler error; false if warning.</param>
        public NotImplementedAttribute(string message, bool error = false)
        {
            Message = message;
            IsError = error;
            if (IsError)
            {
                Debug.Print(message);
            }
        }

        public NotImplementedAttribute() : this(null, false) { }

        public string Message { get; private set; }
        public bool IsError { get; private set; }
    }

}
