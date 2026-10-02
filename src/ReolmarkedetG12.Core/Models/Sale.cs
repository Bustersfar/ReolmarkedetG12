namespace ReolmarkedetG12.Core.Models;

public class Sale
{
    public int SaleId { get; set; }
    public int RackId { get; set; }
    public int RenterId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}