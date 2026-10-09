using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Tests.Fakes;
using ReolmarkedetG12.UI.Services;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public void Constructor_GivenViewModels_ExposesThem()
    {
        // Arrange
        var dialog = new FakeDialogService();
        var rackViewModel = new RackViewModel(
            new FakeRackRepository(), new FakeRenterRepository(), new FakeRentalRepository(),
            new FakePriceTierRepository(), new FakePaymentRepository(), dialog);
        var renterViewModel = new RenterViewModel(new FakeRenterRepository(), new FakeRentalRepository(), dialog);
        var salesViewModel = new SalesViewModel(
            new FakeRackRepository(), new FakeRentalRepository(), new FakeSaleRepository(), new FakeRenterRepository(), dialog);
        var searchSalesViewModel = new SearchSalesViewModel(
            new FakeRackRepository(), new FakeSaleRepository(), new FakeRenterRepository(), dialog, new SecureAreaService("1234"));
        var monthlyStatementViewModel = new MonthlyStatementViewModel(
            new FakeRenterRepository(), new FakeRentalRepository(), new FakeSaleRepository(), new FakePriceTierRepository(), new SecureAreaService("1234"), dialog);

        // Act
        var mainViewModel = new MainViewModel(rackViewModel, renterViewModel, salesViewModel, searchSalesViewModel, monthlyStatementViewModel);

        // Assert
        Assert.AreSame(rackViewModel, mainViewModel.RackViewModel);
        Assert.AreSame(renterViewModel, mainViewModel.RenterViewModel);
        Assert.AreSame(salesViewModel, mainViewModel.SalesViewModel);
        Assert.AreSame(searchSalesViewModel, mainViewModel.SearchSalesViewModel);
        Assert.AreSame(monthlyStatementViewModel, mainViewModel.MonthlyStatementViewModel);
    }

    [TestMethod]
    public void SelectedTabIndex_ReturningToMonthlyStatement_RefreshesStatement()
    {
        // Arrange
        var dialog = new FakeDialogService();
        var renterRepository = new FakeRenterRepository();
        var rentalRepository = new FakeRentalRepository();
        var saleRepository = new FakeSaleRepository();
        var secureAreaService = new SecureAreaService("1234");
        renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });

        var monthlyStatementViewModel = new MonthlyStatementViewModel(
            renterRepository, rentalRepository, saleRepository, new FakePriceTierRepository(), secureAreaService, dialog);
        var mainViewModel = new MainViewModel(
            new RackViewModel(
                new FakeRackRepository(), new FakeRenterRepository(), new FakeRentalRepository(),
                new FakePriceTierRepository(), new FakePaymentRepository(), dialog),
            new RenterViewModel(new FakeRenterRepository(), new FakeRentalRepository(), dialog),
            new SalesViewModel(
                new FakeRackRepository(), new FakeRentalRepository(), new FakeSaleRepository(), new FakeRenterRepository(), dialog),
            new SearchSalesViewModel(
                new FakeRackRepository(), new FakeSaleRepository(), new FakeRenterRepository(), dialog, new SecureAreaService("1234")),
            monthlyStatementViewModel);

        monthlyStatementViewModel.SelectedYear = 2026;
        monthlyStatementViewModel.SelectedMonthIndex = 9;
        secureAreaService.TryUnlock("1234");
        mainViewModel.SelectedTabIndex = MainViewModel.MonthlyStatementTabIndex;
        Assert.AreEqual(0m, monthlyStatementViewModel.TotalSales);

        // Act - registrer et salg på en anden fane og gå tilbage
        mainViewModel.SelectedTabIndex = 2;
        saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 400m, Date = new DateTime(2026, 10, 5) });
        mainViewModel.SelectedTabIndex = MainViewModel.MonthlyStatementTabIndex;

        // Assert
        Assert.AreEqual(400m, monthlyStatementViewModel.TotalSales);
    }
}