using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace BackgroundDynamic {
    public partial class MainWindow : Window {
        public void Exit_Handle(object sender, EventArgs e) {
            notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        public void Autostartup_Handle(object sender, EventArgs e) {
            if (!IsAdministrator())
                notifyIcon.ShowBalloonTip(2000, "设置失败", "Access Denied.", ToolTipIcon.Error);
            else
                try {
                    autorun(true);
                }
                catch (Exception ex) {
                    notifyIcon.ShowBalloonTip(2000, "设置失败", ex.Message, ToolTipIcon.Error);
                }
        }

        public void DelStartUp_Handle(object sender, EventArgs e) {
            if (!IsAdministrator())
                notifyIcon.ShowBalloonTip(2000, "设置失败", "Access Denied.", ToolTipIcon.Error);
            else
                try {
                    autorun(false);
                }
                catch (Exception ex) {
                    notifyIcon.ShowBalloonTip(2000, "设置失败", ex.Message, ToolTipIcon.Error);
                }
        }

        public void Play_Pause_Handle(object sender, EventArgs e) {
            if (mode == Mode.Video) {
                if (playing) {
                    MVideo.Pause();
                    playing = false;
                }
                else {
                    MVideo.Play();
                    playing = true;
                }
            }
            else {
                notifyIcon.ShowBalloonTip(2000, "无法执行操作", "您正在使用网页模式,不能暂停播放", ToolTipIcon.Warning);
            }
        }

        private void Mute_Click_Handle(object sender, EventArgs e) {
            if (ismute) {
                MVideo.IsMuted = false;
                mute.Checked = false;
            }
            else {
                MVideo.IsMuted = true;
                mute.Checked = true;
            }

            ismute = !ismute;
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e) {
            (sender as MediaElement).Stop();
            (sender as MediaElement).Play();
        }

        public bool IsAdministrator() {
            var current = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void autorun(bool s) {
            //获取程序路径 
            var execPath = Process.GetCurrentProcess().MainModule.FileName;
            var isexc = false;
            try {
                var RKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                //设置自启的程序叫获取目录下的程序名字
                var ar = RKey.GetValueNames();
                foreach (var st in ar)
                    if (st.Equals("BackgroundDynamic"))
                        isexc = true;
                if (!isexc && s)
                    RKey.SetValue("BackgroundDynamic", execPath);
                else if (isexc && !s) RKey.DeleteValue("BackgroundDynamic");
                try {
                    notifyIcon.ShowBalloonTip(2000, "设置完成", "", ToolTipIcon.Info);
                }
                catch {
                }
            }
            catch (Exception ex) {
                notifyIcon.ShowBalloonTip(2000, "设置失败", ex.Message, ToolTipIcon.Error);
            }
        }
    }
}