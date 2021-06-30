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

            var fmApplication = new FMApplication();
            fmApplication.run();
        }
    }
}