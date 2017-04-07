using System;
using System.Linq;
using System.Windows.Forms;

namespace RunInTray
{
    public class TaskTrayApplicationContext : ApplicationContext
    {  
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        public TaskTrayApplicationContext()
        {
            MenuItem showMenuItem = new MenuItem("Show", NotifyIconOnDoubleClick);
            MenuItem hideMenuItem = new MenuItem("Hide", Hide);
            hideMenuItem.Enabled = false;
            MenuItem exitMenuItem = new MenuItem("Exit", Exit);

            if(Program.TrayTitle == null)
                Program.TrayTitle = Program.Process.ProcessName;

            _notifyIcon.Text = Program.TrayTitle;
            _notifyIcon.Icon = Program.Icon;
            _notifyIcon.DoubleClick += NotifyIconOnDoubleClick;
            _notifyIcon.ContextMenu = new ContextMenu(new[] { showMenuItem, hideMenuItem, exitMenuItem });
            _notifyIcon.Visible = true;
        }

        private void NotifyIconOnDoubleClick(object sender, EventArgs eventArgs)
        {
            if (!Program.MainWindowVisible) {
                Show(sender, eventArgs);
            }
            else {
                Hide(sender, eventArgs);
            }
        }

        private void Show(object sender, EventArgs e)
        {
            Program.ShowWindow(Program.MainWindowHandle, Program.SW_NORMAL);
            Program.MainWindowVisible = true;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Show").Enabled = false;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Hide").Enabled = true;
        }

        private void Hide(object sender, EventArgs e)
        {
            Program.ShowWindow(Program.MainWindowHandle, Program.SW_HIDE);
            Program.MainWindowVisible = false;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Show").Enabled = true;
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Hide").Enabled = false;
        }

        void Exit(object sender, EventArgs e)
        {
            _notifyIcon.ContextMenu.MenuItems.Cast<MenuItem>().First(i => i.Text == "Exit").Enabled = false;

            if (!Program.MainWindowVisible)
            {
                Program.ShowWindow(Program.MainWindowHandle, Program.SW_MINIMIZE);
                Program.WaitForWindow();
            }

            Program.Process.CloseMainWindow(); // try graceful exit first
            Program.Process.WaitForExit(5000);
            if (!Program.Process.HasExited) {
                Program.Process.Kill();
            }

            // exit event handler will close this program as well
        }
    }
}
