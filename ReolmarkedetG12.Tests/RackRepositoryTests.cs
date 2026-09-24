using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using ReolmarkedetG12.Core.Exceptions;

namespace ReolmarkedetG12.Tests;

[TestClass]
[DoNotParallelize]
public class RackRepositoryTests
{
    private const string TestConnectionString =
        "Server=localhost;Database=ReolmarkedetTest;Trusted_Connection=True;TrustServerCertificate=True;";

    [TestMethod]
    [TestCategory("Database")]
    public void GetAll_TestDatabase_Returns80Racks()
    {
        // Arrange
        var repo = new RackRepository(TestConnectionString);

        // Act
        var racks = repo.GetAll().ToList();

        // Assert
        Assert.AreEqual(80, racks.Count);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void Add_NewRack_IsSavedInDatabase()
    {
        // Arrange
        var repo = new RackRepository(TestConnectionString);
        var rack = new Rack { Number = 999, Status = RackStatus.Available };

        // Act
        repo.Add(rack);

        // Find den nye reol, og slet den igen (oprydning)
        var saved = repo.GetAll().FirstOrDefault(r => r.Number == 999);
        if (saved != null)
        {
            repo.Delete(saved.RackId);
        }

        // Assert
        Assert.IsNotNull(saved);
    }
    [TestMethod]
    [TestCategory("Database")]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var repo = new RackRepository(TestConnectionString);

        // Act
        var rack = repo.GetById(-1);

        // Assert
        Assert.IsNull(rack);
    }
    [TestMethod]
    public void GetAll_ServerDoesNotExist_ThrowsDatabaseConnectionException()
    {
        // Arrange: en server der ikke findes
        var repo = new RackRepository(
            "Server=findes-ikke;Database=x;Connect Timeout=1;Trusted_Connection=True;TrustServerCertificate=True;");

        // Act + Assert
        Assert.ThrowsExactly<DatabaseConnectionException>(() => repo.GetAll());
    }
}