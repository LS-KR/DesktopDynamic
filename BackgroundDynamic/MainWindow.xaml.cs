using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BackgroundDynamic {
    public partial class App : Application {
        private static readonly DispatcherOperationCallback exitFrameCallback = ExitFrame;

        /// <summary>
        ///     刷新界面
        /// </summary>
        public static void DoEvents() {
            var nestedFrame = new DispatcherFrame();
            var exitOperation =
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status !=
                DispatcherOperationStatus.Completed)
                exitOperation.Abort();
        }

        private static object ExitFrame(object state) {
            var frame = state as
                DispatcherFrame;
            frame.Continue = false;
            return null;
        }
    }

    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private static IntPtr programHandle = IntPtr.Zero;
        private static bool playing;
        private static bool ismute;
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private Mode mode;

        public MainWindow() {
            InitializeComponent();
            notify_init();
            SendMsgToProgman();
            //检测显示器设备更改 + 屏幕分辨率更改
            SystemEvents.DisplaySettingsChanged += (s, e) => {
                dwm_init();
                scale_init();
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            dwm_init();
            scale_init();
            mode = Mode.Video;
            var cfgstat = -1;
            var srcstat = -1;
            var config = new string[1];
            if (!File.Exists("config.ini")) {
                if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\config.ini"))
                    cfgstat = 0;
                else
                    cfgstat = 2;
            }
            else {
                cfgstat = 1;
            }

            if (cfgstat == 0) {
                if (!File.Exists("Source.mp4")) {
                    if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\Source.mp4")) {
                        srcstat = 0;
                    }
                    else {
                        MVideo.Source = new Uri(System.Windows.Forms.Application.StartupPath + "\\Source.mp4",
                            UriKind.RelativeOrAbsolute);
                        srcstat = 2;
                    }
                }
                else {
                    MVideo.Source = new Uri("Source.mp4", UriKind.RelativeOrAbsolute);
                    srcstat = 1;
                }
            }

            if (cfgstat == 0 && srcstat == 0) {
                MessageBox.Show("Cannot Found Config and Source File!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            if (cfgstat == 1)
                config = File.ReadAllLines("config.ini");
            if (cfgstat == 2)
                config = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + "\\config.ini");
            try {
                var isf = false;
                for (var i = 0; i < config.Length; i++) {
                    var args = config[i].Split('=');
                    if (args[0].ToLower().Trim() == "source") {
                        if (mode == Mode.Video)
                            try {
                                MVideo.Source = new Uri(args[1].Trim(), UriKind.RelativeOrAbsolute);
                                isf = true;
                            }
                            catch {
                                isf = false;
                                throw;
                            }
                        else if (mode == Mode.Web) webview.Source = new Uri(args[1].Trim());
                    }
                    else if (args[0].ToLower().Trim() == "volume") {
                        MVideo.Volume = Convert.ToDouble(args[1].Trim()) / 100.0;
                    }
                    else if (args[0].ToLower().Trim() == "mode") {
                        if (args[1].ToLower().Trim() == "video") {
                            mode = Mode.Video;
                        }
                        else if (args[1].ToLower().Trim() == "web") {
                            mode = Mode.Web;
                            if (isf) webview.Source = MVideo.Source;
                        }
                    }
                }

                if (!isf)
                    if (mode == Mode.Video) {
                        if (srcstat == 0) {
                            MessageBox.Show("Cannot Found Source File!", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Application.Current.Shutdown();
                        }

                        if (srcstat == 1)
                            MVideo.Source = new Uri("Source.mp4", UriKind.Relative);
                        if (srcstat == 2)
                            MVideo.Source = new Uri(System.Windows.Forms.Application.StartupPath + "\\Source.mp4",
                                UriKind.Relative);
                    }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            if (mode == Mode.Video) {
                MVideo.Play();
                MVideo.MediaEnded += media_MediaEnded;
                playing = true;
            }
            else if (mode == Mode.Web) {
                webview.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        ///     向桌面发送消息
        /// </summary>
        public static void SendMsgToProgman() {
            // 桌面窗口句柄，在外部定义，用于后面将我们自己的窗口作为子窗口放入
            programHandle = Win32Func.FindWindow("Progman", null);
            var result = IntPtr.Zero;
            // 向 Program Manager 窗口发送消息 0x52c 的一个消息，超时设置为2秒
            Win32Func.SendMessageTimeout(programHandle, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 2, result);
            // 遍历顶级窗口
            Win32Func.EnumWindows((hwnd, lParam) => {
                // 找到第一个 WorkerW 窗口，此窗口中有子窗口 SHELLDLL_DefView，所以先找子窗口
                if (Win32Func.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero) {
                    // 找到当前第一个 WorkerW 窗口的，后一个窗口，及第二个 WorkerW 窗口。
                    var tempHwnd = Win32Func.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                    // 隐藏第二个 WorkerW 窗口
                    Win32Func.ShowWindow(tempHwnd, 0);
                }

                return true;
            }, IntPtr.Zero);
        }

        private enum Mode {
            Video = 0,
            Web = 1
        }
    }
}