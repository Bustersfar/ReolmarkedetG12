using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

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
}