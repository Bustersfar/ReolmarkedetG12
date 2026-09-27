using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakePaymentRepository : IRepository<Payment>
{
    public List<Payment> Payments { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;

    private void CheckDatabase()
    {
        if (SimulateDatabaseDown)
            throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", new Exception("Testfejl"));
    }

    public void Add(Payment payment)
    {
        CheckDatabase();
        payment.PaymentId = _nextId++;
        Payments.Add(payment);
    }

    public Payment? GetById(int id)
    {
        CheckDatabase();
        return Payments.FirstOrDefault(p => p.PaymentId == id);
    }

    public void Update(Payment payment)
    {
        CheckDatabase();
        var index = Payments.FindIndex(p => p.PaymentId == payment.PaymentId);
        if (index >= 0) Payments[index] = payment;
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Payments.RemoveAll(p => p.PaymentId == id);
    }

    public IEnumerable<Payment> GetAll()
    {
        CheckDatabase();
        return Payments.ToList();
    }
}
