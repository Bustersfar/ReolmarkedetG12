namespace ReolmarkedetG12.Core.Models;

// Én linje i månedsopgørelsen: status for én lejer i den valgte måned.
public class MonthlyStatementLine
{
    public int RenterId { get; set; }
    public string RenterName { get; set; } = string.Empty;
    public string? RenterEmail { get; set; }
    public int RackCount { get; set; }
    public decimal Rent { get; set; }
    public decimal Sales { get; set; }
    public decimal Commission { get; set; }

    // Negativt beløb betyder, at lejeren skylder butikken penge.
    public decimal Payout => Sales - Commission - Rent;

    public bool IsPayoutNegative => Payout < 0;
}
