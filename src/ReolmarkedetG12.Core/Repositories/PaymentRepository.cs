using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public class PaymentRepository : IRepository<Payment>
    {
        private readonly string _connectionString;

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(Payment entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                INSERT INTO dbo.PAYMENT (RenterId, Date, Amount, Type, PaymentMethod)
                VALUES (@RenterId, @Date, @Amount, @Type, @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RenterId", entity.RenterId);
            command.Parameters.AddWithValue("@Date", entity.Date);
            command.Parameters.AddWithValue("@Amount", entity.Amount);
            command.Parameters.AddWithValue("@Type", (int)entity.Type);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);

            entity.PaymentId = (int)command.ExecuteScalar();
        }

        public Payment? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT PaymentId, RenterId, Date, Amount, Type, PaymentMethod 
                FROM dbo.PAYMENT 
                WHERE PaymentId = @PaymentId;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PaymentId", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapPayment(reader);
            }

            return null;
        }

        public IEnumerable<Payment> GetAll()
        {
            var result = new List<Payment>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT PaymentId, RenterId, Date, Amount, Type, PaymentMethod 
                FROM dbo.PAYMENT 
                ORDER BY Date DESC;";

            using var command = new SqlCommand(sql, connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapPayment(reader));
            }

            return result;
        }

        public IEnumerable<Payment> GetByRenterId(int renterId)
        {
            var result = new List<Payment>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT PaymentId, RenterId, Date, Amount, Type, PaymentMethod 
                FROM dbo.PAYMENT 
                WHERE RenterId = @RenterId 
                ORDER BY Date DESC;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RenterId", renterId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapPayment(reader));
            }

            return result;
        }

        public void Update(Payment entity)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                UPDATE dbo.PAYMENT 
                SET RenterId = @RenterId,
                    Date = @Date,
                    Amount = @Amount,
                    Type = @Type,
                    PaymentMethod = @PaymentMethod
                WHERE PaymentId = @PaymentId;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RenterId", entity.RenterId);
            command.Parameters.AddWithValue("@Date", entity.Date);
            command.Parameters.AddWithValue("@Amount", entity.Amount);
            command.Parameters.AddWithValue("@Type", (int)entity.Type);
            command.Parameters.AddWithValue("@PaymentMethod", (int)entity.PaymentMethod);
            command.Parameters.AddWithValue("@PaymentId", entity.PaymentId);

            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "DELETE FROM dbo.PAYMENT WHERE PaymentId = @PaymentId;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PaymentId", id);

            command.ExecuteNonQuery();
        }

        private static Payment MapPayment(SqlDataReader reader)
        {
            return new Payment
            {
                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                RenterId = reader.GetInt32(reader.GetOrdinal("RenterId")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Type = (PaymentType)reader.GetInt32(reader.GetOrdinal("Type")),
                PaymentMethod = (PaymentMethod)reader.GetInt32(reader.GetOrdinal("PaymentMethod"))
            };
        }
    }
}