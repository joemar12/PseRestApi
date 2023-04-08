using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
        => new SqlConnection(_configuration.GetConnectionString("DefaultConnectionString"));
}
