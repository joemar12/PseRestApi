using PseRestApi.Core.Dto;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;
using System.Globalization;

namespace PseRestApi.Core.Mappers;

public static class ManualMapper
{
    public static SecurityInfo MapToSecurityInfo(StockFromFrames src)
    {
        return new SecurityInfo
        {
            Symbol = src.StockSymbol,
            CompanyName = src.StockName,
            SecurityName = src.StockName
        };
    }

    public static HistoricalTradingData MapToHistoricalTradingData(StockFromFrames src)
    {
        if (src == null) return null!;

        return new HistoricalTradingData
        {
            Symbol = src.StockSymbol,
            TotalVolume = src.Volume,
            TotalValue = double.TryParse(src.Value, out var value) ? value : null,
            ChangeClose = double.TryParse(src.Change, out var change) ? change : null,
            LastTradePrice = double.TryParse(src.Price, out var price) ? price : null,
            PercChangeClose = double.TryParse(src.PercentChange, out var percentChange) ? percentChange : null,
            LastTradedDate = DateTime.Now,
            Currency = "PHP",
        };
    }

    public static Stock MapToStock(HistoricalTradingData src)
    {
        var dest = new Stock
        {
            SecurityName = src.SecurityInfo?.SecurityName,
            PercentChange = src.PercChangeClose ?? 0,
            Volume = src.TotalVolume ?? 0,
            AsOfDate = src.LastTradedDate,
            Symbol = src.Symbol,
            Price = new List<StockPrice>
            {
                new StockPrice { Currency = src.Currency, Price = src.LastTradePrice ?? 0 }
            }
        };
        return dest;
    }

    public static Stock MapToStock(StockFromFrames src)
    {
        var dest = new Stock
        {
            SecurityName = src.StockName,
            Symbol = src.StockSymbol,
            AsOfDate = DateTime.Now,
            PercentChange = double.TryParse(src.PercentChange, NumberStyles.Any, CultureInfo.InvariantCulture, out var pc) ? pc : 0,
            Volume = src.Volume
        };
        var price = new StockPrice { Currency = "PHP", Price = 0 };
        if (double.TryParse(src.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPrice))
        {
            price.Price = parsedPrice;
        }
        dest.Price = new List<StockPrice> { price };
        return dest;
    }
}
