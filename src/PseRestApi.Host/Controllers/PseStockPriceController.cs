using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Dto;
using PseRestApi.Core.Services.PseApi;

namespace PseRestApi.Host.Controllers;

[ApiController]
[Route("api/stocks")]
public class PseStockPriceController : ControllerBase
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
    public async Task<IActionResult> Get(string symbol,[FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest(new { Error = "Symbol is required" });
            }
            if (asOfDate == null)
            {
                return Ok(await _pseApiService.GetStockLatestPrice(symbol.ToUpper().Trim()));
            }
            else
            {
                var result = await _pseApiService.GetStockPriceAsOfDateAsync(symbol, asOfDate);
                return result != null ? Ok(result) : NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    [HttpGet]
    [Route("{symbol}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(string symbol, [FromQuery] StockPriceQueryParams stockPriceQueryParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest(new { Error = "Symbol is required" });
            }

            if (stockPriceQueryParams.StartDate is null || stockPriceQueryParams.StartDate == DateTime.MinValue)
            {
                return BadRequest(new { Error = "Start date is required" });
            }

            if (stockPriceQueryParams.PageNumber < 1 || stockPriceQueryParams.PageSize < 1)
            {
                return BadRequest(new { Error = "Page number and page size must be greater than 0" });
            }

            var result = await _pseApiService.GetStockPriceHistoryByDateRangeAsync(symbol, stockPriceQueryParams);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}