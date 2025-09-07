using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMovement;

public class Wordle
{
    public string WordleWord { get; private set; } = string.Empty;
    public string WordleWordExplain { get; private set; } = string.Empty;
    private int ChancesLeft = 6;
    public string Message { get; private set; } = "Waiting for your guess...";
    public bool GameOver { get; private set; } = false;

    public void StartGame()
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        GameOver = false;
        LoadRandomCodleAnswer();
    }

    public void MakeGuess(string guess)
    {
        guess = guess.ToLower();

        if (GameOver) return;

        if (!IsValidGuess(guess))
        {
            Message = "Not a valid answer, the word must contain 5 letters!";
            return;
        }

        if (guess == WordleWord)
        {
            Message = "You guessed the word!";
            GameOver = true;
            return;
        }

        ChancesLeft--;

        GameOver = ChancesLeft == 0;
        Message = GameOver
            ? $"You didn't guess the word! The correct word was: {WordleWord}!"
            : $"Not the right word, you have {ChancesLeft} guesses left!";
    }

    private void LoadRandomCodleAnswer()
    {
        var lines = File.ReadAllLines("randompickerwordlist.txt");
        var random = new Random();
        int index = random.Next(lines.Length);

        var parts = lines[index].Split(':');
        WordleWord = parts[0].Trim().ToLower();
        WordleWordExplain = parts.Length > 1 ? parts[1].Trim() : "Geen uitleg beschikbaar.";
    }

    public void Reset()
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        GameOver = false;
        LoadRandomCodleAnswer();
    }

    private static bool IsValidGuess(string guess)
    {
        return !string.IsNullOrWhiteSpace(guess) &&
               guess.Length == 5 &&
               !guess.All(char.IsDigit);
    }
}