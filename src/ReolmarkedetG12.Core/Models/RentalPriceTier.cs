namespace ReolmarkedetG12.Core.Models;

public class RentalPriceTier
{
    public int TierId { get; set; }
    public int MinRacks { get; set; }
    public int? MaxRacks { get; set; }
    public decimal PricePerRack { get; set; }
}