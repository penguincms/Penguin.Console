using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Penguin.PgConsole
{
    public static class ConsoleEx
    {
        public static bool Ask(string prompt)
        {
            do
            {
                Console.Write($"{prompt} [y/n]: ");

                ConsoleKeyInfo key  = Console.ReadKey();

                Console.WriteLine();

                if (key.Key == ConsoleKey.Y)
                {
                    return true;
                } else if (key.Key == ConsoleKey.N)
                {
                    return false;
                }

                
            } while (true);
        }


        public static T SelectItem<T>(IEnumerable<T> selectFrom, string prompt, Func<T, string> toString = null)
        {
            List<T> options = selectFrom.ToList();

            do
            {
                int i = 0;

                foreach (T o in options)
                {
                    string label = toString?.Invoke(o) ?? o.ToString();

                    System.Console.WriteLine($"[{i++}] {label}");
                }

                System.Console.WriteLine();

                System.Console.Write($"{prompt}: ");

                string si = System.Console.ReadLine();

                if (int.TryParse(si, out int selected))
                {
                    return options[selected];
                }
            } while (true);
        }
    }
}
