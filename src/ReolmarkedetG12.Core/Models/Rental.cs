namespace ReolmarkedetG12.Core.Models;

public class Rental
{
    public int RentalId { get; set; }
    public int RackId { get; set; }
    public int RenterId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
}