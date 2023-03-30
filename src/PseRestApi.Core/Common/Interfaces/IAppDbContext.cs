using Microsoft.EntityFrameworkCore;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Common.Interfaces;
public interface IAppDbContext
{
    DbSet<HistoricalTradingData> StockData { get; }
    DbSet<SecurityInfo> StockCompanies { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
