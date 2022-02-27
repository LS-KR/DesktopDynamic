using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Lierda.WPFHelper;

namespace BackgroundDynamic
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        LierdaCracker cracker = new LierdaCracker();
        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker(1);
            base.OnStartup(e);
        }
    }
}
