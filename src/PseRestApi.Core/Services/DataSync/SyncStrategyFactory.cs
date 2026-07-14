using Microsoft.Extensions.Logging;

namespace PseRestApi.Core.Services.DataSync;

public class SyncStrategyFactory : ISyncStrategyFactory
{
    private readonly IEnumerable<ISyncStrategy> _strategies;
    private readonly ILogger<SyncStrategyFactory> _logger;

    public SyncStrategyFactory(
        IEnumerable<ISyncStrategy> strategies,
        ILogger<SyncStrategyFactory> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public ISyncStrategy GetStrategy(DataSyncOptions options)
    {
        var targetTypeName = options.SkipStaging
            ? nameof(DirectBulkCopyStrategy)
            : nameof(StagedSyncStrategy);

        var strategy = _strategies.FirstOrDefault(s => s.GetType().Name == targetTypeName);
        if (strategy is null)
        {
            var available = string.Join(", ", _strategies.Select(s => s.GetType().Name));
            throw new InvalidOperationException(
                $"No strategy found for '{targetTypeName}'. Available strategies: {available}");
        }

        _logger.LogDebug("Selecting {StrategyName}", targetTypeName);
        return strategy;
    }
}
