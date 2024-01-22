using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace BackgroundDynamic {
    public partial class MainWindow : Window {
        private System.Windows.Forms.MenuItem mute = new System.Windows.Forms.MenuItem("Mute");//我知道这样看起来很糟糕, 但因为之后需要更改mute.Checked, 所以只能将其设为全局变量. 见private void Mute_Click_Handle(object sender, EventArgs e), in Handle.cs
        private void notify_init() {
            this.notifyIcon.Text = "DynamicBackground";
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Quit");
            exit.Click += new EventHandler(Exit_Handle);
            System.Windows.Forms.MenuItem autostartup = new System.Windows.Forms.MenuItem("Start when login");
            autostartup.Click += new EventHandler(Autostartup_Handle);
            System.Windows.Forms.MenuItem playpause = new System.Windows.Forms.MenuItem("Pause/Play");
            playpause.Click += new EventHandler(Play_Pause_Handle);
            mute.Click += Mute_Click_Handle;
            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[] { playpause, mute, autostartup, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            notifyIcon.Visible = true;
        }
        private void dwm_init() {
            if (Environment.OSVersion.Version.Major < 6) {
                this.notifyIcon.ShowBalloonTip(2000, "不支持当前系统", "您的系统不受支持", ToolTipIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
            else if (Environment.OSVersion.Version.Major == 6) {
                if (Environment.OSVersion.Version.Minor < 2) {
                    this.notifyIcon.ShowBalloonTip(2000, "不支持当前系统", "您的系统不受支持", ToolTipIcon.Error);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            IntPtr programIntPtr = IntPtr.Zero;
            try {
                // 通过类名查找一个窗口，返回窗口句柄。
                programIntPtr = Win32Func.FindWindow("Progman", null);

                // 窗口句柄有效
                if (programIntPtr != IntPtr.Zero) {
                    IntPtr result = IntPtr.Zero;

                    // 向 Program Manager 窗口发送 0x52c 的一个消息，超时设置为0x3e8（1秒）。
                    Win32Func.SendMessageTimeout(programIntPtr, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 0x3e8, result);

                    // 遍历顶级窗口
                    Win32Func.EnumWindows((hwnd, lParam) => {
                        // 找到包含 SHELLDLL_DefView 这个窗口句柄的 WorkerW
                        if (Win32Func.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero) {
                            // 找到当前 WorkerW 窗口的，后一个 WorkerW 窗口。 
                            IntPtr tempHwnd = Win32Func.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);

                            // 隐藏这个窗口
                            Win32Func.ShowWindow(tempHwnd, 0);
                        }
                        return true;
                    }, IntPtr.Zero);
                }
                Win32Func.SetParent(new WindowInteropHelper(this).Handle, programIntPtr);
            }
            catch (Exception ex) {
                notifyIcon.ShowBalloonTip(2000, "启动失败", ex.Message, ToolTipIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
        private void scale_init() {
            this.Left = 0.0;
            this.Top = 0.0;
            foreach (var v in Screen.AllScreens) {
                if (this.Left > v.WorkingArea.Left)
                    this.Left = v.WorkingArea.Left;
                if (this.Top > v.WorkingArea.Top)
                    this.Top = v.WorkingArea.Top;
            }
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.MVideo.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
