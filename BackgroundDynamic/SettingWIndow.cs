using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BackgroundDynamic.SettingWindow;

namespace BackgroundDynamic
{
    public partial class SettingWindow : Form
    {
        public SettingWindow()
        {
            InitializeComponent();
        }
        //----定义 result----
        public struct Result
        {
            public string source;
            public string mode;
            public int volume;
        }
        //----设置窗口初始化和读取ini文件内容----
        private void SettingWIndow_Load(object sender, EventArgs e)
        {
            //----获取config文本内容-从第9字节开始读取----
            FileStream choosetextboxfs = new FileStream("config.ini", FileMode.OpenOrCreate, FileAccess.Read);
            choosetextboxfs.Seek(9, SeekOrigin.Begin);
            //----逐行写入至文本框----
            StreamReader choosetextboxstr = new StreamReader(choosetextboxfs);
            ChooseFileTextbox.Text = choosetextboxstr.ReadLine();
            choosetextboxstr.Close();
            //----将音量选择框文本初始化为0----
            VolumeCombobox.Text = "0";
        }
        //----关闭设置窗口----
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //----保存设置并退出设置窗口----
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //----读取文本框和选择框文本----
            Result result = new Result();
            result.source = this.ChooseFileTextbox.Text.Trim();
            result.volume = Convert.ToInt32(this.VolumeCombobox.Text.Trim());
            result.mode = this.VideoModeCheck.Checked ? "web" : "video";
            FileStream savefsw = new FileStream("config.ini", FileMode.Open, FileAccess.Write);
            savefsw.SetLength(0);
            savefsw.Close();
            //----逐行写入config----
            StreamWriter savesw = File.AppendText(path: "config.ini");
            savesw.WriteLine($"source = {result.source}");
            savesw.WriteLine($"mode = {result.mode}");
            savesw.WriteLine($"volume = {result.volume}");
            savesw.Flush();
            savesw.Close();
            this.Close();
        }
        //----选择文件按钮-选择文件文本框交互----
        private void ChooseFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfileofd = new OpenFileDialog();
            Openfileofd.Filter = "MP4 File(*.mp4)|*.mp4|All FIle(*.*)|*.*";
            Openfileofd.FilterIndex = 1;
            Openfileofd.Title = "Choose an MP4 File";
            Openfileofd.InitialDirectory = @"C:\";
            Openfileofd.Multiselect = false;
            Openfileofd.ValidateNames = true;
            Openfileofd.CheckPathExists = true;
            Openfileofd.CheckFileExists = true;
            if (Openfileofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strFileName = Openfileofd.FileName;
                ChooseFileTextbox.Clear();
                ChooseFileTextbox.Text += strFileName;
            }
        }
        //----选择运行模式----
        private void VideoModeCheck_CheckedChanged(object sender, EventArgs e)
        {
            string[] ss = { "Mp4 File Path", "Web Url" };
            this.label2.Text = ss[Convert.ToInt32(this.VideoModeCheck.Checked)];
            this.ChooseFileButton.Visible = !this.VideoModeCheck.Checked;
            this.ChooseFileTextbox.Text = "";
        }
    }
}
