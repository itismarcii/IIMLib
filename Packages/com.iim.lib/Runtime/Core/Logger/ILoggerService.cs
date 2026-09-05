namespace IIMLib.Core.Logger
{
    public interface ILoggerService : IService
    {
        void Log(string message);
        void LogVerbose(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogFatal(string message);
        void SetLogLevel(LogLevel logLevel);
        void SetLoggerFilePath(string filePath);
    }
}
