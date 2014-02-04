using System;

namespace TPC.Utilities
{
    public static class LogBroker
    {
        public static event LoggerDelegate OnLog;

        public static void Log(string logEvent)
        {
            string format = FormatEvent(logEvent);

            if (OnLog != null)
            {
                OnLog(null, new LoggerEventArgs() { Event = format });
            }
        }

        private static string FormatEvent(string logEvent)
        {
            var now = DateTime.Now;
            return string.Format("{0}:{1}:{2},{3} - {4}",
                now.Hour.ToString().PadLeft(2, '0'),
                now.Minute.ToString().PadLeft(2, '0'),
                now.Second.ToString().PadLeft(2, '0'),
                now.Millisecond.ToString().PadLeft(3, '0'),
                logEvent);
        }
    }

    public delegate void LoggerDelegate (object sender, LoggerEventArgs e);
    public class LoggerEventArgs : EventArgs
    {
        public string Event { get; set; }
    }
}
