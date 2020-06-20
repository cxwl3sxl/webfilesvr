using System.Windows;
using PinFun.Wpf.Theme;

namespace MyWebSvr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ThemeManager.Instance.EnableTheme();
        }
    }
}
