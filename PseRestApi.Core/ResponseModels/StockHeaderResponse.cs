namespace PseRestApi.Core.ResponseModels
{
    public class StockHeaderResponse
    {
        public StockHeaderResponse()
        {
            records = new List<StockHeader>();
        }

        public int count { get; set; }
        public IEnumerable<StockHeader> records { get; set; }
    }

    public class StockHeader
    {
        public double? headerSqLow { get; set; }
        public double? headerFiftyTwoWeekHigh { get; set; }
        public double? headerChangeClose { get; set; }
        public double? headerChangeClosePercChangeClose { get; set; }
        public DateTime? lastTradedDate { get; set; }
        public double? headerTotalValue { get; set; }
        public double? headerLastTradePrice { get; set; }
        public double? headerSqHigh { get; set; }
        public string? currency { get; set; }
        public double? headerPercChangeClose { get; set; }
        public double? headerFiftyTwoWeekLow { get; set; }
        public double? headerSqPrevious { get; set; }
        public string? securitySymbol { get; set; }
        public string? headerCurrentPe { get; set; }
        public double? headerSqOpen { get; set; }
        public double? headerAvgPrice { get; set; }
        public double? headerTotalVolume { get; set; }
    }
}