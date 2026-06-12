using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<HistoricalTradingData> HistoricalTradingData { get; }
    DbSet<SecurityInfo> SecurityInfo { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
