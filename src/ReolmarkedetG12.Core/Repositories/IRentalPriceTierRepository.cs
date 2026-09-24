using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories;

public interface IRentalPriceTierRepository
{
    IEnumerable<RentalPriceTier> GetAll();
}