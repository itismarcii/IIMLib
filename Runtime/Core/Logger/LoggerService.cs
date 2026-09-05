using System;
using System.IO;
using IIMLib.Core.Message;
using UnityEngine;

namespace IIMLib.Core.Logger
{
    public sealed class LoggerService : ILoggerService
    {
        private readonly object _fileLock = new();

        private LogLevel _logLevel = LogLevel.Info;
        private string _filePath = "log.log";
        private bool _enabled = true;

        public void Initialize()
        {
            if (ServiceLocator.TryGet<IMessageService>(out var messageService))
                messageService.Subscribe<ServicesInitializedMessage>(OnServicesInitialized);

            Application.logMessageReceivedThreaded += CatchLog;
        }

        private void OnServicesInitialized(ServicesInitializedMessage message)
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_filePath))
                        File.Delete(_filePath);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Failed to reset log file '{_filePath}': {exception}");
                }
            }
        }

        public void Log(string message) => WriteLog(LogLevel.Info, message);
        public void LogVerbose(string message) => WriteLog(LogLevel.Verbose, message);
        public void LogWarning(string message) => WriteLog(LogLevel.Warning, message);
        public void LogError(string message) => WriteLog(LogLevel.Error, message);
        public void LogFatal(string message) => WriteLog(LogLevel.Fatal, message);

        public void SetLogLevel(LogLevel logLevel) => _logLevel = logLevel;

        public void SetLoggerFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Log file path cannot be empty.", nameof(filePath));

            _filePath = Path.Combine(Application.persistentDataPath, $"{filePath}.log");
        }

        private void CatchLog(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    LogFatal($"{logString}\n{stackTrace}");
                    break;
                case LogType.Assert:
                    LogVerbose($"{logString}\n{stackTrace}");
                    break;
                case LogType.Warning:
                    LogWarning(logString);
                    break;
                case LogType.Log:
                    Log(logString);
                    break;
                case LogType.Exception:
                    LogError($"{logString}\n{stackTrace}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void WriteLog(LogLevel level, string message)
        {
            if (!_enabled || level < _logLevel || _logLevel == LogLevel.None)
                return;

            var line = $"[{DateTime.Now:O}] [{level}] {message}";
            Console.WriteLine(line);

            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_filePath, line + Environment.NewLine);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Failed to write log file '{_filePath}': {exception}");
                }
            }
        }
    }
}
