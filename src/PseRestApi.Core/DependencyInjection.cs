using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Common;
using PseRestApi.Core.Services;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;
using PseRestApi.Core.Services.DataSync.SecurityInfoSync;
using PseRestApi.Core.Services.PseApi;

namespace PseRestApi.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddPseClient(this IServiceCollection services)
    {
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
            .AddTransient<IHistoricalTradingDataSyncService, HistoricalTradingDataSyncService>()
            .AddTransient<IBulkCopyService, NpgsqlBulkCopyService>()
            // Register both ISyncStrategy implementations (factory resolves by type name)
            .AddTransient<ISyncStrategy, StagedSyncStrategy>()
            .AddTransient<ISyncStrategy, DirectBulkCopyStrategy>()
            // Register the factory that selects between them
            .AddTransient<ISyncStrategyFactory, SyncStrategyFactory>()
            // Register options providers with keyed services (.NET 10)
            .AddKeyedScoped<ISyncOptionsProvider, SecurityInfoSyncOptionsProvider>("SecurityInfo")
            .AddKeyedScoped<ISyncOptionsProvider, HistoricalTradingDataSyncOptionsProvider>("HistoricalTradingData");

        return services;
    }
}