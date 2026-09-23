using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using ReolmarkedetG12.Core.Repositories;
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

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");

        var rackRepository = new RackRepository(connectionString);
        var renterRepository = new RenterRepository(connectionString);
        var rentalRepository = new RentalRepository(connectionString);
        var rentalPriceTierRepository = new RentalPriceTierRepository(connectionString);

        var dialogService = new MessageBoxDialogService();

        var mainViewModel = new MainViewModel(
            new RackViewModel(rackRepository, renterRepository, rentalRepository, rentalPriceTierRepository),
            new RenterViewModel(renterRepository));

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        this.MainWindow = mainWindow;
        mainWindow.Show();
    }
}