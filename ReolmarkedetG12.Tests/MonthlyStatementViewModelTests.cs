using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Tests.Fakes;
using ReolmarkedetG12.UI.Services;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class MonthlyStatementViewModelTests
{
    private FakeRenterRepository _renterRepository = null!;
    private FakeRentalRepository _rentalRepository = null!;
    private FakeSaleRepository _saleRepository = null!;
    private FakeDialogService _dialog = null!;
    private SecureAreaService _secureAreaService = null!;
    private MonthlyStatementViewModel _viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _renterRepository = new FakeRenterRepository();
        _rentalRepository = new FakeRentalRepository();
        _saleRepository = new FakeSaleRepository();
        _dialog = new FakeDialogService();
        _secureAreaService = new SecureAreaService("1234");

        _viewModel = new MonthlyStatementViewModel(
            _renterRepository, _rentalRepository, _saleRepository, _secureAreaService, _dialog);
    }

    [TestMethod]
    public void Constructor_DefaultsToPreviousMonth()
    {
        // Arrange
        var previousMonth = DateTime.Today.AddMonths(-1);

        // Assert
        Assert.AreEqual(previousMonth.Year, _viewModel.SelectedYear);
        Assert.AreEqual(previousMonth.Month - 1, _viewModel.SelectedMonthIndex);
    }

    [TestMethod]
    public void Refresh_TwoRenters_FillsLinesAndTotals()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _renterRepository.Add(new Renter { FirstName = "Bo", LastName = "Larsen" });
        _rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _rentalRepository.Add(new Rental { RenterId = 2, RackId = 6, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 2000m, Date = new DateTime(2026, 10, 5) });
        _saleRepository.Add(new Sale { RenterId = 2, RackId = 6, Amount = 500m, Date = new DateTime(2026, 10, 6) });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9; // oktober

        // Act
        _viewModel.Refresh();

        // Assert
        Assert.AreEqual(2, _viewModel.Lines.Count);
        Assert.AreEqual(1700m, _viewModel.TotalRent);
        Assert.AreEqual(2500m, _viewModel.TotalSales);
        Assert.AreEqual(250m, _viewModel.TotalCommission);
        Assert.AreEqual(550m, _viewModel.TotalPayout);
    }

    [TestMethod]
    public void Refresh_CalledTwice_ReplacesLines()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9;

        // Act
        _viewModel.Refresh();
        _viewModel.Refresh();

        // Assert
        Assert.AreEqual(1, _viewModel.Lines.Count);
        Assert.AreEqual(850m, _viewModel.TotalRent);
    }

    [TestMethod]
    public void Refresh_AfterNewSale_RecalculatesWithNewSales()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9;
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 400m, Date = new DateTime(2026, 10, 5) });

        // Act
        _viewModel.Refresh();

        // Assert
        Assert.AreEqual(400m, _viewModel.TotalSales);
    }

    [TestMethod]
    public void Refresh_WhenLocked_DoesNothing()
    {
        // Arrange
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9;

        // Act
        _viewModel.Refresh();

        // Assert
        Assert.AreEqual(0, _viewModel.Lines.Count);
    }

    [TestMethod]
    public void Unlock_ShowsStatementForSelectedMonth()
    {
        // Arrange
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _rentalRepository.Add(new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9;

        // Act
        _secureAreaService.TryUnlock("1234");

        // Assert
        Assert.AreEqual(1, _viewModel.Lines.Count);
    }

    [TestMethod]
    public void SelectedMonthIndex_Changed_ShowsStatementForNewMonth()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 100m, Date = new DateTime(2026, 9, 5) });
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 400m, Date = new DateTime(2026, 10, 5) });
        _viewModel.SelectedYear = 2026;
        _viewModel.SelectedMonthIndex = 9;

        // Act
        _viewModel.SelectedMonthIndex = 8; // september

        // Assert
        Assert.AreEqual(100m, _viewModel.TotalSales);
    }

    [TestMethod]
    public void SelectedYear_Changed_ShowsStatementForNewYear()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _renterRepository.Add(new Renter { FirstName = "Anna", LastName = "Jensen" });
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 100m, Date = new DateTime(2025, 10, 5) });
        _saleRepository.Add(new Sale { RenterId = 1, RackId = 5, Amount = 400m, Date = new DateTime(2026, 10, 5) });
        _viewModel.SelectedMonthIndex = 9;
        _viewModel.SelectedYear = 2026;

        // Act
        _viewModel.SelectedYear = 2025;

        // Assert
        Assert.AreEqual(100m, _viewModel.TotalSales);
    }

    [TestMethod]
    public void Refresh_CurrentMonth_IsPreliminary()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        _viewModel.SelectedYear = DateTime.Today.Year;
        _viewModel.SelectedMonthIndex = DateTime.Today.Month - 1;

        // Act
        _viewModel.Refresh();

        // Assert
        Assert.IsTrue(_viewModel.IsPreliminary);
    }

    [TestMethod]
    public void Refresh_PreviousMonth_IsNotPreliminary()
    {
        // Arrange
        _secureAreaService.TryUnlock("1234");
        var previousMonth = DateTime.Today.AddMonths(-1);
        _viewModel.SelectedYear = previousMonth.Year;
        _viewModel.SelectedMonthIndex = previousMonth.Month - 1;

        // Act
        _viewModel.Refresh();

        // Assert
        Assert.IsFalse(_viewModel.IsPreliminary);
    }

    [TestMethod]
    public void Refresh_DatabaseDown_DoesNotThrow()
    {
        // Arrange
        _saleRepository.SimulateDatabaseDown = true;

        // Act
        _secureAreaService.TryUnlock("1234");

        // Assert
        Assert.AreEqual(0, _viewModel.Lines.Count);
    }
}
