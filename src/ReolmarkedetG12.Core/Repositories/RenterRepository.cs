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
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Address = (string)reader["Address"],
                            PostalCode = (int)reader["PostalCode"],
                            City = (string)reader["City"],
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
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Address = (string)reader["Address"],
                            PostalCode = (int)reader["PostalCode"],
                            City = (string)reader["City"],
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
            string query = "INSERT INTO RENTER (FirstName, LastName, Address, PostalCode, City, Email, Phone) VALUES (@FirstName, @LastName, @Address, @PostalCode, @City, @Email, @Phone)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Address", renter.Address);
                command.Parameters.AddWithValue("@PostalCode", renter.PostalCode);
                command.Parameters.AddWithValue("@City", renter.City);
                command.Parameters.AddWithValue("@Email", renter.Email);
                command.Parameters.AddWithValue("@Phone", renter.Phone);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(Renter renter)
        {
            string query = "UPDATE RENTER SET FirstName = @FirstName, LastName = @LastName, Address = @Address, PostalCode = @PostalCode, City = @City, Email = @Email, Phone = @Phone WHERE RenterId = @RenterId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Address", renter.Address);
                command.Parameters.AddWithValue("@PostalCode", renter.PostalCode);
                command.Parameters.AddWithValue("@City", renter.City);
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
