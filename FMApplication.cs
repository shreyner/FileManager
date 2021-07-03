using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace FileManager
{
    public class FMApplication
    {
        // private string currentPath = Path.GetFullPath(Path.Join(Environment.CurrentDirectory, "..", "..", "..", ".."));
        private string currentPath = Path.GetFullPath("/Users/shreyner/workspace/net/FileManager");

        public FMApplication()
        {
            // TODO: Добавить инициализацию этой конфига

            CommandListDirectoryFile(new[] {"."});
        }

        public void run()
        {
            while (true)
            {
                Console.Write("> ");
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
                case "file":
                    CommandShowFile(arguments);
                    return;
                case "help":
                    Console.WriteLine("Call help command");
                    return;
                default:
                    Console.WriteLine($"command not found: {command}");
                    return;
            }
        }

        private void ShowDirectoryInformation(string targetDirectory)
        {
            var directoryInfo = new DirectoryInfo(targetDirectory);

            Console.WriteLine(new string('=', 10));
            var directorySize = DirectoryUtils.Size(targetDirectory);
            Console.WriteLine(
                "Path: {0}\nSize: {1:N0} byte\nCreated: {2:g}",
                targetDirectory,
                directorySize,
                directoryInfo.CreationTime
            );
            Console.WriteLine(new string('=', 10));
        }


        private void CommandShowFile(string[] arguments)
        {
            var pathToTargetFile = arguments[0];

            if (string.IsNullOrEmpty(pathToTargetFile))
            {
                throw new ArgumentException();
            }

            var fullPathToTargetFile = CombinePathToTargetFile(currentPath, pathToTargetFile);

            if (!Path.HasExtension(fullPathToTargetFile))
            {
                throw new ArgumentException();
            }

            if (!File.Exists(fullPathToTargetFile))
            {
                throw new ArgumentException(); // TODO: Такой файл не существует
            }

            Console.WriteLine(File.ReadAllText(fullPathToTargetFile));
            return;
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

            Console.WriteLine($"{currentPath}/");

            ListDirectoryFile(currentPath, 1);
            ShowDirectoryInformation(currentPath);
        }

        // TODO: deepLength можно вынести в конфиг
        public void ListDirectoryFile(string path, int offset = 0, int deepLength = 2)
        {
            if (deepLength == 0)
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(path);

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                Console.WriteLine($"{new string(' ', offset)}/{directory.Name}/");
                ListDirectoryFile($"{path}/{directory.Name}", offset + 1, deepLength - 1);
            }

            foreach (var fileInfo in directoryInfo.EnumerateFiles())
            {
                Console.WriteLine($"{new string(' ', offset)}/{fileInfo.Name}");
            }
        }

        private void CommandCopy(string[] arguments)
        {
            // TODO: Добавить обработку ошибок с доступом к файлу
            // TODO: Добавить обработку ошибок с копированием не существующего файла
            var sourcePathToFile = arguments.ElementAtOrDefault(0);
            var targetPathToFile = arguments.ElementAtOrDefault(1);
            var isCopyDirectory = arguments.ElementAtOrDefault(2) == "-p";

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

            DirectoryUtils.Copy(fullPathToSourceFile, fullPathToTargetFile);
        }

        private void CommandDelete(string[] arguments)
        {
            // TODO: Разбить функцию
            // TODO: Добавить обработку ошибок с доступом к файлу
            // TODO: Добавить обработку ошибок с копированием не существующего файла
            var sourcePathToFile = arguments.ElementAtOrDefault(0);
            var isDeleteDirectory = arguments.ElementAtOrDefault(1) == "-p";
            var isDeleteWithFile = arguments.ElementAtOrDefault(2) == "-f";

            if (string.IsNullOrEmpty(sourcePathToFile))
            {
                throw new ArgumentNullException();
            }

            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (isDeleteDirectory)
            {
                DeleteDirectory(fullPathToSourceFile, isDeleteWithFile);
                return;
            }

            if (!Path.HasExtension(fullPathToSourceFile) && Directory.Exists(fullPathToSourceFile))
            {
                Console.WriteLine("Укажите -p. Это похоже на папку");
                throw new ArgumentException(); // TODO: Добавить нормальную ошибку
            }

            DeleteFile(fullPathToSourceFile);
            return;
        }

        private void DeleteFile(string fullPathToSourceFile)
        {
            if (!File.Exists(fullPathToSourceFile))
            {
                throw new AggregateException();
            }

            File.Delete(fullPathToSourceFile);
        }

        private void DeleteDirectory(string fullPathToSourceFile, bool deleteWithFile)
        {
            if (!Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException(); // TODO: Такой папки не существует
            }

            if (deleteWithFile)
            {
                DirectoryUtils.DeleteWithFile(fullPathToSourceFile);
                return;
            }

            try
            {
                Directory.Delete(fullPathToSourceFile);
            }
            catch (IOException e)
            {
                if (e.Message.Contains("Directory not empty"))
                {
                    Console.WriteLine("Папка не пустая. Для удаления укажите флаг -f");
                    return;
                }

                throw;
            }
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