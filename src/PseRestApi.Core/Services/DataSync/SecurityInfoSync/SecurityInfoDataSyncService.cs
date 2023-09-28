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
                    FROM dbo.SyncBatchData
                    WHERE BatchId = @BatchId
                    """;
        return sql;
    }

    private string MergeCommand()
    {
        var sql = $$"""
                  MERGE dbo.SecurityInfo as tgt
                  USING (SELECT DISTINCT
                  			JSON_VALUE(Data, '$.SecurityId') as SecurityId,
                  			JSON_VALUE(Data, '$.Symbol') as Symbol,
                  			JSON_VALUE(Data, '$.CompanyId') as CompanyId,
                  			JSON_VALUE(Data, '$.CompanyName') as CompanyName,
                  			JSON_VALUE(Data, '$.SecurityStatus') as SecurityStatus,
                  			JSON_VALUE(Data, '$.SecurityName') as SecurityName
                  		FROM dbo.SyncBatchData
                  		WHERE BatchId = @BatchId)
                  AS src (SecurityId, Symbol, CompanyId, CompanyName, SecurityStatus, SecurityName)
                  On src.Symbol = tgt.Symbol
                  WHEN MATCHED THEN
                  	UPDATE SET
                  		Symbol = src.Symbol,
                  		CompanyName = src.CompanyName,
                  		SecurityStatus = src.SecurityStatus,
                  		SecurityName = src.SecurityName,
                        LastModified = SYSDATETIME()
                  WHEN NOT MATCHED THEN
                  	INSERT (Symbol,
                  			CompanyId,
                  			CompanyName,
                  			SecurityStatus,
                  			SecurityName,
                  			Created,
                  			LastModified)
                  	VALUES (src.Symbol,
                  			src.CompanyId,
                  			src.CompanyName,
                  			src.SecurityStatus,
                  			src.SecurityName,
                  			SYSDATETIME(),
                  			SYSDATETIME());
                  """;
        return sql;
    }
}
