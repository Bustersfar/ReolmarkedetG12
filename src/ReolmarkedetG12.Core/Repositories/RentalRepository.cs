using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public class RentalRepository : IRentalRepository
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
            const string query = "SELECT RentalId, RackId, RenterId, StartDate, EndDate, MonthlyRent FROM dbo.RENTAL;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rentals.Add(MapRental(reader));
                }
            }

            return rentals;
        }

        public Rental? GetById(int id)
        {
            Rental? rental = null;
            const string query = "SELECT RentalId, RackId, RenterId, StartDate, EndDate, MonthlyRent FROM dbo.RENTAL WHERE RentalId = @RentalId;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RentalId", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rental = MapRental(reader);
                    }
                }
            }

            return rental;
        }

        // Standard Add (IRepository<Rental>)
        public void Add(Rental rental)
        {
            const string query = @"
                INSERT INTO dbo.RENTAL (RackId, RenterId, StartDate, EndDate, MonthlyRent) 
                VALUES (@RackId, @RenterId, @StartDate, @EndDate, @MonthlyRent);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RackId", rental.RackId);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate.HasValue ? (object)rental.EndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@MonthlyRent", rental.MonthlyRent);

                rental.RentalId = (int)command.ExecuteScalar();
            }
        }

        // Punkt 1: Transaktionsstyret oprettelse af lejemål + opdatering af reol-status
        public void AddRentalWithRackStatus(Rental rental, int rackStatus = 1)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    const string insertRentalSql = @"
                        INSERT INTO dbo.RENTAL (RackId, RenterId, StartDate, EndDate, MonthlyRent) 
                        VALUES (@RackId, @RenterId, @StartDate, @EndDate, @MonthlyRent);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (var insertCommand = new SqlCommand(insertRentalSql, connection, transaction))
                    {
                        insertCommand.Parameters.AddWithValue("@RackId", rental.RackId);
                        insertCommand.Parameters.AddWithValue("@RenterId", rental.RenterId);
                        insertCommand.Parameters.AddWithValue("@StartDate", rental.StartDate);
                        insertCommand.Parameters.AddWithValue("@EndDate", rental.EndDate.HasValue ? (object)rental.EndDate.Value : DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@MonthlyRent", rental.MonthlyRent);

                        rental.RentalId = (int)insertCommand.ExecuteScalar();
                    }

                    const string updateRackSql = @"
                        UPDATE dbo.RACK 
                        SET Status = @Status 
                        WHERE RackId = @RackId;";

                    using (var updateCommand = new SqlCommand(updateRackSql, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Status", rackStatus);
                        updateCommand.Parameters.AddWithValue("@RackId", rental.RackId);
                        updateCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Punkt 3: Hent aktivt lejemål for en bestemt reol
        public Rental? GetActiveRentalByRackId(int rackId)
        {
            Rental? rental = null;
            const string query = @"
                SELECT TOP 1 RentalId, RackId, RenterId, StartDate, EndDate, MonthlyRent 
                FROM dbo.RENTAL 
                WHERE RackId = @RackId AND (EndDate IS NULL OR EndDate > SYSUTCDATETIME())
                ORDER BY StartDate DESC;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RackId", rackId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rental = MapRental(reader);
                    }
                }
            }

            return rental;
        }

        // Punkt 3: Hent alle lejemål tilhørende en bestemt lejer
        public IEnumerable<Rental> GetByRenterId(int renterId)
        {
            var rentals = new List<Rental>();
            const string query = @"
                SELECT RentalId, RackId, RenterId, StartDate, EndDate, MonthlyRent 
                FROM dbo.RENTAL 
                WHERE RenterId = @RenterId 
                ORDER BY StartDate DESC;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RenterId", renterId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(MapRental(reader));
                    }
                }
            }

            return rentals;
        }

        // Punkt 14: Historik for en specifik reol (afsluttede lejemål)
        public IEnumerable<Rental> GetCompletedRentalsByRackId(int rackId)
        {
            var rentals = new List<Rental>();
            const string query = @"
                SELECT RentalId, RackId, RenterId, StartDate, EndDate, MonthlyRent 
                FROM dbo.RENTAL 
                WHERE RackId = @RackId AND EndDate IS NOT NULL AND EndDate <= SYSUTCDATETIME()
                ORDER BY EndDate DESC;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RackId", rackId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(MapRental(reader));
                    }
                }
            }

            return rentals;
        }

        public void Update(Rental rental)
        {
            const string query = @"
                UPDATE dbo.RENTAL 
                SET RackId = @RackId,
                    RenterId = @RenterId,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    MonthlyRent = @MonthlyRent 
                WHERE RentalId = @RentalId;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RackId", rental.RackId);
                command.Parameters.AddWithValue("@RenterId", rental.RenterId);
                command.Parameters.AddWithValue("@StartDate", rental.StartDate);
                command.Parameters.AddWithValue("@EndDate", rental.EndDate.HasValue ? (object)rental.EndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@MonthlyRent", rental.MonthlyRent);
                command.Parameters.AddWithValue("@RentalId", rental.RentalId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            const string query = "DELETE FROM dbo.RENTAL WHERE RentalId = @RentalId;";

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RentalId", id);
                command.ExecuteNonQuery();
            }
        }

        private static Rental MapRental(SqlDataReader reader)
        {
            return new Rental
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