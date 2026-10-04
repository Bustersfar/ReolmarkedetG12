using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories;

public class RentalPriceTierRepository : IRentalPriceTierRepository
{
    private readonly string _connectionString;

    public RentalPriceTierRepository(string connectionString)
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

    public IEnumerable<RentalPriceTier> GetAll()
    {
        var tiers = new List<RentalPriceTier>();
        const string query = "SELECT * FROM dbo.RENTAL_PRICE_TIER ORDER BY MinRacks ASC;";

        using (SqlConnection connection = OpenConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                int priceOrdinal = -1;

                while (reader.Read())
                {
                    if (priceOrdinal == -1)
                    {
                        try
                        {
                            priceOrdinal = reader.GetOrdinal("PricePerRack");
                        }
                        catch (IndexOutOfRangeException)
                        {
                            priceOrdinal = reader.GetOrdinal("MonthlyPrice");
                        }
                    }

                    tiers.Add(new RentalPriceTier
                    {
                        TierId = (int)reader["TierId"],
                        MinRacks = (int)reader["MinRacks"],
                        MaxRacks = reader["MaxRacks"] == DBNull.Value ? null : (int?)reader["MaxRacks"],
                        PricePerRack = Convert.ToDecimal(reader[priceOrdinal])
                    });
                }
            }
        }

        return tiers;
    }
}