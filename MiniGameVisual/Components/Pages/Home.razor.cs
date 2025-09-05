using ConsoleMovement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MiniGameVisual.Components.Pages;

public partial class Home
{
    string CurrentGuess = "";
    private readonly char[,] grid = new char[7, 6];
    private readonly string[,] gridStyles = new string[7, 6];
    private int CurrentColumn;
    private int CurrentRow;
    private readonly Wordle wordle = new();
    public List<char> CheckedLetters { get; private set; } = new();
    private ElementReference WordleResetFix;
    private readonly string[] TopRowVisibleKeyboard = ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"];
    private readonly string[] MiddleRowVisibleKeyboard = ["A", "S", "D", "F", "G", "H", "J", "K", "L" ];
    private readonly string[] BottomRowVisibleKeyboard = ["Z", "X", "C", "V", "B", "N", "M"];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];

    private void HandleKeyPress(KeyboardEventArgs evt)
    {
        if (evt.Code == "Backspace" && CurrentGuess.Length > 0)
        {
            CurrentGuess = CurrentGuess[..^1];
            if (CurrentColumn > 0)
            {
                CurrentColumn--;
                gridStyles[CurrentRow, CurrentColumn] = "";
                grid[CurrentRow, CurrentColumn] = ' ';
            }
            Console.WriteLine(CurrentGuess);
            return;
        }
        if (CurrentGuess.Length < 5 && evt.Key.Length == 1)
        {
            CurrentGuess += evt.Key;
            if (CurrentColumn < 5)
            {
                evt.Key = evt.Key.ToUpper();
                grid[CurrentRow, CurrentColumn] = evt.Key[0];
                gridStyles[CurrentRow, CurrentColumn] = "typed";
                CurrentColumn++;
            }
            return;
        }
        if (evt.Code == "Enter" && CurrentRow <= 6 && CurrentGuess.Length == 5 && CurrentGuess.All(char.IsLetter))
        {
            wordle.MakeGuess(CurrentGuess);
            CheckCorrectLetters(CurrentGuess);
            if (wordle.GameOver) return;
            CurrentGuess = string.Empty;
            CurrentRow++;
            CurrentColumn = 0;
            return;
        }
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

    private void CheckCorrectLetters(string guess)
    {
        string target = wordle.WordleWord;
        Dictionary<char, int> targetLetterCounts = new();

        foreach (char letter in target)
        {
            if (targetLetterCounts.ContainsKey(letter))
            {
                targetLetterCounts[letter]++;
            }
            else
            {
                targetLetterCounts[letter] = 1;
            }
        }

        Dictionary<char, int> matchedLetterCounts = new();

        for (int i = 0; i < guess.Length; i++)
        {
            char guessedLetter = guess[i];

            if (guessedLetter == target[i])
            {
                gridStyles[CurrentRow, i] = "correct";
                VisibleKeyboardStyle[guessedLetter.ToString().ToUpper()] = "CorrectLetter";
                matchedLetterCounts[guessedLetter] = matchedLetterCounts.GetValueOrDefault(guessedLetter) + 1;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            char guessedLetter = guess[i];

            if (gridStyles[CurrentRow, i] == "correct") continue;

            bool isInTarget = target.Contains(guessedLetter);
            int matchedSoFar = matchedLetterCounts.GetValueOrDefault(guessedLetter);
            int allowedMatches = targetLetterCounts.GetValueOrDefault(guessedLetter);

            if (isInTarget && matchedSoFar < allowedMatches)
            {
                gridStyles[CurrentRow, i] = "present";
                if (!VisibleKeyboardStyle.ContainsKey(guessedLetter.ToString().ToUpper()) || VisibleKeyboardStyle[guessedLetter.ToString().ToUpper()] == "AbsentLetter")
                {
                    VisibleKeyboardStyle[guessedLetter.ToString().ToUpper()] = "PresentLetter";
                }
                matchedLetterCounts[guessedLetter] = matchedSoFar + 1;
            }
            else
            {
                gridStyles[CurrentRow, i] = "absent";
                if (!VisibleKeyboardStyle.ContainsKey(guessedLetter.ToString().ToUpper()))
                    VisibleKeyboardStyle[guessedLetter.ToString().ToUpper()] = "AbsentLetter";
            }
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
}