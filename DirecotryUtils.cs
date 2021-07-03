using System;
using System.IO;

namespace FileManager
{
    public record DirectoryUtilsInfo(long Size, long Files)
    {
    }

    public class DirectoryUtils
    {
        public static DirectoryUtilsInfo Info(string pathToDirectory)
        {
            if (!Path.IsPathRooted(pathToDirectory))
            {
                throw new ArgumentException(); // Должен быть полный путь
            }

            var directoryInfo = new DirectoryInfo(pathToDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException(); // Такой папки не существует
            }

            long size = 0;
            long files = 0;

            foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fileInfo.Length;
                files += 1;
            }

            return new DirectoryUtilsInfo(Size: size, Files: files);
        }

        public static void Copy(string pathToSourceDirectory, string pathToTargetDirectory)
        {
            if (!Path.IsPathRooted(pathToSourceDirectory))
            {
                throw new ArgumentException(); // TODO: Должен быть полный путь
            }

            if (!Path.IsPathRooted(pathToTargetDirectory))
            {
                throw new ArgumentException(); // TODO: Должен быть полный путь
            }

            var directoryInfo = new DirectoryInfo(pathToSourceDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException(); // TODO: Такой папки не существует
            }

            if (!Directory.Exists(pathToTargetDirectory))
            {
                Directory.CreateDirectory(pathToTargetDirectory);
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.CopyTo(Path.Join(pathToTargetDirectory, fileInfo.Name));
            }

            foreach (var subDirectoryInfo in directoryInfo.GetDirectories())
            {
                DirectoryUtils.Copy(
                    subDirectoryInfo.FullName,
                    Path.Join(pathToTargetDirectory, subDirectoryInfo.Name)
                );
            }
        }

        public static void DeleteWithFile(string fullPathSourceDirectory)
        {
            if (!Path.IsPathRooted(fullPathSourceDirectory))
            {
                throw new ArgumentException(); // TODO: Должен быть полный путь
            }

            var directoryInfo = new DirectoryInfo(fullPathSourceDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException(); // TODO: Такой папки не существует
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.Delete();
            }

            foreach (var subDirectoryInfo in directoryInfo.GetDirectories())
            {
                DeleteWithFile(subDirectoryInfo.FullName);
                subDirectoryInfo.Delete();
            }

            directoryInfo.Delete();
        }
    }
}
