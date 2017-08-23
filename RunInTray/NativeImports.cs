using System;
using System.Runtime.InteropServices;

namespace RunInTray
{
    public static class NativeImports
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_NORMAL = 1;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);
    }
}