using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Collections;
using LiveChartsCore.Defaults;
using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

// DatabaseManager类
public class DatabaseManager
{
    // 所有产线
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

    // 产线A
    public static readonly List<string> ProductionLinesA = ProductionLinesTotal.Take(6).ToList();

    // 产线B
    public static readonly List<string> ProductionLinesB = ProductionLinesTotal.Skip(6).ToList();

    // 数据库连接字符串
    private readonly string _connectionString;

    // 生成过去七天的日期
    private readonly List<DateTime> _expectedDates = Enumerable.Range(-6, 7)
        .Select(offset => DateTime.Today.AddDays(offset).Date)
        .ToList();

    // 自定义的生产信息类
    public readonly ProgressInfo[] ProgressInfos;

    // 生产进度哈希表
    public readonly Dictionary<string, ObservableValue> ProgressMap = new();

    // 各产线合格率和不合格率哈希表
    public readonly Dictionary<string, KeyValuePair<ObservableValue, ObservableValue>> RateMap = new();

    // 过去一周生产数据哈希表
    public readonly Dictionary<string, List<ObservableCollection<ObservableValue>>> WeeklyDataMap = new();

    // 构造函数
    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
        ProgressInfos = new ProgressInfo[ProductionLinesTotal.Count];
        InitializeData();
    }

    // 初始化数据
    private void InitializeData()
    {
        var index = 0;
        foreach (var line in ProductionLinesTotal) 
        {
            RateMap[line] = new KeyValuePair<ObservableValue, ObservableValue>(new ObservableValue(0), new ObservableValue(1));
            ProgressMap[line] = new ObservableValue(0);
            ProgressInfos[index++] = new ProgressInfo(line, 0, ProgressInfo.Paints[index % 9]);
        }
        
        var totalA = new List<ObservableCollection<ObservableValue>>();
        var totalB = new List<ObservableCollection<ObservableValue>>();
        var rateA = new List<ObservableCollection<ObservableValue>>();
        var rateB = new List<ObservableCollection<ObservableValue>>();

        // 初始化过去七天的数据
        for (var i = 0; i < 6; i++)
        {
            totalA.Add([]);
            totalB.Add([]);
            rateA.Add([]);
            rateB.Add([]);
        }

        // 初始化过去七天的数据
        foreach (var _ in _expectedDates)
        {
            for (var i = 0; i < 6; i++)
            {
                totalA[i].Add(new ObservableValue(0.0));
                totalB[i].Add(new ObservableValue(0.0));
                rateA[i].Add(new ObservableValue(0.0));
                rateB[i].Add(new ObservableValue(0.0));
            }
        }

        WeeklyDataMap["totalA"] = totalA;
        WeeklyDataMap["rateA"] = rateA;
        WeeklyDataMap["totalB"] = totalB;
        WeeklyDataMap["rateB"] = rateB;
    }

    // 连接数据库并加载当日的数据
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
                TargetCount = reader.GetInt32("target_amount"),
                Date = reader.GetDateTime("date")
            };
            var name = productionData.Name;
            var progressResult = Math.Round((double)totalCount / productionData.TargetCount * 100, 3);
            RateMap[name].Key.Value = productionData.QualifiedRate;
            RateMap[name].Value.Value = Math.Round(1 - productionData.QualifiedRate, 3);
            ProgressMap[name].Value = progressResult;
            ProgressInfos[ProductionLinesTotal.IndexOf(name)].Value = progressResult;

            data.Add(productionData);
        }

        reader.Close();
        connection.Close();

        return new AvaloniaList<ProductionData>(data.OrderBy(x => x.Name));
    }
    
    // 连接数据库并加载过去一周的数据
    public void LoadWeeklyData()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var totalA = WeeklyDataMap["totalA"];
        var totalB = WeeklyDataMap["totalB"];
        var rateA = WeeklyDataMap["rateA"];
        var rateB = WeeklyDataMap["rateB"];

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");
        for (var i = 0; i < ProductionLinesA.Count; i++)
        {
            var line = ProductionLinesA[i];
            queryBuilder.Append($"""
                                 MAX(IF(name = '{line}', qualified_count + defective_count, 0)) AS total_count{i + 1},
                                 MAX(IF(name = '{line}', 
                                    CASE 
                                        WHEN qualified_count + defective_count > 0 THEN qualified_count / (qualified_count + defective_count) 
                                        ELSE 0 
                                    END, 0)) AS qualified_rate{i + 1}
                                 """);
            if (i < ProductionLinesA.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }

        queryBuilder.Append("""
                             FROM production_data
                             WHERE Date >= CURDATE() - INTERVAL 6 DAY
                             GROUP BY DATE(Date)
                             ORDER BY DATE(Date);
                            """);

        var query = queryBuilder.ToString();
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = _expectedDates.IndexOf(date);
                totalA[i][dataIndex].Value = reader.GetDouble($"total_count{i + 1}");
                rateA[i][dataIndex].Value = Math.Round(reader.GetDouble($"qualified_rate{i + 1}"), 3);
            }
        }

        WeeklyDataMap["totalA"] = totalA;
        WeeklyDataMap["rateA"] = rateA;

        reader.Close();

        queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT DATE(Date) AS Date,");

        for (var i = 0; i < ProductionLinesB.Count; i++)
        {
            var line = ProductionLinesB[i];
            queryBuilder.Append($"""
                                 MAX(IF(name = '{line}', qualified_count + defective_count, 0)) AS total_count{i + 1},
                                 MAX(IF(name = '{line}', 
                                    CASE 
                                        WHEN qualified_count + defective_count > 0 THEN qualified_count / (qualified_count + defective_count) 
                                        ELSE 0 
                                    END, 0)) AS qualified_rate{i + 1}
                                 """);
            if (i < ProductionLinesB.Count - 1)
            {
                queryBuilder.Append(',');
            }
        }

        queryBuilder.Append("""
                             FROM production_data
                             WHERE Date >= CURDATE() - INTERVAL 6 DAY
                             GROUP BY DATE(Date)
                             ORDER BY DATE(Date);
                            """);

        query = queryBuilder.ToString();
        command = new MySqlCommand(query, connection);
        reader = command.ExecuteReader();

        while (reader.Read())
        {
            for (var i = 0; i < 6; i++)
            {
                var date = reader.GetDateTime("Date").Date;
                var dataIndex = _expectedDates.IndexOf(date);
                totalB[i][dataIndex].Value = reader.GetDouble($"total_count{i + 1}");
                rateB[i][dataIndex].Value = Math.Round(reader.GetDouble($"qualified_rate{i + 1}"), 3);
            }
        }

        WeeklyDataMap["totalB"] = totalB;
        WeeklyDataMap["rateB"] = rateB;

        reader.Close();
        connection.Close();
    }

    // 连接数据库并加载所有数据以实现查找单工位数据
    public AvaloniaDictionary<string, AvaloniaDictionary<string, AvaloniaList<ProductionDetails>>> LoadAllData()
    {
        AvaloniaDictionary<string, AvaloniaDictionary<string, AvaloniaList<ProductionDetails>>> avaloniaDictionary =
            [];
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        const string query = "SELECT * FROM production_details ORDER BY production_time;";
        var command = new MySqlCommand(query, connection);
        var reader = command.ExecuteReader();

        foreach (var line in ProductionLinesTotal)
        {
            avaloniaDictionary[line] = [];
        }

        while (reader.Read())
        {
            var productionDetails = new ProductionDetails
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                OperatorId = reader.GetInt32("operator_id"),
                MachineId = reader.GetInt32("machine_id"),
                IsQualified = reader.GetBoolean("is_qualified") ? "OK" : "NG",
                ProductionTime = reader.GetDateTime("production_time")
            };
            if (!avaloniaDictionary.TryGetValue(productionDetails.Name, out var dateDictionary))
            {
                dateDictionary = new AvaloniaDictionary<string, AvaloniaList<ProductionDetails>>();
                avaloniaDictionary[productionDetails.Name] = dateDictionary;
            }

            var productionDate = productionDetails.ProductionTime.Date.ToString("yyyy-MM-dd");
            if (!dateDictionary.TryGetValue(productionDate, out var productionList))
            {
                productionList = new AvaloniaList<ProductionDetails>();
                dateDictionary[productionDate] = productionList;
            }

            productionList.Add(productionDetails);
        }

        reader.Close();
        connection.Close();

        return avaloniaDictionary;
    }
}