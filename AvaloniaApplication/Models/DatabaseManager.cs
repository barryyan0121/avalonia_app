using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

public class DatabaseManager(string connectionString)
{
    public AvaloniaList<ProductionData> LoadData()
    {
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        var query = "SELECT * FROM production_data WHERE DATE(date) = CURDATE();";
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

    public Dictionary<string, List<List<double>>> LoadWeeklyData()
    {
        using var connection = new MySqlConnection(connectionString);
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


        var query = @"SELECT DATE(Date) AS Date,
                           MAX(IF(name = '胶纸切割', total_count, 0)) AS name1,
                           MAX(IF(name = '板框焊接', total_count, 0)) AS name2,
                           MAX(IF(name = '板组件A', total_count, 0)) AS name3,
                           MAX(IF(name = '板组件B', total_count, 0)) AS name4,
                           MAX(IF(name = '膜框组件A', total_count, 0)) AS name5,
                           MAX(IF(name = '膜框组件B', total_count, 0)) AS name6
                        FROM production_data
                        WHERE Date >= CURDATE() - INTERVAL 6 DAY
                        GROUP BY DATE(Date)
                        ORDER BY DATE(Date);";

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

        // Repeat the above process for totalB, rateA, and rateB
        query = @"SELECT DATE(Date) AS Date,
                       MAX(IF(name = '胶纸切割', qualified_rate, 0)) AS name1,
                       MAX(IF(name = '板框焊接', qualified_rate, 0)) AS name2,
                       MAX(IF(name = '板组件A', qualified_rate, 0)) AS name3,
                       MAX(IF(name = '板组件B', qualified_rate, 0)) AS name4,
                       MAX(IF(name = '膜框组件A', qualified_rate, 0)) AS name5,
                       MAX(IF(name = '膜框组件B', qualified_rate, 0)) AS name6
                    FROM production_data
                    WHERE Date >= CURDATE() - INTERVAL 6 DAY
                    GROUP BY DATE(Date)
                    ORDER BY DATE(Date);";

        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                rateA[i][dataIndex] = reader.GetDouble($"name{i + 1}");
            }

        weeklyData.Add("rateA", rateA);

        reader.Close();

        query = @"SELECT DATE(Date) AS Date,
                       MAX(IF(name = '三合一电池A', total_count, 0)) AS name1,
                       MAX(IF(name = '三合一电池B', total_count, 0)) AS name2,
                       MAX(IF(name = '三合一电池C', total_count, 0)) AS name3,
                       MAX(IF(name = '三合一电池检测', total_count, 0)) AS name4,
                       MAX(IF(name = '总装线', total_count, 0)) AS name5,
                       MAX(IF(name = '框膜组件检测', total_count, 0)) AS name6
                    FROM production_data
                    WHERE Date >= CURDATE() - INTERVAL 6 DAY
                    GROUP BY DATE(Date)
                    ORDER BY DATE(Date);";

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

        query = @"SELECT DATE(Date) AS Date,
                       MAX(IF(name = '三合一电池A', qualified_rate, 0)) AS name1,
                       MAX(IF(name = '三合一电池B', qualified_rate, 0)) AS name2,
                       MAX(IF(name = '三合一电池C', qualified_rate, 0)) AS name3,
                       MAX(IF(name = '三合一电池检测', qualified_rate, 0)) AS name4,
                       MAX(IF(name = '总装线', qualified_rate, 0)) AS name5,
                       MAX(IF(name = '框膜组件检测', qualified_rate, 0)) AS name6
                    FROM production_data
                    WHERE Date >= CURDATE() - INTERVAL 6 DAY
                    GROUP BY DATE(Date)
                    ORDER BY DATE(Date);";

        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = expectedDates.IndexOf(date);
                rateB[i][dataIndex] = reader.GetDouble($"name{i + 1}");
            }

        weeklyData.Add("rateB", rateB);

        reader.Close();

        return weeklyData;
    }
}