using System;
using System.Collections.Generic;
using System.Linq;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeSaleRepository : IRepository<Sale>
{
    public List<Sale> Sales { get; } = new();
    public List<SaleAuditLog> AuditLogs { get; } = new();
    public bool SimulateDatabaseDown { get; set; }
    private int _nextId = 1;
    private int _nextAuditId = 1;

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

    public void AddMany(IEnumerable<Sale> sales)
    {
        CheckDatabase();
        foreach (var sale in sales)
        {
            Add(sale);
        }
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

    public void UpdateWithAudit(Sale updatedSale, Sale originalSale)
    {
        CheckDatabase();
        Update(updatedSale);
        AuditLogs.Add(new SaleAuditLog(
            updatedSale.SaleId,
            "UPDATE",
            originalSale.Amount,
            updatedSale.Amount,
            originalSale.Description,
            updatedSale.Description,
            DateTime.UtcNow)
        {
            AuditId = _nextAuditId++
        });
    }

    public void Delete(int id)
    {
        CheckDatabase();
        Sales.RemoveAll(s => s.SaleId == id);
    }

    public void DeleteWithAudit(Sale saleToDelete)
    {
        CheckDatabase();
        Sales.RemoveAll(s => s.SaleId == saleToDelete.SaleId);
        AuditLogs.Add(new SaleAuditLog(
            saleToDelete.SaleId,
            "DELETE",
            saleToDelete.Amount,
            null,
            saleToDelete.Description,
            null,
            DateTime.UtcNow)
        {
            AuditId = _nextAuditId++
        });
    }

    public IEnumerable<Sale> GetAll()
    {
        CheckDatabase();
        return Sales.ToList();
    }

    public IEnumerable<Sale> GetByDateRange(DateTime fromUtc, DateTime toUtc)
    {
        CheckDatabase();
        return Sales.Where(s => s.Date >= fromUtc && s.Date <= toUtc).OrderByDescending(s => s.Date).ToList();
    }

    public IEnumerable<Sale> GetByRackId(int rackId)
    {
        CheckDatabase();
        return Sales.Where(s => s.RackId == rackId).OrderByDescending(s => s.Date).ToList();
    }

    public IEnumerable<SaleAuditLog> GetAuditLogsForSale(int saleId)
    {
        CheckDatabase();
        return AuditLogs.Where(l => l.SaleId == saleId).OrderByDescending(l => l.Timestamp).ToList();
    }
}