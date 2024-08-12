using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBoardWf
{
    internal class CustomTraceListener : TextWriterTraceListener
    {

        public CustomTraceListener(string fileName) : base(fileName) { }

        public override void WriteLine(string message)
        {
            base.WriteLine(message);
        }
    }
}
