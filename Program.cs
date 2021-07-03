using System;

namespace FileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            var fmApplication = new FMApplication();
            fmApplication.run();
        }
    }
}