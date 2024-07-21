using Lierda.WPFHelper;
using System.Windows;

namespace BackgroundDynamic {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        LierdaCracker cracker = new LierdaCracker();
        private void Application_Startup(object sender, StartupEventArgs e) {

        }
        protected override void OnStartup(StartupEventArgs e) {
            cracker.Cracker(1);
            base.OnStartup(e);
        }
    }
}
