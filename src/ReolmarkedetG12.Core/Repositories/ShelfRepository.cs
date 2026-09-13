using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Models;
namespace ReolmarkedetG12.Core.Repositories;

public class ShelfRepository : IRepository<Shelf>
{
    private readonly string _connectionString;

    public ShelfRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Shelf> GetAll()
    {
        var shelves = new List<Shelf>();
        string query = "SELECT * FROM SHELF";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    shelves.Add(new Shelf
                    {
                        ShelfId = (int)reader["ShelfId"],
                        Number = (int)reader["Number"]
                    });
                }
            }
        }

        return shelves;
    }

    public Shelf? GetById(int id)
    {
        Shelf? shelf = null;
        string query = "SELECT * FROM SHELF WHERE ShelfId = @ShelfId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShelfId", id);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    shelf = new Shelf
                    {
                        ShelfId = (int)reader["ShelfId"],
                        Number = (int)reader["Number"]
                    };
                }
            }
        }

        return shelf;
    }

    public void Add(Shelf shelf)
    {
        string query = "INSERT INTO SHELF (Number) VALUES (@Number)";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Number", shelf.Number);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public void Update(Shelf shelf)
    {
        string query = "UPDATE SHELF SET Number = @Number WHERE ShelfId = @ShelfId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Number", shelf.Number);
            command.Parameters.AddWithValue("@ShelfId", shelf.ShelfId);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public void Delete(int id)
    {
        string query = "DELETE FROM SHELF WHERE ShelfId = @ShelfId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShelfId", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
