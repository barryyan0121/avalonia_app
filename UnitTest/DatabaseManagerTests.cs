using AvaloniaApplication.Models;
using MySql.Data.MySqlClient;

namespace UnitTest;

[TestFixture]
public class DatabaseManagerTests
{
    [SetUp]
    public void Setup()
    {
        // Clear tables and insert initial test data
        using var connection = new MySqlConnection(TestConnectionString);
        connection.Open();
        using var command = new MySqlCommand();
        command.Connection = connection;

        command.CommandText = """
                              TRUNCATE TABLE production_data;
                              TRUNCATE TABLE production_details;
                              INSERT INTO production_data (name, qualified_count, defective_count, target_amount, date) VALUES
                                  ('胶纸切割', 10, 2, 20, CURDATE()),
                                  ('板框焊接', 20, 1, 25, CURDATE()),
                                  ('总装线', 18, 2, 20, CURDATE()),
                                  ('框膜组件检测', 24, 1, 25, CURDATE());
                              INSERT INTO production_details (name, operator_id, machine_id, is_qualified, production_time) VALUES
                                  ('胶纸切割', 1, 1, true, '2024-05-30 08:00:00'),
                                  ('板框焊接', 2, 2, false, '2024-05-30 08:00:00'),
                                  ('板框焊接', 3, 2, true, '2024-05-30 08:00:00');
                              """;
        command.ExecuteNonQuery();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the test database
        using var connection = new MySqlConnection(TestConnectionString);
        connection.Open();
        using var command = new MySqlCommand();
        command.Connection = connection;
        command.CommandText = "TRUNCATE TABLE production_data; TRUNCATE TABLE production_details;";
        command.ExecuteNonQuery();
    }

    private const string TestConnectionString =
        "Server=localhost;Port=3306;Database=test_db;Uid=sample_user;Pwd=sample_password;";

    [Test]
    public void LoadData_ShouldReturnDataForToday()
    {
        var dbManager = new DatabaseManager(TestConnectionString);
        var result = dbManager.LoadData();
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0].Name, Is.EqualTo("总装线"));
            Assert.That(result[0].QualifiedCount, Is.EqualTo(18));
            Assert.That(result[0].DefectiveCount, Is.EqualTo(2));
            Assert.That(result[1].Name, Is.EqualTo("板框焊接"));
            Assert.That(result[1].QualifiedCount, Is.EqualTo(20));
            Assert.That(result[1].DefectiveCount, Is.EqualTo(1));
            Assert.That(result[2].Name, Is.EqualTo("框膜组件检测"));
            Assert.That(result[2].QualifiedCount, Is.EqualTo(24));
            Assert.That(result[2].DefectiveCount, Is.EqualTo(1));
            Assert.That(result[3].Name, Is.EqualTo("胶纸切割"));
            Assert.That(result[3].QualifiedCount, Is.EqualTo(10));
            Assert.That(result[3].DefectiveCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void LoadWeeklyData_ShouldPopulateWeeklyDataMap()
    {
        var dbManager = new DatabaseManager(TestConnectionString);
        dbManager.LoadWeeklyData();

        // Verify data for ProductionLinesA
        var totalA = dbManager.WeeklyDataMap["totalA"];
        var rateA = dbManager.WeeklyDataMap["rateA"];
        // Verify data for ProductionLinesB
        var totalB = dbManager.WeeklyDataMap["totalB"];
        var rateB = dbManager.WeeklyDataMap["rateB"];

        Assert.Multiple(() =>
        {
            Assert.That(totalA, Has.Count.EqualTo(6));
            Assert.That(rateA, Has.Count.EqualTo(6));
            Assert.That(totalB, Has.Count.EqualTo(6));
            Assert.That(rateB, Has.Count.EqualTo(6));
            Assert.That(totalA[0], Has.Count.EqualTo(7));
            Assert.That(rateA[0], Has.Count.EqualTo(7));
            Assert.That(totalB[0], Has.Count.EqualTo(7));
            Assert.That(rateB[0], Has.Count.EqualTo(7));
            //  测试七天内的总产量和合格率
            Assert.That(totalA[0][6].Value, Is.EqualTo(12));
            Assert.That(totalA[1][6].Value, Is.EqualTo(21));
            Assert.That(totalB[4][6].Value, Is.EqualTo(20));
            Assert.That(totalB[5][6].Value, Is.EqualTo(25));
            Assert.That(Math.Round((double)rateA[0][6].Value!, 2), Is.EqualTo(Math.Round(10.0 / 12, 2)));
            Assert.That(Math.Round((double)rateA[1][6].Value!, 2), Is.EqualTo(Math.Round(20.0 / 21, 2)));
            Assert.That(rateB[4][6].Value, Is.EqualTo(18.0 / 20));
            Assert.That(rateB[5][6].Value, Is.EqualTo(24.0 / 25));
        });
    }

    [Test]
    public void LoadAllData_ShouldReturnAllProductionDetails()
    {
        var dbManager = new DatabaseManager(TestConnectionString);
        var result = dbManager.LoadAllData();
        var details = result["板框焊接"];

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(12));
            Assert.That(result.ContainsKey("胶纸切割"), Is.True);
            Assert.That(result.ContainsKey("板框焊接"), Is.True);
            Assert.That(details, Has.Count.EqualTo(1));
            Assert.That(details["2024-05-30"], Has.Count.EqualTo(2));
            Assert.That(details["2024-05-30"][0].OperatorId, Is.EqualTo(2));
        });
    }
}