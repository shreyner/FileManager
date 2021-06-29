using System;
using System.IO;
using System.Linq;

namespace FileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            // Window Size
            var windowHeight = Console.WindowHeight;
            var windowWidth = Console.WindowWidth;

            // var consoleWindow = new ConsoleWindow(heigth: windowHeight - 2, width: windowWidth);
            // consoleWindow.Print();
            // Console.ReadLine();

            // Listring

            // var startedPath = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..");
            // Listing(startedPath);

            var commandManager = new CommandManager();

            while (true)
            {
                var str = Console.ReadLine();
                var (command, arguments) = commandManager.parseStringToCommand(str);
                commandManager.execCommand(command, arguments);
            }
        }

        static void Listing(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                Console.WriteLine($"/{directory.Name}/");
            }

            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                Console.WriteLine($"/{fileInfo.Name}");
            }

            Console.WriteLine(Directory.GetDirectories(Directory.GetCurrentDirectory()));
            Console.WriteLine(Directory.GetFiles(Directory.GetCurrentDirectory()));
        }
    }
}