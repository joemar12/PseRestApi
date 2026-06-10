using PseRestApi.Core.Dto;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;
using System.Globalization;

namespace PseRestApi.Core.Mappers;

public static class ManualMapper
{
    public static Stock MapToStock(StockCompany src)
    {
        var dest = new Stock();
        dest.SecurityName = src.SecurityName;
        dest.Symbol = src.Symbol;
        return dest;
    }

    public static SecurityInfo MapToSecurityInfo(StockCompany src)
    {
        return new SecurityInfo
        {
            SecurityStatus = src.SecurityStatus,
            CompanyId = src.CompanyId,
            Symbol = src.Symbol,
            CompanyName = src.CompanyName,
            SecurityId = src.SecurityId,
            SecurityName = src.SecurityName
        };
    }

    public static SecurityInfo MapToSecurityInfo(StockFromFrames src)
    {
        return new SecurityInfo
        {
            SecurityStatus = null,
            CompanyId = 0,
            Symbol = src.StockSymbol,
            CompanyName = src.StockName,
            SecurityId = 0,
            SecurityName = src.StockName
        };
    }

    public static HistoricalTradingData MapToHistoricalTradingData(StockHeader src)
    {
        if (src == null) return null!;
        double? currentPe = null;
        if (!string.IsNullOrEmpty(src.CurrentPe) && double.TryParse(src.CurrentPe, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            currentPe = double.IsNaN(parsed) || double.IsInfinity(parsed) ? null : parsed;
        }

        return new HistoricalTradingData
        {
            SqLow = src.SqLow,
            FiftyTwoWeekHigh = src.FiftyTwoWeekHigh,
            ChangeClose = src.ChangeClose,
            ChangeClosePercChangeClose = src.ChangeClosePercChangeClose,
            LastTradedDate = src.LastTradedDate,
            TotalValue = src.TotalValue,
            LastTradePrice = src.LastTradePrice,
            SqHigh = src.SqHigh,
            Currency = src.Currency,
            PercChangeClose = src.PercChangeClose,
            FiftyTwoWeekLow = src.FiftyTwoWeekLow,
            SqPrevious = src.SqPrevious,
            Symbol = src.Symbol,
            CurrentPe = currentPe,
            SqOpen = src.SqOpen,
            AvgPrice = src.AvgPrice,
            TotalVolume = src.TotalVolume
        };
    }

    public static StockPrice MapToStockPrice(StockHeader src)
    {
        return new StockPrice
        {
            Currency = src.Currency,
            Price = src.LastTradePrice ?? 0
        };
    }

    public static Stock MapToStock(IEnumerable<StockHeader> headers)
    {
        var dest = new Stock();
        var first = headers?.FirstOrDefault() ?? new StockHeader();
        dest.AsOfDate = first.LastTradedDate;
        dest.PercentChange = first.PercChangeClose ?? 0;
        dest.Volume = first.TotalVolume ?? 0;
        dest.Price = headers?.Select(h => MapToStockPrice(h)) ?? new List<StockPrice>();
        return dest;
    }

    public static Stock MapToStock(StockHeaderResponse src)
    {
        return MapToStock(src.Records ?? Enumerable.Empty<StockHeader>());
    }

    public static Stock MapToStock(HistoricalTradingData src)
    {
        var dest = new Stock();
        dest.SecurityName = src.SecurityInfo?.SecurityName;
        dest.PercentChange = src.PercChangeClose ?? 0;
        dest.Volume = src.TotalVolume ?? 0;
        dest.AsOfDate = src.LastTradedDate;
        dest.Symbol = src.Symbol;
        dest.Price = new List<StockPrice>
        {
            new StockPrice { Currency = src.Currency, Price = src.LastTradePrice ?? 0 }
        };
        return dest;
    }

    public static Stock MapToStock(StockFromFrames src)
    {
        var dest = new Stock();
        dest.SecurityName = src.StockName;
        dest.Symbol = src.StockSymbol;
        dest.AsOfDate = DateTime.Now;
        dest.PercentChange = double.TryParse(src.PercentChange, NumberStyles.Any, CultureInfo.InvariantCulture, out var pc) ? pc : 0;
        dest.Volume = src.Volume;
        var price = new StockPrice { Currency = "PHP", Price = 0 };
        if (double.TryParse(src.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPrice))
        {
            price.Price = parsedPrice;
        }
        dest.Price = new List<StockPrice> { price };
        return dest;
    }
}
