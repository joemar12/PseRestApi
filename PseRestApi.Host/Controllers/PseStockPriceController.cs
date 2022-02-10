using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Dto;
using PseRestApi.Core.Services;

namespace PseRestApi.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PseStockPriceController : ControllerBase
    {
        private readonly ILogger<PseStockPriceController> _logger;
        private IPseApiService _pseApiService;

        public PseStockPriceController(ILogger<PseStockPriceController> logger, IPseApiService pseApiService)
        {
            _logger = logger;
            _pseApiService = pseApiService;
        }

        [HttpGet(Name = "GetStockPrice")]
        public async Task<Stock> Get([FromQuery] string symbol)
        {
            return await _pseApiService.GetStockLatestPrice(symbol.ToUpper().Trim());
        }
    }
}