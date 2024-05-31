using Avalonia.Collections;
using Avalonia.Headless.NUnit;
using AvaloniaApplication.Models;
using Moq;
using MySql.Data.MySqlClient;

namespace UnitTest;

[TestFixture]
public class DatabaseManagerTests
{
    private Mock<MySqlConnection> _mockConnection;
    private Mock<MySqlCommand> _mockCommand;
    private Mock<MySqlDataReader> _mockReader;
    
    [SetUp]
    public void SetUp()
    {
        _mockConnection = new Mock<MySqlConnection>();
        _mockCommand = new Mock<MySqlCommand>();
        _mockReader = new Mock<MySqlDataReader>();

        _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
    }
    
    // [AvaloniaTest]
    // public void LoadData_ShouldReturnProductionData_WhenDataExists()
    // {
    //     // Arrange
    //     var connectionString = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
    //     var databaseManager = new DatabaseManager(connectionString);
    //     var expectedData = GetSampleProductionData();
    //         
    //     _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockReader.Object);
    //     _mockReader.SetupSequence(reader => reader.Read())
    //         .Returns(true)
    //         .Returns(false);
    //     _mockReader.Setup(reader => reader.GetInt32(It.IsAny<string>())).Returns(100);
    //     _mockReader.Setup(reader => reader.GetString(It.IsAny<string>())).Returns("Test Line");
    //     _mockReader.Setup(reader => reader.GetDateTime(It.IsAny<string>())).Returns(DateTime.Today);
    //         
    //     // Act
    //     var result = databaseManager.LoadData();
    //
    //     // Assert
    //     Assert.That(result, Is.Not.Null);
    //     Assert.That(result, Has.Count.EqualTo(expectedData.Count));
    //     // Add more assertions as needed to validate the contents of the result
    // }
    //
    // private static AvaloniaList<ProductionData> GetSampleProductionData()
    // {
    //     var data = new AvaloniaList<ProductionData>
    //     {
    //         new()
    //         {
    //             Id = 1,
    //             Name = "Test Line",
    //             QualifiedCount = 100,
    //             DefectiveCount = 20,
    //             QualifiedRate = 0.83,
    //             TotalCount = 120,
    //             TargetCount = 150,
    //             Date = DateTime.Today
    //         }
    //     };
    //     return data;
    // }
}