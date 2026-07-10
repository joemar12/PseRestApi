using System.Data;

namespace PseRestApi.Core.Services.DataSync;

public interface IBulkCopyService
{
    Task CopyAsync(string targetTable, IEnumerable<string> columnNames, IEnumerable<IDataRecord> data);
}
