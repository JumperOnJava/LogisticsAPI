using Microsoft.Data.Sqlite;

namespace LogisticsAPI;

public class DatabaseConnection
{
    private readonly string _connectionString;

    public DatabaseConnection()
    {
        _connectionString = "Data Source=database.db"; // SQLite connection string
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}