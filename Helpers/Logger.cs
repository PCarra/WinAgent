using System;
using System.IO;

namespace WindowsAgentService.Helpers
{
    public static class Logger
    {
        private static readonly string logDirectory = @"C:\Logs";
        private static readonly string logFilePath = @"C:\Logs\AgentLog.txt";

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            Log("ERROR", $"{message} {ex?.ToString()}");
        }

        private static void Log(string level, string message)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                string logMessage = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {level} | {message}";
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log: {ex.Message}");
            }
        }
    }
}
