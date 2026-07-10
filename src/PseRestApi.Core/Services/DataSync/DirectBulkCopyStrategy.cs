using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;
using System.Data;

namespace PseRestApi.Core.Services.DataSync;

public class DirectBulkCopyStrategy : ISyncStrategy
{
    private readonly IBulkCopyService _bulkCopyService;

    public DirectBulkCopyStrategy(IBulkCopyService bulkCopyService)
    {
        _bulkCopyService = bulkCopyService;
    }

    public async Task ExecuteAsync<T>(IAsyncEnumerable<T> syncData, DataSyncOptions options, ILogger logger) where T : class
    {
        var bulkInsertData = await syncData.ToListAsync();

        var sourceTable = bulkInsertData.ToDataTable();
        var columnNames = options.ColumnMappings?.Select(m => m.Value).ToList()
            ?? sourceTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

        var records = sourceTable.AsEnumerable().Select(row => new DataRowDataRecordAdapter(row));
        await _bulkCopyService.CopyAsync(options.TargetTable ?? string.Empty, columnNames, records);
        logger.LogDebug("Direct bulk copy completed for {EntityType} to table {TargetTable}", typeof(T).Name, options.TargetTable);
    }
}
