using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    public class CommandManager
    {
        public (string command, string[] arguments) parseStringToCommand(string str)
        {
            var arrayList = new List<string>(str.Trim().Split(" "));

            var command = arrayList.First();
            var arguments = arrayList.Count > 1
                ? arrayList.GetRange(1, arrayList.Count - 1).ToArray()
                : Array.Empty<string>();

            return (command, arguments);
        }


        public void execCommand(string command, string[] arguments)
        {
            switch (command)
            {
                case "cd":
                    Console.WriteLine("Change current path");
                    return;
                case "ls":
                    Console.WriteLine("Call listing");
                    return;
                case "help":
                    Console.WriteLine("Call help command");
                    return;
                default:
                    Console.WriteLine($"command not found: {command}");
                    return;
            }
        }
    }
}