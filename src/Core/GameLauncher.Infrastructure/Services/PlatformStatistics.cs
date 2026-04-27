using System;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Statistics for a gaming platform.
    /// </summary>
    public class PlatformStatistics
    {
        public string PlatformName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int FavoriteGames { get; set; }
        public int CompletedGames { get; set; }
        public long TotalPlayTimeSeconds { get; set; }
        public int TotalPlayCount { get; set; }
        public double CompletionRate => TotalGames > 0 ? (double)CompletedGames / TotalGames * 100 : 0;
        public string MostPlayedGameTitle { get; set; } = string.Empty;
        public long MostPlayedGameTime { get; set; }
        public DateTime? LastPlayed { get; set; }

        public TimeSpan TotalPlayTime => TimeSpan.FromSeconds(TotalPlayTimeSeconds);

        public string FormattedTotalPlayTime =>
            TotalPlayTimeSeconds < 3600
                ? $"{(int)TotalPlayTime.TotalMinutes}m"
                : $"{(int)TotalPlayTime.TotalHours}h {TotalPlayTime.Minutes}m";
    }
}
