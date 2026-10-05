using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories;

// Udvider IRepository<Sale> med de salgs-specifikke operationer, så ViewModels
// kan kalde dem direkte uden at skulle tjekke, om repositoryet "egentlig" er SaleRepository.
public interface ISaleRepository : IRepository<Sale>
{
    // Indsætter hele kurven i én transaktion
    void AddMany(IEnumerable<Sale> sales);

    // Opdaterer et salg og skriver en linje i audit-loggen
    void UpdateWithAudit(Sale updatedSale, Sale originalSale);

    // Sletter et salg og skriver en linje i audit-loggen
    void DeleteWithAudit(Sale saleToDelete);

    IEnumerable<SaleAuditLog> GetAuditLogsForSale(int saleId);
}