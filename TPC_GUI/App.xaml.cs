using System.Windows;
using TPC_GUI.ViewModels;
using TPC_GUI.Views;

namespace TPC_GUI
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainView = new MainView();
            mainView.DataContext = new MainViewModel();

            mainView.Show();
        }
    }
}
