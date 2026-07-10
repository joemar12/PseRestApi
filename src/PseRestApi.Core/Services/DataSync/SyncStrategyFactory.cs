using Microsoft.Extensions.Logging;

namespace PseRestApi.Core.Services.DataSync;

public class SyncStrategyFactory : ISyncStrategyFactory
{
    private readonly StagedSyncStrategy _stagedStrategy;
    private readonly DirectBulkCopyStrategy _directStrategy;
    private readonly ILogger _logger;

    public SyncStrategyFactory(
        StagedSyncStrategy stagedStrategy,
        DirectBulkCopyStrategy directStrategy,
        ILogger<SyncStrategyFactory> logger)
    {
        _stagedStrategy = stagedStrategy;
        _directStrategy = directStrategy;
        _logger = logger;
    }

    public ISyncStrategy GetStrategy(DataSyncOptions options)
    {
        if (!options.SkipStaging)
        {
            _logger.LogDebug("Selecting StagedSyncStrategy");
            return _stagedStrategy;
        }

        _logger.LogDebug("Selecting DirectBulkCopyStrategy");
        return _directStrategy;
    }
}
