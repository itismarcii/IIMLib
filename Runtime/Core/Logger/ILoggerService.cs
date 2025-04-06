namespace IIMLib.Core
{
    public interface ILoggerService : IService
    {
        void Log(in string message);
        void LogVerbose(in string message);
        void LogWarning(in string message);
        void LogError(in string message);
        void LogFatal(in string message);
        void SetLogLevel(in LogLevel level);
        void SetLoggerFilePath(in string filePath);
    }
}