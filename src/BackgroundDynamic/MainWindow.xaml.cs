using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;

namespace BackgroundDynamic
{
    /// <summary>
    /// Win32方法
    /// </summary>
    public static class Win32Func
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string winName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlage, uint timeout, IntPtr result);
        //查找窗口的委托 查找逻辑
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string winName);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwnd, IntPtr parentHwnd);
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);
        /// <summary>
        /// 窗口与要获得句柄的窗口之间的关系。
        /// </summary>
        enum GetWindowCmd : uint
        {
            /// <summary>
            /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// 返回的句柄标识了在z序最低端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
            /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
            /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
            /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
            /// 如果无使能窗口，则获得的句柄与指定窗口相同。
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }
        /*GetWindowCmd指定结果窗口与源窗口的关系，它们建立在下述常数基础上：
              GW_CHILD
              寻找源窗口的第一个子窗口
              GW_HWNDFIRST
              为一个源子窗口寻找第一个兄弟（同级）窗口，或寻找第一个顶级窗口
              GW_HWNDLAST
              为一个源子窗口寻找最后一个兄弟（同级）窗口，或寻找最后一个顶级窗口
              GW_HWNDNEXT
              为源窗口寻找下一个兄弟窗口
              GW_HWNDPREV
              为源窗口寻找前一个兄弟窗口
              GW_OWNER
              寻找窗口的所有者
         */
    }
    public partial class App : System.Windows.Application
    {
        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);
        /// <summary>
        /// 刷新界面
        /// </summary>
        public static void DoEvents()
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status !=
            DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }
        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as
            DispatcherFrame;
            frame.Continue = false;
            return null;
        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static IntPtr programHandle = IntPtr.Zero;
        enum Mode { Video = 0, Web = 1 };
        private NotifyIcon notifyIcon = new NotifyIcon();
        public MainWindow()
        {
            InitializeComponent();
            this.notifyIcon.Text = "DynamicBackground";
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Quit");
            exit.Click += new EventHandler(Exit_Handle);
            System.Windows.Forms.MenuItem[] children = new System.Windows.Forms.MenuItem[] { exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(children);
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            notifyIcon.Visible = true;
            SendMsgToProgman();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*IntPtr hWndMyWindow = Win32Func.FindWindow(null, this.Title);//通过窗口的标题获得句柄
            IntPtr hWndDesktop = Win32Func.FindWindow("Progman", "Program Manager");//获得桌面句柄
            Win32Func.SetParent(hWndMyWindow, hWndDesktop); //将窗口设置为桌面的子窗体*/
            
            IntPtr handle = new WindowInteropHelper(this).Handle;
            //ShowInTaskbar = false : causing issue with windows10 Taskview.
            WindowOperations.RemoveWindowFromTaskbar(handle);
            //this hides the window from taskbar and also fixes crash when win10 taskview is launched. 
            IntPtr hWndDesktop = Win32Func.FindWindow("Progman", null);//获得桌面句柄
            WindowOperations.SetParentSafe(handle, hWndDesktop);
            //this.ShowInTaskbar = false;
            this.ShowInTaskbar = true;

            this.Left = 0.0;
            this.Top = 0.0;
            foreach (var v in Screen.AllScreens)
            {
                if (this.Left > v.WorkingArea.Left)
                    this.Left = v.WorkingArea.Left;
                if (this.Top > v.WorkingArea.Top)
                    this.Top = v.WorkingArea.Top;
            }
            App.DoEvents();
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            App.DoEvents();
            this.MVideo.Margin = new Thickness(0, 0, 0, 0);
            this.MVideo.MediaEnded += new RoutedEventHandler(media_MediaEnded);
            Mode mode = new Mode();
            mode = Mode.Video;
            if (!File.Exists("config.ini"))
            {
                if (!File.Exists("Source.mp4"))
                {
                    System.Windows.MessageBox.Show("Cannot Found Config and Source File!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Windows.Application.Current.Shutdown();
                }
                else
                    this.MVideo.Source = new Uri("Source.mp4", UriKind.Relative);
            }
            else
            {
                bool isf = false;
                string[] config = File.ReadAllLines("config.ini");
                for (int i = 0; i < config.Length; i++)
                {
                    string[] args = config[i].Split('=');
                    if (args[0].ToLower() == "source")
                    {
                        if (mode == Mode.Video)
                        {
                            try
                            {
                                this.MVideo.Source = new Uri(args[1], UriKind.RelativeOrAbsolute);
                                isf = true;
                            }
                            catch
                            {
                                isf = false;
                                throw;
                            }
                        }
                        else if (mode == Mode.Web)
                        {
                            try
                            {
                                this.webview.Source = new Uri(args[1]);
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                    else if (args[0].ToLower() == "volume")
                    {
                        try
                        {
                            this.MVideo.Volume = (Convert.ToDouble(args[1]) / 100.0);
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else if (args[0].ToLower() == "mode")
                    {
                        if (args[1].ToLower() == "video")
                            mode = Mode.Video;
                        else if (args[1].ToLower() == "web")
                        {
                            mode = Mode.Web;
                            if (isf)
                            {
                                this.webview.Source = this.MVideo.Source;
                            }
                        }
                    }
                }
                if (!isf)
                {
                    if (mode == Mode.Video)
                    {
                        if (!File.Exists("Source.mp4"))
                        {
                            System.Windows.MessageBox.Show("Cannot Found Config and Source File!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            System.Windows.Application.Current.Shutdown();
                        }
                        else
                            this.MVideo.Source = new Uri("Source.mp4", UriKind.Relative);
                    }
                }
            }
            if (mode == Mode.Video)
                this.MVideo.Play();
            else if(mode == Mode.Web)
                this.webview.Visibility = Visibility.Visible;
        }
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Stop();
            (sender as MediaElement).Play();
        }
        /// <summary>
        /// 向桌面发送消息
        /// </summary>
        public static void SendMsgToProgman()
        {
            // 桌面窗口句柄，在外部定义，用于后面将我们自己的窗口作为子窗口放入
            programHandle = Win32Func.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            // 向 Program Manager 窗口发送消息 0x52c 的一个消息，超时设置为2秒
            Win32Func.SendMessageTimeout(programHandle, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 2, result);
            // 遍历顶级窗口
            Win32Func.EnumWindows((hwnd, lParam) =>
            {
                // 找到第一个 WorkerW 窗口，此窗口中有子窗口 SHELLDLL_DefView，所以先找子窗口
                if (Win32Func.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                {
                    // 找到当前第一个 WorkerW 窗口的，后一个窗口，及第二个 WorkerW 窗口。
                    IntPtr tempHwnd = Win32Func.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                    // 隐藏第二个 WorkerW 窗口
                    Win32Func.ShowWindow(tempHwnd, 0);
                }
                return true;
            }, IntPtr.Zero);
        }
        public void Exit_Handle(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
