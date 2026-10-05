using Microsoft.Data.SqlClient;
using ReolmarkedetG12.Core.Exceptions;
using ReolmarkedetG12.Core.Models;
using ReolmarkedetG12.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReolmarkedetG12.Core.Repositories
{
    public class ItemRepository : IItemRepository
    {
       private readonly string _connectionString;
        public ItemRepository(string connectionString)
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

        public IEnumerable<Item> GetAll()
        {
            var items = new List<Item>();
            string query = "SELECT * FROM ITEM";
            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new Item
                        {
                            ItemId = (int)reader["ItemId"],
                            ItemNumber = (int)reader["ItemNumber"],
                            Name = (string)reader["Name"],
                            Price = (decimal)reader["Price"],
                            RackId = (int)reader["RackId"]
                        });
                    }
                }
            } 
            return items;
        }

        public Item? GetById(int itemId)
        {
            Item? item = null;
            string query = "SELECT * FROM ITEM WHERE ItemId = @ItemId";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemId", itemId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item = new Item
                        {
                            ItemId = (int)reader["ItemId"],
                            ItemNumber = (int)reader["ItemNumber"],
                            Name = (string)reader["Name"],
                            Price = (decimal)reader["Price"],
                            RackId = (int)reader["RackId"]
                        };
                    }
                }
            }
            return item;
        }

        public void Add(Item item)
        {
            string query = "INSERT INTO ITEM (ItemNumber, Name, Price, RackId) VALUES (@ItemNumber, @Name, @Price, @RackId)";

            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemNumber", item.ItemNumber);
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@RackId", item.RackId);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Item item)
        {
            string query = "UPDATE ITEM SET ItemNumber = @ItemNumber, Name = @Name, Price = @Price, RackId = @RackId WHERE ItemId = @ItemId";
            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemId", item.ItemId);
                command.Parameters.AddWithValue("@ItemNumber", item.ItemNumber);
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@RackId", item.RackId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int itemId)
        {
            string query = "DELETE FROM ITEM WHERE ItemId = @ItemId";
            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemId", itemId);
                command.ExecuteNonQuery();
            }
        }

        public Item? GetByItemNumber(int itemNumber)
        {
            Item? item = null;
            string query = "SELECT * FROM ITEM WHERE ItemNumber = @ItemNumber";
            using (SqlConnection connection = OpenConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemNumber", itemNumber);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item = new Item
                        {
                            ItemId = (int)reader["ItemId"],
                            ItemNumber = (int)reader["ItemNumber"],
                            Name = (string)reader["Name"],
                            Price = (decimal)reader["Price"],
                            RackId = (int)reader["RackId"]
                        };
                    }
                }
            }
            return item;
        }

    }
}
