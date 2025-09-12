using ConsoleMovement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using static ConsoleMovement.Wordle;

namespace MiniGameVisual.Components.Pages;

public partial class Home
{
    string CurrentGuess = "";
    private readonly char[,] grid = new char[7, 6];
    private readonly string[,] gridStyles = new string[7, 6];
    private int CurrentColumn;
    private int CurrentRow;
    private readonly Wordle wordle = new();
    public List<char> CheckedLetters { get; private set; } = [];
    private ElementReference WordleResetFix;
    private readonly string[] TopRowVisibleKeyboard = ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"];
    private readonly string[] MiddleRowVisibleKeyboard = ["A", "S", "D", "F", "G", "H", "J", "K", "L" ];
    private readonly string[] BottomRowVisibleKeyboard = ["Z", "X", "C", "V", "B", "N", "M"];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];
    private readonly List<Wordle.LetterFeedback> LastGuessFeedback = [];
    private CancellationTokenSource? computerCancelSource;
    readonly List<string> allowedWords = Wordle.LoadAllWords();
    private int elapsedSeconds = 0;
    private int? savedTime = null;
    private System.Threading.Timer? timer;
    private List<LeaderboardEntry> leaderboard = [];

    protected override void OnInitialized()
    {
        wordle.StartGame();
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }
    }

    private void OnPhysicalKeyboardClick(KeyboardEventArgs evt)
    {
        if (!showPopup && !wordle.GameOver) HandleKeyPress(evt);
    }

    private void OnVisibleKeyboardClick(string letter)
    {
        var evt = new KeyboardEventArgs { Key = letter, Code = letter };
        Console.WriteLine(evt);
        HandleKeyPress(evt);
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
            HandleKeyPress(evt);
            if (wordle.GameOver) break;
            await Task.Delay(200);
            StateHasChanged();
        }

        var enterEvt = new KeyboardEventArgs
        {
            Key = "Enter",
            Code = "Enter"
        };
        LastGuessFeedback.Clear();
        HandleKeyPress(enterEvt);
    }

    private async Task RunComputerAttemptsAsync()
    {
        await RestartGame();
        computerCancelSource = new CancellationTokenSource();
        var token = computerCancelSource.Token;
        wordle.DidComputerPlay = true;

        for (int i = 0; i < 6; i++)
        {
            if (wordle.GameOver || token.IsCancellationRequested)
                break;

            string guess = wordle.MakeComputerGuess(LastGuessFeedback);
            await SendComputerGuessAsync(guess);
            StateHasChanged();

            if (wordle.GameOver || token.IsCancellationRequested)
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

    private void HandleKeyPress(KeyboardEventArgs evt)
    {
        CurrentGuess = CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        switch (evt.Code)
        {
            case "Backspace":
                HandleBackspace();
                return;
            case "Enter":
                HandleEnter();
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

    private void HandleEnter()
    {
        if (CurrentGuess.Length != 5 || CurrentRow > 6 || !CurrentGuess.All(char.IsLetter)) return;
        if (!CheckIfGuessIsValidWord(CurrentGuess)) return;

        wordle.MakeGuess(CurrentGuess);
        CheckCorrectLetters(CurrentGuess);

        if (string.Equals(CurrentGuess, wordle.WordleWord, StringComparison.OrdinalIgnoreCase)
            && !wordle.DidComputerPlay)
        {
            GiveDataToLeaderboard();
        }

        CurrentGuess = string.Empty;
        CurrentRow++;
        CurrentColumn = 0;
    }

    private void CheckCorrectLetters(string guess)
    {
        string target = wordle.WordleWord;
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

        wordle.Reset();
        CurrentGuess = string.Empty;
        CurrentRow = 0;
        CurrentColumn = 0;
        VisibleKeyboardStyle.Clear();
        LastGuessFeedback.Clear();

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }

        await WordleResetFix.FocusAsync();
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
        wordle.SetUserChosenWord(UserGivenGuessable);
        StateHasChanged();
        await RunComputerAttemptsAsync();
    }

    private void StartTimer()
    {
        timer = new Timer(_ =>
        {
            elapsedSeconds++;

            if (wordle.GameOver)
            {
                StopAndSave();
            }

            InvokeAsync(StateHasChanged);
        }, null, 0, 1000);
    }

    private void StopAndSave()
    {
        Dispose();
        savedTime = elapsedSeconds;
        Console.WriteLine($"Game finished in {savedTime} seconds.");
        elapsedSeconds = 0;
    }

    public void Dispose()
    {
        if (timer == null) return;
        timer.Dispose();
        timer = null;
    }
    public class LeaderboardEntry
    {
        public required string PlayerName { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public int Attempts { get; set; }
    }

    private void GiveDataToLeaderboard()
    {
        var entry = new LeaderboardEntry
        {
            PlayerName = "Player1",
            TimeTaken = TimeSpan.FromSeconds(elapsedSeconds),
            Attempts = CurrentRow + 1
        };

        leaderboard.Add(entry);
        leaderboard = [.. leaderboard
            .OrderBy(e => e.TimeTaken)
            .ThenBy(e => e.Attempts)
            .Take(10)];
    }

    private static string FormatTime(TimeSpan time)
    {
        if (time.TotalSeconds < 60)
            return $"{time.Seconds}s";
        else
            return $"{time.Minutes}m {time.Seconds}s";
    }

    private void GivePlayerName()
    {
        // get teh player name to put on leaderboard
    }
}