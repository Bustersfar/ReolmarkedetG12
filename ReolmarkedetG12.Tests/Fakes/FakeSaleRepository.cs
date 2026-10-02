using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeSaleRepository : IRepository<Sale>
{
    public List<Sale> Sales { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;

    private void CheckDatabase()
    {
        if (SimulateDatabaseDown)
            throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", new Exception("Testfejl"));
    }

    public void Add(Sale sale)
    {
        CheckDatabase();
        sale.SaleId = _nextId++;
        Sales.Add(sale);
    }

    public Sale? GetById(int id)
    {
        CheckDatabase();
        return Sales.FirstOrDefault(s => s.SaleId == id);
    }

    public void Update(Sale sale)
    {
        CheckDatabase();
        var index = Sales.FindIndex(s => s.SaleId == sale.SaleId);
        if (index >= 0) Sales[index] = sale;
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Sales.RemoveAll(s => s.SaleId == id);
    }

    public IEnumerable<Sale> GetAll()
    {
        CheckDatabase();
        return Sales.ToList();
    }
}