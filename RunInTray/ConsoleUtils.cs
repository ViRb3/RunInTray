using System;

namespace RunInTray
{
    public static class ConsoleUtils
    {
        public static void ShowHelp()
        {
            Console.WriteLine(@"Runs a program and hides it in tray with its own icon.

runintray FILE [-t TITLE] [ARGUMENTS]

  FILE          Full path to the file to run
  -t            Display TITLE as the tray icon title
  ARGUMENTS     Optional arguments to pass to the file");
        }

        public static string GetFullArguments(string[] args)
        {
            string result = "";
            foreach (string arg in args)
                result += FormatArgument(arg);

            return result.TrimEnd(' ');
        }

        public static string GetSubArguments(string[] args)
        {
            string result = "";
            for (int i = 1; i < args.Length; i++)
            {
                if((i == 1 || i == 2) && Program.TrayTitle != null)
                    continue;
                result += FormatArgument(args[i]);
            }

            return result.TrimEnd(' ');
        }

        private static string FormatArgument(string arg)
        {
            return $"\"{arg}\" "; // wrap in quotes to prevent whitespace issues
        }
    }
}