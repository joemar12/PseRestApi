using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PseRestApi.Core.Dto;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Host.Controllers;
using Xunit;
using NSubstitute;
using AwesomeAssertions;

namespace PseRestApi.Test.UnitTests;

public class PseStockPriceControllerTests
{
    private readonly ILogger<PseStockPriceController> _logger;
    private readonly IPseApiService _pseApiService;
    private readonly PseStockPriceController _controller;

    private readonly Stock _mockStock = new()
    {
        SecurityName = "Test Company",
        Symbol = "TEST",
        Price = new List<StockPrice> { new() { Currency = "PHP", Price = 100.00 } },
        AsOfDate = new DateTime(2025, 1, 15, 15, 30, 0),
        PercentChange = 2.5,
        Volume = 1500000
    };

    public PseStockPriceControllerTests()
    {
        _logger = Substitute.For<ILogger<PseStockPriceController>>();
        _pseApiService = Substitute.For<IPseApiService>();
        _controller = new PseStockPriceController(_logger, _pseApiService);
    }

    #region Get Tests

    [Fact]
    public async Task Get_WithEmptySymbol_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "";

        // Act
        IActionResult result = await _controller.Get(symbol);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Symbol is required");
    }

    [Fact]
    public async Task Get_WithWhiteSpaceSymbol_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "   ";

        // Act
        IActionResult result = await _controller.Get(symbol);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Symbol is required");
    }

    [Fact]
    public async Task Get_WithNullAsOfDate_ReturnsLatestPrice()
    {
        // Arrange
        string symbol = _mockStock.Symbol!;
        _pseApiService.GetStockLatestPriceAsync(Arg.Any<string>()).Returns(Task.FromResult(_mockStock));

        // Act
        IActionResult result = await _controller.Get(symbol);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<Stock>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Symbol.Should().Be(symbol);
        apiResponse.Data.SecurityName.Should().Be(_mockStock.SecurityName);
        apiResponse.Message.Should().Be("Latest stock price returned successfully");

        await _pseApiService.Received(1).GetStockLatestPriceAsync(symbol);
    }

    [Fact]
    public async Task Get_WithNullAsOfDateAndNullStock_ReturnsNotFound()
    {
        // Arrange
        string symbol = "INVALID";
        _pseApiService.GetStockLatestPriceAsync(Arg.Any<string>()).Returns(Task.FromResult<Stock>(null!));

        // Act
        IActionResult result = await _controller.Get(symbol);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain($"'{symbol}'");
        apiResponse.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Get_WithAsOfDate_ReturnsHistoricalPrice()
    {
        // Arrange
        string symbol = _mockStock.Symbol!;
        DateTime asOfDate = new DateTime(2025, 1, 15);
        _pseApiService.GetStockPriceAsOfDateAsync(Arg.Any<string>(), asOfDate).Returns(Task.FromResult(_mockStock));

        // Act
        IActionResult result = await _controller.Get(symbol, asOfDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<Stock>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Message.Should().Be("Historical stock price returned successfully");

        await _pseApiService.Received(1).GetStockPriceAsOfDateAsync(symbol, asOfDate);
    }

    [Fact]
    public async Task Get_WithAsOfDateAndNullStock_ReturnsNotFound()
    {
        // Arrange
        string symbol = "INVALID";
        DateTime asOfDate = new DateTime(2025, 1, 15);
        _pseApiService.GetStockPriceAsOfDateAsync(Arg.Any<string>(), asOfDate).Returns(Task.FromResult<Stock>(null!));

        // Act
        IActionResult result = await _controller.Get(symbol, asOfDate);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain($"'{symbol}'");
        apiResponse.Message.Should().Contain(asOfDate.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task Get_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        string symbol = "BPI";
        _pseApiService.GetStockLatestPriceAsync(Arg.Any<string>()).Returns(Task.FromException<Stock>(new InvalidOperationException("Service unavailable")));

        // Act
        IActionResult result = await _controller.Get(symbol);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("An internal error occurred");
    }

    [Fact]
    public async Task Get_SymbolIsNormalizedToUpperCase()
    {
        // Arrange
        string symbol = "bPi";
        _pseApiService.GetStockLatestPriceAsync(Arg.Any<string>()).Returns(Task.FromResult(_mockStock));

        // Act
        await _controller.Get(symbol);

        // Assert
        await _pseApiService.Received(1).GetStockLatestPriceAsync("BPI");
    }

    [Fact]
    public async Task Get_SymbolWhitespaceIsTrimmed()
    {
        // Arrange
        string symbol = "  BPI  ";
        _pseApiService.GetStockLatestPriceAsync(Arg.Any<string>()).Returns(Task.FromResult(_mockStock));

        // Act
        await _controller.Get(symbol);

        // Assert
        await _pseApiService.Received(1).GetStockLatestPriceAsync("BPI");
    }

    #endregion

    #region GetHistory Tests

    [Fact]
    public async Task GetHistory_WithEmptySymbol_ReturnsBadRequest()
    {
        // Arrange
        string? symbol = "";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol!, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Symbol is required");
    }

    [Fact]
    public async Task GetHistory_WithNullSymbol_ReturnsBadRequest()
    {
        // Arrange
        string? symbol = null;
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol!, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Symbol is required");
    }

    [Fact]
    public async Task GetHistory_WithWhitespaceSymbol_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "   ";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Symbol is required");
    }

    [Fact]
    public async Task GetHistory_WithNullStartDate_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = null,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Start date is required");
    }

    [Fact]
    public async Task GetHistory_WithPageNumberLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Page number must be greater than 0");
    }

    [Fact]
    public async Task GetHistory_WithPageSizeZero_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 0
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Page size must be between 1 and 100");
    }

    [Fact]
    public async Task GetHistory_WithPageSizeExceedingMaximum_ReturnsBadRequest()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 101
        };

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Page size must be between 1 and 100");
    }

    [Fact]
    public async Task GetHistory_WithValidParams_ReturnsPaginatedResult()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            PageNumber = 1,
            PageSize = 10
        };

        var paginatedResult = new PaginatedResult<Stock>
        {
            Items = new List<Stock> { _mockStock },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _pseApiService.GetStockPriceHistoryByDateRangeAsync(Arg.Any<string>(), Arg.Any<StockPriceQueryParams>())
            .Returns(Task.FromResult(paginatedResult));

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<PaginatedResult<Stock>>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Items.Should().ContainSingle();
        apiResponse.Data.TotalCount.Should().Be(1);
        apiResponse.Data.PageNumber.Should().Be(1);
        apiResponse.Data.PageSize.Should().Be(10);
        apiResponse.Message.Should().Be("Stock price history returned successfully");

        await _pseApiService.Received(1).GetStockPriceHistoryByDateRangeAsync("BPI", queryParams);
    }

    [Fact]
    public async Task GetHistory_SymbolIsNormalizedToUpperCase()
    {
        // Arrange
        string symbol = "bpi";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        var paginatedResult = new PaginatedResult<Stock>
        {
            Items = Enumerable.Empty<Stock>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _pseApiService.GetStockPriceHistoryByDateRangeAsync(Arg.Any<string>(), Arg.Any<StockPriceQueryParams>())
            .Returns(Task.FromResult(paginatedResult));

        // Act
        await _controller.GetHistory(symbol, queryParams);

        // Assert
        await _pseApiService.Received(1).GetStockPriceHistoryByDateRangeAsync("BPI", queryParams);
    }

    [Fact]
    public async Task GetHistory_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        string symbol = "BPI";
        var queryParams = new StockPriceQueryParams
        {
            StartDate = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        _pseApiService.GetStockPriceHistoryByDateRangeAsync(Arg.Any<string>(), Arg.Any<StockPriceQueryParams>())
            .Returns(Task.FromException<PaginatedResult<Stock>>(new InvalidOperationException("Service unavailable")));

        // Act
        IActionResult result = await _controller.GetHistory(symbol, queryParams);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var apiResponse = objectResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("An internal error occurred");
    }

    #endregion
}
