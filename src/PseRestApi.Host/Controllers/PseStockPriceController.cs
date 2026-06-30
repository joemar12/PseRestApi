using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Dto;
using PseRestApi.Core.Services.PseApi;

namespace PseRestApi.Host.Controllers;

[Route("api/stocks")]
public class PseStockPriceController : BaseController
{
    private readonly ILogger<PseStockPriceController> _logger;
    private IPseApiService _pseApiService;

    public PseStockPriceController(ILogger<PseStockPriceController> logger, IPseApiService pseApiService)
    {
        _logger = logger;
        _pseApiService = pseApiService;
    }

    [HttpGet]
    [Route("{symbol}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(string symbol, [FromQuery] DateOnly? asOfDate = null)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return ApiError("Symbol is required", StatusCodes.Status400BadRequest);
        }

        var normalizedSymbol = symbol.ToUpper().Trim();

        try
        {
            if (asOfDate == null)
            {
                var result = await _pseApiService.GetStockLatestPriceAsync(normalizedSymbol);
                return result != null
                    ? Success(result, "Latest stock price returned successfully")
                    : ApiError($"Stock '{normalizedSymbol}' not found", StatusCodes.Status404NotFound);
            }
            else
            {
                var result = await _pseApiService.GetStockPriceAsOfDateAsync(normalizedSymbol, asOfDate);
                return result != null
                    ? Success(result, "Historical stock price returned successfully")
                    : ApiError($"No price data found for '{normalizedSymbol}' on {asOfDate:yyyy-MM-dd}", StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for symbol: {Symbol}", symbol);
            return ApiError("An internal error occurred", StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Route("{symbol}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(string symbol, [FromQuery] StockPriceQueryParams stockPriceQueryParams)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return ApiError("Symbol is required", StatusCodes.Status400BadRequest);
        }

        if (stockPriceQueryParams.StartDate is null)
        {
            return ApiError("Start date is required", StatusCodes.Status400BadRequest);
        }

        if (stockPriceQueryParams.PageNumber < 1)
        {
            return ApiError("Page number must be greater than 0", StatusCodes.Status400BadRequest);
        }

        if (stockPriceQueryParams.PageSize < 1 || stockPriceQueryParams.PageSize > 100)
        {
            return ApiError("Page size must be between 1 and 100", StatusCodes.Status400BadRequest);
        }

        var normalizedSymbol = symbol.ToUpper().Trim();

        try
        {
            var result = await _pseApiService.GetStockPriceHistoryByDateRangeAsync(normalizedSymbol, stockPriceQueryParams);
            var response = new ApiResponse<PaginatedResult<Stock>>
            {
                Success = true,
                Data = result,
                Message = "Stock price history returned successfully",
                Pagination = new PaginationMetadata
                {
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                }
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching history for symbol: {Symbol}", symbol);
            return ApiError("An internal error occurred", StatusCodes.Status500InternalServerError);
        }
    }
}