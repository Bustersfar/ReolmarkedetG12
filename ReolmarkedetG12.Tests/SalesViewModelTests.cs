using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Tests.Fakes;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class SalesViewModelTests
{
    private readonly FakeRackRepository _rackRepository = new();
    private readonly FakeRentalRepository _rentalRepository = new();
    private readonly FakeSaleRepository _saleRepository = new();
    private readonly FakeRenterRepository _renterRepository = new();
    private readonly FakeDialogService _dialogService = new();

    private SalesViewModel CreateViewModel() =>
        new SalesViewModel(_rackRepository, _rentalRepository, _saleRepository, _renterRepository, _dialogService);

    private void SetupRentedRack(int rackNumber, string firstName, string lastName)
    {
        var rack = new Rack { Number = rackNumber, Status = RackStatus.Rented };
        _rackRepository.Add(rack);

        var renter = new Renter
        {
            FirstName = firstName,
            LastName = lastName,
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse"
        };
        _renterRepository.Add(renter);

        _rentalRepository.Add(new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = DateTime.Now.AddDays(-10),
            EndDate = null,
            MonthlyRent = 850m
        });
    }

    [TestMethod]
    public void NewRackNumber_ValidActiveRental_SetsValidAndShowsRenterName()
    {
        // Arrange
        SetupRentedRack(10, "Peter", "Hansen");
        var vm = CreateViewModel();

        // Act
        vm.NewRackNumber = 10;

        // Assert
        Assert.IsTrue(vm.IsRackValid);
        Assert.IsTrue(vm.RackValidationMessage.Contains("Peter Hansen"));
    }

    [TestMethod]
    public void NewRackNumber_RackDoesNotExist_SetsInvalidAndShowsWarning()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        vm.NewRackNumber = 999;

        // Assert
        Assert.IsFalse(vm.IsRackValid);
        Assert.IsTrue(vm.RackValidationMessage.Contains("findes ikke"));
    }

    [TestMethod]
    public void NewRackNumber_RackAvailableNoRenter_SetsInvalidAndShowsWarning()
    {
        // Arrange
        _rackRepository.Add(new Rack { Number = 5, Status = RackStatus.Available });
        var vm = CreateViewModel();

        // Act
        vm.NewRackNumber = 5;

        // Assert
        Assert.IsFalse(vm.IsRackValid);
        Assert.IsTrue(vm.RackValidationMessage.Contains("ingen aktiv lejer"));
    }

    [TestMethod]
    public void AddItemCommand_ValidItem_AddsToCartAndCalculatesTotal()
    {
        // Arrange
        SetupRentedRack(10, "Peter", "Hansen");
        var vm = CreateViewModel();
        vm.NewRackNumber = 10;
        vm.NewPrice = 125m;
        vm.NewRemark = "Vase";

        // Act
        vm.AddItemCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, vm.CurrentSaleItems.Count);
        Assert.AreEqual(125m, vm.TotalAmount);
        Assert.IsNull(vm.NewRackNumber);
        Assert.IsNull(vm.NewPrice); // Prisfeltet skal ryddes helt, ikke vise "0"
    }

    [TestMethod]
    public void RegisterSaleCommand_CashPaymentWithEnoughMoney_SavesSaleAndClearsCart()
    {
        // Arrange
        SetupRentedRack(10, "Peter", "Hansen");
        var vm = CreateViewModel();
        vm.NewRackNumber = 10;
        vm.NewPrice = 100m;
        vm.AddItemCommand.Execute(null);

        vm.PaymentMethod = PaymentMethod.Cash;
        vm.CashReceived = 150m;

        // Act
        Assert.AreEqual(50m, vm.Change);
        Assert.IsTrue(vm.RegisterSaleCommand.CanExecute(null));
        vm.RegisterSaleCommand.Execute(null);

        // Assert
        Assert.AreEqual(1, _saleRepository.Sales.Count);
        Assert.AreEqual(100m, _saleRepository.Sales[0].Amount);
        Assert.AreEqual(0, vm.CurrentSaleItems.Count);
        Assert.IsNull(vm.CashReceived);
    }

    [TestMethod]
    public void RegisterSaleCommand_CashPaymentNotEnoughMoney_CannotExecute()
    {
        // Arrange
        SetupRentedRack(10, "Peter", "Hansen");
        var vm = CreateViewModel();
        vm.NewRackNumber = 10;
        vm.NewPrice = 100m;
        vm.AddItemCommand.Execute(null);

        vm.PaymentMethod = PaymentMethod.Cash;
        vm.CashReceived = 80m;

        // Act & Assert
        Assert.IsFalse(vm.RegisterSaleCommand.CanExecute(null));
    }
}