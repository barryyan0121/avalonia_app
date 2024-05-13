using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Collections;
using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

public class DatabaseManager
{
    public DatabaseManager(string connectionString)
    {
        this._connectionString = connectionString;
        foreach (var line in ProductionLinesTotal)
        {
            RateMap[line] = 0.0;
        }
    }
    
    private readonly string _connectionString;
    public static readonly List<string> ProductionLinesA =
    [
        "胶纸切割",
        "板框焊接",
        "板组件A",
        "板组件B",
        "膜框组件A",
        "膜框组件B"
    ];

    public static readonly List<string> ProductionLinesB =
    [
        "三合一电池A",
        "三合一电池B",
        "三合一电池C",
        "三合一电池检测",
        "总装线",
        "框膜组件检测"
    ];

    public static readonly List<string> ProductionLinesTotal =
    [
        "胶纸切割",
        "板框焊接",
        "板组件A",
        "板组件B",
        "膜框组件A",
        "膜框组件B",
        "三合一电池A",
        "三合一电池B",
        "三合一电池C",
        "三合一电池检测",
        "总装线",
        "框膜组件检测"
    ];

    public readonly Dictionary<string, double> RateMap = new();

    public AvaloniaList<ProductionData> LoadData()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        const string query = "SELECT * FROM production_data WHERE DATE(date) = CURDATE();";
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
                QualifiedRate = Math.Round(reader.GetDouble("qualified_rate"), 5),
                TotalCount = reader.GetInt32("total_count"),
                Date = reader.GetDateTime("date")
            };
            RateMap[productionData.Name] = productionData.QualifiedRate;

            data.Add(productionData);
        }

        return data;
    }

    public Dictionary<string, List<List<double>>> LoadWeeklyData()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        // Create a list of all expected dates for the last 7 days
        var expectedDates = Enumerable.Range(-6, 7)
            .Select(offset => DateTime.Today.AddDays(offset).Date)
            .ToList();

        var weeklyData = new Dictionary<string, List<List<double>>>();
        var totalA = new List<List<double>>();
        var totalB = new List<List<double>>();
        var rateA = new List<List<double>>();
        var rateB = new List<List<double>>();

        for (var i = 0; i < 6; i++)
        {
            totalA.Add([]);
            totalB.Add([]);
            rateA.Add([]);
            rateB.Add([]);
        }

        // Initialize totalA with 0s for all expected dates
        foreach (var _ in expectedDates)
            for (var i = 0; i < 6; i++)
            {
                totalA[i].Add(0);
                totalB[i].Add(0);
                rateA[i].Add(0);
                rateB[i].Add(0);
            }

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");
        for (var i = 0; i < ProductionLinesA.Count; i++)
        {
            queryBuilder.Append($"MAX(IF(name = '{ProductionLinesA[i]}', total_count, 0)) AS name{i + 1}");
            if (i < ProductionLinesA.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }
        queryBuilder.Append(" FROM production_data");
        queryBuilder.Append(" WHERE Date >= CURDATE() - INTERVAL 6 DAY");
        queryBuilder.Append(" GROUP BY DATE(Date)");
        queryBuilder.Append(" ORDER BY DATE(Date);");

        var query = queryBuilder.ToString();
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                totalA[i][dataIndex] = reader.GetDouble($"name{i + 1}");
            }

        weeklyData.Add("totalA", totalA);

        reader.Close();

        queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");
        for (var i = 0; i < ProductionLinesA.Count; i++)
        {
            queryBuilder.Append($"MAX(IF(name = '{ProductionLinesA[i]}', qualified_rate, 0)) AS name{i + 1}");
            if (i < ProductionLinesA.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }
        queryBuilder.Append(" FROM production_data");
        queryBuilder.Append(" WHERE Date >= CURDATE() - INTERVAL 6 DAY");
        queryBuilder.Append(" GROUP BY DATE(Date)");
        queryBuilder.Append(" ORDER BY DATE(Date);");

        query = queryBuilder.ToString();
        // Repeat the above process for totalB, rateA, and rateB
        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                rateA[i][dataIndex] = Math.Round(reader.GetDouble($"name{i + 1}"), 2);
            }

        weeklyData.Add("rateA", rateA);

        reader.Close();
        
        queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");
        for (var i = 0; i < ProductionLinesB.Count; i++)
        {
            queryBuilder.Append($"MAX(IF(name = '{ProductionLinesB[i]}', total_count, 0)) AS name{i + 1}");
            if (i < ProductionLinesB.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }
        queryBuilder.Append(" FROM production_data");
        queryBuilder.Append(" WHERE Date >= CURDATE() - INTERVAL 6 DAY");
        queryBuilder.Append(" GROUP BY DATE(Date)");
        queryBuilder.Append(" ORDER BY DATE(Date);");

        query = queryBuilder.ToString();
        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                totalB[i][dataIndex] = reader.GetDouble($"name{i + 1}");
            }

        weeklyData.Add("totalB", totalB);

        reader.Close();
        
        queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");
        for (var i = 0; i < ProductionLinesB.Count; i++)
        {
            queryBuilder.Append($"MAX(IF(name = '{ProductionLinesB[i]}', qualified_rate, 0)) AS name{i + 1}");
            if (i < ProductionLinesB.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }
        queryBuilder.Append(" FROM production_data");
        queryBuilder.Append(" WHERE Date >= CURDATE() - INTERVAL 6 DAY");
        queryBuilder.Append(" GROUP BY DATE(Date)");
        queryBuilder.Append(" ORDER BY DATE(Date);");
        query = queryBuilder.ToString();
        
        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                rateB[i][dataIndex] = Math.Round(reader.GetDouble($"name{i + 1}"), 2);
            }

        weeklyData.Add("rateB", rateB);

        reader.Close();

        return weeklyData;
    }
}