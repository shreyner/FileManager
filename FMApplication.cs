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
        private string currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private readonly IConfigurationRoot configurationRoot;

        private readonly DirectorySettings directorySettings;

        private readonly Configuration configuration;

        private long currentShowLines = 0;
        private long totalDirectoryAndFiles = 0;

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
            try
            {
                switch (command)
                {
                    case "cd":
                    case "ls":
                        CommandListDirectoryFile(arguments);
                        return;
                    case "next":
                        CommandNextPage();
                        break;
                    case "prev":
                        CommandPrevPage();
                        break;
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
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ShowDirectoryInformation(string targetDirectory)
        {
            var directoryInfo = new DirectoryInfo(targetDirectory);

            Console.WriteLine(new string('=', 10));
            var directoryUtilsInfoSize = DirectoryUtils.Info(targetDirectory);

            Console.WriteLine("Information about directory: {0}", directoryInfo.Name);
            Console.WriteLine(
                "Path: {0}\nCurrentPage: {1}, TotalPage: {2}\nSize: {3:N0} byte, Files: {4:N0}\nCreated: {5:g}, LastAccess: {6:g}, LastWrite: {7:g}",
                directoryInfo.FullName,
                (currentShowLines / directorySettings.limit) + 1,
                totalDirectoryAndFiles / directorySettings.limit + 1, // FIXME: Change type on float and around up
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
                throw new ArgumentException($"{fullPathToTargetFile} isn't has extension");
            }

            if (!File.Exists(fullPathToTargetFile))
            {
                throw new ArgumentException($"{fullPathToTargetFile} isn't has extension");
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
            configuration.AppSettings.Settings.Remove("lastVisitDirectory");
            configuration.AppSettings.Settings.Add("lastVisitDirectory", path);

            configuration.Save();
        }

        private void CommandListDirectoryFile(string[] arguments)
        {
            string newPath = arguments.ElementAtOrDefault(0) ?? ".";

            if (string.IsNullOrEmpty(newPath))
            {
                throw new ArgumentNullException("Path", "Path is not empty");
            }


            try
            {
                currentPath = CombinePathToTargetFile(currentPath, newPath);
                currentPath = Path.GetFullPath(currentPath);
                Console.WriteLine($"{currentPath}/");

                currentShowLines = 0;

                long offset = currentShowLines;
                int limit = directorySettings.limit;
                totalDirectoryAndFiles = 0;

                ListDirectoryFile(path: currentPath, offset: ref offset, limit: ref limit,
                    totalCounter: ref totalDirectoryAndFiles);
                ShowDirectoryInformation(currentPath);
                UpdateLastVisitDirectory(currentPath);
            }
            catch (ArgumentException error)
            {
                Console.WriteLine(error);
            }
        }

        private void CommandNextPage()
        {
            if (totalDirectoryAndFiles < currentShowLines + directorySettings.limit)
            {
                Console.WriteLine("This last page.");
                return;
            }

            currentShowLines += directorySettings.limit;
            long offset = currentShowLines;
            int limit = directorySettings.limit;
            long totalCounter = 0;

            ListDirectoryFile(path: currentPath, offset: ref offset, limit: ref limit, totalCounter: ref totalCounter);
            ShowDirectoryInformation(currentPath);
        }

        private void CommandPrevPage()
        {
            if (currentShowLines <= 0)
            {
                Console.WriteLine("Последняя страница");
                return;
            }

            currentShowLines -= directorySettings.limit;

            long offset = currentShowLines;
            int limit = directorySettings.limit;
            long totalCounter = 0;

            ListDirectoryFile(path: currentPath, offset: ref offset, limit: ref limit, totalCounter: ref totalCounter);
            ShowDirectoryInformation(currentPath);
        }

        // TODO: deepLength можно вынести в конфиг
        public void ListDirectoryFile(
            string path,
            ref int limit,
            ref long offset,
            ref long totalCounter,
            int currentDeep = 0,
            int maxDeep = 2
        )
        {
            if (maxDeep == 0)
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(path);

            try
            {
                foreach (var directory in directoryInfo.EnumerateDirectories())
                {
                    totalCounter += 1;

                    if (offset == 0 && limit > 0)
                    {
                        limit -= 1;
                        Console.WriteLine($"{new string(' ', currentDeep * 2)}/{directory.Name}/");
                    }

                    if (offset > 0)
                    {
                        offset -= 1;
                    }

                    ListDirectoryFile(
                        path: $"{path}/{directory.Name}",
                        currentDeep: currentDeep + 1,
                        maxDeep: maxDeep - 1,
                        offset: ref offset,
                        limit: ref limit,
                        totalCounter: ref totalCounter
                    );
                }
            }
            catch (UnauthorizedAccessException error)
            {
            }

            try
            {
                foreach (var fileInfo in directoryInfo.EnumerateFiles())
                {
                    totalCounter += 1;

                    if (offset == 0 && limit > 0)
                    {
                        limit -= 1;
                        Console.WriteLine($"{new string(' ', currentDeep)}/{fileInfo.Name}");
                    }

                    if (offset > 0)
                    {
                        offset -= 1;
                    }
                }
            }
            catch (UnauthorizedAccessException error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private void CommandCopy(string[] arguments)
        {
            var pathToSourceFile = arguments.ElementAtOrDefault(0);
            var pathToDistFile = arguments.ElementAtOrDefault(1);
            var isCopyDirectory = arguments.ElementAtOrDefault(2) == "-p";

            if (string.IsNullOrEmpty(pathToSourceFile))
            {
                throw new ArgumentNullException(nameof(pathToSourceFile), "Path to source is not empty");
            }

            if (string.IsNullOrEmpty(pathToDistFile))
            {
                throw new ArgumentNullException(nameof(pathToDistFile), "Path to dist is not empty");
            }

            if (isCopyDirectory)
            {
                CopyDirectory(pathToSourceFile, pathToDistFile);
                return;
            }

            CopyFile(pathToSourceFile, pathToDistFile);
        }

        private void CopyFile(string sourcePathToFile, string targetPathToFile)
        {
            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException("It's not file. It's a Directory");
            }

            if (!File.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException($"{fullPathToSourceFile} doesn't exist");
            }

            var fullPathToTargetFile = CombinePathToTargetFile(currentPath, targetPathToFile);

            if (!Path.HasExtension(fullPathToTargetFile))
            {
                throw new ArgumentException($"{fullPathToTargetFile} isn't a file");
            }

            try
            {
                File.Copy(fullPathToSourceFile, fullPathToTargetFile); // TODO: Добавить флаг для перезаписи
            }
            catch (UnauthorizedAccessException error)
            {
                Console.WriteLine(error.Message);
            }
        }

        private void CopyDirectory(string sourcePathToFile, string targetPathToFile)
        {
            var fullPathToSourceFile = CombinePathToTargetFile(currentPath, sourcePathToFile);

            if (!Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException("Path to source directory is not exist");
            }

            var fullPathToTargetFile = CombinePathToTargetFile(currentPath, targetPathToFile);

            DirectoryUtils.Copy(fullPathToSourceFile, fullPathToTargetFile);
        }

        private void CommandDelete(string[] arguments)
        {
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
                throw new ArgumentException("You must set -p for delete Directory");
            }

            DeleteFile(fullPathToSourceFile);
            return;
        }

        private void DeleteFile(string fullPathToSourceFile)
        {
            if (!File.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException($"{nameof(fullPathToSourceFile)} must is a full path",
                    nameof(fullPathToSourceFile));
            }

            File.Delete(fullPathToSourceFile);
        }

        private void DeleteDirectory(string fullPathToSourceFile, bool deleteWithFile)
        {
            if (!Directory.Exists(fullPathToSourceFile))
            {
                throw new ArgumentException($"{fullPathToSourceFile} doesn't exist");
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

        private string CombinePathToTargetFile(string pathRoot, string targetPathToFile)
        {
            if (!Path.IsPathRooted(pathRoot))
            {
                throw new ArgumentException($"{nameof(pathRoot)} is not root path", nameof(pathRoot));
            }

            if (Path.IsPathRooted(targetPathToFile))
            {
                return targetPathToFile;
            }

            return Path.Join(pathRoot, targetPathToFile);
        }
    }
}