using Avalonia.Collections;
using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public AvaloniaList<ProductionData> LoadData()
    {
        var tableData = new AvaloniaList<ProductionData>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var query = "SELECT * FROM production_data";
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var productionData = new ProductionData
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                QualifiedCount = reader.GetInt32("qualified_count"),
                DefectiveCount = reader.GetInt32("defective_count"),
                QualifiedRate = reader.GetDouble("qualified_rate"),
                TotalCount = reader.GetInt32("total_count"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date")
            };

            tableData.Add(productionData);
        }

        return tableData;
    }
}