using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BackgroundDynamic
{
    public partial class MainWindow : Window
    {
        public void Exit_Handle(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        public void Autostartup_Handle(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                notifyIcon.ShowBalloonTip(2000, "设置失败", "Access Denied.", System.Windows.Forms.ToolTipIcon.Error);
            }
            else
            {
                try
                {
                    autorun();
                }
                catch (Exception ex)
                {
                    notifyIcon.ShowBalloonTip(2000, "设置失败", ex.Message, System.Windows.Forms.ToolTipIcon.Error);
                }
            }
        }
        public void Play_Pause_Handle(object sender, EventArgs e)
        {
            if (mode == Mode.Video)
            {
                if (playing)
                {
                    MVideo.Pause();
                    playing = false;
                }
                else
                {
                    MVideo.Play();
                    playing = true;
                }
            }
            else
            {
                notifyIcon.ShowBalloonTip(2000, "无法执行操作", "您正在使用网页模式,不能暂停播放", System.Windows.Forms.ToolTipIcon.Warning);
            }
        }
        private void Mute_Click_Handle(object sender, EventArgs e)
        {
            if (ismute)
            {
                this.MVideo.IsMuted = false;
                mute.Checked = false;
            }
            else
            {
                this.MVideo.IsMuted = true;
                mute.Checked = true;
            }
            ismute = !ismute;
        }
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Stop();
            (sender as MediaElement).Play();
        }
        public bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void autorun()
        {
            //获取程序路径
            string execPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            bool isexc = false;
            try
            {
                RegistryKey RKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                //设置自启的程序叫获取目录下的程序名字
                string[] ar = RKey.GetValueNames();
                foreach (string st in ar)
                {
                    if (st.Equals("BackgroundDynamic"))
                    {
                        isexc = true;
                    }
                }
                if (!isexc)
                {
                    RKey.SetValue("BackgroundDynamic", execPath);
                }
                try
                {
                    notifyIcon.ShowBalloonTip(2000, "设置完成", "您可以在任务管理器中关闭开机自启", System.Windows.Forms.ToolTipIcon.Info);
                }
                catch { }
            }
            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(2000, "设置失败", ex.Message, System.Windows.Forms.ToolTipIcon.Error);
            }
        }
    }
}
