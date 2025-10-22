using System;
using System.Collections.Generic;

namespace SimpleCommandLineParser
{
    class CommandLineParser
    {
        private Dictionary<string, string> options = new Dictionary<string, string>();
        private List<string> positionalArgs = new List<string>();

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    string key = args[i].Substring(2); // Remove the "--"
                    string value = null;

                    // Check if the next argument is not an option, then it's a value
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        value = args[++i];
                    }

                    options[key] = value;
                }
                else
                {
                    // 不带 -- 的视为位置参数
                    positionalArgs.Add(args[i]);
                }
            }
        }

        public bool HasOption(string key)
        {
            return options.ContainsKey(key);
        }

        public string GetOption(string key)
        {
            if (options.TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }

        public List<string> GetPositionalArgs()
        {
            return positionalArgs;
        }

        public void PrintHelp()
        {
            Console.WriteLine("Usage: program [options] [positional arguments]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --src <file>   Source file");
            Console.WriteLine("  --dst <file>   Destination file");
            Console.WriteLine("  --force        Force overwrite");
            Console.WriteLine("  -h, --help     Show this help message");
            Console.WriteLine("Positional arguments:");
            Console.WriteLine("  <arg1>         First positional argument");
            Console.WriteLine("  <arg2>         Second positional argument");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser parser = new CommandLineParser();
            parser.Parse(args);

            if (parser.HasOption("help") || parser.HasOption("h"))
            {
                parser.PrintHelp();
                return;
            }

            string src = parser.GetOption("src");
            string dst = parser.GetOption("dst");
            bool force = parser.HasOption("force");

            List<string> positionalArgs = parser.GetPositionalArgs();

            if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dst))
            {
                Console.WriteLine("Error: --src and --dst are required.");
                parser.PrintHelp();
                return;
            }

            Console.WriteLine($"Source file: {src}");
            Console.WriteLine($"Destination file: {dst}");
            Console.WriteLine($"Force overwrite: {force}");
            Console.WriteLine("Positional arguments:");
            foreach (var arg in positionalArgs)
            {
                Console.WriteLine($"  {arg}");
            }
        }
    }
}