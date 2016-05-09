using System;

namespace Lab5
{
    class Program
    {
        static void ShowArguments()
        {
            Console.WriteLine("Использовать: ");
            Console.WriteLine("либо lab5.exe lunch <количество обедающих>");
            Console.WriteLine("либо lab5.exe phil");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowArguments();
                return;
            }

            switch (args[0])
            {
                case "lunch":
                    if (args.Length < 2)
                    {
                        ShowArguments();
                        return;
                    }
                    byte capacity = byte.Parse(args[1]);
                    LunchServer server = new LunchServer(capacity);
                    server.Start();
                    break;
                case "phil":
                    PhilosopherClient client = new PhilosopherClient();
                    client.Activate();
                    break;
                default:
                    ShowArguments();
                    break;
            }
        }
    }
}
