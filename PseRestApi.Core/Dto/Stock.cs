namespace PseRestApi.Core.Dto;

public class Stock
{
    public Stock()
    {
        Price = new List<StockPrice>();
    }

    public string? SecurityName { get; set; }
    public string? Symbol { get; set; }
    public IEnumerable<StockPrice> Price { get; set; }
    public DateTime? LastTradeDate { get; set; }
    public double PercentChange { get; set; }
    public double Volume { get; set; }
}