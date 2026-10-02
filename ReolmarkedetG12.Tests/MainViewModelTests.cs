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
            new FakeRackRepository(), new FakeRentalRepository(), new FakeSaleRepository(), dialog);
        var searchSalesViewModel = new SearchSalesViewModel(
            new FakeRackRepository(), new FakeSaleRepository(), dialog, new SecureAreaService("1234"));
        var monthlyStatementViewModel = new MonthlyStatementViewModel(new SecureAreaService("1234"), dialog);

        // Act
        var mainViewModel = new MainViewModel(rackViewModel, renterViewModel, salesViewModel, searchSalesViewModel, monthlyStatementViewModel);

        // Assert
        Assert.AreSame(rackViewModel, mainViewModel.RackViewModel);
        Assert.AreSame(renterViewModel, mainViewModel.RenterViewModel);
        Assert.AreSame(salesViewModel, mainViewModel.SalesViewModel);
        Assert.AreSame(searchSalesViewModel, mainViewModel.SearchSalesViewModel);
        Assert.AreSame(monthlyStatementViewModel, mainViewModel.MonthlyStatementViewModel);
    }
}