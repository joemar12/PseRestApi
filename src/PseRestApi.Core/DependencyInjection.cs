using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Common;
using PseRestApi.Core.Services;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;
using PseRestApi.Core.Services.DataSync.SecurityInfoSync;
using PseRestApi.Core.Services.PseApi;
using System.Reflection;

namespace PseRestApi.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddPseClient(this IServiceCollection services)
    {
        services.AddAutoMapper(
            cfg => cfg.ShouldMapMethod = m => false,
            Assembly.GetExecutingAssembly());
        services
            .AddScoped<IPseClient, PseClient>()
            .AddScoped<IPseApiService, PseApiService>()
            .AddMemoryCache()
            .AddSingleton<ICacheProvider, CacheProvider>();

        return services;
    }

    public static IServiceCollection AddDataSyncServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IDbConnectionProvider, DbConnectionProvider>()
            .AddTransient<ISyncDataStagingService, SyncDataStagingService>()
            .AddTransient<IHistoricalTradingDataSyncDataProvider, HistoricalTradingDataSyncDataProvider>()
            .AddTransient<ISecurityInfoSyncDataProvider, SecurityInfoSyncDataProvider>()
            .AddTransient<ISecurityInfoDataSyncService, SecurityInfoDataSyncService>()
            .AddTransient<IHistoricalTradingDataSyncService, HistoricalTradingDataSyncService>();
        return services;
    }
}