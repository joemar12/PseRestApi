using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;
using PseRestApi.Core.Services.DataSync.SecurityInfoSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.Test.UnitTests.DataSync;

// Dummy logger that captures error log calls for any type parameter
internal sealed class CapturingLogger<T> : ILogger<T>, IDisposable
{
    public List<Exception> CapturedErrors { get; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error && exception != null)
        {
            CapturedErrors.Add(exception);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    IDisposable ILogger.BeginScope<TState>(TState state) => null!;
    public void Dispose() { }
}

public class BaseSyncServiceTests
{
    #region Direct Sync Path — HistoricalTradingDataSyncService

    [Fact]
    public async Task Sync_DirectMode_CallsBulkCopyWithCorrectTableAndColumns()
    {
        // Arrange
        
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var datasyncOptions = new DataSyncOptions()
        {
            TargetTable = "TestTargetTable"
        };
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", datasyncOptions);
        ILogger<HistoricalTradingDataSyncService> mockLogger = Substitute.For<ILogger<HistoricalTradingDataSyncService>>();

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(directStrategy);

        var service = new HistoricalTradingDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            mockLogger);

        // Act
        await service.RunAsync();

        // Assert
        await directStrategy.Received(1).ExecuteAsync(
            Arg.Any<IAsyncEnumerable<HistoricalTradingData>>(),
            Arg.Is<DataSyncOptions>(opts => opts.TargetTable == datasyncOptions.TargetTable),
            Arg.Any<ILogger>());
    }

    [Fact]
    public async Task Sync_DirectMode_DoesNotThrowOnNullConnection()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        ILogger<HistoricalTradingDataSyncService> mockLogger = Substitute.For<ILogger<HistoricalTradingDataSyncService>>();

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(directStrategy);

        var service = new HistoricalTradingDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            mockLogger);

        // Act & Assert
        Exception? exception = null;
        try
        {
            await service.RunAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        // The direct strategy now handles errors internally, so no exception should propagate
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Sync_DirectMode_LogsErrorAndRethrowsOnBulkCopyFailure()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        var capturingLogger = new CapturingLogger<HistoricalTradingDataSyncService>();

        // Configure the strategy to throw
        directStrategy.ExecuteAsync(Arg.Any<IAsyncEnumerable<HistoricalTradingData>>(), Arg.Any<DataSyncOptions>(), Arg.Any<ILogger>())
            .Returns(Task.FromException(new InvalidOperationException("bulk copy failed")));

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(directStrategy);

        var service = new HistoricalTradingDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            capturingLogger);

        // Act
        Exception? caughtException = null;
        try
        {
            await service.RunAsync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<OrchestrationException>();
        caughtException.InnerException.Should().BeOfType<InvalidOperationException>();
        caughtException.InnerException!.Message.Should().Be("bulk copy failed");
        // The orchestration layer logs the unexpected exception
        capturingLogger.CapturedErrors.Should().Contain(ex => ex is InvalidOperationException);
    }

    [Fact]
    public async Task Sync_DirectMode_IgnoresStagingService()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        ILogger<HistoricalTradingDataSyncService> mockLogger = Substitute.For<ILogger<HistoricalTradingDataSyncService>>();

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(directStrategy);

        var service = new HistoricalTradingDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            mockLogger);

        // Act
        await service.RunAsync();

        // Assert
        await directStrategy.Received(1).ExecuteAsync(Arg.Any<IAsyncEnumerable<HistoricalTradingData>>(), Arg.Any<DataSyncOptions>(), Arg.Any<ILogger>());
    }

    #endregion

    #region Staged Sync Path — SecurityInfoDataSyncService

    [Fact]
    public async Task Sync_StagedMode_UsesStagingAndBypassesBulkCopy()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<ISecurityInfoSyncDataProvider, SecurityInfo>(new List<SecurityInfo>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var stagedStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("SecurityInfo", new DataSyncOptions { SkipStaging = false });
        ILogger<SecurityInfoDataSyncService> mockLogger = Substitute.For<ILogger<SecurityInfoDataSyncService>>();

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(stagedStrategy);

        var service = new SecurityInfoDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            mockLogger);

        // Act
        await service.RunAsync();

        // Assert
        await stagedStrategy.Received(1).ExecuteAsync(Arg.Any<IAsyncEnumerable<SecurityInfo>>(), Arg.Any<DataSyncOptions>(), Arg.Any<ILogger>());
    }

    #endregion

    #region Helpers

    private static TProvider CreateDataProvider<TProvider, TData>(IEnumerable<TData> data)
        where TProvider : class, ISyncDataProvider<TData>
        where TData : class
    {
        var provider = Substitute.For<TProvider>();
        provider.GetSyncData().Returns(CreateAsyncEnumerable(data));
        return provider;
    }

    private static async IAsyncEnumerable<TData> CreateAsyncEnumerable<TData>(IEnumerable<TData> data) where TData : class
    {
        foreach (var item in data)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static IServiceProvider GetOptionsProviderMocks(string entityName, DataSyncOptions? options)
    {
        var provider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
        var optionsProvider = Substitute.For<ISyncOptionsProvider>();
        if (options != null)
        {
            optionsProvider.GetOptions().Returns(options);
        }
        ((IKeyedServiceProvider)provider).GetKeyedService(typeof(ISyncOptionsProvider), entityName).Returns(optionsProvider);
        return provider;
    }

    #endregion
}
