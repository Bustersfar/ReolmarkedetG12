using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeRackRepository : IRepository<Rack>
{
    public List<Rack> Racks { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;

    private void CheckDatabase()
    {
        if (SimulateDatabaseDown)
            throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", new Exception("Testfejl"));
    }

    public void Add(Rack rack)
    {
        CheckDatabase();
        rack.RackId = _nextId++;
        Racks.Add(rack);
    }

    public Rack? GetById(int id)
    {
        CheckDatabase();
        return Racks.FirstOrDefault(r => r.RackId == id);
    }

    public void Update(Rack rack)
    {
        CheckDatabase();
        var index = Racks.FindIndex(r => r.RackId == rack.RackId);
        if (index >= 0) Racks[index] = rack;
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Racks.RemoveAll(r => r.RackId == id);
    }

    public IEnumerable<Rack> GetAll()
    {
        CheckDatabase();
        return Racks.ToList();
    }
}