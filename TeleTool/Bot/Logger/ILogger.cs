using Microsoft.Extensions.Logging;

namespace TeleTool.Bot.Logger
{
    public interface ILogger
    {
        public void Log(LogLevel Level, string Message);
        public void LogInformation(string Message) => Log(LogLevel.Information, Message);
        public void LogWarning(string Message) => Log(LogLevel.Warning, Message);
        public void LogDebug(string Message) => Log(LogLevel.Debug, Message);
        public void LogCritical(string Message) => Log(LogLevel.Critical, Message);
        public void LogCritical(Exception Ex) => Log(LogLevel.Critical, $"""
                                                                        {Ex.Message}
                                                                        {Ex.StackTrace}
                                                                        """);
    }
}
