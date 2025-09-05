using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMovement;

public class DrawInConsole
{
    int x = 10;
    int y = 10;

    public void Run()
    {

        Console.SetCursorPosition(x, y);
        Console.Write(' ');
        while (true)
        {
            while (Console.KeyAvailable)
            {
                var userInput = Console.ReadKey(true);
                if (userInput.KeyChar == 'q')
                {
                    break;
                }
                else if (userInput.KeyChar == 'd')
                {
                    Move(1, 0, true);
                }
                else if (userInput.KeyChar == 'D')
                {
                    Move(1, 0, false);
                }
                else if (userInput.KeyChar == 'a')
                {
                    Move(-1, 0, true);
                }
                else if (userInput.KeyChar == 'A')
                {
                    Move(-1, 0, false);
                }
                else if (userInput.KeyChar == 's')
                {
                    Move(0, 1, true);
                }
                else if (userInput.KeyChar == 'S')
                {
                    Move(0, 1, false);
                }
                else if (userInput.KeyChar == 'w')
                {
                    Move(0, -1, true);
                }
                else if (userInput.KeyChar == 'W')
                {
                    Move(0, -1, false);
                }
                else if(userInput.KeyChar == 'c')
                {
                    Console.Clear();
                }
            }

            // render tijd
            Console.SetCursorPosition(112, 0);
            var time = DateTime.Now.ToLongTimeString();
            Console.Write(time);

            // zorg voor 60fps
            Thread.Sleep(1000 / 60);
        }
    }

    private void Move(int dx, int dy, bool draw)
    {
        int newx = x + dx;
        int newy = y + dy;

        if(newx >= 0 && newx < Console.WindowWidth) x = newx;
        if(newy >= 0 && newy < Console.WindowHeight) y = newy;

        Console.SetCursorPosition(x, y);
        if (draw)
        {
            if (dx == 1 && dy == 0)
            {
                Console.Write('d');
            }
            else if (dx == 0 && dy == 1)
            {
                Console.Write('s');
            }
            else if (dx == -1 && dy == 0)
            {
                Console.Write('a');
            }
            else if (dx == 0 && dy == -1)
            {
                Console.Write('w');
            }
        }
        else
        {
            Console.Write(' ');
        }
    }
}
