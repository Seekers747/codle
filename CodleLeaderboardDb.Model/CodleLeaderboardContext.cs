using Microsoft.EntityFrameworkCore;

namespace CodleLeaderboardDb.Model;

public class CodleLeaderboardContext : DbContext
{
    public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var server = "(local)";
        var user = "CodleLeaderBoardDb";
        var password = "CodleLeaderBoardDb";
        var database = "CodleLeaderBoardDb";

        optionsBuilder.UseSqlServer(
            @$"Server={server};
            Database={database};
            TrustServerCertificate=True;
            ConnectRetryCount=0;
            User Id={user};
            Password={password};
            Encrypt=False;"
        );
    }
}