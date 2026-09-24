using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeRentalRepository : IRepository<Rental>
{
    public List<Rental> Rentals { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;

    private void CheckDatabase()
    {
        if (SimulateDatabaseDown)
            throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", new Exception("Testfejl"));
    }

    public void Add(Rental rental)
    {
        CheckDatabase();
        rental.RentalId = _nextId++;
        Rentals.Add(rental);
    }

    public Rental? GetById(int id)
    {
        CheckDatabase();
        return Rentals.FirstOrDefault(r => r.RentalId == id);
    }

    public void Update(Rental rental)
    {
        CheckDatabase();
        var index = Rentals.FindIndex(r => r.RentalId == rental.RentalId);
        if (index >= 0) Rentals[index] = rental;
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Rentals.RemoveAll(r => r.RentalId == id);
    }

    public IEnumerable<Rental> GetAll()
    {
        CheckDatabase();
        return Rentals.ToList();
    }
}