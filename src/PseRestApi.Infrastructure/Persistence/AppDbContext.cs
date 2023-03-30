using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Domain.Entities;
using PseRestApi.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace PseRestApi.Infrastructure.Persistence;
public class AppDbContext : DbContext, IAppDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }
    public DbSet<HistoricalTradingData> HistoricalTradingData => Set<HistoricalTradingData>();
    public DbSet<SecurityInfo> SecurityInfo => Set<SecurityInfo>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }
}
