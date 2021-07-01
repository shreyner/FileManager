using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace FileManager
{
    public class FMApplication
    {
        private string currentPath = Path.GetFullPath(Path.Join(Environment.CurrentDirectory, "..", "..", "..", ".."));

        public FMApplication()
        {
            // TODO: Добавить инициализацию этой конфига
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
                case "cd":
                case "ls":
                    CommandListDirectoryFile(arguments);
                    return;
                case "cp":
                    CommandCopy(arguments);
                    return;
                case "rm":
                    CommandDelete(arguments);
                    return;
                case "help":
                    Console.WriteLine("Call help command");
                    return;
                default:
                    Console.WriteLine($"command not found: {command}");
                    return;
            }
        }

        private void CommandListDirectoryFile(string[] arguments)
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

            currentPath = Path.GetFullPath(currentPath);

            var directoryInfo = new DirectoryInfo(currentPath);

            Console.WriteLine($"Current path: {currentPath}");

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                Console.WriteLine($"/{directory.Name}/");
            }

            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                Console.WriteLine($"/{fileInfo.Name}");
            }
        }

        private void CommandCopy(string[] arguments)
        {
            // TODO: Добавить обработку ошибок с доступом к файлу
            // TODO: Добавить обработку ошибок с копированием не существующего файла
            var sourcePathToFile = arguments[0];
            var targetPathToFile = arguments[1];
            var isCopyDirectory = arguments[2] == "-p";

            if (string.IsNullOrEmpty(sourcePathToFile))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(targetPathToFile))
            {
                throw new ArgumentNullException();
            }

            if (isCopyDirectory)
            {
                CopyDirectory(sourcePathToFile, targetPathToFile);
                return;
            }

            CopyFile(sourcePathToFile, targetPathToFile);
        }

        private void CopyFile(string sourcePathToFile, string targetPathToFile)
        {
            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException("Это не файл а дерриктория");
            }

            if (!File.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException(); //TODO: Такого файла нету
            }

            var fullPathToTargetFile = CombinePathToTargetFile(currentPath, targetPathToFile);

            if (!Path.HasExtension(fullPathToTargetFile))
            {
                throw new ArgumentException(); //TODO: Файл не имеет расширения
            }

            File.Copy(fullPathToSourceFile, fullPathToTargetFile); // Добавить флаг forse для перезаписи   
        }

        private void CopyDirectory(string sourcePathToFile, string targetPathToFile)
        {
            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (!Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException(); //TODO: Не является директорией или не существует
            }

            var fullPathToTargetFile = CombinePathToTargetFile(currentPath, targetPathToFile);

            if (Path.HasExtension(fullPathToTargetFile))
            {
                throw new ArgumentException(); //TODO: Не верный формат target
            }

            // Directory.Cop
        }

        private void CommandDelete(string[] arguments)
        {
            // TODO: Разбить функцию
            // TODO: Добавить обработку ошибок с доступом к файлу
            // TODO: Добавить обработку ошибок с копированием не существующего файла
            var sourcePathToFile = arguments[0];

            if (string.IsNullOrEmpty(sourcePathToFile))
            {
                throw new ArgumentNullException();
            }

            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (!File.Exists(fullPathToSourceFile))
            {
                throw new AggregateException();
            }

            File.Delete(fullPathToSourceFile);
        }

        private void DeleteFile(string[] arguments)
        {
        }

        private void DeleteDirectory(string[] arguments)
        {
        }

        private string CombinePathToTargetFile(string fullPathRoot, string targetPathToFile)
        {
            if (!Path.IsPathRooted(fullPathRoot)) // TODO: Почитать
            {
                throw new ArgumentException(); // TODO: Добавить описание ошибки. По всему файлу
            }

            if (Path.IsPathRooted(targetPathToFile))
            {
                return targetPathToFile;
            }

            return Path.Join(fullPathRoot, targetPathToFile);
        }
    }
}