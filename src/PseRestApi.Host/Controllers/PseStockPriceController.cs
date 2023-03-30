using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Services;

namespace PseRestApi.Host.Controllers;

[ApiController]
[Route("stockprice")]
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
    [Route("{symbol}/{asOfDate?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(string symbol, DateTime? asOfDate = null)
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
                var result = await _pseApiService.GetHistoricalPrice(symbol, asOfDate);
                return result != null ? Ok(result) : NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}