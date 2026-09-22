using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public class RenterRepository : IRepository<Renter>
    {
        private readonly string _connectionString;

        public RenterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Renter> GetAll()
        {
            var renters = new List<Renter>();
            string query = "SELECT * FROM RENTER";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        renters.Add(new Renter
                        {
                            RenterId = (int)reader["RenterId"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"]
                        });
                    }
                }
            }

            return renters;
        }

        public Renter? GetById(int id)
        {
            Renter? renter = null;
            string query = "SELECT * FROM RENTER WHERE RenterId = @RenterId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renter = new Renter
                        {
                            RenterId = (int)reader["RenterId"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],
                            Phone = (string)reader["Phone"]
                        };
                    }
                }
            }

            return renter;
        }

        public void Add(Renter renter)
        {
            string query = "INSERT INTO RENTER (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", renter.Name);
                command.Parameters.AddWithValue("@Email", renter.Email);
                command.Parameters.AddWithValue("@Phone", renter.Phone);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(Renter renter)
        {
            string query = "UPDATE TENANT SET Name = @Name, Email = @Email, Phone = @Phone WHERE TenantId = @TenantId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", renter.Name);
                command.Parameters.AddWithValue("@Email", renter.Email);
                command.Parameters.AddWithValue("@Phone", renter.Phone);
                command.Parameters.AddWithValue("@RenterId", renter.RenterId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM RENTER WHERE RenterId = @RenterId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
