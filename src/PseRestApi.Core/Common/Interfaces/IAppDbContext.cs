using Microsoft.EntityFrameworkCore;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Common.Interfaces;
public interface IAppDbContext
{
    DbSet<HistoricalTradingData> HistoricalTradingData { get; }
    DbSet<SecurityInfo> SecurityInfo { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
