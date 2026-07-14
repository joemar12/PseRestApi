using Microsoft.Extensions.Logging;
using Npgsql;
using PseRestApi.Core.Common;
using System.Reflection;

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

    public async Task CopyAsync<T>(string targetTable, IAsyncEnumerable<T> data, DataSyncOptions? options = null)
        where T : class
    {
        try
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

            var propertyMap = BuildPropertyMap(typeof(T), options?.ColumnMappings);
            var columnNames = propertyMap.Select(p => p.ColumnName).ToArray();
            var joinedColumnNames = string.Join(", ", columnNames.Select(c => $"\"{c}\""));

            await using var copyWriter = await npgsqlConnection.BeginBinaryImportAsync(
                $"COPY \"{targetTableName}\" ({joinedColumnNames}) FROM STDIN (FORMAT BINARY)");

            await foreach (var item in data)
            {
                await copyWriter.StartRowAsync();
                foreach (var (propertyName, columnName, property) in propertyMap)
                {
                    var value = property.GetValue(item) ?? DBNull.Value;
                    await copyWriter.WriteAsync(value == DBNull.Value ? null : value);
                }
            }

            await copyWriter.CompleteAsync();
        }
        catch (BulkCopyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BulkCopyException($"Bulk copy to table '{targetTable}' failed", ex);
        }
    }

    private IEnumerable<(string PropertyName, string ColumnName, PropertyInfo Property)>
        BuildPropertyMap(Type type, Dictionary<string, string>? columnMappings)
    {
        var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyByName = propertyInfos.ToDictionary(p => p.Name, StringComparer.Ordinal);

        if (columnMappings != null && columnMappings.Count > 0)
        {
            foreach (var (propertyName, columnName) in columnMappings)
            {
                if (propertyByName.TryGetValue(propertyName, out var property))
                {
                    yield return (propertyName, columnName, property);
                }
                else
                {
                    _logger.LogWarning(
                        "Column mapping property '{PropertyName}' not found on type '{TypeName}', skipping.",
                        propertyName, type.Name);
                }
            }
        }
        else
        {
            foreach (var property in propertyInfos)
            {
                yield return (property.Name, property.Name, property);
            }
        }
    }
}
