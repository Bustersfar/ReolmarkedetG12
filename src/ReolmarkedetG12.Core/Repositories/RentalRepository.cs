using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReolmarkedetG12.Core.Repositories
{
    public class RentalRepository : IRepository<Rental>
    {
        private readonly string _connectionString;

        public RentalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Rental> GetAll()
        {
            var rentals = new List<Rental>();
            string query = "SELECT * FROM RENTAL";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new Rental
                        {
                            RentalId = (int)reader["RentalId"],
                            RackId = (int)reader["RackId"],
                            TenantId = (int)reader["TenantId"],
                            StartDate = (DateTime)reader["StartDate"],
                            EndDate = (DateTime?)reader["EndDate"]
                        });
                    }
                }
            }

            return rentals;
        }

        public Rental? GetById(int id)
        {
            Rental? rental = null;
            string query = "SELECT * FROM RENTAL WHERE RentalId = @RentalId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalId", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rental = new Rental
                        {
                            RentalId = (int)reader["RentalId"],
                            RackId = (int)reader["RackId"],
                            TenantId = (int)reader["TenantId"],
                            StartDate = (DateTime)reader["StartDate"],
                            EndDate = (DateTime?)reader["EndDate"]
                        };
                    }
                }
            }

            return rental;
        }

        public void Add(Rental rental)
        {
            string query = "INSERT INTO RENTAL (ShelfId, TenantId, StartDate, EndDate) VALUES (@ShelfId, @TenantId, @StartDate, @EndDate)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ShelfId", rental.RackId);
                command.Parameters.AddWithValue("@TenantId", rental.TenantId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate ?? (object)DBNull.Value);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(Rental rental)
        {
            string query = "UPDATE RENTAL SET ShelfId = @ShelfId, TenantId = @TenantId, StartDate = @StartDate, EndDate = @EndDate WHERE RentalId = @RentalId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ShelfId", rental.RackId);
                command.Parameters.AddWithValue("@TenantId", rental.TenantId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@RentalId", rental.RentalId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM RENTAL WHERE RentalId = @RentalId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalId", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}

