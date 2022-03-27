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
using System.Security.Principal;
using Microsoft.Win32;

namespace BackgroundDynamic
{
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
        static bool playing = false;
        static bool ismute = false;
        enum Mode { Video = 0, Web = 1 };
        private NotifyIcon notifyIcon = new NotifyIcon();
        Mode mode = new Mode();
        public MainWindow()
        {
            InitializeComponent();
            notify_init();
            SendMsgToProgman();
            //检测显示器设备更改 + 屏幕分辨率更改
            SystemEvents.DisplaySettingsChanged += new EventHandler((s, e) =>
            {
                dwm_init();
                scale_init();
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dwm_init();
            scale_init();
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
                    if (args[0].ToLower().Trim() == "source")
                    {
                        if (mode == Mode.Video)
                        {
                            try
                            {
                                this.MVideo.Source = new Uri(args[1].Trim(), UriKind.RelativeOrAbsolute);
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
                                this.webview.Source = new Uri(args[1].Trim());
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                    else if (args[0].ToLower().Trim() == "volume")
                    {
                        try
                        {
                            this.MVideo.Volume = (Convert.ToDouble(args[1].Trim()) / 100.0);
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else if (args[0].ToLower().Trim() == "mode")
                    {
                        if (args[1].ToLower().Trim() == "video")
                            mode = Mode.Video;
                        else if (args[1].ToLower().Trim() == "web")
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
            {
                this.MVideo.Play();
                this.MVideo.MediaEnded += media_MediaEnded;
                playing = true;
            }
            else if(mode == Mode.Web)
            {
                this.webview.Visibility = Visibility.Visible;
            }
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
    }
}
