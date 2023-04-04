using System.Data;

namespace PseRestApi.Core.Common;
public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}