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
                throw new ArgumentException($"{nameof(pathToDirectory)} must is a full path",
                    nameof(pathToDirectory));
            }

            var directoryInfo = new DirectoryInfo(pathToDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"{directoryInfo.FullName} doesn't exist");
            }

            long size = 0;
            long files = 0;

            try
            {
                foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += fileInfo.Length;
                    files += 1;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                return new DirectoryUtilsInfo(Size: 0, Files: 0);
            }

            return new DirectoryUtilsInfo(Size: size, Files: files);
        }

        public static void Copy(string pathToSourceDirectory, string pathToDistDirectory)
        {
            if (!Path.IsPathRooted(pathToSourceDirectory))
            {
                throw new ArgumentException($"{nameof(pathToSourceDirectory)} must is a full path",
                    nameof(pathToSourceDirectory));
            }

            if (!Path.IsPathRooted(pathToDistDirectory))
            {
                throw new ArgumentException($"{nameof(pathToDistDirectory)} must is a full path",
                    nameof(pathToDistDirectory));
            }

            var directoryInfo = new DirectoryInfo(pathToSourceDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"{directoryInfo.FullName} doesn't exist");
            }

            if (!Directory.Exists(pathToDistDirectory))
            {
                Directory.CreateDirectory(pathToDistDirectory);
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.CopyTo(Path.Join(pathToDistDirectory, fileInfo.Name));
            }

            foreach (var subDirectoryInfo in directoryInfo.GetDirectories())
            {
                DirectoryUtils.Copy(
                    subDirectoryInfo.FullName,
                    Path.Join(pathToDistDirectory, subDirectoryInfo.Name)
                );
            }
        }

        public static void DeleteWithFile(string fullPathSourceDirectory)
        {
            if (!Path.IsPathRooted(fullPathSourceDirectory))
            {
                throw new ArgumentException($"{nameof(fullPathSourceDirectory)} must is a full path",
                    nameof(fullPathSourceDirectory));
            }

            var directoryInfo = new DirectoryInfo(fullPathSourceDirectory);

            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"{directoryInfo.FullName} doesn't exist");
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