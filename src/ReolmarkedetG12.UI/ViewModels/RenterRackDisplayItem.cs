using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.UI.ViewModels;

public class RenterRackDisplayItem
{
    public RackDisplayItem RackItem { get; }
    public Rental Rental { get; }

    public RenterRackDisplayItem(RackDisplayItem rackItem, Rental rental)
    {
        RackItem = rackItem;
        Rental = rental;
    }
}