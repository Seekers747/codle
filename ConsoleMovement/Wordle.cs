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
    public List<string> ComputerGuessedWordsList = [];
    private string? UserChosenWord = null;
    public bool DidComputerPlay { get; set; } = false;

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
        var lines = File.ReadAllLines("ExpandedWordList.txt");
        var random = new Random();
        int index = random.Next(lines.Length);
        var parts = lines[index].Split(':');

        if (!string.IsNullOrWhiteSpace(UserChosenWord))
        {
            WordleWord = UserChosenWord;

            string? match = lines.FirstOrDefault(line =>
                line.StartsWith(UserChosenWord + ":", StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                WordleWordExplain = "Geen uitleg beschikbaar.";
            }
            else
            {
                var userParts = match.Split(':');
                WordleWordExplain = userParts.Length > 1 ? userParts[1].Trim() : "Geen uitleg beschikbaar.";
            }

            UserChosenWord = null;
            return;
        }
        else
        {
            WordleWord = parts[0].Trim().ToLower();
            WordleWordExplain = parts.Length > 1 ? parts[1].Trim() : "Geen uitleg beschikbaar.";
        }
    }

    public void Reset()
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        DidComputerPlay = false;
        GameOver = false;
        LoadRandomCodleAnswer();
        ComputerGuessedWordsList.Clear();
    }

    private static bool IsValidGuess(string guess)
    {
        return !string.IsNullOrWhiteSpace(guess) &&
               guess.Length == 5 &&
               !guess.All(char.IsDigit);
    }

    public class LetterFeedback(char letter, StateEnum state, int position)
    {
        public char Letter { get; set; } = letter;
        public StateEnum State { get; set; } = state;
        public int Position { get; set; } = position;
    }

    public static List<string> LoadAllWords()
    {
        return [.. File.ReadAllLines("ExpandedWordList.txt")
            .Select(line => line.Split(':')[0].Trim().ToLower())
            .Where(word => word.Length == 5)];
    }

    public string MakeComputerGuess(List<LetterFeedback> previousFeedback)
    {
        List<string> allWords = LoadAllWords();
        List<string> possibleWords = [.. allWords.Where(word => !ComputerGuessedWordsList.Contains(word))];

        foreach (var feedback in previousFeedback)
        {
            char letter = feedback.Letter;
            int position = feedback.Position;
            StateEnum state = feedback.State;

            possibleWords = [.. possibleWords.Where(word =>
            {
                return state switch
                {
                    StateEnum.Correct => word[position] == letter,
                    StateEnum.Present => word.Contains(letter) && word[position] != letter,
                    StateEnum.Absent => !word.Contains(letter),
                    _ => true,
                };
            })];
        }

        if (possibleWords.Count == 0)
        {
            Console.WriteLine("No valid words left based on feedback. Picking random.");
            possibleWords = [.. allWords.Where(word => !ComputerGuessedWordsList.Contains(word))];
        }
        else
        {
            Console.WriteLine("Found some feedback to work with.");
        }

        var random = new Random();
        string computerGuess = possibleWords[random.Next(possibleWords.Count)];

        ComputerGuessedWordsList.Add(computerGuess);

        Console.WriteLine($"Computer guesses: {computerGuess}");
        return computerGuess;
    }
    public void SetUserChosenWord(string word) => UserChosenWord = word.Trim().ToLower();
}