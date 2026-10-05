using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories;

// Udvider IRepository<Rental> med de lejemåls-specifikke operationer, så ViewModels
// kan kalde dem direkte uden at skulle tjekke, om repositoryet "egentlig" er RentalRepository.
public interface IRentalRepository : IRepository<Rental>
{
    // Opretter lejemålet og sætter reolens status i samme transaktion
    void AddRentalWithRackStatus(Rental rental, int rackStatus = 1);

    IEnumerable<Rental> GetCompletedRentalsByRackId(int rackId);
}