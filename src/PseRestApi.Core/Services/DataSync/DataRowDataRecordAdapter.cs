using System.Data;

namespace PseRestApi.Core.Services.DataSync;

/// <summary>
/// Adapts a <see cref="DataRow"/> to <see cref="IDataRecord"/> for compatibility with bulk copy operations.
/// </summary>
public class DataRowDataRecordAdapter : IDataRecord
{
    private readonly DataRow _row;
    private readonly DataColumnCollection _columns;

    public DataRowDataRecordAdapter(DataRow row)
    {
        _row = row;
        _columns = row.Table.Columns;
    }

    public int FieldCount => _columns.Count;

    public bool IsDBNull(int i) => _row.IsNull(i);

    public string GetName(int i) => _columns[i].ColumnName;

    public Type GetFieldType(int i) => _columns[i].DataType;

    public object this[string name] => _row[name];

    public object this[int i] => _row[i];

    public int GetOrdinal(string name) => _columns.IndexOf(_columns[name]);

    public object GetValue(int i) => _row[i];

    public int GetValues(object[] values) => _row.ItemArray.Length;

    public bool GetBoolean(int i) => Convert.ToBoolean(_row[i]);

    public byte GetByte(int i) => Convert.ToByte(_row[i]);

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

    public char GetChar(int i) => Convert.ToChar(_row[i]);

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

    public IDataReader GetDataReader(int i) => throw new NotImplementedException();

    public string GetDataTypeName(int i) => _columns[i].DataType.Name;

    public DateTime GetDateTime(int i) => Convert.ToDateTime(_row[i]);

    public decimal GetDecimal(int i) => Convert.ToDecimal(_row[i]);

    public double GetDouble(int i) => Convert.ToDouble(_row[i]);

    public Type GetProviderType(int i) => _columns[i].DataType;

    public Guid GetGuid(int i) => _row[i] is Guid g ? g : throw new InvalidCastException($"Value at index {i} is not a Guid");

    public short GetInt16(int i) => Convert.ToInt16(_row[i]);

    public int GetInt32(int i) => Convert.ToInt32(_row[i]);

    public long GetInt64(int i) => Convert.ToInt64(_row[i]);

    public string GetString(int i) => Convert.ToString(_row[i]) ?? string.Empty;

    public object GetValues(int i) => _row[i];

    public float GetFloat(int i) => Convert.ToSingle(_row[i]);

    IDataReader IDataRecord.GetData(int i) => throw new NotImplementedException();

    public bool TryGetValues(out object[] values) => throw new NotImplementedException();

    public bool NextResult() => false;

    public void Reset(int i) { }
}
