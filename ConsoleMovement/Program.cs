namespace ConsoleMovement;
internal class Program
{
    static readonly DrawInConsole game = new();
    static readonly Wordle game2 = new();

    static void Main(string[] args)
    {
        game.Run();
    }
}