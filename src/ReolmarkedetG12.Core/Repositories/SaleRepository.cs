using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public class SaleRepository : IRepository<Sale>
    {
        private readonly string _connectionString;

        public SaleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Standard IRepository: Tilføj et enkelt salg
        public void Add(Sale entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                INSERT INTO dbo.SALE (RackId, RenterId, Amount, Description, Date, PaymentMethod)
                VALUES (@RackId, @RenterId, @Amount, @Description, @Date, @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RackId", (object?)entity.RackId ?? DBNull.Value);
            command.Parameters.AddWithValue("@RenterId", (object?)entity.RenterId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", entity.Amount);
            command.Parameters.AddWithValue("@Description", entity.Description);
            command.Parameters.AddWithValue("@Date", entity.Date);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);

            entity.SaleId = (int)command.ExecuteScalar();
        }

        // Punkt 1: Transaktionsstyret oprettelse af en hel kurv (kassesalg)
        public void AddMany(IEnumerable<Sale> sales)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"
                    INSERT INTO dbo.SALE (RackId, RenterId, Amount, Description, Date, PaymentMethod)
                    VALUES (@RackId, @RenterId, @Amount, @Description, @Date, @PaymentMethod);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                foreach (var sale in sales)
                {
                    using var command = new SqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@RackId", (object?)sale.RackId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RenterId", (object?)sale.RenterId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Amount", sale.Amount);
                    command.Parameters.AddWithValue("@Description", sale.Description);
                    command.Parameters.AddWithValue("@Date", sale.Date);
                    command.Parameters.AddWithValue("@PaymentMethod", (int)sale.PaymentMethod);

                    sale.SaleId = (int)command.ExecuteScalar();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Standard IRepository: Hent et specifikt salg på ID
        public Sale? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "SELECT SaleId, RackId, RenterId, Amount, Description, Date, PaymentMethod FROM dbo.SALE WHERE SaleId = @SaleId;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SaleId", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapSale(reader);
            }

            return null;
        }

        // Standard IRepository: Hent alle salg
        public IEnumerable<Sale> GetAll()
        {
            var result = new List<Sale>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "SELECT SaleId, RackId, RenterId, Amount, Description, Date, PaymentMethod FROM dbo.SALE ORDER BY Date DESC;";
            using var command = new SqlCommand(sql, connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapSale(reader));
            }

            return result;
        }

        // Punkt 3: Målrettede forespørgsler med SQL WHERE-klausuler
        public IEnumerable<Sale> GetByDateRange(DateTime fromUtc, DateTime toUtc)
        {
            var result = new List<Sale>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT SaleId, RackId, RenterId, Amount, Description, Date, PaymentMethod 
                FROM dbo.SALE 
                WHERE Date >= @FromUtc AND Date <= @ToUtc
                ORDER BY Date DESC;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FromUtc", fromUtc);
            command.Parameters.AddWithValue("@ToUtc", toUtc);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapSale(reader));
            }

            return result;
        }

        public IEnumerable<Sale> GetByRackId(int rackId)
        {
            var result = new List<Sale>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT SaleId, RackId, RenterId, Amount, Description, Date, PaymentMethod 
                FROM dbo.SALE 
                WHERE RackId = @RackId
                ORDER BY Date DESC;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RackId", rackId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapSale(reader));
            }

            return result;
        }

        // Standard IRepository Update
        public void Update(Sale entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                UPDATE dbo.SALE 
                SET RackId = @RackId,
                    RenterId = @RenterId,
                    Amount = @Amount,
                    Description = @Description,
                    Date = @Date,
                    PaymentMethod = @PaymentMethod
                WHERE SaleId = @SaleId;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RackId", (object?)entity.RackId ?? DBNull.Value);
            command.Parameters.AddWithValue("@RenterId", (object?)entity.RenterId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", entity.Amount);
            command.Parameters.AddWithValue("@Description", entity.Description);
            command.Parameters.AddWithValue("@Date", entity.Date);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);
            command.Parameters.AddWithValue("@SaleId", entity.SaleId);

            command.ExecuteNonQuery();
        }

        // Punkt 17: Update med audit-log transaktion
        public void UpdateWithAudit(Sale updatedSale, Sale originalSale)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateSql = @"
                    UPDATE dbo.SALE 
                    SET RackId = @RackId,
                        RenterId = @RenterId,
                        Amount = @Amount,
                        Description = @Description,
                        Date = @Date,
                        PaymentMethod = @PaymentMethod
                    WHERE SaleId = @SaleId;";

                using var updateCommand = new SqlCommand(updateSql, connection, transaction);
                updateCommand.Parameters.AddWithValue("@RackId", (object?)updatedSale.RackId ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("@RenterId", (object?)updatedSale.RenterId ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("@Amount", updatedSale.Amount);
                updateCommand.Parameters.AddWithValue("@Description", updatedSale.Description);
                updateCommand.Parameters.AddWithValue("@Date", updatedSale.Date);
                updateCommand.Parameters.AddWithValue("@PaymentMethod", (int)updatedSale.PaymentMethod);
                updateCommand.Parameters.AddWithValue("@SaleId", updatedSale.SaleId);
                updateCommand.ExecuteNonQuery();

                const string auditSql = @"
                    INSERT INTO dbo.SALE_AUDIT_LOG (SaleId, ActionType, OldAmount, NewAmount, OldDescription, NewDescription, Timestamp)
                    VALUES (@SaleId, 'UPDATE', @OldAmount, @NewAmount, @OldDescription, @NewDescription, SYSUTCDATETIME());";

                using var auditCommand = new SqlCommand(auditSql, connection, transaction);
                auditCommand.Parameters.AddWithValue("@SaleId", updatedSale.SaleId);
                auditCommand.Parameters.AddWithValue("@OldAmount", originalSale.Amount);
                auditCommand.Parameters.AddWithValue("@NewAmount", updatedSale.Amount);
                auditCommand.Parameters.AddWithValue("@OldDescription", (object?)originalSale.Description ?? DBNull.Value);
                auditCommand.Parameters.AddWithValue("@NewDescription", (object?)updatedSale.Description ?? DBNull.Value);
                auditCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Standard IRepository Delete
        public void Delete(int id)
        {
            var sale = GetById(id);
            if (sale != null)
            {
                DeleteWithAudit(sale);
            }
        }

        // Punkt 17: Sletning med audit-log transaktion
        public void DeleteWithAudit(Sale saleToDelete)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string auditSql = @"
                    INSERT INTO dbo.SALE_AUDIT_LOG (SaleId, ActionType, OldAmount, NewAmount, OldDescription, NewDescription, Timestamp)
                    VALUES (@SaleId, 'DELETE', @OldAmount, NULL, @OldDescription, NULL, SYSUTCDATETIME());";

                using var auditCommand = new SqlCommand(auditSql, connection, transaction);
                auditCommand.Parameters.AddWithValue("@SaleId", saleToDelete.SaleId);
                auditCommand.Parameters.AddWithValue("@OldAmount", saleToDelete.Amount);
                auditCommand.Parameters.AddWithValue("@OldDescription", (object?)saleToDelete.Description ?? DBNull.Value);
                auditCommand.ExecuteNonQuery();

                const string deleteSql = "DELETE FROM dbo.SALE WHERE SaleId = @SaleId;";
                using var deleteCommand = new SqlCommand(deleteSql, connection, transaction);
                deleteCommand.Parameters.AddWithValue("@SaleId", saleToDelete.SaleId);
                deleteCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Hent ændringslog for et salg
        public IEnumerable<SaleAuditLog> GetAuditLogsForSale(int saleId)
        {
            var result = new List<SaleAuditLog>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT AuditId, SaleId, ActionType, OldAmount, NewAmount, OldDescription, NewDescription, Timestamp 
                FROM dbo.SALE_AUDIT_LOG 
                WHERE SaleId = @SaleId 
                ORDER BY Timestamp DESC;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SaleId", saleId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SaleAuditLog
                {
                    AuditId = reader.GetInt32(reader.GetOrdinal("AuditId")),
                    SaleId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                    ActionType = reader.GetString(reader.GetOrdinal("ActionType")),
                    OldAmount = reader.GetDecimal(reader.GetOrdinal("OldAmount")),
                    NewAmount = reader.IsDBNull(reader.GetOrdinal("NewAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("NewAmount")),
                    OldDescription = reader.IsDBNull(reader.GetOrdinal("OldDescription")) ? null : reader.GetString(reader.GetOrdinal("OldDescription")),
                    NewDescription = reader.IsDBNull(reader.GetOrdinal("NewDescription")) ? null : reader.GetString(reader.GetOrdinal("NewDescription")),
                    Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                });
            }

            return result;
        }

        private static Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                SaleId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                RackId = reader.IsDBNull(reader.GetOrdinal("RackId")) ? null : reader.GetInt32(reader.GetOrdinal("RackId")),
                RenterId = reader.IsDBNull(reader.GetOrdinal("RenterId")) ? null : reader.GetInt32(reader.GetOrdinal("RenterId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                PaymentMethod = (PaymentMethod)reader.GetInt32(reader.GetOrdinal("PaymentMethod"))
            };
        }
    }
}