using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakePriceTierRepository : IRentalPriceTierRepository
{
    public List<RentalPriceTier> Tiers { get; } = new()
    {
        new RentalPriceTier { TierId = 1, MinRacks = 1, MaxRacks = 1, PricePerRack = 850 },
        new RentalPriceTier { TierId = 2, MinRacks = 2, MaxRacks = 3, PricePerRack = 825 },
        new RentalPriceTier { TierId = 3, MinRacks = 4, MaxRacks = null, PricePerRack = 800 }
    };

    public IEnumerable<RentalPriceTier> GetAll() => Tiers.ToList();
}