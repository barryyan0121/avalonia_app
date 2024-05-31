using MySql.Data.MySqlClient;

namespace AvaloniaApplication.Models;

public class MySqlConnectionWrapper
{
    private readonly string _connectionString;
    private MySqlConnection _connection;

    public MySqlConnectionWrapper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual MySqlConnection OpenConnection()
    {
        _connection = new MySqlConnection(_connectionString);
        _connection.Open();
        return _connection;
    }

    public virtual MySqlCommand CreateCommand(string query)
    {
        var command = new MySqlCommand(query, _connection);
        return command;
    }
}