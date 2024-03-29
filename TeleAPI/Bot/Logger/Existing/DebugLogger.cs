﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TeleAPI.Bot.Logger.Existing
{
    public class TraceLogger : ILogger
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
                    Trace.WriteLine(Message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Trace.Fail(Message);
                    break;
            }
        }
    }
}
