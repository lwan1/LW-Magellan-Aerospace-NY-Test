using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Data;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Port=5432;Database=Part;Username=dbuser;Password=pass;";

        [HttpPost]
        [Route("create")]
        public IActionResult CreateItem([FromBody] ItemRequest itemRequest)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@itemName, @parentItem, @cost, @reqDate) RETURNING id;";
                    command.Parameters.AddWithValue("@itemName", itemRequest.ItemName);
                    command.Parameters.AddWithValue("@parentItem", itemRequest.ParentItem);
                    command.Parameters.AddWithValue("@cost", itemRequest.Cost);
                    command.Parameters.AddWithValue("@reqDate", itemRequest.ReqDate);

                    int newId = Convert.ToInt32(command.ExecuteScalar());

                    return Ok(new { id = newId });
                }
            }
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetItem(int id)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT id, item_name, parent_item, cost, req_date FROM item WHERE id = @id;";
                    command.Parameters.AddWithValue("@id", id);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Ok(new
                            {
                                id = reader.GetInt32(0),
                                item_name = reader.GetString(1),
                                parent_item = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                cost = reader.GetInt32(3),
                                req_date = reader.GetDateTime(4)
                            });
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }

        [HttpGet]
        [Route("getTotalCost/{itemName}")]
        public IActionResult GetTotalCost(string itemName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Get_Total_Cost(@itemName);";
                    command.Parameters.AddWithValue("@itemName", itemName);

                    int? totalCost = command.ExecuteScalar() as int?;

                    return Ok(new { totalCost });
                }
            }
        }
    }

    public class ItemRequest
    {
        public string ItemName { get; set; }
        public int? ParentItem { get; set; }
        public int Cost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
