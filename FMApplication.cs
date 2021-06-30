using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace FileManager
{
    public class FMApplication
    {
        private string currentPath = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..");

        public FMApplication()
        {
        }

        public void run()
        {
            while (true)
            {
                var str = Console.ReadLine();
                var (command, arguments) = parseStringToCommand(str);
                execCommand(command, arguments);
            }
        }

        private (string command, string[] arguments) parseStringToCommand(string str)
        {
            var arrayList = new List<string>(str.Trim().Split(" "));

            var command = arrayList.First();
            var arguments = arrayList.Count > 1
                ? arrayList.GetRange(1, arrayList.Count - 1).ToArray()
                : Array.Empty<string>();

            return (command, arguments);
        }

        private void execCommand(string command, string[] arguments)
        {
            switch (command)
            {
                case "ls":
                    ListDirectoryFile(arguments);
                    return;
                case "cp":
                    CopyFile(arguments);
                    return;
                case "rm":
                    RemoveFile(arguments);
                    return;
                case "help":
                    Console.WriteLine("Call help command");
                    return;
                default:
                    Console.WriteLine($"command not found: {command}");
                    return;
            }
        }

        private void ListDirectoryFile(string[] arguments)
        {
            string newPath = arguments[0];

            if (string.IsNullOrEmpty(newPath))
            {
                throw new ArgumentNullException();
            }

            if (Path.IsPathRooted(newPath))
            {
                currentPath = newPath;
            }
            else if (Directory.Exists(Path.Join(currentPath, newPath)))
            {
                currentPath = Path.Join(currentPath, newPath);
            }
            else
            {
                // Не валидный путь
                throw new ArgumentException();
            }


            var directoryInfo = new DirectoryInfo(currentPath);

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                Console.WriteLine($"/{directory.Name}/");
            }

            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                Console.WriteLine($"/{fileInfo.Name}");
            }

            // Console.WriteLine(Directory.GetDirectories(Directory.GetCurrentDirectory()));
            // Console.WriteLine(Directory.GetFiles(Directory.GetCurrentDirectory()));
        }

        private void CopyFile(string[] arguments)
        {
            var sourcePathToFile = arguments[0];
            var targetPathToFile = arguments[1];

            if (!string.IsNullOrEmpty(sourcePathToFile))
            {
                throw new ArgumentNullException();
            }

            if (!string.IsNullOrEmpty(targetPathToFile))
            {
                throw new ArgumentNullException();
            }

            if (!File.Exists(sourcePathToFile))
            {
                throw new AggregateException();
            }

            if (!Path.HasExtension(targetPathToFile))
            {
                throw new ArgumentException();
            }

            File.Copy(sourcePathToFile, targetPathToFile); // Добавить флаг forse для перезаписи
        }

        private void RemoveFile(string[] arguments)
        {
            var sourcePathToFile = arguments[0];

            if (!string.IsNullOrEmpty(sourcePathToFile))
            {
                throw new ArgumentNullException();
            }

            if (!File.Exists(sourcePathToFile))
            {
                throw new AggregateException();
            }

            File.Delete(sourcePathToFile);
        }
    }
}