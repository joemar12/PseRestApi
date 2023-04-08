namespace PseRestApi.Core.ResponseModels;

public class StockSummaryResponse
{
    public double? TotalVolume { get; set; }
    public string? Indicator { get; set; }
    public double? PercChangeClose { get; set; }
    public string? LastTradedPrice { get; set; }
    public string? SecurityAlias { get; set; }
    public string? SecuritySymbol { get; set; }
}