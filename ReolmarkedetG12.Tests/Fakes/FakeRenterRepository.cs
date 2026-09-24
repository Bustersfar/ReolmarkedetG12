using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeRenterRepository : IRepository<Renter>
{
    public List<Renter> Renters { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;

    private void CheckDatabase()
    {
        if (SimulateDatabaseDown)
            throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", new Exception("Testfejl"));
    }

    public void Add(Renter renter)
    {
        CheckDatabase();
        renter.RenterId = _nextId++;
        Renters.Add(renter);
    }

    public Renter? GetById(int id)
    {
        CheckDatabase();
        return Renters.FirstOrDefault(r => r.RenterId == id);
    }

    public void Update(Renter renter)
    {
        CheckDatabase();
        var index = Renters.FindIndex(r => r.RenterId == renter.RenterId);
        if (index >= 0) Renters[index] = renter;
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Renters.RemoveAll(r => r.RenterId == id);
    }

    public IEnumerable<Renter> GetAll()
    {
        CheckDatabase();
        return Renters.ToList();
    }
}