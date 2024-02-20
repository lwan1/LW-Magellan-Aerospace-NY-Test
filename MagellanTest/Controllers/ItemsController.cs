using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        // Postgres connection string
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=Part;Username=user;Password=password";

        // Endpoint for creating a new entry in item table
        // returns id
        [HttpPost]
        public IActionResult CreateItem([FromBody] ItemRequest itemRequest)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@ItemName, @ParentItem, @Cost, @ReqDate) RETURNING id", connection))
                {
                    command.Parameters.AddWithValue("@ItemName", itemRequest.ItemName);
                    command.Parameters.AddWithValue("@ParentItem", itemRequest.ParentItem);
                    command.Parameters.AddWithValue("@Cost", itemRequest.Cost);
                    command.Parameters.AddWithValue("@ReqDate", itemRequest.ReqDate);

                    var id = command.ExecuteScalar();
                    return Ok(new { Id = id });
                }
            }
        }

        // Endpoint to query the item table based on id supplied
        // returns id, item_name, parent_item, cost, req_date
        [HttpGet("{id}")]
        public IActionResult GetItem(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT * FROM item WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = new
                            {
                                Id = reader.GetInt32(0),
                                ItemName = reader.GetString(1),
                                ParentItem = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                Cost = reader.GetInt32(3),
                                ReqDate = reader.GetDateTime(4)
                            };

                            return Ok(result);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
        }

        // Endpoint that calls Get_Total_Cost function
        [HttpGet("totalcost")]
        public IActionResult GetTotalCost([FromQuery] string itemName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand($"SELECT Get_Total_Cost('{itemName}')", connection))
                {
                    var result = command.ExecuteScalar();
                    return Ok(result);
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
