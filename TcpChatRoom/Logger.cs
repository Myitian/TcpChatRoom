namespace TcpChatRoom;

public class Logger
{
    public static readonly object LoggerLock = new();
#if DEBUG
    public static LogLevel MaxLogLevel = LogLevel.Information;
#else
    public static LogLevel MaxLogLevel = LogLevel.Debug;
#endif
    public static string LogLevelToString(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => "FATAL",
            LogLevel.Error => "ERROR",
            LogLevel.Warning => "WARN",
            LogLevel.Information => "INFO",
            LogLevel.Debug => "DEBUG",
            _ => ((int)level).ToString(),
        };
    }
    public static ConsoleColor LogLevelToColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => ConsoleColor.DarkRed,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.DarkYellow,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Debug => ConsoleColor.Gray,
            _ => ConsoleColor.Gray,
        };
    }
    public static void Log(string? message, LogLevel logLevel, string? source = null)
    {
        if (logLevel > MaxLogLevel)
            return;
        lock (LoggerLock)
        {
            Console.ForegroundColor = LogLevelToColor(logLevel);
            Console.Write($"[{DateTimeOffset.Now:o}] [{LogLevelToString(logLevel)}] ");
            switch (source)
            {
                case null:
                    Console.Write($"({Thread.CurrentThread.Name}) ");
                    break;
                case "":
                    break;
                default:
                    Console.Write($"({source}) ");
                    break;
            }
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
    public static void Fatal(string? message, string? source = null)
    {
        Log(message, LogLevel.Fatal, source);
    }
    public static void Error(string? message, string? source = null)
    {
        Log(message, LogLevel.Error, source);
    }
    public static void Warning(string? message, string? source = null)
    {
        Log(message, LogLevel.Warning, source);
    }
    public static void Information(string? message, string? source = null)
    {
        Log(message, LogLevel.Information, source);
    }
    public static void Debug(string? message, string? source = null)
    {
        Log(message, LogLevel.Debug, source);
    }
}
public enum LogLevel
{
    Fatal,
    Error,
    Warning,
    Information,
    Debug
}
