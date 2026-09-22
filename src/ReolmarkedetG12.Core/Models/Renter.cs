namespace ReolmarkedetG12.Core.Models;

public class Renter
{
    public int RenterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}