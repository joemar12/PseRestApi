namespace PseRestApi.Core.ResponseModels
{
    public class StockSummaryResponse
    {
        public double? totalVolume { get; set; }
        public string? indicator { get; set; }
        public double? percChangeClose { get; set; }
        public string? lastTradedPrice { get; set; }
        public string? securityAlias { get; set; }
        public string? securitySymbol { get; set; }
    }
}