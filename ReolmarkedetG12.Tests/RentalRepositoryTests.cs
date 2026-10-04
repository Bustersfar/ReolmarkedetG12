using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RentalRepositoryTests
{
    private const string TestConnectionString = TestDatabase.ConnectionString;

    private static (Renter renter, Rack rack) CreateTestEntities()
    {
        var renterRepo = new RenterRepository(TestConnectionString);
        var rackRepo = new RackRepository(TestConnectionString);
        var rentalRepo = new RentalRepository(TestConnectionString);

        var renter = new Renter
        {
            FirstName = "RentalTest_" + Guid.NewGuid().ToString("N")[..8],
            LastName = "Person",
            Address = "Testgade 1",
            PostalCode = 4200,
            City = "Slagelse"
        };
        renterRepo.Add(renter);
        var createdRenter = renterRepo.GetAll().First(r => r.FirstName == renter.FirstName);

        // Find en reol uden eksisterende aktive lejemål
        var allRacks = rackRepo.GetAll().Where(r => r.Number >= 1 && r.Number <= 80).ToList();
        var allRentals = rentalRepo.GetAll().ToList();
        var racksWithActiveRental = allRentals
            .Where(r => r.EndDate == null || r.EndDate > DateTime.UtcNow)
            .Select(r => r.RackId)
            .ToHashSet();

        var freeRack = allRacks.FirstOrDefault(r => !racksWithActiveRental.Contains(r.RackId))
                       ?? allRacks.First();

        return (createdRenter, freeRack);
    }

    [TestMethod]
    public void Constructor_ServerDoesNotExist_ThrowsDatabaseConnectionException()
    {
        var invalidConnectionString = "Server=server_der_ikke_findes;Database=ReolmarkedetTest;Integrated Security=True;TrustServerCertificate=True;";
        var repo = new RentalRepository(invalidConnectionString);

        Assert.ThrowsExactly<DatabaseConnectionException>(() => repo.GetAll());
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Add_ValidRental_CanBeRetrieved()
    {
        var (renter, rack) = CreateTestEntities();
        var repo = new RentalRepository(TestConnectionString);
        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = null,
            MonthlyRent = 850m
        };

        repo.Add(rental);
        var retrieved = repo.GetById(rental.RentalId);

        // Oprydning
        repo.Delete(rental.RentalId);
        new RenterRepository(TestConnectionString).Delete(renter.RenterId);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(rack.RackId, retrieved.RackId);
        Assert.AreEqual(renter.RenterId, retrieved.RenterId);
        Assert.AreEqual(850m, retrieved.MonthlyRent);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void AddRentalWithRackStatus_ValidRental_InsertsRentalAndUpdatesRackStatusInTransaction()
    {
        // Arrange
        var (renter, rack) = CreateTestEntities();
        var rentalRepo = new RentalRepository(TestConnectionString);
        var rackRepo = new RackRepository(TestConnectionString);

        var originalStatus = rack.Status;

        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = DateTime.UtcNow,
            EndDate = null,
            MonthlyRent = 850m
        };

        try
        {
            // Act
            rentalRepo.AddRentalWithRackStatus(rental, (int)RackStatus.Rented);

            // Assert
            var createdRental = rentalRepo.GetById(rental.RentalId);
            var updatedRack = rackRepo.GetById(rack.RackId);

            Assert.IsNotNull(createdRental);
            Assert.AreEqual(rack.RackId, createdRental.RackId);
            Assert.IsNotNull(updatedRack);
            Assert.AreEqual(RackStatus.Rented, updatedRack.Status);
        }
        finally
        {
            rack.Status = originalStatus;
            rackRepo.Update(rack);

            if (rental.RentalId > 0)
            {
                rentalRepo.Delete(rental.RentalId);
            }
            new RenterRepository(TestConnectionString).Delete(renter.RenterId);
        }
    }

    [TestMethod]
    [TestCategory("Database")]
    public void GetActiveRentalByRackId_ReturnsActiveRental()
    {
        var (renter, rack) = CreateTestEntities();
        var repo = new RentalRepository(TestConnectionString);
        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = DateTime.UtcNow.AddMinutes(5), // Sikrer at den er nyere end evt. historik
            EndDate = null,
            MonthlyRent = 850m
        };
        repo.Add(rental);

        var active = repo.GetActiveRentalByRackId(rack.RackId);

        repo.Delete(rental.RentalId);
        new RenterRepository(TestConnectionString).Delete(renter.RenterId);

        Assert.IsNotNull(active);
        Assert.AreEqual(rental.RentalId, active.RentalId);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void GetCompletedRentalsByRackId_ReturnsOnlyCompletedRentals()
    {
        var (renter, rack) = CreateTestEntities();
        var repo = new RentalRepository(TestConnectionString);

        var completedRental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 1),
            MonthlyRent = 850m
        };
        repo.Add(completedRental);

        var activeRental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2025, 7, 1),
            EndDate = null,
            MonthlyRent = 850m
        };
        repo.Add(activeRental);

        var completedList = repo.GetCompletedRentalsByRackId(rack.RackId).ToList();

        repo.Delete(completedRental.RentalId);
        repo.Delete(activeRental.RentalId);
        new RenterRepository(TestConnectionString).Delete(renter.RenterId);

        Assert.IsTrue(completedList.Any(r => r.RentalId == completedRental.RentalId));
        Assert.IsFalse(completedList.Any(r => r.RentalId == activeRental.RentalId));
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Update_ChangesRentalData()
    {
        var (renter, rack) = CreateTestEntities();
        var repo = new RentalRepository(TestConnectionString);
        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = null,
            MonthlyRent = 850m
        };
        repo.Add(rental);

        rental.EndDate = new DateTime(2026, 6, 1);
        rental.MonthlyRent = 900m;
        repo.Update(rental);

        var updated = repo.GetById(rental.RentalId);

        repo.Delete(rental.RentalId);
        new RenterRepository(TestConnectionString).Delete(renter.RenterId);

        Assert.IsNotNull(updated);
        Assert.AreEqual(new DateTime(2026, 6, 1), updated.EndDate);
        Assert.AreEqual(900m, updated.MonthlyRent);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Delete_RemovesRental()
    {
        var (renter, rack) = CreateTestEntities();
        var repo = new RentalRepository(TestConnectionString);
        var rental = new Rental
        {
            RackId = rack.RackId,
            RenterId = renter.RenterId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = null,
            MonthlyRent = 850m
        };
        repo.Add(rental);

        repo.Delete(rental.RentalId);
        var retrieved = repo.GetById(rental.RentalId);

        new RenterRepository(TestConnectionString).Delete(renter.RenterId);

        Assert.IsNull(retrieved);
    }
}