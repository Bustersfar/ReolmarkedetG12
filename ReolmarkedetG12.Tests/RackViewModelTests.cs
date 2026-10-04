using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Services;
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
    private readonly FakePaymentRepository _payments = new();
    private readonly FakeDialogService _dialog = new();

    private RackViewModel CreateViewModel() =>
        new RackViewModel(_racks, _renters, _rentals, new FakePriceTierRepository(), _payments, _dialog);

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
    public void CreateRentalCommand_TwoRacks_EachGetsItsOwnTierPrice()
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

        // Assert: 1. reol = 850 (pristrin 1), 2. reol = 825 (pristrin 2) — ALDRIG et gennemsnit af de to
        Assert.HasCount(2, _rentals.Rentals);
        Assert.AreEqual(850m, _rentals.Rentals[0].MonthlyRent);
        Assert.AreEqual(825m, _rentals.Rentals[1].MonthlyRent);
    }

    [TestMethod]
    public void CreateRentalCommand_FifthRackForExistingRenter_GetsTierPriceNotAverage()
    {
        // Arrange: lejeren har allerede 4 aktive reoler, hver med sin egen oprindelige pristrins-pris
        var renter = AddRenter();
        var rack1 = AddRackWithStatus(1, RackStatus.Rented);
        var rack2 = AddRackWithStatus(2, RackStatus.Rented);
        var rack3 = AddRackWithStatus(3, RackStatus.Rented);
        var rack4 = AddRackWithStatus(4, RackStatus.Rented);

        var rental1 = new Rental { RackId = rack1.RackId, RenterId = renter.RenterId, StartDate = new DateTime(2026, 1, 1), MonthlyRent = 850m };
        var rental2 = new Rental { RackId = rack2.RackId, RenterId = renter.RenterId, StartDate = new DateTime(2026, 1, 1), MonthlyRent = 825m };
        var rental3 = new Rental { RackId = rack3.RackId, RenterId = renter.RenterId, StartDate = new DateTime(2026, 1, 1), MonthlyRent = 825m };
        var rental4 = new Rental { RackId = rack4.RackId, RenterId = renter.RenterId, StartDate = new DateTime(2026, 1, 1), MonthlyRent = 800m };
        _rentals.Add(rental1);
        _rentals.Add(rental2);
        _rentals.Add(rental3);
        _rentals.Add(rental4);

        AddRack(5);
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks.Single(r => r.Rack.Number == 5));

        // Act
        viewModel.CreateRentalCommand.Execute(null);

        // Assert: den 5. reol koster 800 kr. (pristrinnet "4 reoler og derover") —
        // IKKE 820 kr. (det forkerte gennemsnit af 4100/5, som var buggen)
        var rack5Id = viewModel.Racks.Single(r => r.Rack.Number == 5).Rack.RackId;
        var newRental = _rentals.Rentals.Single(r => r.RackId == rack5Id);
        Assert.AreEqual(800m, newRental.MonthlyRent);

        // De 4 eksisterende reolers priser er fuldstændig uændrede
        Assert.AreEqual(850m, rental1.MonthlyRent);
        Assert.AreEqual(825m, rental2.MonthlyRent);
        Assert.AreEqual(825m, rental3.MonthlyRent);
        Assert.AreEqual(800m, rental4.MonthlyRent);
    }

    [TestMethod]
    public void CreateRentalCommand_Confirmed_CreatesFirstMonthPaymentInRepository()
    {
        // Arrange: to ledige reoler og én lejer, brugeren svarer ja
        AddRack(1);
        AddRack(2);
        var renter = AddRenter();
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[1]);

        // Act
        viewModel.CreateRentalCommand.Execute(null);

        // Assert: 1. måneds leje er registreret som Payment-poster i PaymentRepository
        var today = DateOnly.FromDateTime(DateTime.Now);
        var expectedTotal = RentalPriceCalculator.CalculatePartialMonthRent(850m + 825m, today);

        Assert.HasCount(2, _payments.Payments);
        Assert.AreEqual(expectedTotal, _payments.Payments.Sum(p => p.Amount));
        Assert.IsTrue(_payments.Payments.All(p => p.Type == PaymentType.FirstMonthPayment));
        Assert.IsTrue(_payments.Payments.All(p => p.RenterId == renter.RenterId));
    }

    [TestMethod]
    public void CreateRentalCommand_Declined_CreatesNoPaymentOrRental()
    {
        // Arrange: brugeren svarer nej i bekræftelsesdialogen
        AddRack(1);
        var renter = AddRenter();
        var viewModel = CreateViewModel();
        viewModel.SelectRenterCommand.Execute(renter);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);
        _dialog.ConfirmResult = false;

        // Act
        viewModel.CreateRentalCommand.Execute(null);

        // Assert
        Assert.HasCount(0, _payments.Payments);
        Assert.HasCount(0, _rentals.Rentals);
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
    public void SelectRackCommand_RackBelongsToOtherRenter_SwitchesToThatRenter()
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

        // Assert: systemet skifter roligt til lejer B i stedet for at blokere
        Assert.AreEqual(renterB.RenterId, viewModel.FoundRenter!.RenterId);
        Assert.IsTrue(viewModel.Racks[1].IsSelected);
    }

    [TestMethod]
    public void SelectRackCommand_DeselectLastSelectedRack_ClearsFoundRenter()
    {
        // Arrange: en udlejet reol klikkes, så kunden vises
        var renter = AddRenter();
        var rack = AddRackWithStatus(1, RackStatus.Rented);
        AddRental(rack, renter, null);
        var viewModel = CreateViewModel();
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act: klik samme reol igen (fravælg)
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Assert: kunden må ikke stadig stå i vinduet
        Assert.IsNull(viewModel.FoundRenter);
        Assert.HasCount(0, viewModel.RenterRacks);
    }

    [TestMethod]
    public void SelectRackCommand_AvailableRackAfterDeselectingOtherRenter_HasNoLeftoverRenter()
    {
        // Arrange: kunde A's reol vises og fravælges igen (som når man bare viser kunden deres reol)
        var renterA = AddRenter();
        var rentedRack = AddRackWithStatus(1, RackStatus.Rented);
        AddRental(rentedRack, renterA, null);
        AddRack(2); // ledig reol
        var viewModel = CreateViewModel();
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act: en ny kunde kommer og vælger en ledig reol
        viewModel.SelectRackCommand.Execute(viewModel.Racks[1]);

        // Assert: ingen kunde må være "hængende" fra før
        Assert.IsNull(viewModel.FoundRenter);
        Assert.IsTrue(viewModel.Racks[1].IsSelected);
    }

    [TestMethod]
    public void SelectRackCommand_AvailableRackWhileViewingSameRenterRack_KeepsRenterForExtension()
    {
        // Arrange: kunde A's udlejede reol klikkes (viser kunden), uden at fravælge den igen
        var renterA = AddRenter();
        var rentedRack = AddRackWithStatus(1, RackStatus.Rented);
        AddRental(rentedRack, renterA, null);
        AddRack(2); // ledig reol, som kunden vil leje ekstra
        var viewModel = CreateViewModel();
        viewModel.SelectRackCommand.Execute(viewModel.Racks[0]);

        // Act: klik den ledige reol - kunden skal IKKE forsvinde, det er en udvidelse af deres leje
        viewModel.SelectRackCommand.Execute(viewModel.Racks[1]);

        // Assert
        Assert.IsNotNull(viewModel.FoundRenter);
        Assert.AreEqual(renterA.RenterId, viewModel.FoundRenter!.RenterId);
        Assert.IsTrue(viewModel.Racks[1].IsSelected);
        Assert.IsTrue(viewModel.CreateRentalCommand.CanExecute(null));
    }

    [TestMethod]
    public void TerminateRentalCommand_MixedRentedAndTerminatedSelection_CannotExecute()
    {
        // Arrange: samme kunde har både en udlejet og en opsagt reol, begge markeres
        var renter = AddRenter();
        var rentedRack = AddRackWithStatus(1, RackStatus.Rented);
        AddRental(rentedRack, renter, null);
        var terminatedRack = AddRackWithStatus(2, RackStatus.Terminated);
        AddRental(terminatedRack, renter, new DateTime(2100, 1, 1));
        var viewModel = CreateViewModel();
        viewModel.SelectRackCommand.Execute(viewModel.Racks.Single(r => r.Rack.Number == 1)); // udlejet
        viewModel.SelectRackCommand.Execute(viewModel.Racks.Single(r => r.Rack.Number == 2)); // opsagt, samme kunde

        // Assert: "Opsig aftale" må ikke kunne køres når valget blander status
        Assert.IsFalse(viewModel.TerminateRentalCommand.CanExecute(null));
    }
}