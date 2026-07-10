using Microsoft.Extensions.Logging;
using Npgsql;
using PseRestApi.Core.Common;
using System.Data;

namespace PseRestApi.Core.Services.DataSync;

public class NpgsqlBulkCopyService : IBulkCopyService
{
    private readonly IDbConnectionProvider _connectionProvider;
    private readonly ILogger<NpgsqlBulkCopyService> _logger;

    public NpgsqlBulkCopyService(
        IDbConnectionProvider connectionProvider,
        ILogger<NpgsqlBulkCopyService> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task CopyAsync(string targetTable, IEnumerable<string> columnNames, IEnumerable<IDataRecord> data)
    {
        using var connection = _connectionProvider.CreateConnection();
        if (connection == null)
        {
            throw new InvalidOperationException("Database connection is not available.");
        }

        await using var npgsqlConnection = connection as NpgsqlConnection
            ?? throw new InvalidOperationException("Connection is not an NpgsqlConnection");

        await npgsqlConnection.OpenAsync();

        var targetTableName = targetTable?.Replace("dbo.", "");
        var joinedColumnNames = string.Join(", ", columnNames.Select(c => $"\"{c}\""));

        await using var copyWriter = await npgsqlConnection.BeginBinaryImportAsync(
            $"COPY \"{targetTableName}\" ({joinedColumnNames}) FROM STDIN (FORMAT BINARY)");

        foreach (IDataRecord record in data)
        {
            await copyWriter.StartRowAsync();
            foreach (var columnName in columnNames)
            {
                var value = record[columnName];
                await copyWriter.WriteAsync(value == DBNull.Value ? null : value);
            }
        }

        await copyWriter.CompleteAsync();
    }
}
