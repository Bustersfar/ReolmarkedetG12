using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Services;

public static class MonthlyStatementCalculator
{
    public const decimal CommissionRate = 0.10m;

    // Provisionen afrundes til hele øre (2 decimaler), ligesom lejen i RentalPriceCalculator.
    public static decimal CalculateCommission(decimal sales)
    {
        return Math.Round(sales * CommissionRate, 2, MidpointRounding.AwayFromZero);
    }

    // Én linje pr. lejer, der enten har solgt noget i måneden eller skal betale leje.
    // Lejen betales en måned forud: opgørelsen for f.eks. oktober trækker den fulde
    // leje for november for hver reol, lejeren stadig har pr. 1. november. En reol,
    // der er opsagt til den 1., tæller derfor ikke med.
    public static List<MonthlyStatementLine> Calculate(
        int year,
        int month,
        IEnumerable<Renter> renters,
        IEnumerable<Rental> rentals,
        IEnumerable<Sale> sales)
    {
        var monthStart = new DateTime(year, month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        var rentalsToCharge = rentals
            .Where(r => r.StartDate < nextMonthStart && (r.EndDate == null || r.EndDate > nextMonthStart))
            .ToList();

        // Butikkens egne salg (reol 0) har ingen RenterId og hører ikke til nogen lejer.
        var salesInMonth = sales
            .Where(s => s.RenterId.HasValue && s.Date >= monthStart && s.Date < nextMonthStart)
            .ToList();

        var lines = new List<MonthlyStatementLine>();

        foreach (var renter in renters)
        {
            var renterRentals = rentalsToCharge.Where(r => r.RenterId == renter.RenterId).ToList();
            decimal renterSales = salesInMonth.Where(s => s.RenterId == renter.RenterId).Sum(s => s.Amount);

            if (renterRentals.Count == 0 && renterSales == 0)
                continue;

            lines.Add(new MonthlyStatementLine
            {
                RenterId = renter.RenterId,
                RenterName = $"{renter.FirstName} {renter.LastName}",
                RenterEmail = renter.Email,
                RackCount = renterRentals.Count,
                Rent = renterRentals.Sum(r => r.MonthlyRent),
                Sales = renterSales,
                Commission = CalculateCommission(renterSales)
            });
        }

        return lines.OrderBy(l => l.RenterName).ToList();
    }
}
