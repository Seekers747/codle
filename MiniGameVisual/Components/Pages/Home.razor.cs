using CodleLeaderboardDb.Model;
using ConsoleMovement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using static ConsoleMovement.Codle;

namespace MiniGameVisual.Components.Pages;

public partial class Home
{
    string CurrentGuess = "";
    private readonly char[,] grid = new char[7, 6];
    private readonly string[,] gridStyles = new string[7, 6];
    private int CurrentColumn;
    private int CurrentRow;
    private readonly Codle codle = new();
    public List<char> CheckedLetters { get; private set; } = [];
    private ElementReference CodleResetFix;
    private readonly string[] TopRowVisibleKeyboard = ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"];
    private readonly string[] MiddleRowVisibleKeyboard = ["A", "S", "D", "F", "G", "H", "J", "K", "L"];
    private readonly string[] BottomRowVisibleKeyboard = ["Z", "X", "C", "V", "B", "N", "M"];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];
    private readonly List<LetterFeedback> LastGuessFeedback = [];
    private CancellationTokenSource? computerCancelSource;
    readonly List<string> allowedWords = LoadAllWords();
    private int elapsedSeconds;
    private int savedTime;
    private Timer? timer;
    public List<LeaderboardEntry> leaderboard = [];
    private CancellationTokenSource? timerCts;
    private bool ShowUsernameInputPopup = false;

    protected override void OnInitialized()
    {
        codle.StartGame();
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }
    }

    private async Task OnPhysicalKeyboardClick(KeyboardEventArgs evt)
    {
        if (!showPopup && !codle.GameOver) await HandleKeyPress(evt);
    }

    private async Task OnVisibleKeyboardClick(string letter)
    {
        var evt = new KeyboardEventArgs { Key = letter, Code = letter };
        Console.WriteLine(evt);
        await HandleKeyPress(evt);
    }

    private async Task SendComputerGuessAsync(string ComputerGuess)
    {
        foreach (char letter in ComputerGuess)
        {
            var evt = new KeyboardEventArgs
            {
                Key = letter.ToString(),
                Code = $"Key{char.ToUpper(letter)}"
            };
            await HandleKeyPress(evt);
            if (codle.GameOver) break;
            await Task.Delay(200);
            StateHasChanged();
        }

        var enterEvt = new KeyboardEventArgs
        {
            Key = "Enter",
            Code = "Enter"
        };
        LastGuessFeedback.Clear();
        await HandleKeyPress(enterEvt);
    }

    private async Task RunComputerAttemptsAsync()
    {
        await RestartGame();
        computerCancelSource = new CancellationTokenSource();
        var token = computerCancelSource.Token;
        codle.DidComputerPlay = true;

        for (int i = 0; i < 6; i++)
        {
            if (codle.GameOver || token.IsCancellationRequested)
                break;

            string guess = codle.MakeComputerGuess(LastGuessFeedback);
            await SendComputerGuessAsync(guess);
            StateHasChanged();

            if (codle.GameOver || token.IsCancellationRequested)
                break;

            try
            {
                await Task.Delay(1500, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void SendCharAndState(char letter, StateEnum state, int position)
    {
        LastGuessFeedback.Add(new LetterFeedback(letter, state, position));
    }

    private async Task HandleKeyPress(KeyboardEventArgs evt)
    {
        CurrentGuess = CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        switch (evt.Code)
        {
            case "Backspace":
                HandleBackspace();
                return;
            case "Enter":
                await HandleEnter();
                return;
            case "Tab":
                return;
        }

        if (evt.Key.Length == 1 && char.IsLetter(evt.Key[0]))
        {
            HandleLetterInput(evt.Key[0]);
        }
    }

    private void HandleLetterInput(char key)
    {
        if (CurrentGuess.Length >= 5 || CurrentColumn >= 5) return;

        char upperKey = char.ToUpper(key);
        CurrentGuess += upperKey;
        grid[CurrentRow, CurrentColumn] = upperKey;
        gridStyles[CurrentRow, CurrentColumn] = "typed";
        CurrentColumn++;
        if (grid[0, 0] != ' ' && timer == null)
        {
            StartTimer();
        }
    }

    private void HandleBackspace()
    {
        if (CurrentGuess.Length == 0 || CurrentColumn == 0) return;

        CurrentGuess = CurrentGuess[..^1];
        CurrentColumn--;
        gridStyles[CurrentRow, CurrentColumn] = "";
        grid[CurrentRow, CurrentColumn] = ' ';
    }

    private async Task HandleEnter()
    {
        if (CurrentGuess.Length != 5 || CurrentRow > 6 || !CurrentGuess.All(char.IsLetter)) return;
        if (!CheckIfGuessIsValidWord(CurrentGuess)) return;

        codle.MakeGuess(CurrentGuess);
        CheckCorrectLetters(CurrentGuess);

        if (string.Equals(CurrentGuess, codle.CodleWord, StringComparison.OrdinalIgnoreCase)
            && !codle.DidComputerPlay)
        {
            ShowUsernameInputPopup = true;
            await SubmitPlayerName();
        }

        CurrentGuess = string.Empty;
        CurrentRow++;
        CurrentColumn = 0;
    }

    private void CheckCorrectLetters(string guess)
    {
        string target = codle.CodleWord;
        var targetCounts = new Dictionary<char, int>();
        var matchedCounts = new Dictionary<char, int>();

        foreach (char letter in target)
            targetCounts[letter] = targetCounts.GetValueOrDefault(letter) + 1;

        for (int i = 0; i < guess.Length; i++)
        {
            char letter = guess[i];
            if (letter == target[i])
            {
                gridStyles[CurrentRow, i] = "correct";
                UpdateKeyboardStyle(letter, "CorrectLetter");
                SendCharAndState(letter, StateEnum.Correct, i);
                matchedCounts[letter] = matchedCounts.GetValueOrDefault(letter) + 1;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            if (gridStyles[CurrentRow, i] == "correct") continue;

            char letter = guess[i];
            bool isInTarget = target.Contains(letter);
            int matchedSoFar = matchedCounts.GetValueOrDefault(letter);
            int allowedMatches = targetCounts.GetValueOrDefault(letter);

            if (isInTarget && matchedSoFar < allowedMatches)
            {
                gridStyles[CurrentRow, i] = "present";
                UpdateKeyboardStyle(letter, "PresentLetter");
                SendCharAndState(letter, StateEnum.Present, i);
                matchedCounts[letter] = matchedSoFar + 1;
            }
            else
            {
                gridStyles[CurrentRow, i] = "absent";
                UpdateKeyboardStyle(letter, "AbsentLetter");
                SendCharAndState(letter, StateEnum.Absent, i);
            }
        }
    }

    private void UpdateKeyboardStyle(char letter, string style)
    {
        string key = letter.ToString().ToUpper();

        if (!VisibleKeyboardStyle.TryGetValue(key, out string? value) ||
            (style == "PresentLetter" && value == "AbsentLetter") ||
            style == "CorrectLetter")
        {
            value = style;
            VisibleKeyboardStyle[key] = value;
        }
    }

    private async Task RestartGame()
    {
        computerCancelSource?.Cancel();
        computerCancelSource = null;

        codle.Reset();
        CurrentGuess = string.Empty;
        CurrentRow = 0;
        CurrentColumn = 0;
        VisibleKeyboardStyle.Clear();
        LastGuessFeedback.Clear();
        playerName = string.Empty;
        showNameError = false;
        StopAndSave();

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }

        await CodleResetFix.FocusAsync();
    }

    private string LetterColorChange(string letter) =>
        (VisibleKeyboardStyle.TryGetValue(letter, out var style)) ? style : string.Empty;

    readonly static string[] lines = [.. File.ReadAllLines("combined_wordlist.txt").OrderBy(line => line)];

    private static bool CheckIfGuessIsValidWord(string ValidGuess) => Array.BinarySearch(lines, ValidGuess) > 0;

    private bool showPopup = false;
    private void OpenPopup() => showPopup = true;
    private void ClosePopup() => showPopup = false;

    public async Task SubmitUserSelectedWord(string UserGivenGuessable)
    {
        ClosePopup();
        codle.SetUserChosenWord(UserGivenGuessable);
        StateHasChanged();
        await RunComputerAttemptsAsync();
    }

    private void StartTimer()
    {
        timerCts = new CancellationTokenSource();
        timer = new Timer(_ =>
        {
            elapsedSeconds++;

            if (codle.GameOver)
            {
                StopAndSave();
            }

            InvokeAsync(StateHasChanged);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void StopAndSave()
    {
        DisposeTimer();
        savedTime = elapsedSeconds;
        Console.WriteLine($"Game finished in {savedTime} seconds.");
        elapsedSeconds = 0;
    }

    public void DisposeTimer()
    {
        timer?.Dispose();
        timer = null;

        timerCts?.Cancel();
        timerCts?.Dispose();
        timerCts = null;
    }

    private async Task GiveDataToLeaderboard(string playerName)
    {
        var entry = new LeaderboardEntry
        {
            Username = playerName,
            TimeTaken = TimeSpan.FromSeconds(savedTime),
            Attempts = CurrentRow,
            DateAchieved = DateTime.Now
        };

        using var context = new CodleLeaderboardContext();
        context.LeaderboardEntries.Add(entry);
        await context.SaveChangesAsync();
        await RefreshLeaderboardAsync();
    }

    private static string FormatTime(TimeSpan time) =>
        time.TotalSeconds < 60 ? $"{time.Seconds}s" : $"{(int)time.TotalMinutes}m {time.Seconds}s";

    private async Task RefreshLeaderboardAsync()
    {
            using var context = new CodleLeaderboardContext();
            leaderboard = await context.LeaderboardEntries
                .OrderBy(e => e.TimeTaken)
                .Take(10)
                .ToListAsync()
                .ConfigureAwait(false);

            await InvokeAsync(StateHasChanged);
    }


    private bool showNameError = false;
    private string? playerName;
    private async Task SubmitPlayerName()
    {
        if (!string.IsNullOrWhiteSpace(playerName) && playerName.Length <= 20)
        {
            await GiveDataToLeaderboard(playerName);
            showNameError = false;
            ShowUsernameInputPopup = false;
        }
        else
        {
            showNameError = true;
        }
    }
}