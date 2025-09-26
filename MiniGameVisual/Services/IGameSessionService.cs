namespace MiniGameVisual.Services;

public interface IGameSessionService
{
    /// <summary>Return current count of restarts that happened while the game was unfinished.</summary>
    Task<int> GetUnfinishedRestartCountAsync();

    /// <summary>Call when the player restarts while the game is not finished (increment + persist).</summary>
    Task IncrementRestartCountAsync();

    /// <summary>Call when the player finishes a game (win or lose) to reset the unfinished restart counter.</summary>
    Task ResetRestartCountAsync();

    /// <summary>Optional helper to ensure the service has loaded persisted data.</summary>
    Task InitializeAsync();
}
