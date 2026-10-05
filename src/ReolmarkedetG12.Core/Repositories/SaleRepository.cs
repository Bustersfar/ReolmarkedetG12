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
                INSERT INTO dbo.SALE (SaleDate,TotalAmount, PaymentMethod)
                VALUES (@SaleDate,@TotalAmount, @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SaleDate", entity.SaleDate);
            command.Parameters.AddWithValue("@TotalAmount", entity.TotalAmount);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);

            entity.SaleId = (int)command.ExecuteScalar();
        }


       

        // Standard IRepository: Hent et specifikt salg på ID
        public Sale? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = "SELECT * FROM dbo.SALE WHERE SaleId = @SaleId";

            using var command = new SqlCommand(query, connection);

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

            const string query = "SELECT * FROM dbo.SALE";

            using var command = new SqlCommand(query, connection);
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

            const string query = @"
                UPDATE dbo.SALE 
                SET 
                    SaleDate = @SaleDate,
                    TotalAmount = @TotalAmount,
                    PaymentMethod = @PaymentMethod
                WHERE SaleId = @SaleId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleId", entity.SaleId);
            command.Parameters.AddWithValue("@SaleDate", entity.SaleDate);
            command.Parameters.AddWithValue("@TotalAmount", entity.TotalAmount);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);

            command.ExecuteNonQuery();
        }


        // Standard IRepository Delete
        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM dbo.SALE WHERE SaleId = @SaleId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleId", id);
            command.ExecuteNonQuery();
        }


        private static Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                SaleId = reader.GetInt32(reader.GetOrdinal("SaleId")),
                SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                PaymentMethod = (PaymentMethod)reader.GetInt32(reader.GetOrdinal("PaymentMethod"))
            };
        }
    }
}