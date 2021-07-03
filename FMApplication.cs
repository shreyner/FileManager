using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;


namespace FileManager
{
    public class DirectorySettings
    {
        public int limit { get; set; }
    }

    public class FMApplication
    {
        // private string currentPath = Path.GetFullPath(Path.Join(Environment.CurrentDirectory, "..", "..", "..", ".."));
        private string currentPath = Path.GetFullPath("/Users/shreyner/workspace/net/FileManager");

        private readonly IConfigurationRoot configurationRoot;

        private readonly DirectorySettings directorySettings;

        private readonly Configuration configuration;

        public FMApplication()
        {
            configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            directorySettings = configurationRoot.GetSection("Directory").Get<DirectorySettings>();

            var roaming = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            var fileMap = new ExeConfigurationFileMap {ExeConfigFilename = roaming.FilePath};

            configuration =
                ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            var lastVisitDirectory = configuration.AppSettings.Settings["lastVisitDirectory"]?.Value ??
                                     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            CommandListDirectoryFile(new[] {lastVisitDirectory});
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
                    CommandShowFileInfo(arguments);
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
            var directoryUtilsInfoSize = DirectoryUtils.Info(targetDirectory);

            Console.WriteLine("Information about directory: {0}", directoryInfo.Name);
            Console.WriteLine(
                "Path: {0}\nSize: {1:N0} byte, Files: {2:N0}\nCreated: {3:g}, LastAccess: {4:g}, LastWrite: {5:g}",
                directoryInfo.FullName,
                directoryUtilsInfoSize.Size == 0 ? "Unknown" : directoryUtilsInfoSize.Size,
                directoryUtilsInfoSize.Files == 0 ? "Unknown" : directoryUtilsInfoSize.Files,
                directoryInfo.CreationTime,
                directoryInfo.LastAccessTime,
                directoryInfo.LastWriteTime
            );
            Console.WriteLine(new string('=', 10));
        }


        private void CommandShowFileInfo(string[] arguments)
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

            var fileInfo = new FileInfo(fullPathToTargetFile);

            Console.WriteLine(new string('=', 10));
            Console.WriteLine("Information about file: {0}", fileInfo.Name);
            Console.WriteLine(
                "Path: {0}\nName: {1}, ext: {2}\nSize: {3:N0}\nCreated: {4:g}, LastAccess: {5:g}, LastWrite: {6:g}",
                fileInfo.FullName, fileInfo.Name, fileInfo.Extension, fileInfo.Length, fileInfo.CreationTime,
                fileInfo.LastAccessTime, fileInfo.LastWriteTime);
            Console.WriteLine(new string('=', 10));
            return;
        }

        private void UpdateLastVisitDirectory(string path)
        {
            configuration.AppSettings.Settings.Add("lastVisitDirectory", path);

            configuration.Save();
        }

        private void CommandListDirectoryFile(string[] arguments)
        {
            string newPath = arguments.ElementAtOrDefault(0) ?? ".";

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
            UpdateLastVisitDirectory(currentPath);
        }

        // TODO: deepLength можно вынести в конфиг
        public void ListDirectoryFile(string path, int offset = 0, int deepLength = 2)
        {
            if (deepLength == 0)
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(path);

            try
            {
                foreach (var directory in directoryInfo.EnumerateDirectories())
                {
                    Console.WriteLine($"{new string(' ', offset)}/{directory.Name}/");
                    ListDirectoryFile($"{path}/{directory.Name}", offset + 1, deepLength - 1);
                }
            }
            catch (UnauthorizedAccessException e)
            {
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