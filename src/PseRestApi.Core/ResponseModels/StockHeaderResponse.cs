using Newtonsoft.Json;

namespace PseRestApi.Core.ResponseModels;

public class StockHeaderResponse
{
    public StockHeaderResponse()
    {
        Records = new List<StockHeader>();
    }

    public int Count { get; set; }
    public IEnumerable<StockHeader> Records { get; set; }
}

public class StockHeader
{
    [JsonProperty("headerSqLow")]
    public double? SqLow { get; set; }
    [JsonProperty("headerFiftyTwoWeekHigh")]
    public double? FiftyTwoWeekHigh { get; set; }
    [JsonProperty("headerChangeClose")]
    public double? ChangeClose { get; set; }
    [JsonProperty("headerChangeClosePercChangeClose")]
    public double? ChangeClosePercChangeClose { get; set; }
    public DateTime? LastTradedDate { get; set; }
    [JsonProperty("headerTotalValue")]
    public double? TotalValue { get; set; }
    [JsonProperty("headerLastTradePrice")]
    public double? LastTradePrice { get; set; }
    [JsonProperty("headerSqHigh")]
    public double? SqHigh { get; set; }
    public string? Currency { get; set; }
    [JsonProperty("headerPercChangeClose")]
    public double? PercChangeClose { get; set; }
    [JsonProperty("headerFiftyTwoWeekLow")]
    public double? FiftyTwoWeekLow { get; set; }
    [JsonProperty("headerSqPrevious")]
    public double? SqPrevious { get; set; }
    [JsonProperty("securitySymbol")]
    public string? Symbol { get; set; }
    [JsonProperty("headerCurrentPe")]
    public string? CurrentPe { get; set; }
    [JsonProperty("headerSqOpen")]
    public double? SqOpen { get; set; }
    [JsonProperty("headerAvgPrice")]
    public double? AvgPrice { get; set; }
    [JsonProperty("headerTotalVolume")]
    public double? TotalVolume { get; set; }
}