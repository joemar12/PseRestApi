using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoDataSyncService : BaseSyncService<SecurityInfo>, ISecurityInfoDataSyncService
{
    public SecurityInfoDataSyncService(
        ISecurityInfoSyncDataProvider syncDataProvider,
        ISyncDataStagingService syncDataStagingService,
        IDbConnectionProvider connectionProvider,
        ILogger<SecurityInfoDataSyncService> logger) : base(syncDataProvider, syncDataStagingService, connectionProvider, logger)
    {
    }

    public override async Task Sync()
    {
        await Sync(options =>
            options
            .WithStaging()
            .WithMergeCommand(MergeCommand())
            .WithCleanupCommand(CleanupCommand())
        );
    }

    private string CleanupCommand()
    {
        var sql = $$"""
                    DELETE
                    FROM SyncBatchData
                    WHERE BatchId = @BatchId
                    """;
        return sql;
    }

    private string MergeCommand()
    {
        var sql = $$"""
                  INSERT INTO SecurityInfo (Symbol, CompanyId, CompanyName, SecurityStatus, SecurityName, Created, LastModified)
                  SELECT DISTINCT
                  			Data->>'SecurityId'::text,
                  			Data->>'Symbol'::text,
                  			Data->>'CompanyId'::text,
                  			Data->>'CompanyName'::text,
                  			Data->>'SecurityStatus'::text,
                  			Data->>'SecurityName'::text,
                  			NOW(),
                  			NOW()
                  FROM SyncBatchData
                  WHERE BatchId = @BatchId
                  ON CONFLICT (Symbol) DO UPDATE SET
                  		Symbol = EXCLUDED.Symbol,
                  		CompanyId = EXCLUDED.CompanyId,
                  		CompanyName = EXCLUDED.CompanyName,
                  		SecurityStatus = EXCLUDED.SecurityStatus,
                  		SecurityName = EXCLUDED.SecurityName,
                  		LastModified = NOW()
                  """;
        return sql;
    }
}
