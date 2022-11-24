using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TeleTool.Bot.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Log(LogLevel Level, string Message)
        {
            switch (Level)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.None:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"[{Level}] [{DateTime.Now:T}] {Message}");
        }
    }
}
