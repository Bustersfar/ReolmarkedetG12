using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Services;

public static class RentalPriceCalculator
{
    public static decimal CalculateMonthlyRent(int numberOfRacks, IEnumerable<RentalPriceTier> priceTiers)
    {
        if (numberOfRacks <= 0)
            throw new ArgumentException("Antal reoler skal være mindst 1.", nameof(numberOfRacks));

        var tiers = priceTiers.ToList();
        decimal total = 0;

        for (int position = 1; position <= numberOfRacks; position++)
        {
            var matchingTier = tiers.FirstOrDefault(tier =>
                position >= tier.MinRacks &&
                (tier.MaxRacks == null || position <= tier.MaxRacks));

            if (matchingTier == null)
                throw new InvalidOperationException($"Ingen prisregel matcher reol nr. {position}.");

            total += matchingTier.PricePerRack;
        }

        return total;
    }

    public static decimal CalculatePartialMonthRent(decimal monthlyRent, DateOnly startDate)
    {
        int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
        int remainingDays = daysInMonth - startDate.Day + 1;
        return monthlyRent * remainingDays / daysInMonth;
    }
}