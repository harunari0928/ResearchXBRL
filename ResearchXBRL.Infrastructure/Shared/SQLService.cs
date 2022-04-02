using System;
using System.Threading.Tasks;
using Npgsql;

namespace ResearchXBRL.Infrastructure.Shared;

public abstract class SQLService : IDisposable, IAsyncDisposable
{
    protected readonly NpgsqlConnection connection;

    public SQLService()
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVERNAME");
        var userId = Environment.GetEnvironmentVariable("DB_USERID");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var connectionString = $"Server={server};Port={port};Database={dbName};User Id={userId};Password={password};Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100";
        connection = new NpgsqlConnection(connectionString);
        connection.Open();
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }
}
