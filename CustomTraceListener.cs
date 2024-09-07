using System.Diagnostics;

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
