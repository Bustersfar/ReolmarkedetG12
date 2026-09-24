using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Tests.Fakes;
using ReolmarkedetG12.UI.ViewModels;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RackViewModelTests
{
    // MSTest laver en ny instans af klassen til hver test, så fakes starter altid tomme
    private readonly FakeRackRepository _racks = new();
    private readonly FakeRenterRepository _renters = new();
    private readonly FakeRentalRepository _rentals = new();
    private readonly FakeDialogService _dialog = new();

    private RackViewModel CreateViewModel() =>
        new RackViewModel(_racks, _renters, _rentals, new FakePriceTierRepository(), _dialog);

    private void AddRack(int number) =>
        _racks.Add(new Rack { Number = number, Status = RackStatus.Available });

    private Renter AddRenter()
    {
        var renter = new Renter
        {
            FirstName = "Anna",
            LastName = "Andersen",
            Address = "Vej 1",
            PostalCode = 4200,
            City = "Slagelse"
        };
        _renters.Add(renter);
        return renter;
    }
    private RackViewModel CreateViewModelWithTwoRenters()
    {
        _renters.Add(new Renter
        {
            FirstName = "Anna",
            LastName = "Andersen",
            Address = "Testvej 2",
            PostalCode = 4200,
            City = "Slagelse",
            Email = "anna@test.dk",
            Phone = "20000001"
        });
        _renters.Add(new Renter
        {
            FirstName = "Bo",
            LastName = "Bertelsen",
            Address = "Havnevej 5",
            PostalCode = 4000,
            City = "Roskilde",
            Email = "bo@test.dk",
            Phone = "20000002"
        });
        return CreateViewModel();
    }
    private Rack AddRackWithStatus(int number, RackStatus status)
    {
        var rack = new Rack { Number = number, Status = status };
        _racks.Add(rack);
        return rack;
    }

    private Rental AddRental(Rack rack, Renter renter, DateTime? endDate)
    {
        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = endDate,
            MonthlyRent = 850m
        };
        _rentals.Add(rental);
        return rental;
    }

    [TestMethod]
    public void Constructor_RacksInRepository_LoadsAllRacks()
    {
        // Arrange
        AddRack(1);
        AddRack(2);
        AddRack(3);

        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.HasCount(3, viewModel.Racks);
    }

    [TestMethod]
    public void Constructor_RackNumbers_AreSortedIntoTheirArea()
    {
        // Arrange
        AddRack(5);   // venstre kolonne (1-13)
        AddRack(14);  // gruppe 14-18
        AddRack(80);  // gruppe 79-80

        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.HasCount(1, viewModel.LeftColumnRacks);
        Assert.HasCount(1, viewModel.Cluster_14_18);
        Assert.HasCount(1, viewModel.Cluster_79_80);
    }

    [TestMethod]
    public void SelectRackCommand_AvailableRack_SelectsIt()
    {
        // Arrange
        AddRack(1);
        var viewModel = CreateViewModel();
        var rackItem = viewModel.Racks[0];

        // Act: klik på reolen
        viewModel.SelectRackCommand.Execute(rackItem);

        // Assert
        Assert.IsTrue(rackItem.IsSelected);
        Assert.HasCount(1, viewModel.SelectedRacks);
    }

    [TestMethod]
    public void SelectRackCommand_ClickedTwice_DeselectsIt()
    {
        // Arrange
        AddRack(1);
        var viewModel = CreateViewModel();
        var rackItem = viewModel.Racks[0];

        // Act: klik to gange
        viewModel.SelectRackCommand.Execute(rackItem);
        viewModel.SelectRackCommand.Execute(rackItem);

        // Assert
        Assert.IsFalse(rackItem.IsSelected);
        Assert.HasCount(0, viewModel.SelectedRacks);
    }

    [TestMethod]
    public void CreateRentalCommand_OneRack_RentsRackToRenter()
    {
        // Arrange: én ledig reol, én lejer, og begge er valgt
        AddRack(1);
        var renter = AddRenter();
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act
        viewModel.CreateRentalCommand.Execute(null);

        // Assert
        Assert.HasCount(1, _rentals.Rentals);
        Assert.AreEqual(renter.RenterId, _rentals.Rentals[0].RenterId);
        Assert.AreEqual(RackStatus.Rented, _racks.Racks[0].Status);
        Assert.AreEqual(850m, _rentals.Rentals[0].MonthlyRent);
    }

    [TestMethod]
    public void CreateRentalCommand_TwoRacks_SplitsPriceEvenly()
    {
        // Arrange: to ledige reoler og én lejer
        AddRack(1);
        AddRack(2);
        var renter = AddRenter();
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[1]);

        // Act
        viewModel.CreateRentalCommand.Execute(null);

        // Assert: 850 + 825 = 1675, delt på to reoler
        Assert.HasCount(2, _rentals.Rentals);
        Assert.AreEqual(837.5m, _rentals.Rentals[0].MonthlyRent);
        Assert.AreEqual(837.5m, _rentals.Rentals[1].MonthlyRent);
    }
    [TestMethod]
    public void CalculateTerminationEffectiveDate_On19th_ReturnsFirstOfNextMonth()
    {
        var result = RackViewModel.CalculateTerminationEffectiveDate(new DateTime(2026, 1, 19));

        Assert.AreEqual(new DateTime(2026, 2, 1), result);
    }

    [TestMethod]
    public void CalculateTerminationEffectiveDate_On20th_ReturnsFirstOfMonthAfterNext()
    {
        var result = RackViewModel.CalculateTerminationEffectiveDate(new DateTime(2026, 1, 20));

        Assert.AreEqual(new DateTime(2026, 3, 1), result);
    }

    [TestMethod]
    public void CalculateTerminationEffectiveDate_InDecember_RollsOverToNextYear()
    {
        var result = RackViewModel.CalculateTerminationEffectiveDate(new DateTime(2026, 12, 25));

        Assert.AreEqual(new DateTime(2027, 2, 1), result);
    }

    [TestMethod]
    public void TerminateRentalCommand_RentedRack_SetsStatusTerminatedAndEndDate()
    {
        // Arrange: en udlejet reol med en aktiv lejeaftale
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Rented);
        var rental = AddRental(rack, renter, null);
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act
        viewModel.TerminateRentalCommand.Execute(null);

        // Assert
        var expectedDate = RackViewModel.CalculateTerminationEffectiveDate(DateTime.Now);
        Assert.AreEqual(RackStatus.Terminated, rack.Status);
        Assert.IsNotNull(rental.EndDate);
        Assert.AreEqual(expectedDate, rental.EndDate.Value);
    }

    [TestMethod]
    public void CancelTerminationCommand_TerminatedRack_SetsStatusBackToRented()
    {
        // Arrange: en opsagt reol, hvor opsigelsen først træder i kraft i fremtiden
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Terminated);
        var rental = AddRental(rack, renter, new DateTime(2100, 1, 1));
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act
        viewModel.CancelTerminationCommand.Execute(null);

        // Assert
        Assert.AreEqual(RackStatus.Rented, rack.Status);
        Assert.IsNull(rental.EndDate);
    }

    [TestMethod]
    public void Constructor_TerminationDateHasPassed_MakesRackAvailable()
    {
        // Arrange: en opsagt reol, hvor slutdatoen er overskredet for længe siden
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Terminated);
        AddRental(rack, renter, new DateTime(2000, 1, 1));

        // Act
        _ = CreateViewModel();

        // Assert
        Assert.AreEqual(RackStatus.Available, rack.Status);
    }

    [TestMethod]
    public void SelectRenterCommand_DatabaseDown_ShowsErrorInsteadOfCrashing()
    {
        // Arrange: databasen går ned, efter programmet er startet
        var renter = AddRenter();
        var viewModel = CreateViewModel();
        _rentals.SimulateDatabaseDown = true;

        // Act
        viewModel.SelectRenterCommand.Execute(renter);

        // Assert
        Assert.IsNotNull(_dialog.LastError);
        Assert.Contains("Kunne ikke forbinde", _dialog.LastError);
    }
    [TestMethod]
    public void SearchQuery_ByFirstName_FindsRenterIgnoringCase()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "anna";

        Assert.HasCount(1, viewModel.SearchResults);
        Assert.AreEqual("Anna", viewModel.SearchResults[0].FirstName);
    }

    [TestMethod]
    public void SearchQuery_ByFullName_FindsRenter()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "Anna Andersen";

        Assert.HasCount(1, viewModel.SearchResults);
        Assert.AreEqual("Anna", viewModel.SearchResults[0].FirstName);
    }

    [TestMethod]
    public void SearchQuery_ByPhone_FindsRenter()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "20000002";

        Assert.HasCount(1, viewModel.SearchResults);
        Assert.AreEqual("Bo", viewModel.SearchResults[0].FirstName);
    }

    [TestMethod]
    public void SearchQuery_ByAddress_FindsRenter()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "Havnevej";

        Assert.HasCount(1, viewModel.SearchResults);
        Assert.AreEqual("Bo", viewModel.SearchResults[0].FirstName);
    }

    [TestMethod]
    public void SearchQuery_ByCity_FindsRenter()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "Slagelse";

        Assert.HasCount(1, viewModel.SearchResults);
        Assert.AreEqual("Anna", viewModel.SearchResults[0].FirstName);
    }

    [TestMethod]
    public void SearchQuery_NoMatch_ReturnsEmptyList()
    {
        var viewModel = CreateViewModelWithTwoRenters();

        viewModel.SearchQuery = "xyz";

        Assert.HasCount(0, viewModel.SearchResults);
    }
    [TestMethod]
    public void CheckTerminationDatesFromTimer_DatabaseStaysDown_ShowsErrorOnlyOnce()
    {
        // Arrange: en opsagt reol, så tjekket rammer databasen
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Terminated);
        AddRental(rack, renter, new DateTime(2100, 1, 1));
        var viewModel = CreateViewModel();
        _rentals.SimulateDatabaseDown = true;

        // Act: timeren tikker tre gange, mens databasen er nede
        viewModel.CheckTerminationDatesFromTimer();
        viewModel.CheckTerminationDatesFromTimer();
        viewModel.CheckTerminationDatesFromTimer();

        // Assert
        Assert.AreEqual(1, _dialog.ErrorCount);
    }

    [TestMethod]
    public void CheckTerminationDatesFromTimer_DatabaseComesBackAndFailsAgain_ShowsErrorAgain()
    {
        // Arrange
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Terminated);
        AddRental(rack, renter, new DateTime(2100, 1, 1));
        var viewModel = CreateViewModel();

        // Act: databasen går ned, kommer op igen, og går ned igen
        _rentals.SimulateDatabaseDown = true;
        viewModel.CheckTerminationDatesFromTimer();
        _rentals.SimulateDatabaseDown = false;
        viewModel.CheckTerminationDatesFromTimer();
        _rentals.SimulateDatabaseDown = true;
        viewModel.CheckTerminationDatesFromTimer();

        // Assert: fejlen vises én gang for hver gang, databasen går ned
        Assert.AreEqual(2, _dialog.ErrorCount);
    }
    [TestMethod]
    public void SelectRackCommand_RackBelongsToOtherRenter_ShowsInfoAndDoesNotSelect()
    {
        // Arrange: to lejere, som hver har en udlejet reol
        var renterA = AddRenter();
        var renterB = AddRenter();
        var rackA = AddRackWithStatus(1, RackStatus.Rented);
        var rackB = AddRackWithStatus(2, RackStatus.Rented);
        AddRental(rackA, renterA, null);
        AddRental(rackB, renterB, null);
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renterA);

        // Act: klik på lejer B's reol, mens lejer A er valgt
        viewModel.SelectRackCommand.Execute(viewModel.Racks[1]);

        // Assert
        Assert.IsNotNull(_dialog.LastInfo);
        Assert.IsFalse(viewModel.Racks[1].IsSelected);
    }
}