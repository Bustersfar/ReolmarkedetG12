using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;

namespace ReolmarkedetG12.Tests;

[TestClass]
[DoNotParallelize]
public class SaleRepositoryTests
{
    private const string TestConnectionString = TestDatabase.ConnectionString;

    private static Renter CreateTestRenter()
    {
        var renterRepo = new RenterRepository(TestConnectionString);
        var uniqueName = "SaleTest_" + Guid.NewGuid().ToString("N")[..8];
        var renter = new Renter
        {
            FirstName = uniqueName,
            LastName = "Person",
            Address = "Testgade 12",
            PostalCode = 4200,
            City = "Slagelse"
        };
        renterRepo.Add(renter);
        return renterRepo.GetAll().First(r => r.FirstName == uniqueName);
    }

    private static int GetExistingRackId()
    {
        var rackRepo = new RackRepository(TestConnectionString);
        return rackRepo.GetAll().First(r => r.Number >= 1 && r.Number <= 80).RackId;
    }

    private static void CleanupSalesAndRenter(int renterId)
    {
        using var connection = new SqlConnection(TestConnectionString);
        connection.Open();

        const string cleanupSql = @"
            IF OBJECT_ID('dbo.SALE_AUDIT_LOG', 'U') IS NOT NULL
            BEGIN
                DELETE al 
                FROM dbo.SALE_AUDIT_LOG al
                INNER JOIN dbo.SALE s ON al.SaleId = s.SaleId
                WHERE s.RenterId = @RenterId;
            END

            DELETE FROM dbo.SALE WHERE RenterId = @RenterId;
            DELETE FROM dbo.RENTER WHERE RenterId = @RenterId;";

        using var command = new SqlCommand(cleanupSql, connection);
        command.Parameters.AddWithValue("@RenterId", renterId);
        command.ExecuteNonQuery();
    }

    [TestMethod]
    [TestCategory("Database")]
    public void AddMany_MultipleSales_InsertsAllWithinTransaction()
    {
        // Arrange
        var renter = CreateTestRenter();
        int rackId = GetExistingRackId();
        var repo = new SaleRepository(TestConnectionString);

        var sales = new List<Sale>
        {
            new Sale(rackId, renter.RenterId, 150m, "Vare 1", PaymentMethod.Cash, DateTime.UtcNow),
            new Sale(rackId, renter.RenterId, 250m, "Vare 2", PaymentMethod.MobilePay, DateTime.UtcNow)
        };

        try
        {
            // Act
            repo.AddMany(sales);

            var retrieved = repo.GetAll().Where(s => s.RenterId == renter.RenterId).ToList();

            // Assert
            Assert.AreEqual(2, retrieved.Count);
            Assert.IsTrue(retrieved.Any(s => s.Amount == 150m && s.Description == "Vare 1"));
            Assert.IsTrue(retrieved.Any(s => s.Amount == 250m && s.Description == "Vare 2"));
        }
        finally
        {
            CleanupSalesAndRenter(renter.RenterId);
        }
    }

    [TestMethod]
    [TestCategory("Database")]
    public void UpdateWithAudit_ExistingSale_UpdatesSaleAndCreatesAuditEntry()
    {
        // Arrange
        var renter = CreateTestRenter();
        int rackId = GetExistingRackId();
        var repo = new SaleRepository(TestConnectionString);

        var sale = new Sale(rackId, renter.RenterId, 100m, "Original vare", PaymentMethod.Cash, DateTime.UtcNow);
        repo.Add(sale);

        var originalSale = new Sale
        {
            SaleId = sale.SaleId,
            RackId = sale.RackId,
            RenterId = sale.RenterId,
            Amount = sale.Amount,
            Description = sale.Description,
            Date = sale.Date,
            PaymentMethod = sale.PaymentMethod
        };

        sale.Amount = 175m;
        sale.Description = "Rettet vare";

        try
        {
            // Act
            repo.UpdateWithAudit(sale, originalSale);

            var updatedSale = repo.GetById(sale.SaleId);
            var auditLogs = repo.GetAuditLogsForSale(sale.SaleId).ToList();

            // Assert
            Assert.IsNotNull(updatedSale);
            Assert.AreEqual(175m, updatedSale.Amount);
            Assert.AreEqual("Rettet vare", updatedSale.Description);

            Assert.AreEqual(1, auditLogs.Count);
            Assert.AreEqual("UPDATE", auditLogs[0].ActionType);
            Assert.AreEqual(100m, auditLogs[0].OldAmount);
            Assert.AreEqual(175m, auditLogs[0].NewAmount);
            Assert.AreEqual("Original vare", auditLogs[0].OldDescription);
            Assert.AreEqual("Rettet vare", auditLogs[0].NewDescription);
        }
        finally
        {
            CleanupSalesAndRenter(renter.RenterId);
        }
    }

    [TestMethod]
    [TestCategory("Database")]
    public void DeleteWithAudit_ExistingSale_RemovesSaleAndLogsDelete()
    {
        // Arrange
        var renter = CreateTestRenter();
        int rackId = GetExistingRackId();
        var repo = new SaleRepository(TestConnectionString);

        var sale = new Sale(rackId, renter.RenterId, 220m, "Skal slettes", PaymentMethod.Bank, DateTime.UtcNow);
        repo.Add(sale);
        int saleId = sale.SaleId;

        try
        {
            // Act
            repo.DeleteWithAudit(sale);

            var deletedSale = repo.GetById(saleId);
            var auditLogs = repo.GetAuditLogsForSale(saleId).ToList();

            // Assert
            Assert.IsNull(deletedSale);
            Assert.AreEqual(1, auditLogs.Count);
            Assert.AreEqual("DELETE", auditLogs[0].ActionType);
            Assert.AreEqual(220m, auditLogs[0].OldAmount);
            Assert.IsNull(auditLogs[0].NewAmount);
            Assert.AreEqual("Skal slettes", auditLogs[0].OldDescription);
            Assert.IsNull(auditLogs[0].NewDescription);
        }
        finally
        {
            using var connection = new SqlConnection(TestConnectionString);
            connection.Open();
            using var cmd = new SqlCommand("IF OBJECT_ID('dbo.SALE_AUDIT_LOG', 'U') IS NOT NULL DELETE FROM dbo.SALE_AUDIT_LOG WHERE SaleId = @SaleId;", connection);
            cmd.Parameters.AddWithValue("@SaleId", saleId);
            cmd.ExecuteNonQuery();

            CleanupSalesAndRenter(renter.RenterId);
        }
    }
}