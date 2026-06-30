using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace PseRestApi.Core.Common;

public class DbConnectionProvider : IDbConnectionProvider
{
    private readonly IConfiguration _configuration;
    public DbConnectionProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnectionString"));
}
