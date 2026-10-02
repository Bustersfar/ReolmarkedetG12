using ReolmarkedetG12.Tests.Fakes;
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

        // Act
        var salesViewModel = new SalesViewModel();
        var mainViewModel = new MainViewModel(rackViewModel, renterViewModel, salesViewModel);

        // Assert
        Assert.AreSame(rackViewModel, mainViewModel.RackViewModel);
        Assert.AreSame(renterViewModel, mainViewModel.RenterViewModel);
        Assert.AreSame(salesViewModel, mainViewModel.SalesViewModel);

        // Assert
        Assert.AreSame(rackViewModel, mainViewModel.RackViewModel);
        Assert.AreSame(renterViewModel, mainViewModel.RenterViewModel);
    }
}