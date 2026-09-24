using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public class RentalRepository : IRepository<Rental>
    {
        private readonly string _connectionString;

        public RentalRepository(string connectionString)
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

        public IEnumerable<Rental> GetAll()
        {
            var rentals = new List<Rental>();
            string query = "SELECT * FROM RENTAL";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new Rental
                        {
                            RentalId = (int)reader["RentalId"],
                            RackId = (int)reader["RackId"],
                            RenterId = (int)reader["RenterId"],
                            StartDate = (DateTime)reader["StartDate"],
                            EndDate = reader["EndDate"] == DBNull.Value ? null : (DateTime?)reader["EndDate"],
                            MonthlyRent = (decimal)reader["MonthlyRent"]
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

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalId", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rental = new Rental
                        {
                            RentalId = (int)reader["RentalId"],
                            RackId = (int)reader["RackId"],
                            RenterId = (int)reader["RenterId"],
                            StartDate = (DateTime)reader["StartDate"],
                            EndDate = reader["EndDate"] == DBNull.Value ? null : (DateTime?)reader["EndDate"],
                            MonthlyRent = (decimal)reader["MonthlyRent"]
                        };
                    }
                }
            }

            return rental;
        }

        public void Add(Rental rental)
        {
            string query = "INSERT INTO RENTAL (RackId, RenterId, StartDate, EndDate, MonthlyRent) VALUES (@RackId, @RenterId, @StartDate, @EndDate, @MonthlyRent)";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackId", rental.RackId);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MonthlyRent", rental.MonthlyRent);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Rental rental)
        {
            string query = "UPDATE RENTAL SET RackId = @RackId, RenterId = @RenterId, StartDate = @StartDate, EndDate = @EndDate, MonthlyRent = @MonthlyRent WHERE RentalId = @RentalId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackId", rental.RackId);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MonthlyRent", rental.MonthlyRent);
                command.Parameters.AddWithValue("@RentalId", rental.RentalId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM RENTAL WHERE RentalId = @RentalId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RentalId", id);
                command.ExecuteNonQuery();
            }
        }
    }
}