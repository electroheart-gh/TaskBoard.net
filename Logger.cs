using System;
using System.Diagnostics;
using System.Reflection;

namespace TaskBoardWf
{
    internal static class Logger
    {
        public static TraceLevel LogLevel { get; set; }

        static Logger()
        {
            Trace.Listeners.Clear();
            var logFileName = Program.appSettings.LogFileName ?? "log.txt";
            Trace.Listeners.Add(new CustomTraceListener(logFileName));

            var logLevel = Program.appSettings.LogLevel ?? "Error";
            LogLevel = (TraceLevel)Enum.Parse(typeof(TraceLevel), logLevel, true);

            Trace.AutoFlush = true;
        }

        public static void LogInfo(string message)
        {
            if (LogLevel >= TraceLevel.Info) {
                Log(message, TraceEventType.Information);
            }
        }

        public static void LogWarning(string message)
        {
            if (LogLevel >= TraceLevel.Warning) {
                Log(message, TraceEventType.Warning);
            }
        }

        public static void LogError(string message)
        {
            if (LogLevel >= TraceLevel.Error) {
                Log(message, TraceEventType.Error);
            }
        }

        private static void Log(string message, TraceEventType eventType)
        {
            StackFrame frame = new StackFrame(2, true);
            MethodBase method = frame.GetMethod();
            string className = method.DeclaringType.FullName;
            string methodName = method.Name;

            string formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {eventType} - {className}.{methodName} - {message}";
            Trace.WriteLine(formattedMessage);
        }
    }
}
