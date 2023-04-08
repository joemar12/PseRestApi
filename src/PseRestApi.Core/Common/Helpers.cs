using System.ComponentModel;
using System.Data;

namespace PseRestApi.Core.Common;
public static class Helpers
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
        {
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }
        foreach (T item in list)
        {
            DataRow row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
            {
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }
        return table;
    }
}
