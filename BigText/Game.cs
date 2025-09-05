using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigText;

internal class Game
{
    internal void Run()
    {
        Console.Write("Enter a letter: ");
        char input = char.ToLower(Console.ReadKey().KeyChar);
        Console.WriteLine();

        if (input >= 'a' && input <= 'z')
        {
            Console.WriteLine($"You pressed the letter: {input}");
        }
        else
        {
            Console.WriteLine("That's not a letter.");
        }
    }
}
