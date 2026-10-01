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
            total += CalculateRackPriceAtPosition(position, tiers);
        }

        return total;
    }

    // Prisen for ÉN bestemt reol, ud fra hvilken "plads" den fylder i lejerens stak af reoler
    // (1. reol = 850, 2.-3. reol = 825 hver, 4. reol og opefter = 800 hver).
    // Bruges til at give hver reol sin egen faste pristrins-pris — ALDRIG et gennemsnit på tværs af reoler.
    public static decimal CalculateRackPriceAtPosition(int position, IEnumerable<RentalPriceTier> priceTiers)
    {
        if (position <= 0)
            throw new ArgumentException("Position skal være mindst 1.", nameof(position));

        var matchingTier = priceTiers.FirstOrDefault(tier =>
            position >= tier.MinRacks &&
            (tier.MaxRacks == null || position <= tier.MaxRacks));

        if (matchingTier == null)
            throw new InvalidOperationException($"Ingen prisregel matcher reol nr. {position}.");

        return matchingTier.PricePerRack;
    }

    public static decimal CalculatePartialMonthRent(decimal monthlyRent, DateOnly startDate)
    {
        int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
        int remainingDays = daysInMonth - startDate.Day + 1;
        return monthlyRent * remainingDays / daysInMonth;
    }
}