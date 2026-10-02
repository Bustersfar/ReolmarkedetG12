using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
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

        public IEnumerable<Sale> GetAll()
        {
            var sales = new List<Sale>();
            string query = "SELECT * FROM SALE";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new Sale
                        {
                            SaleId = (int)reader["SaleId"],
                            RackId = (int)reader["RackId"],
                            RenterId = (int)reader["RenterId"],
                            Date = (DateTime)reader["Date"],
                            Amount = (decimal)reader["Amount"],
                            Description = reader["Description"] as string
                        });
                    }
                }
            }

            return sales;
        }

        public Sale? GetById(int id)
        {
            Sale? sale = null;
            string query = "SELECT * FROM SALE WHERE SaleId = @SaleId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleId", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sale = new Sale
                        {
                            SaleId = (int)reader["SaleId"],
                            RackId = (int)reader["RackId"],
                            RenterId = (int)reader["RenterId"],
                            Date = (DateTime)reader["Date"],
                            Amount = (decimal)reader["Amount"],
                            Description = reader["Description"] as string
                        };
                    }
                }
            }

            return sale;
        }

        public void Add(Sale sale)
        {
            string query = "INSERT INTO SALE (RackId, RenterId, Date, Amount, Description) VALUES (@RackId, @RenterId, @Date, @Amount, @Description)";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackId", sale.RackId);
                command.Parameters.AddWithValue("@RenterId", sale.RenterId);
                command.Parameters.AddWithValue("@Date", sale.Date);
                command.Parameters.AddWithValue("@Amount", sale.Amount);
                command.Parameters.AddWithValue("@Description", sale.Description ?? (object)DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Sale sale)
        {
            string query = "UPDATE SALE SET RackId = @RackId, RenterId = @RenterId, Date = @Date, Amount = @Amount, Description = @Description WHERE SaleId = @SaleId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RackId", sale.RackId);
                command.Parameters.AddWithValue("@RenterId", sale.RenterId);
                command.Parameters.AddWithValue("@Date", sale.Date);
                command.Parameters.AddWithValue("@Amount", sale.Amount);
                command.Parameters.AddWithValue("@Description", sale.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SaleId", sale.SaleId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM SALE WHERE SaleId = @SaleId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleId", id);
                command.ExecuteNonQuery();
            }
        }
    }
}