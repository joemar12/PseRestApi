namespace PseRestApi.Core.ResponseModels;

public class PseFramesResponse
{
    public StockFromFrames[]? Stocks { get; set; }
}
public class StockFromFrames
{
    public string? StockName { get; set; }
    public string? StockSymbol { get; set; }
    public double Volume { get; set; }
    public string? Value { get; set; }
    public string? Price { get; set; }
    public string? Change { get; set; }
    public string? PercentChange { get; set; }
    public string? TradeDate { get; set; }
}