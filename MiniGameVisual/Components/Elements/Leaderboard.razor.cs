using CodleLeaderboardDb.Model;
using Microsoft.AspNetCore.Components;

namespace MiniGameVisual.Components.Elements
{
  public partial class Leaderboard
  {
    [Parameter] public List<LeaderboardEntry> LeaderboardList { get; set; } = [];
    private static string FormatTime(TimeSpan time) =>
    time.TotalSeconds < 60 ? $"{time.Seconds}s" : $"{(int)time.TotalMinutes}m {time.Seconds}s";
  }
}
