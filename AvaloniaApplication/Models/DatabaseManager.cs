using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Collections;
using LiveChartsCore.Defaults;
using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

public class DatabaseManager
{
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

    private readonly string _connectionString;

    public readonly Dictionary<string, ObservableValue> ProgressMap = new();

    public readonly Dictionary<string, KeyValuePair<ObservableValue, ObservableValue>> RateMap = new();

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
        foreach (var line in ProductionLinesTotal)
        {
            RateMap[line] =
                new KeyValuePair<ObservableValue, ObservableValue>(new ObservableValue(0), new ObservableValue(1));

            ProgressMap[line] = new ObservableValue(0);
        }
    }

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
            var qualifiedCount = reader.GetInt32("qualified_count");
            var defectiveCount = reader.GetInt32("defective_count");
            var totalCount = qualifiedCount + defectiveCount;
            var productionData = new ProductionData
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                QualifiedCount = qualifiedCount,
                DefectiveCount = defectiveCount,
                QualifiedRate = Math.Round((double)qualifiedCount / totalCount, 3),
                TotalCount = totalCount,
                TargetCount = reader.GetInt32("target_count"),
                Date = reader.GetDateTime("date")
            };

            RateMap[productionData.Name].Key.Value = productionData.QualifiedRate;
            RateMap[productionData.Name].Value.Value = Math.Round(1 - productionData.QualifiedRate, 3);
            ProgressMap[productionData.Name].Value =
                Math.Round((double)totalCount / productionData.TargetCount * 100, 3);

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
            var line = ProductionLinesA[i];
            queryBuilder.Append($@"
                MAX(IF(name = '{line}', qualified_count + defective_count, 0)) AS total_count{i + 1},
                MAX(IF(name = '{line}', 
                       CASE 
                           WHEN qualified_count + defective_count > 0 THEN qualified_count / (qualified_count + defective_count) 
                           ELSE 0 
                       END, 0)) AS qualified_rate{i + 1}");
            if (i < ProductionLinesA.Count - 1) queryBuilder.Append(',');
        }

        queryBuilder.Append(@"
             FROM production_data
             WHERE Date >= CURDATE() - INTERVAL 6 DAY
             GROUP BY DATE(Date)
             ORDER BY DATE(Date);");

        var query = queryBuilder.ToString();
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                totalA[i][dataIndex] = reader.GetDouble($"total_count{i + 1}");
                rateA[i][dataIndex] = Math.Round(reader.GetDouble($"qualified_rate{i + 1}"), 3);
            }

        weeklyData.Add("totalA", totalA);
        weeklyData.Add("rateA", rateA);

        reader.Close();

        queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");

        for (var i = 0; i < ProductionLinesB.Count; i++)
        {
            var line = ProductionLinesB[i];
            queryBuilder.Append($@"
                MAX(IF(name = '{line}', qualified_count + defective_count, 0)) AS total_count{i + 1},
                MAX(IF(name = '{line}', 
                       CASE 
                           WHEN qualified_count + defective_count > 0 THEN qualified_count / (qualified_count + defective_count) 
                           ELSE 0 
                       END, 0)) AS qualified_rate{i + 1}");
            if (i < ProductionLinesB.Count - 1) queryBuilder.Append(',');
        }

        queryBuilder.Append(@"
             FROM production_data
             WHERE Date >= CURDATE() - INTERVAL 6 DAY
             GROUP BY DATE(Date)
             ORDER BY DATE(Date);");

        query = queryBuilder.ToString();
        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                totalB[i][dataIndex] = reader.GetDouble($"total_count{i + 1}");
                rateB[i][dataIndex] = Math.Round(reader.GetDouble($"qualified_rate{i + 1}"), 3);
            }

        weeklyData.Add("totalB", totalB);
        weeklyData.Add("rateB", rateB);

        reader.Close();

        return weeklyData;
    }
}