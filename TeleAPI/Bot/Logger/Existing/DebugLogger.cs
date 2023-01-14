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
                case LogLevel.Trace:
                    Debug.WriteLine(Message);
                    break;
                case LogLevel.Debug:
                    Debug.WriteLine(Message);
                    break;
                case LogLevel.Information:
                    Debug.WriteLine(Message);
                    break;
                case LogLevel.Warning:
                    Debug.WriteLine(Message);
                    break;
                case LogLevel.Error:
                    Debug.Fail(Message);
                    break;
                case LogLevel.Critical:
                    Debug.Fail(Message);
                    break;
                case LogLevel.None:
                    Debug.WriteLine(Message);
                    break;
            }
        }
    }
}
