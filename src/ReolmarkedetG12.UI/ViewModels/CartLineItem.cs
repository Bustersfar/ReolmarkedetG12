namespace ReolmarkedetG12.UI.ViewModels;

// Én vare i det igangværende salg, endnu ikke gemt nogen steder.
// Når "Registrer betaling" trykkes, bliver hver linje i kurven til sit eget Sale-salg.
public class CartLineItem
{
    public int ItemId { get; set; }
    public int ItemNumber { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int RackId { get; set; }
    public decimal Price { get; set; }
}