using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TeleAPI.Bot.Logger.Existing
{
    public class DebugLogger : ILogger
    {
        public void Log(LogLevel Level, string Message)
        {
            switch (Level)
            {
                case LogLevel.None:
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                case LogLevel.Warning:
                    Debug.WriteLine(Message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.Fail(Message);
                    break;
            }
        }
    }
}
