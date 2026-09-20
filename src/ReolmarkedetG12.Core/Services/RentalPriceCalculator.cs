using System.Linq;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Services;

public static class RentalPriceCalculator
{
    public static decimal CalculateMonthlyRent(int numberOfRacks, IEnumerable<RentalPriceTier> priceTiers)
    {
        if (numberOfRacks <= 0)
            throw new ArgumentException("Antal reoler skal være mindst 1.", nameof(numberOfRacks));

        var matchingTier = priceTiers.FirstOrDefault(tier =>
            numberOfRacks >= tier.MinRacks &&
            (tier.MaxRacks == null || numberOfRacks <= tier.MaxRacks));

        if (matchingTier == null)
            throw new InvalidOperationException($"Ingen prisregel matcher {numberOfRacks} reoler.");

        return numberOfRacks * matchingTier.PricePerRack;
    }
}