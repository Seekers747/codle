using ConsoleMovement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

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

    protected override void OnInitialized()
    {
        wordle.StartGame();
    }

    private void OnPhysicalKeyboardClick(KeyboardEventArgs evt)
    {
        HandleKeyPress(evt);
    }

    private void OnVisibleKeyboardClick(string letter)
    {
        var evt = new KeyboardEventArgs { Key = letter, Code = letter };
        Console.WriteLine(evt);
        HandleKeyPress(evt);
    }

    private void HandleKeyPress(KeyboardEventArgs evt)
    {
        CurrentGuess = CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        if (evt.Code == "Backspace")
        {
            HandleBackspace();
            return;
        }

        if (evt.Key.Length == 1 && char.IsLetter(evt.Key[0]))
        {
            HandleLetterInput(evt.Key[0]);
            return;
        }

        if (evt.Code == "Enter")
        {
            HandleEnter();
            return;
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

        if (wordle.GameOver) return;

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
                matchedCounts[letter] = matchedSoFar + 1;
            }
            else
            {
                gridStyles[CurrentRow, i] = "absent";
                UpdateKeyboardStyle(letter, "AbsentLetter");
            }
        }
    }

    private void UpdateKeyboardStyle(char letter, string style)
    {
        string key = letter.ToString().ToUpper();

        if (!VisibleKeyboardStyle.ContainsKey(key) ||
            (style == "PresentLetter" && VisibleKeyboardStyle[key] == "AbsentLetter") ||
            style == "CorrectLetter")
        {
            VisibleKeyboardStyle[key] = style;
        }
    }

    private void RestartGame()
    {
        wordle.Reset();
        CurrentGuess = string.Empty;
        CurrentRow = 0;
        CurrentColumn = 0;
        VisibleKeyboardStyle.Clear();

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }

        WordleResetFix.FocusAsync();
    }

    private string LetterColorChange(string letter)
    {
        if (VisibleKeyboardStyle.TryGetValue(letter, out var style))
        {
            return style;
        }
        return "";
    }

    private static bool CheckIfGuessIsValidWord(string ValidGuess)
    {
        var lines = File.ReadAllLines("combined_wordlist.txt");
        foreach (var line in lines)
        {
            if (line.StartsWith(ValidGuess))
            {
                return true;
            }
        }
        return false;
    }
}