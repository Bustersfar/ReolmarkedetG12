using System.Windows;
using ReolmarkedetG12.UI.Services;
using ReolmarkedetG12.UI.ViewModels;
using ReolmarkedetG12.UI.Views;

namespace ReolmarkedetG12.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dialogService = new MessageBoxDialogService();

        var mainViewModel = new MainViewModel(
            new ReolerViewModel(),
            new KunderViewModel());

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        this.MainWindow = mainWindow;
        mainWindow.Show();
    }
}
