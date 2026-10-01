namespace ReolmarkedetG12.UI.ViewModels;

// Én vare i det igangværende salg, endnu ikke gemt nogen steder.
// Når "Registrer betaling" trykkes, bliver hver linje i kurven til sit eget Sale-salg.
public class CartLineItem
{
    public int RackNumber { get; set; }
    public string Remark { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}