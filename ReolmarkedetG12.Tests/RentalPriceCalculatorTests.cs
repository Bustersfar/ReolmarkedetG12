using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Services;

namespace ReolmarkedetG12.Tests;

[TestClass]
public class RentalPriceCalculatorTests
{
    private static List<RentalPriceTier> GetTiers()
    {
        return new List<RentalPriceTier>
        {
            new RentalPriceTier { MinRacks = 1, MaxRacks = 1, PricePerRack = 850 },
            new RentalPriceTier { MinRacks = 2, MaxRacks = 3, PricePerRack = 825 },
            new RentalPriceTier { MinRacks = 4, MaxRacks = null, PricePerRack = 800 }
        };
    }

    [TestMethod]
    public void CalculateMonthlyRent_OneRack_Returns850()
    {
        // Arrange
        var tiers = GetTiers();

        // Act
        decimal result = RentalPriceCalculator.CalculateMonthlyRent(1, tiers);

        // Assert
        Assert.AreEqual(850m, result);
    }

    [TestMethod]
    public void CalculateMonthlyRent_TwoRacks_Returns1675()
    {
        // Arrange
        var tiers = GetTiers();

        // Act
        decimal result = RentalPriceCalculator.CalculateMonthlyRent(2, tiers);

        // Assert
        Assert.AreEqual(1675m, result);
    }
    [TestMethod]
    public void CalculateMonthlyRent_ThreeRacks_Returns2500()
    {
        // Arrange
        var tiers = GetTiers();

        // Act
        decimal result = RentalPriceCalculator.CalculateMonthlyRent(3, tiers);

        // Assert
        Assert.AreEqual(2500m, result);
    }
    [TestMethod]
    public void CalculateMonthlyRent_FourRacks_Returns3300()
    {
        // Arrange
        var tiers = GetTiers();

        // Act
        decimal result = RentalPriceCalculator.CalculateMonthlyRent(4, tiers);

        // Assert
        Assert.AreEqual(3300m, result);
    }
    [TestMethod]
    public void CalculateMonthlyRent_ZeroRacks_ThrowsArgumentException()
    {
        // Arrange
        var tiers = GetTiers();

        // Act + Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            RentalPriceCalculator.CalculateMonthlyRent(0, tiers));
    }
    [TestMethod]
    public void CalculateMonthlyRent_NoPriceTiers_ThrowsInvalidOperationException()
    {
        // Arrange: en tom liste uden prisregler
        var tiers = new List<RentalPriceTier>();

        // Act + Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            RentalPriceCalculator.CalculateMonthlyRent(1, tiers));
    }

    [TestMethod]
    public void CalculatePartialMonthRent_StartOnFirstDay_ReturnsFullMonthlyRent()
    {
        // Arrange: oktober har 31 dage, og lejen starter den 1.
        var startDate = new DateOnly(2026, 10, 1);

        // Act
        decimal result = RentalPriceCalculator.CalculatePartialMonthRent(850m, startDate);

        // Assert
        Assert.AreEqual(850m, result);
    }

    [TestMethod]
    public void CalculatePartialMonthRent_UnevenAmount_IsRoundedToTwoDecimals()
    {
        // Arrange: 22 af 31 dage tilbage i oktober, 850 * 22 / 31 = 603,2258...
        var startDate = new DateOnly(2026, 10, 10);

        // Act
        decimal result = RentalPriceCalculator.CalculatePartialMonthRent(850m, startDate);

        // Assert: afrundet til hele øre, så det matcher DECIMAL(18,2) i databasen
        Assert.AreEqual(603.23m, result);
        Assert.AreEqual(result, Math.Round(result, 2));
    }
}