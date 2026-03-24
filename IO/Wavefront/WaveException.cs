using System;

namespace Common.IO.Wavefront
{
    [Serializable]
    public class InconsistentDataException : Exception
    {
        public InconsistentDataException() { }

        public InconsistentDataException(string name, int atTextRow)
            : base(string.Format("Inconsistent data at : {0}", atTextRow)) { }
    }
}
