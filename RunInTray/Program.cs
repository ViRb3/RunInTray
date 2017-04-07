using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace RunInTray
{
    static class Program
    {
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        internal const int SW_HIDE = 0;
        internal const int SW_SHOW = 5;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_NORMAL = 1;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        internal static Icon Icon;
        internal static Process Process;
        internal static string TrayTitle;
        internal static IntPtr MainWindowHandle;
        internal static bool MainWindowVisible;

        private static readonly string ThisFile = System.Reflection.Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args?.Length < 1)
            {
                Console.WriteLine(@"Runs a program and hides it in tray with its own icon.

runintray FILE [-t TITLE] [ARGUMENTS]

  FILE          Full path to the file to run
  -t            Display TITLE as the tray icon title
  ARGUMENTS     Optional arguments to pass to the file");
                return;
            }

            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_HIDE);

            if (args.Length > 2 && args[1].ToLower() == "-t") { // title provided
                TrayTitle = args[2];
            }

            string fullArguments = GetFullArguments(args);
            string subArguments = GetSubArguments(args);

            string filePath = args[0];   
            string externalFileName = Path.GetFileNameWithoutExtension(filePath);
            string expectedName = externalFileName + "-tray.exe";

            if (Path.GetFileName(ThisFile) != expectedName)
            {
                Install(expectedName, fullArguments);
                return;
            }

            RunTray(filePath, subArguments);
        }

        private static string GetFullArguments(string[] args)
        {
            string result = "";
            foreach (string arg in args)
                result += FormatArgument(arg);

            return result.TrimEnd(' ');
        }

        private static string GetSubArguments(string[] args)
        {
            string result = "";
            for (int i = 1; i < args.Length; i++)
            {
                if((i == 1 || i == 2) && TrayTitle != null)
                    continue;
                result += FormatArgument(args[i]);
            }

            return result.TrimEnd(' ');
        }

        private static string FormatArgument(string arg)
        {
            return $"\"{arg}\" "; // wrap in quotes to prevent whitespace issues
        }


        private static void Install(string newName, string arguments)
        {
            if (!Directory.Exists("generated"))
                Directory.CreateDirectory("generated");

            string newFile = Path.Combine(Path.GetDirectoryName(ThisFile), "generated", newName);
            if (!File.Exists(newFile))
            {
                File.Copy(ThisFile, newFile);
                Unblock(newFile);
            }    

            Process.Start(newFile, arguments);
        }

        private static bool Unblock(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }

        private static void RunTray(string filePath, string subArguments)
        {
            Icon = Icon.ExtractAssociatedIcon(filePath);
            Process = new Process();
            Process.StartInfo = new ProcessStartInfo(filePath, subArguments);
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Process.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
            Process.EnableRaisingEvents = true;
            Process.Exited += Process_Exited;
            Process.Start();

            WaitForWindow();
            MainWindowHandle = Process.MainWindowHandle;
            ShowWindow(MainWindowHandle, SW_HIDE);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskTrayApplicationContext());
        }

        public static void WaitForWindow()
        {
            while (Process.MainWindowHandle == IntPtr.Zero)
                Thread.Sleep(50);
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
