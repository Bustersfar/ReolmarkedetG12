using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Services;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class MonthlyStatementCalculatorTests
{
    private static List<Renter> GetRenters()
    {
        return new List<Renter>
        {
            new Renter { RenterId = 1, FirstName = "Anna", LastName = "Jensen", Email = "anna@example.dk" },
            new Renter { RenterId = 2, FirstName = "Bo", LastName = "Larsen" }
        };
    }

    private static List<RentalPriceTier> GetTiers()
    {
        return new List<RentalPriceTier>
        {
            new RentalPriceTier { TierId = 1, MinRacks = 1, MaxRacks = 1, PricePerRack = 850m },
            new RentalPriceTier { TierId = 2, MinRacks = 2, MaxRacks = 3, PricePerRack = 825m },
            new RentalPriceTier { TierId = 3, MinRacks = 4, MaxRacks = null, PricePerRack = 800m }
        };
    }

    [TestMethod]
    public void CalculateCommission_Sales1000_Returns100()
    {
        // Act
        decimal result = MonthlyStatementCalculator.CalculateCommission(1000m);

        // Assert
        Assert.AreEqual(100m, result);
    }

    [TestMethod]
    public void CalculateCommission_RoundsToTwoDecimals()
    {
        // Act
        decimal result = MonthlyStatementCalculator.CalculateCommission(99.95m);

        // Assert
        Assert.AreEqual(10.00m, result);
    }

    [TestMethod]
    public void Calculate_RenterWithRackAndSales_ReturnsRentSalesCommissionAndPayout()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m }
        };
        var sales = new List<Sale>
        {
            new Sale { RenterId = 1, RackId = 5, Amount = 1200m, Date = new DateTime(2026, 10, 3) },
            new Sale { RenterId = 1, RackId = 5, Amount = 300m, Date = new DateTime(2026, 10, 31, 17, 30, 0) }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, sales, GetTiers());

        // Assert
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("Anna Jensen", lines[0].RenterName);
        Assert.AreEqual("anna@example.dk", lines[0].RenterEmail);
        Assert.AreEqual(1, lines[0].RackCount);
        Assert.AreEqual(850m, lines[0].Rent);
        Assert.AreEqual(1500m, lines[0].Sales);
        Assert.AreEqual(150m, lines[0].Commission);
        Assert.AreEqual(500m, lines[0].Payout);
    }

    [TestMethod]
    public void Calculate_SalesOutsideMonth_AreNotIncluded()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m }
        };
        var sales = new List<Sale>
        {
            new Sale { RenterId = 1, RackId = 5, Amount = 100m, Date = new DateTime(2026, 9, 30, 23, 59, 0) },
            new Sale { RenterId = 1, RackId = 5, Amount = 200m, Date = new DateTime(2026, 10, 15) },
            new Sale { RenterId = 1, RackId = 5, Amount = 400m, Date = new DateTime(2026, 11, 1) }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, sales, GetTiers());

        // Assert
        Assert.AreEqual(200m, lines[0].Sales);
    }

    [TestMethod]
    public void Calculate_RentalStartedMidMonth_ChargesFullRent()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 10, 20), MonthlyRent = 850m }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, new List<Sale>(), GetTiers());

        // Assert
        Assert.AreEqual(850m, lines[0].Rent);
    }

    [TestMethod]
    public void Calculate_RentalTerminatedByFirstOfNextMonth_ChargesNoRentButKeepsSales()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental
            {
                RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10),
                EndDate = new DateTime(2026, 11, 1), MonthlyRent = 850m
            }
        };
        var sales = new List<Sale>
        {
            new Sale { RenterId = 1, RackId = 5, Amount = 500m, Date = new DateTime(2026, 10, 15) }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, sales, GetTiers());

        // Assert
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual(0m, lines[0].Rent);
        Assert.AreEqual(450m, lines[0].Payout);
    }

    [TestMethod]
    public void Calculate_RenterWithSeveralRacks_SumsRentPerRack()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m },
            new Rental { RenterId = 1, RackId = 6, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 825m }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, new List<Sale>(), GetTiers());

        // Assert
        Assert.AreEqual(2, lines[0].RackCount);
        Assert.AreEqual(1675m, lines[0].Rent);
        Assert.AreEqual(-1675m, lines[0].Payout);
    }

    [TestMethod]
    public void Calculate_FirstRackTerminated_RemainingRackIsChargedAsSingleRack()
    {
        // Arrange: lejeren har to reoler (850 + 825), og den første er opsagt til den 1. november
        var rentals = new List<Rental>
        {
            new Rental
            {
                RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10),
                EndDate = new DateTime(2026, 11, 1), MonthlyRent = 850m
            },
            new Rental { RenterId = 1, RackId = 6, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 825m }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, new List<Sale>(), GetTiers());

        // Assert: én reol tilbage koster 850 kr., ikke de 825 kr. der står på lejemålet
        Assert.AreEqual(1, lines[0].RackCount);
        Assert.AreEqual(850m, lines[0].Rent);
    }

    [TestMethod]
    public void Calculate_ShopSalesWithoutRenter_AreIgnored()
    {
        // Arrange
        var sales = new List<Sale>
        {
            new Sale { RenterId = null, RackId = 0, Amount = 25m, Date = new DateTime(2026, 10, 15) }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), new List<Rental>(), sales, GetTiers());

        // Assert
        Assert.AreEqual(0, lines.Count);
    }

    [TestMethod]
    public void Calculate_RenterWithoutRacksOrSales_IsNotListed()
    {
        // Arrange
        var rentals = new List<Rental>
        {
            new Rental { RenterId = 1, RackId = 5, StartDate = new DateTime(2026, 8, 10), MonthlyRent = 850m }
        };

        // Act
        var lines = MonthlyStatementCalculator.Calculate(2026, 10, GetRenters(), rentals, new List<Sale>(), GetTiers());

        // Assert
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual(1, lines[0].RenterId);
    }
}
