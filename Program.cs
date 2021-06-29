using System;
using System.IO;

namespace FileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            
            // Window Size
            var windowHeight = Console.WindowHeight;
            var windowWidth = Console.WindowWidth;

            var consoleWindow = new ConsoleWindow(heigth: windowHeight - 2, width: windowWidth);
            Console.Clear();
            consoleWindow.Print();
            Console.ReadLine();

            // Listring

            var startedPath = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..");

            var directoryInfo = new DirectoryInfo(startedPath);

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