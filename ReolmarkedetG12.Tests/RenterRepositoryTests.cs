using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.Core.Exceptions;

namespace ReolmarkedetG12.Tests;

[TestClass]
[DoNotParallelize]
public class RenterRepositoryTests
{
    private const string TestConnectionString = TestDatabase.ConnectionString;

    [TestMethod]
    [TestCategory("Database")]
    public void Add_NewRenter_AllFieldsAreSavedInDatabase()
    {
        // Arrange: alle felter er udfyldt
        var repo = new RenterRepository(TestConnectionString);
        var renter = new Renter
        {
            FirstName = "TestFornavn",
            LastName = "TestEfternavn",
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse",
            Email = "test@test.dk",
            Phone = "12345678"
        };

        // Act
        repo.Add(renter);

        // Find den nye lejer, og slet den igen (oprydning)
        var saved = repo.GetAll().FirstOrDefault(r => r.FirstName == "TestFornavn");
        if (saved != null)
        {
            repo.Delete(saved.RenterId);
        }

        // Assert
        Assert.IsNotNull(saved);
        Assert.AreEqual("TestEfternavn", saved.LastName);
        Assert.AreEqual("Testvej 1", saved.Address);
        Assert.AreEqual(4200, saved.PostalCode);
        Assert.AreEqual("Slagelse", saved.City);
        Assert.AreEqual("test@test.dk", saved.Email);
        Assert.AreEqual("12345678", saved.Phone);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Add_RenterWithoutEmailAndPhone_SavesThemAsNull()
    {
        // Arrange: Email og Phone er ikke udfyldt
        var repo = new RenterRepository(TestConnectionString);
        repo.Add(new Renter
        {
            FirstName = "NullTest",
            LastName = "Test",
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse"
        });

        // Act
        var saved = repo.GetAll().FirstOrDefault(r => r.FirstName == "NullTest");

        // Oprydning
        if (saved != null)
        {
            repo.Delete(saved.RenterId);
        }

        // Assert
        Assert.IsNotNull(saved);
        Assert.IsNull(saved.Email);
        Assert.IsNull(saved.Phone);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var repo = new RenterRepository(TestConnectionString);

        // Act
        var renter = repo.GetById(-1);

        // Assert
        Assert.IsNull(renter);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Update_ChangeCity_IsSavedInDatabase()
    {
        // Arrange: opret en lejer
        var repo = new RenterRepository(TestConnectionString);
        repo.Add(new Renter
        {
            FirstName = "UpdateTest",
            LastName = "Test",
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse"
        });
        var renter = repo.GetAll().First(r => r.FirstName == "UpdateTest");

        // Act: ret byen, og hent lejeren igen
        renter.City = "Korsør";
        repo.Update(renter);
        var updated = repo.GetById(renter.RenterId);

        // Oprydning
        repo.Delete(renter.RenterId);

        // Assert
        Assert.IsNotNull(updated);
        Assert.AreEqual("Korsør", updated.City);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Delete_ExistingRenter_RemovesItFromDatabase()
    {
        // Arrange: opret en lejer
        var repo = new RenterRepository(TestConnectionString);
        repo.Add(new Renter
        {
            FirstName = "DeleteTest",
            LastName = "Test",
            Address = "Testvej 1",
            PostalCode = 4200,
            City = "Slagelse"
        });
        var renter = repo.GetAll().First(r => r.FirstName == "DeleteTest");

        // Act: slet lejeren
        repo.Delete(renter.RenterId);
        var result = repo.GetById(renter.RenterId);

        // Assert
        Assert.IsNull(result);
    }
    [TestMethod]
    public void GetAll_ServerDoesNotExist_ThrowsDatabaseConnectionException()
    {
        // Arrange: en server der ikke findes
        var repo = new RenterRepository(
            "Server=findes-ikke;Database=x;Connect Timeout=1;Trusted_Connection=True;TrustServerCertificate=True;");

        // Act + Assert
        Assert.ThrowsExactly<DatabaseConnectionException>(() => repo.GetAll());
    }
}