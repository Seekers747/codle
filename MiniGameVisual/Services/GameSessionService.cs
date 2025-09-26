using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MiniGameVisual.Services;

public class GameSessionService(ProtectedLocalStorage localStorage) : IGameSessionService
{
    private const string RestartKey = "wordle_unfinished_restarts";
  private int _count;
    private bool _initialized;

  public async Task InitializeAsync()
    {
        if (_initialized) return;

        var stored = await localStorage.GetAsync<int?>(RestartKey);
        if (stored.Success && stored.Value.HasValue)
            _count = stored.Value.Value;
        else
            _count = 0;

        _initialized = true;
    }

    public async Task<int> GetUnfinishedRestartCountAsync()
    {
        if (!_initialized) await InitializeAsync();
        return _count;
    }

    public async Task IncrementRestartCountAsync()
    {
        if (!_initialized) await InitializeAsync();
        _count++;
        await localStorage.SetAsync(RestartKey, _count);
    }

    public async Task ResetRestartCountAsync()
    {
        _count = 0;
        await localStorage.DeleteAsync(RestartKey);
    }
}
