namespace CodleLeaderboardDb.Model;

public class LeaderboardEntry
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public TimeSpan TimeTaken { get; set; }
    public int Attempts { get; set; }
    public DateTime DateAchieved { get; set; }
}