using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMovement;

public class Wordle
{
    public string WordleWord { get; private set; } = string.Empty;
    int ChancesLeft = 6;
    public string Message { get; private set; } = "Waiting for your guess...";
    public bool GameOver = false;
    public Wordle()
    {
        GetWordFromFile();
    }
    public void MakeGuess(string guess)
    {
        if (GameOver) return;

        if (string.IsNullOrWhiteSpace(guess) || guess.Length != 5 || guess.All(char.IsDigit))
        {
            Message = "Not a valid answer, the word must contain 5 letters!";
            return;
        }

        if (guess.ToLower() == WordleWord)
        {
            Message = "You guessed the word!";
            GameOver = true;
        }
        else
        {
            ChancesLeft--;
            if (ChancesLeft == 0)
            {
                Message = $"You didn't guess the word! The correct word was: {WordleWord}!";
                GameOver = true;
            }
            else
            {
                Message = $"Not the right word, you have {ChancesLeft} guesses left!";
            }
        }
    }

    public string GetWordFromFile()
    {
        var lines = File.ReadAllLines("randompickerwordlist.txt");
        var random = new Random();
        int index = random.Next(lines.Length);
        WordleWord = lines[index].Trim().ToLower();
        return WordleWord;
    }
    public void Reset()
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        GameOver = false;
        GetWordFromFile();
    }
}