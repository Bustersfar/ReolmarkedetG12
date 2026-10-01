using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
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

        public IEnumerable<Payment> GetAll()
        {
            var payments = new List<Payment>();
            string query = "SELECT * FROM PAYMENT";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = (int)reader["PaymentId"],
                            RenterId = (int)reader["RenterId"],
                            Date = (DateTime)reader["Date"],
                            Amount = (decimal)reader["Amount"],
                            Type = (PaymentType)(int)reader["Type"],
                            PaymentMethod = (PaymentMethod)(int)reader["PaymentMethod"]
                        });
                    }
                }
            }

            return payments;
        }

        public Payment? GetById(int id)
        {
            Payment? payment = null;
            string query = "SELECT * FROM PAYMENT WHERE PaymentId = @PaymentId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentId", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        payment = new Payment
                        {
                            PaymentId = (int)reader["PaymentId"],
                            RenterId = (int)reader["RenterId"],
                            Date = (DateTime)reader["Date"],
                            Amount = (decimal)reader["Amount"],
                            Type = (PaymentType)(int)reader["Type"],
                            PaymentMethod = (PaymentMethod)(int)reader["PaymentMethod"]
                        };
                    }
                }
            }

            return payment;
        }

        public void Add(Payment payment)
        {
            string query = "INSERT INTO PAYMENT (RenterId, Date, Amount, Type, PaymentMethod) VALUES (@RenterId, @Date, @Amount, @Type, @PaymentMethod)";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", payment.RenterId);
                command.Parameters.AddWithValue("@Date", payment.Date);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                command.Parameters.AddWithValue("@Type", (int)payment.Type);
                command.Parameters.AddWithValue("@PaymentMethod", (int)payment.PaymentMethod);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Payment payment)
        {
            string query = "UPDATE PAYMENT SET RenterId = @RenterId, Date = @Date, Amount = @Amount, Type = @Type, PaymentMethod = @PaymentMethod WHERE PaymentId = @PaymentId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RenterId", payment.RenterId);
                command.Parameters.AddWithValue("@Date", payment.Date);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                command.Parameters.AddWithValue("@Type", (int)payment.Type);
                command.Parameters.AddWithValue("@PaymentMethod", (int)payment.PaymentMethod);
                command.Parameters.AddWithValue("@PaymentId", payment.PaymentId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = "DELETE FROM PAYMENT WHERE PaymentId = @PaymentId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentId", id);
                command.ExecuteNonQuery();
            }
        }
    }
}
