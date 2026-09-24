using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using ReolmarkedetG12.Core.Exceptions;
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

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                connection.Dispose();
                throw new DatabaseConnectionException("Kunne ikke forbinde til databasen.", ex);
            }
        }

        public IEnumerable<Renter> GetAll()
        {
            var renters = new List<Renter>();
            string query = "SELECT * FROM RENTER";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

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
                            Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                            Phone = reader["Phone"] == DBNull.Value ? null : (string)reader["Phone"]
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

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);

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
                            Email = reader["Email"] == DBNull.Value ? null : (string)reader["Email"],
                            Phone = reader["Phone"] == DBNull.Value ? null : (string)reader["Phone"]
                        };
                    }
                }
            }

            return renter;
        }

        public void Add(Renter renter)
        {
            string query = "INSERT INTO RENTER (FirstName, LastName, Address, PostalCode, City, Email, Phone) VALUES (@FirstName, @LastName, @Address, @PostalCode, @City, @Email, @Phone)";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Address", renter.Address);
                command.Parameters.AddWithValue("@PostalCode", renter.PostalCode);
                command.Parameters.AddWithValue("@City", renter.City);
                command.Parameters.AddWithValue("@Email", (object?)renter.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object?)renter.Phone ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Renter renter)
        {
            string query = "UPDATE RENTER SET FirstName = @FirstName, LastName = @LastName, Address = @Address, PostalCode = @PostalCode, City = @City, Email = @Email, Phone = @Phone WHERE RenterId = @RenterId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", renter.FirstName);
                command.Parameters.AddWithValue("@LastName", renter.LastName);
                command.Parameters.AddWithValue("@Address", renter.Address);
                command.Parameters.AddWithValue("@PostalCode", renter.PostalCode);
                command.Parameters.AddWithValue("@City", renter.City);
                command.Parameters.AddWithValue("@Email", (object?)renter.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object?)renter.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@RenterId", renter.RenterId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM RENTER WHERE RenterId = @RenterId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", id);
                command.ExecuteNonQuery();
            }
        }
    }
}