using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories;

public class RentalPriceTierRepository
{
    private readonly string _connectionString;

    public RentalPriceTierRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<RentalPriceTier> GetAll()
    {
        var tiers = new List<RentalPriceTier>();
        string query = "SELECT * FROM RENTAL_PRICE_TIER";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tiers.Add(new RentalPriceTier
                    {
                        TierId = (int)reader["TierId"],
                        MinRacks = (int)reader["MinRacks"],
                        MaxRacks = reader["MaxRacks"] as int?,
                        PricePerRack = (decimal)reader["PricePerRack"]
                    });
                }
            }
        }

        return tiers;
    }
}