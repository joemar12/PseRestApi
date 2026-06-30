using AwesomeAssertions;
using NSubstitute;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Dto;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;
using PseRestApi.Test.Helpers.Db;
using Xunit;

namespace PseRestApi.Test.UnitTests.Services;

public class PseApiServiceTests
{
    private readonly IPseClient _pseClient;
    private readonly IAppDbContext _appDbContext;
    private readonly PseApiService _service;

    // Mock data
    private readonly HistoricalTradingData _mockHistoricalData1 = new()
    {
        Id = 1,
        SecurityId = 1,
        Symbol = "ABC",
        Currency = "PHP",
        Price = 50.00,
        TradeDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
        PercentChange = 2.5,
        Volume = 1000000,
        SecurityInfo = new SecurityInfo
        {
            SecurityId = 1,
            Symbol = "ABC",
            SecurityName = "ABC Corporation"
        }
    };

    private readonly HistoricalTradingData _mockHistoricalData2 = new()
    {
        Id = 2,
        SecurityId = 1,
        Symbol = "ABC",
        Currency = "PHP",
        Price = 49.00,
        TradeDate = DateOnly.FromDateTime(new DateTime(2026, 6, 24)),
        PercentChange = -1.0,
        Volume = 900000,
        SecurityInfo = new SecurityInfo
        {
            SecurityId = 1,
            Symbol = "ABC",
            SecurityName = "ABC Corporation"
        }
    };

    private readonly HistoricalTradingData _mockHistoricalDataNoSecurityInfo = new()
    {
        Id = 3,
        SecurityId = 1,
        Symbol = "DEF",
        Currency = "PHP",
        Price = 25.00,
        TradeDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
        PercentChange = 0.5,
        Volume = 500000,
        SecurityInfo = null!
    };

    private readonly StockFromFrames _mockStockFromFrames = new()
    {
        StockName = "XYZ Corp",
        StockSymbol = "XYZ",
        Price = "100.50",
        Volume = 2000000,
        Value = "201000000",
        Change = "2.50",
        PercentChange = "2.55",
        TradeDate = "2026-06-29"
    };

    public PseApiServiceTests()
    {
        _pseClient = Substitute.For<IPseClient>();
        _appDbContext = Substitute.For<IAppDbContext>();
        _service = new PseApiService(_pseClient, _appDbContext);
    }

    #region GetStockPriceAsOfDateAsync Tests

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithValidSymbolAndDate_ReturnsMatchingStock()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act
        var result = await _service.GetStockPriceAsOfDateAsync("ABC", DateOnly.FromDateTime(new DateTime(2026, 6, 26)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("ABC");
        result.SecurityName.Should().Be("ABC Corporation");
        result.AsOfDate.Should().Be(DateOnly.FromDateTime(new DateTime(2026, 6, 25)));
        result.Price.Should().ContainSingle();
        result.Price.First().Price.Should().Be(50.00);
        result.Price.First().Currency.Should().Be("PHP");
        result.PercentChange.Should().Be(2.5);
        result.Volume.Should().Be(1000000);
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithExactDateMatch_ReturnsMatchingStock()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act
        var result = await _service.GetStockPriceAsOfDateAsync("ABC", DateOnly.FromDateTime(new DateTime(2026, 6, 25)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("ABC");
        result.AsOfDate.Should().Be(DateOnly.FromDateTime(new DateTime(2026, 6, 25)));
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_ReturnsMostRecentBeforeDate()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1, _mockHistoricalData2 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act - requesting date after both records, should return the most recent one (June 25)
        var result = await _service.GetStockPriceAsOfDateAsync("ABC", DateOnly.FromDateTime(new DateTime(2026, 6, 26)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("ABC");
        result.AsOfDate.Should().Be(DateOnly.FromDateTime(new DateTime(2026, 6, 25)));
        result.Price.First().Price.Should().Be(50.00);
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithNoMatchingRecords_ReturnsEmptyStock()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new List<HistoricalTradingData>());
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act
        var result = await _service.GetStockPriceAsOfDateAsync("NONEXISTENT", DateOnly.FromDateTime(new DateTime(2026, 6, 26)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().BeNull();
        result.SecurityName.Should().BeNull();
        result.Price.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithDateBeforeAllRecords_ReturnsEmptyStock()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act - date is before the only record
        var result = await _service.GetStockPriceAsOfDateAsync("ABC", DateOnly.FromDateTime(new DateTime(2026, 6, 1)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().BeNull();
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithNullAsOfDate_ReturnsEmptyStock()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act
        var result = await _service.GetStockPriceAsOfDateAsync("ABC", null);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().BeNull();
    }

    [Fact]
    public async Task GetStockPriceAsOfDateAsync_WithNullSecurityInfo_ReturnsStockWithNullSecurityName()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalDataNoSecurityInfo });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);

        // Act
        var result = await _service.GetStockPriceAsOfDateAsync("DEF", DateOnly.FromDateTime(new DateTime(2026, 6, 26)));

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("DEF");
        result.SecurityName.Should().BeNull();
        result.Price.First().Price.Should().Be(25.00);
    }

    #endregion

    #region GetStockLatestPriceAsync Tests

    [Fact]
    public async Task GetStockLatestPriceAsync_WithValidSymbol_ReturnsMappedStock()
    {
        // Arrange
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(new[] { _mockStockFromFrames }));

        // Act
        var result = await _service.GetStockLatestPriceAsync("XYZ");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("XYZ");
        result.SecurityName.Should().Be("XYZ Corp");
        result.Price.Should().ContainSingle();
        result.Price.First().Price.Should().Be(100.50);
        result.Price.First().Currency.Should().Be("PHP");
        result.PercentChange.Should().BeApproximately(2.55, 0.01);
        result.Volume.Should().Be(2000000);
    }

    [Fact]
    public async Task GetStockLatestPriceAsync_WithNonExistentSymbol_ReturnsEmptyStock()
    {
        // Arrange
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(new[] { _mockStockFromFrames }));

        // Act
        var result = await _service.GetStockLatestPriceAsync("NONEXISTENT");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().BeNull();
        result.SecurityName.Should().BeNull();
        result.Price.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStockLatestPriceAsync_WithEmptyStocksList_ReturnsEmptyStock()
    {
        // Arrange
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(Array.Empty<StockFromFrames>()));

        // Act
        var result = await _service.GetStockLatestPriceAsync("ANY");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().BeNull();
    }

    [Fact]
    public async Task GetStockLatestPriceAsync_CallsClientGetStocksOnce()
    {
        // Arrange
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(new[] { _mockStockFromFrames }));

        // Act
        await _service.GetStockLatestPriceAsync("XYZ");

        // Assert
        await _pseClient.Received(1).GetStocks();
    }

    [Fact]
    public async Task GetStockLatestPriceAsync_WithInvalidPriceString_ReturnsZeroPrice()
    {
        // Arrange
        var stockWithInvalidPrice = new StockFromFrames
        {
            StockName = "Invalid Price Corp",
            StockSymbol = "INV",
            Price = "invalid",
            Volume = 100000
        };
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(new[] { stockWithInvalidPrice }));

        // Act
        var result = await _service.GetStockLatestPriceAsync("INV");

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("INV");
        result.Price.First().Price.Should().Be(0);
    }

    [Fact]
    public async Task GetStockLatestPriceAsync_WithInvalidPercentChange_ReturnsZeroPercentChange()
    {
        // Arrange
        var stockWithInvalidPercent = new StockFromFrames
        {
            StockName = "Invalid Percent Corp",
            StockSymbol = "INV",
            Price = "50.00",
            PercentChange = "notanumber"
        };
        _pseClient.GetStocks().Returns(Task.FromResult<IEnumerable<StockFromFrames>>(new[] { stockWithInvalidPercent }));

        // Act
        var result = await _service.GetStockLatestPriceAsync("INV");

        // Assert
        result.Should().NotBeNull();
        result.PercentChange.Should().Be(0);
    }

    #endregion

    #region GetStockPriceHistoryByDateRangeAsync Tests

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithValidRange_ReturnsPaginatedResults()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1, _mockHistoricalData2 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().ContainSingle();
        result.TotalCount.Should().Be(1); // Only one record matches the date range after deduplication
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_DefaultPageNumber_ReturnsPageOne()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_DefaultPageSize_ReturnsTenItems()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithNoEndDate_ReturnsAllFromStartDate()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1, _mockHistoricalData2 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 24))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.Should().NotBeNull();
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithNoMatchingRecords_ReturnsEmptyResult()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new List<HistoricalTradingData>());
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 10))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithNonExistentSymbol_ReturnsEmptyResult()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new List<HistoricalTradingData>());
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("NONEXISTENT", queryParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_ResultContainsCorrectStockFields()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        var item = result.Items.Should().ContainSingle().Subject;
        item.Symbol.Should().Be("ABC");
        item.SecurityName.Should().Be("ABC Corporation");
        item.AsOfDate.Should().Be(DateOnly.FromDateTime(new DateTime(2026, 6, 25)));
        item.PercentChange.Should().Be(2.5);
        item.Volume.Should().Be(1000000);
        item.Price.Should().ContainSingle();
        item.Price.First().Price.Should().Be(50.00);
        item.Price.First().Currency.Should().Be("PHP");
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithNullSecurityInfo_ReturnsEmptySecurityName()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalDataNoSecurityInfo });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("DEF", queryParams);

        // Assert
        var item = result.Items.Should().ContainSingle().Subject;
        item.Symbol.Should().Be("DEF");
        item.SecurityName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithCustomPagination_ReturnsCorrectPage()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new[] { _mockHistoricalData1 });
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25)),
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetStockPriceHistoryByDateRangeAsync_WithNullStartDate_ReturnsEmptyResult()
    {
        // Arrange
        var mockQueryable = Utils.BuildMockDbSet(new List<HistoricalTradingData>());
        _appDbContext.HistoricalTradingData.Returns(mockQueryable);
        var queryParams = new StockPriceQueryParams
        {
            StartDate = null,
            EndDate = DateOnly.FromDateTime(new DateTime(2026, 6, 25))
        };

        // Act
        var result = await _service.GetStockPriceHistoryByDateRangeAsync("ABC", queryParams);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
