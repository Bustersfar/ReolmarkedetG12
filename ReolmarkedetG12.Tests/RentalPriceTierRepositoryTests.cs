using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RentalPriceTierRepositoryTests
{
    private const string TestConnectionString = TestDatabase.ConnectionString;

    [TestMethod]
    [TestCategory("Database")]
    public void GetAll_TestDatabase_Returns3Tiers()
    {
        // Arrange
        var repo = new RentalPriceTierRepository(TestConnectionString);

        // Act
        var tiers = repo.GetAll().ToList();

        // Assert
        Assert.HasCount(3, tiers);
    }

    [TestMethod]
    [TestCategory("Database")]
    public void GetAll_TestDatabase_ReturnsCorrectPrices()
    {
        // Arrange
        var repo = new RentalPriceTierRepository(TestConnectionString);

        // Act
        var tiers = repo.GetAll().OrderBy(t => t.MinRacks).ToList();

        // Assert: første trin er 1 reol til 850 kr.
        Assert.AreEqual(1, tiers[0].MinRacks);
        Assert.AreEqual(1, tiers[0].MaxRacks);
        Assert.AreEqual(850m, tiers[0].PricePerRack);

        // Assert: sidste trin har ingen øvre grænse (4 reoler og op) til 800 kr.
        Assert.AreEqual(4, tiers[2].MinRacks);
        Assert.IsNull(tiers[2].MaxRacks);
        Assert.AreEqual(800m, tiers[2].PricePerRack);
    }

    [TestMethod]
    public void GetAll_ServerDoesNotExist_ThrowsDatabaseConnectionException()
    {
        // Arrange: en server der ikke findes
        var repo = new RentalPriceTierRepository(
            "Server=findes-ikke;Database=x;Connect Timeout=1;Trusted_Connection=True;TrustServerCertificate=True;");

        // Act + Assert
        Assert.ThrowsExactly<DatabaseConnectionException>(() => repo.GetAll());
    }
}