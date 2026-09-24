using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests;

[TestClass]
[DoNotParallelize]
public class RentalRepositoryTests
{
    private const string TestConnectionString = TestDatabase.ConnectionString;

    // En lejeaftale skal høre til en lejer, så vi opretter en testlejer
    private static Renter CreateTestRenter()
    {
        var renterRepo = new RenterRepository(TestConnectionString);
        renterRepo.Add(new Renter
        {
            FirstName = "RentalTest",
            LastName = "Test",
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse"
        });
        return renterRepo.GetAll().First(r => r.FirstName == "RentalTest");
    }

    // En lejeaftale skal også høre til en reol, så vi bruger den første reol
    private static int GetAnyRackId()
    {
        return new RackRepository(TestConnectionString).GetAll().First().RackId;
    }

    // Sletter testlejerens lejeaftaler først og derefter selve lejeren
    private static void DeleteTestRenter(Renter renter)
    {
        var rentalRepo = new RentalRepository(TestConnectionString);
        foreach (var rental in rentalRepo.GetAll().Where(r => r.RenterId == renter.RenterId))
        {
            rentalRepo.Delete(rental.RentalId);
        }
        new RenterRepository(TestConnectionString).Delete(renter.RenterId);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Add_NewRental_AllFieldsAreSavedInDatabase()
    {
        // Arrange
        var renter = CreateTestRenter();
        int rackId = GetAnyRackId();
        var repo = new RentalRepository(TestConnectionString);

        // Act
        repo.Add(new Rental
        {
            RackId = rackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 15),
            EndDate = null,
            MonthlyRent = 850m
        });
        var saved = repo.GetAll().FirstOrDefault(r => r.RenterId == renter.RenterId);

        // Oprydning
        DeleteTestRenter(renter);

        // Assert
        Assert.IsNotNull(saved);
        Assert.AreEqual(rackId, saved.RackId);
        Assert.AreEqual(new DateTime(2026, 1, 15), saved.StartDate);
        Assert.IsNull(saved.EndDate);
        Assert.AreEqual(850m, saved.MonthlyRent);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Add_RentalWithEndDate_SavesEndDate()
    {
        // Arrange
        var renter = CreateTestRenter();
        var repo = new RentalRepository(TestConnectionString);

        // Act
        repo.Add(new Rental
        {
            RackId = GetAnyRackId(),
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 15),
            EndDate = new DateTime(2026, 6, 30),
            MonthlyRent = 850m
        });
        var saved = repo.GetAll().FirstOrDefault(r => r.RenterId == renter.RenterId);

        // Oprydning
        DeleteTestRenter(renter);

        // Assert
        Assert.IsNotNull(saved);
        Assert.AreEqual(new DateTime(2026, 6, 30), saved.EndDate);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var repo = new RentalRepository(TestConnectionString);

        // Act
        var rental = repo.GetById(-1);

        // Assert
        Assert.IsNull(rental);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Update_ChangeMonthlyRent_IsSavedInDatabase()
    {
        // Arrange: opret en lejeaftale
        var renter = CreateTestRenter();
        var repo = new RentalRepository(TestConnectionString);
        repo.Add(new Rental
        {
            RackId = GetAnyRackId(),
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 15),
            MonthlyRent = 850m
        });
        var rental = repo.GetAll().First(r => r.RenterId == renter.RenterId);

        // Act: ret huslejen, og hent lejeaftalen igen
        rental.MonthlyRent = 1675m;
        repo.Update(rental);
        var updated = repo.GetById(rental.RentalId);

        // Oprydning
        DeleteTestRenter(renter);

        // Assert
        Assert.IsNotNull(updated);
        Assert.AreEqual(1675m, updated.MonthlyRent);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Delete_ExistingRental_RemovesItFromDatabase()
    {
        // Arrange: opret en lejeaftale
        var renter = CreateTestRenter();
        var repo = new RentalRepository(TestConnectionString);
        repo.Add(new Rental
        {
            RackId = GetAnyRackId(),
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 15),
            MonthlyRent = 850m
        });
        var rental = repo.GetAll().First(r => r.RenterId == renter.RenterId);

        // Act: slet lejeaftalen
        repo.Delete(rental.RentalId);
        var result = repo.GetById(rental.RentalId);

        // Oprydning
        DeleteTestRenter(renter);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAll_ServerDoesNotExist_ThrowsDatabaseConnectionException()
    {
        // Arrange: en server der ikke findes
        var repo = new RentalRepository(
            "Server=findes-ikke;Database=x;Connect Timeout=1;Trusted_Connection=True;TrustServerCertificate=True;");

        // Act + Assert
        Assert.ThrowsExactly<DatabaseConnectionException>(() => repo.GetAll());
    }
}