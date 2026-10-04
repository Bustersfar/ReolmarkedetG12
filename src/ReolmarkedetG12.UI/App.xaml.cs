using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReolmarkedetG12.Core.Exceptions;
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

        var dialogService = new MessageBoxDialogService();

        IConfigurationRoot config;
        string connectionString;
        string searchSalesPassword;

        try
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");

            // BEMÆRK: "1234" er kun en nødløsning, hvis appsettings.json mangler
            // SearchSalesPassword. Det er ikke en sikker adgangskode, og
            // SecureAreaService er i øvrigt ikke "rigtig" sikkerhed (se
            // ISecureAreaService.cs) - kun en simpel adgangsspærre for et
            // skoleprojekt. Bør nævnes som en kendt begrænsning i rapporten.
            searchSalesPassword = config["SearchSalesPassword"] ?? "1234";
        }
        catch (Exception ex)
        {
            dialogService.ShowError(
                $"Kunne ikke læse konfigurationen (appsettings.json).\n\nDetaljer: {ex.Message}",
                "Opstartsfejl");
            Shutdown();
            return;
        }

        var secureAreaService = new SecureAreaService(searchSalesPassword);

        var rackRepository = new RackRepository(connectionString);
        var renterRepository = new RenterRepository(connectionString);
        var rentalRepository = new RentalRepository(connectionString);
        var rentalPriceTierRepository = new RentalPriceTierRepository(connectionString);
        var paymentRepository = new PaymentRepository(connectionString);
        var saleRepository = new SaleRepository(connectionString);

        try
        {
            rackRepository.GetAll();
        }
        catch (DatabaseConnectionException ex)
        {
            dialogService.ShowError(
                "Kunne ikke forbinde til databasen. Tjek at SQL Server kører, og at connection string'en i appsettings.json er korrekt.\n\n" +
                $"Teknisk besked: {ex.InnerException?.Message ?? ex.Message}",
                "Databasefejl");
            Shutdown();
            return;
        }
        catch (SqlException ex)
        {
            dialogService.ShowError(
                $"Der opstod en fejl i databasen. Tjek at databasen er oprettet (database/schema.sql).\n\nTeknisk besked: {ex.Message}",
                "Databasefejl");
            Shutdown();
            return;
        }

        var mainViewModel = new MainViewModel(
            new RackViewModel(rackRepository, renterRepository, rentalRepository, rentalPriceTierRepository, paymentRepository, dialogService),
            new RenterViewModel(renterRepository, rentalRepository, dialogService),
            new SalesViewModel(rackRepository, rentalRepository, saleRepository, renterRepository, dialogService),
            new SearchSalesViewModel(rackRepository, saleRepository, renterRepository, dialogService, secureAreaService),
            new MonthlyStatementViewModel(secureAreaService, dialogService));

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        this.MainWindow = mainWindow;
        mainWindow.Show();
    }
}