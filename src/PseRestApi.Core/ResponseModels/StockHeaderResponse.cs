using System.Text.Json.Serialization;

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
    [JsonPropertyName("headerSqLow")]
    public double? SqLow { get; set; }
    [JsonPropertyName("headerFiftyTwoWeekHigh")]
    public double? FiftyTwoWeekHigh { get; set; }
    [JsonPropertyName("headerChangeClose")]
    public double? ChangeClose { get; set; }
    [JsonPropertyName("headerChangeClosePercChangeClose")]
    public double? ChangeClosePercChangeClose { get; set; }
    public DateTime? LastTradedDate { get; set; }
    [JsonPropertyName("headerTotalValue")]
    public double? TotalValue { get; set; }
    [JsonPropertyName("headerLastTradePrice")]
    public double? LastTradePrice { get; set; }
    [JsonPropertyName("headerSqHigh")]
    public double? SqHigh { get; set; }
    public string? Currency { get; set; }
    [JsonPropertyName("headerPercChangeClose")]
    public double? PercChangeClose { get; set; }
    [JsonPropertyName("headerFiftyTwoWeekLow")]
    public double? FiftyTwoWeekLow { get; set; }
    [JsonPropertyName("headerSqPrevious")]
    public double? SqPrevious { get; set; }
    [JsonPropertyName("securitySymbol")]
    public string? Symbol { get; set; }
    [JsonPropertyName("headerCurrentPe")]
    public string? CurrentPe { get; set; }
    [JsonPropertyName("headerSqOpen")]
    public double? SqOpen { get; set; }
    [JsonPropertyName("headerAvgPrice")]
    public double? AvgPrice { get; set; }
    [JsonPropertyName("headerTotalVolume")]
    public double? TotalVolume { get; set; }
}