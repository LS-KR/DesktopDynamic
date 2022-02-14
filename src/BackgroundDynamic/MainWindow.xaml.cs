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
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static IntPtr programHandle = IntPtr.Zero;
        public MainWindow()
        {
            InitializeComponent();
            SendMsgToProgman();
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 0.0;
            this.Top = 0.0;
            this.MVideo.Margin = new Thickness(0, 0, 0, 0);
            this.MVideo.MediaEnded += new RoutedEventHandler(media_MediaEnded);
            Win32Func.SetParent(new WindowInteropHelper(this).Handle, programHandle);
            if (!File.Exists("config.ini"))
            {
                if (!File.Exists("Source.mp4"))
                {
                    MessageBox.Show("Cannot Found Config and Source File!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
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
                        try
                        {
                            this.MVideo.Source = new Uri(args[1], UriKind.Relative);
                            isf = true;
                        }
                        catch
                        {
                            isf = false;
                            throw;
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
                    if (!isf)
                        this.MVideo.Source = new Uri("Source.mp4", UriKind.Relative);
                }
            }
            this.MVideo.Play();
        }
        private void stateon()
        {
            
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
    }
}
