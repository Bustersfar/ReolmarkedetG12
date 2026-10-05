using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;


namespace ReolmarkedetG12.Core.Repositories
{
    public class SaleLineRepository : IRepository<SaleLine>
    {
        private readonly string _connectionString;
        public SaleLineRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public void Add(SaleLine entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = @"
                INSERT INTO dbo.SALELINE (SaleId, ItemId, SalePrice)
                VALUES (@SaleId, @ItemId, @SalePrice);";

            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@SaleId", entity.SaleId);
            command.Parameters.AddWithValue("@ItemId", entity.ItemId);
            command.Parameters.AddWithValue("@SalePrice", entity.SalePrice);

            command.ExecuteNonQuery();
        }

        public IEnumerable<SaleLine> GetAll()
        {
            List<SaleLine> saleLines = new List<SaleLine>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = "SELECT * FROM dbo.SALELINE";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                saleLines.Add(MapSaleLine(reader));
            }
            return saleLines;
        }

        public SaleLine? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = "SELECT * FROM dbo.SALELINE WHERE SaleLineId = @SaleLineId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineId", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapSaleLine(reader);
            }
            return null;
        }

        public void Update(SaleLine entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = @"
                UPDATE dbo.SALELINE 
                SET 
                    SaleId = @SaleId,
                    ItemId = @ItemId,
                    SalePrice = @SalePrice
                WHERE SaleLineId = @SaleLineId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineId", entity.SaleLineId);
            command.Parameters.AddWithValue("@SaleId", entity.SaleId);
            command.Parameters.AddWithValue("@ItemId", entity.ItemId);
            command.Parameters.AddWithValue("@SalePrice", entity.SalePrice);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM dbo.SALELINE WHERE SaleLineId = @SaleLineId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SaleLineId", id);
            command.ExecuteNonQuery();
        }

        private static SaleLine MapSaleLine(SqlDataReader reader)
        {
            return new SaleLine
            {
                SaleLineId = (int)reader["SaleLineId"],
                SaleId = (int)reader["SaleId"],
                ItemId = (int)reader["ItemId"],
                SalePrice = (decimal)reader["SalePrice"]
            };
        }

    }
}
