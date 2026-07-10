namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoSyncOptionsProvider : ISyncOptionsProvider
{
    public DataSyncOptions GetOptions() => new DataSyncOptionsBuilder()
        .WithSkipStaging(false)
        .WithMergeCommand(MergeCommand)
        .WithCleanupCommand(CleanupCommand)
        .Build();

    private const string MergeCommand = """
        INSERT INTO SecurityInfo (Symbol, CompanyId, CompanyName, SecurityStatus, SecurityName, Created, LastModified)
        SELECT DISTINCT
            (Data::jsonb)->>'Symbol',
            ((Data::jsonb)->>'CompanyId')::int,
            (Data::jsonb)->>'CompanyName',
            (Data::jsonb)->>'SecurityStatus',
            (Data::jsonb)->>'SecurityName',
            NOW(), NOW()
        FROM SyncBatchData
        WHERE BatchId = @BatchId
        ON CONFLICT (Symbol) DO UPDATE SET
            CompanyId = EXCLUDED.CompanyId,
            CompanyName = EXCLUDED.CompanyName,
            SecurityStatus = EXCLUDED.SecurityStatus,
            SecurityName = EXCLUDED.SecurityName,
            LastModified = NOW();
        """;

    private const string CleanupCommand = """
        DELETE FROM SyncBatchData
        WHERE BatchId = @BatchId
        """;
}
