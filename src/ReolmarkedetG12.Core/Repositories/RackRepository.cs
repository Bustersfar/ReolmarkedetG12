using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;
namespace ReolmarkedetG12.Core.Repositories;

public class RackRepository : IRepository<Rack>
{
    private readonly string _connectionString;

    public RackRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Rack> GetAll()
    {
        var racks = new List<Rack>();
        string query = "SELECT * FROM RACK";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    racks.Add(new Rack
                    {
                        RackId = (int)reader["RackId"],
                        Number = (int)reader["Number"],
                        Status = (RackStatus)(int)reader["Status"]
                    });
                }
            }
        }

        return racks;
    }

    public Rack? GetById(int id)
    {
        Rack? rack = null;
        string query = "SELECT * FROM RACK WHERE RackId = @RackId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackId", id);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    rack = new Rack
                    {
                        RackId = (int)reader["RackId"],
                        Number = (int)reader["Number"],
                        Status = (RackStatus)(int)reader["Status"]
                    };
                }
            }
        }

        return rack;
    }

    public void Add(Rack rack)
    {
        string query = "INSERT INTO RACK (Number, Status) VALUES (@Number, @Status)";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Number", rack.Number);
            command.Parameters.AddWithValue("@Status", (int)rack.Status);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public void Update(Rack rack)
    {
        string query = "UPDATE RACK SET Number = @Number, Status = @Status WHERE RackId = @RackId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Number", rack.Number);
            command.Parameters.AddWithValue("@Status", (int)rack.Status);
            command.Parameters.AddWithValue("@RackId", rack.RackId);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public void Delete(int id)
    {
        string query = "DELETE FROM RACK WHERE RackId = @RackId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RackId", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}