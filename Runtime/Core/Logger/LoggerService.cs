using System;
using System.IO;
using UnityEngine;

namespace IIMLib.Core
{
    public class LoggerService : ILoggerService
    {
        private const string DEFAULT_PATH_NAME = "log.log";
        private const string INFO_LVL_NAME = "INFO";
        private const string VERBOSE_LVL_NAME = "VERBOSE";
        private const string WARNING_LVL_NAME = "WARNING";
        private const string ERROR_LVL_NAME = "ERROR";
        private const string FATAL_LVL_NAME = "FATAL";

        private LogLevel _LOGLevel;
        private string _LogFilePath;

        public bool Enabled { get; set; }

        public LoggerService(string logFilePath = DEFAULT_PATH_NAME, LogLevel logLevel = LogLevel.Info)
        {
            _LogFilePath = logFilePath;
            _LOGLevel = logLevel;
            Enabled = logLevel != LogLevel.None;
        }

        public void Initialize()
        {
            ServiceLocator.Get<IMessageService>().Subscribe<ServicesInitializedMessage>(ServicesInitialized);
        }

        private void ServicesInitialized(ServicesInitializedMessage obj)
        {
            Application.logMessageReceivedThreaded += CatchLog;
            File.Delete(_LogFilePath);
        }

        private void CatchLog(string condition, string stacktrace, LogType type)
        {
            var message = condition;

            if (type is LogType.Exception or LogType.Error)
            {
                message += $"\nStackTrace:\n{stacktrace}";
            }

            // Categorize the log based on LogType.
            switch (type)
            {
                case LogType.Error:
                    LogFatal(message);
                    break;
                case LogType.Assert:
                    LogVerbose(message);
                    break;
                case LogType.Warning:
                    LogWarning(message);
                    break;
                case LogType.Log:
                    Log(message);
                    break;
                case LogType.Exception:
                    LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Log(in string message)
        {
            if (!Enabled) return;

            if (_LOGLevel <= LogLevel.Info)
            {
                WriteLog(INFO_LVL_NAME, message);
            }
        }

        public void LogVerbose(in string message)
        {
            if (!Enabled) return;

            if (_LOGLevel <= LogLevel.Verbose)
            {
                WriteLog(VERBOSE_LVL_NAME, message);
            }
        }

        public void LogWarning(in string message)
        {
            if (!Enabled) return;

            if (_LOGLevel <= LogLevel.Warning)
            {
                WriteLog(WARNING_LVL_NAME, message);
            }
        }

        public void LogError(in string message)
        {
            if (!Enabled) return;

            if (_LOGLevel <= LogLevel.Error)
            {
                WriteLog(ERROR_LVL_NAME, message);
            }
        }

        public void LogFatal(in string message)
        {
            if (!Enabled) return;

            if (_LOGLevel <= LogLevel.Fatal)
            {
                WriteLog(FATAL_LVL_NAME, message);
                LogFatalExtraCalls(message);
            }
        }

        protected virtual void LogFatalExtraCalls(in string message)
        {
        }

        public void SetLogLevel(in LogLevel level)
        {
            _LOGLevel = level;
            Log($"Log level set to: {level}");
            Enabled = level != LogLevel.None;
        }

        public void SetLoggerFilePath(in string filePath)
        {
            _LogFilePath = Path.Combine(Application.persistentDataPath, filePath + ".log");
        }

        private void WriteLog(in string level, in string message)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] :: [{level}] :: {message}";
            Console.WriteLine(logEntry); // Outputs to console, useful for in-editor viewing
            File.AppendAllText(_LogFilePath, logEntry + Environment.NewLine);
        }
    }
}