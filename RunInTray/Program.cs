using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RunInTray
{
    internal static class Program
    {
        public static Icon Icon;
        public static Process Process;
        public static string TrayTitle;
        public static IntPtr MainWindowHandle;
        public static bool MainWindowVisible;

        private static readonly string ThisFile = System.Reflection.Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args?.Length < 1)
            {
                ConsoleUtils.ShowHelp();
                return;
            }

            NativeImports.ShowWindow(Process.GetCurrentProcess().MainWindowHandle, NativeImports.SW_HIDE);

            if (args.Length > 2 && string.Equals(args[1], "-t", StringComparison.OrdinalIgnoreCase)) { // title provided
                TrayTitle = args[2];
            }

            string fullArguments = ConsoleUtils.GetFullArguments(args);
            string subArguments = ConsoleUtils.GetSubArguments(args);

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
            return NativeImports.DeleteFile(fileName + ":Zone.Identifier");
        }

        private static void RunTray(string filePath, string subArguments)
        {
            Icon = Icon.ExtractAssociatedIcon(filePath);
            Process = new Process();
            Process.StartInfo = new ProcessStartInfo(filePath, subArguments)
            {
                WindowStyle = ProcessWindowStyle.Minimized,
                WorkingDirectory = Path.GetDirectoryName(filePath)
            };
            Process.EnableRaisingEvents = true;
            Process.Exited += (sender, args) => Application.Exit();
            Process.Start();

            WaitForWindow();
            MainWindowHandle = Process.MainWindowHandle;
            NativeImports.ShowWindow(MainWindowHandle, NativeImports.SW_HIDE);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskTrayApplicationContext());
        }

        public static void WaitForWindow()
        {
            while (Process.MainWindowHandle == IntPtr.Zero)
                Thread.Sleep(50);
        }
    }
}
