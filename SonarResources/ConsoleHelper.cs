using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources
{
    public static class ConsoleHelper
    {
        public static bool AskUserYN(string message)
        {
            Console.Write($"{message} (Y/N): ");
            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Y)
                {
                    Console.WriteLine("Yes");
                    return true;
                }
                else if (key == ConsoleKey.N)
                {
                    Console.WriteLine("No");
                    return false;
                }
            }
        }
    }
}
