using System;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Represents the result of launching a game.
    /// </summary>
    public class LaunchResult
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }
        public int PlayTimeSeconds { get; }
        public DateTime? StartTime { get; }
        public DateTime? EndTime { get; }
        public int ExitCode { get; }

        private LaunchResult(bool success, string? errorMessage, int playTimeSeconds,
            DateTime? startTime, DateTime? endTime, int exitCode)
        {
            Success = success;
            ErrorMessage = errorMessage;
            PlayTimeSeconds = playTimeSeconds;
            StartTime = startTime;
            EndTime = endTime;
            ExitCode = exitCode;
        }

        public static LaunchResult Successful(int playTimeSeconds, DateTime startTime, DateTime endTime, int exitCode = 0)
        {
            return new LaunchResult(true, null, playTimeSeconds, startTime, endTime, exitCode);
        }

        public static LaunchResult Failure(string errorMessage)
        {
            return new LaunchResult(false, errorMessage, 0, null, null, -1);
        }
    }
}
