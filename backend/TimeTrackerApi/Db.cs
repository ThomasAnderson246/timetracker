using Npgsql;

public static class Db
{
    private static string? _connectionString;

    public static void Configure(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static NpgsqlConnection CreateConnection()
    {
        if (_connectionString is null)
            throw new InvalidOperationException("Database is not configured correction.");
        return new NpgsqlConnection(_connectionString);
    }
}