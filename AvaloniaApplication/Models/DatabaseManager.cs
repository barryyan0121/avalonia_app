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
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var query = "SELECT * FROM production_data WHERE DATE(date) = CURDATE() + 2;";
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();
        var data = new AvaloniaList<ProductionData>();

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
                Date = reader.GetDateTime("date")
            };

            data.Add(productionData);
        }

        return data;
    }

    public AvaloniaList<WeeklyProductionData> LoadWeeklyData()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var query =
            "SELECT name, MIN(date) AS start_date, " +
            "MAX(date) AS end_date," +
            "SUM(qualified_count) AS total_qualified_count, " +
            "SUM(defective_count) AS total_defective_count, " +
            "SUM(total_count) AS total_total_count " +
            "FROM production_data WHERE date >= CURDATE() - INTERVAL 6 DAY " +
            "AND date < CURDATE() + INTERVAL 6 DAY GROUP BY name;";
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();
        var data = new AvaloniaList<WeeklyProductionData>();

        while (reader.Read())
        {
            var productionData = new WeeklyProductionData
            {
                Name = reader.GetString("name"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date"),
                TotalQualifiedCount = reader.GetInt32("total_qualified_count"),
                TotalDefectiveCount = reader.GetInt32("total_defective_count"),
                TotalCount = reader.GetInt32("total_total_count"),
                TotalQualifiedRate = (double)reader.GetInt32("total_qualified_count") /
                                     reader.GetInt32("total_total_count")
            };

            data.Add(productionData);
        }

        return data;
    }
}