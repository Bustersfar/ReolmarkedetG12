namespace ReolmarkedetG12.Core.Models;

public class Renter
{
    public int RenterId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int PostalCode { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}