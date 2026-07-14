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

public class BaseSyncServiceErrorHandlingTests
{
    [Fact]
    public async Task Sync_DataSyncException_CapturesErrorInLogger()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        var capturingLogger = new CapturingLogger<HistoricalTradingDataSyncService>();

        // Configure the strategy to throw a DataSyncException
        var dataSyncEx = new DataProviderException("Data provider failed");
        directStrategy.ExecuteAsync(
            Arg.Any<IAsyncEnumerable<HistoricalTradingData>>(),
            Arg.Any<DataSyncOptions>(),
            Arg.Any<ILogger>())
            .Returns(Task.FromException(dataSyncEx));

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
            await service.Sync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<DataProviderException>();
        capturingLogger.CapturedErrors.Should().Contain(dataSyncEx);
    }

    [Fact]
    public async Task Sync_UnexpectedException_WrapsInOrchestrationException()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        var capturingLogger = new CapturingLogger<HistoricalTradingDataSyncService>();

        // Configure the strategy to throw an unexpected exception
        var unexpectedEx = new InvalidOperationException("Unexpected error");
        directStrategy.ExecuteAsync(
            Arg.Any<IAsyncEnumerable<HistoricalTradingData>>(),
            Arg.Any<DataSyncOptions>(),
            Arg.Any<ILogger>())
            .Returns(Task.FromException(unexpectedEx));

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
            await service.Sync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<OrchestrationException>();
        ((OrchestrationException)caughtException!).Message.Should().Contain("HistoricalTradingData");
        caughtException.InnerException.Should().Be(unexpectedEx);
    }

    [Fact]
    public async Task Sync_Success_DoesNotCaptureErrors()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<IHistoricalTradingDataSyncDataProvider, HistoricalTradingData>(new List<HistoricalTradingData>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var directStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("HistoricalTradingData", new DataSyncOptions { TargetTable = "HistoricalTradingData", SkipStaging = true });
        var capturingLogger = new CapturingLogger<HistoricalTradingDataSyncService>();

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(directStrategy);

        var service = new HistoricalTradingDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            capturingLogger);

        // Act
        await service.Sync();

        // Assert
        capturingLogger.CapturedErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task Sync_StagingException_CapturesErrorInLogger()
    {
        // Arrange
        var mockDataProvider = CreateDataProvider<ISecurityInfoSyncDataProvider, SecurityInfo>(new List<SecurityInfo>());
        var mockStrategyFactory = Substitute.For<ISyncStrategyFactory>();
        var stagedStrategy = Substitute.For<ISyncStrategy>();
        var mockServiceProvider = GetOptionsProviderMocks("SecurityInfo", new DataSyncOptions { SkipStaging = false });
        var capturingLogger = new CapturingLogger<SecurityInfoDataSyncService>();

        // Configure the strategy to throw a StagingException
        var stagingEx = new StagingException("Staging failed");
        stagedStrategy.ExecuteAsync(
            Arg.Any<IAsyncEnumerable<SecurityInfo>>(),
            Arg.Any<DataSyncOptions>(),
            Arg.Any<ILogger>())
            .Returns(Task.FromException(stagingEx));

        mockStrategyFactory.GetStrategy(Arg.Any<DataSyncOptions>()).Returns(stagedStrategy);

        var service = new SecurityInfoDataSyncService(
            mockDataProvider,
            mockStrategyFactory,
            mockServiceProvider,
            capturingLogger);

        // Act
        Exception? caughtException = null;
        try
        {
            await service.Sync();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<StagingException>();
        capturingLogger.CapturedErrors.Should().Contain(stagingEx);
    }

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
