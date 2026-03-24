using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tools
{
    public class DebugListener : TraceListener
    {
        StringBuilder builder;

        public bool NeedUpdate { get; private set; }

        public DebugListener()
        {
            builder = new StringBuilder();
        }

        protected DebugListener(string name) : base(name)
        {
        }

        public override void Write(string message)
        {
            builder.Append(message);
            NeedUpdate = true;
        }

        public override void WriteLine(string message)
        {
            builder.AppendLine(message);
            NeedUpdate = true;
        }

        public override string ToString()
        {
            NeedUpdate = false;
            return builder.ToString();
        }
    }
}
