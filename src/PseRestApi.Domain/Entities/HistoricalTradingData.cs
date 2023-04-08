namespace PseRestApi.Domain.Entities;

public class HistoricalTradingData : AuditableEntity
{
    public Guid Id { get; set; }
    public int SecurityId { get; set; }
    public string? Symbol { get; set; }
    public string? Currency { get; set; }
    public double? SqPrevious { get; set; }
    public double? SqOpen { get; set; }
    public double? SqHigh { get; set; }
    public double? SqLow { get; set; }
    public double? FiftyTwoWeekHigh { get; set; }
    public double? FiftyTwoWeekLow { get; set; }
    public double? ChangeClose { get; set; }
    public double? PercChangeClose { get; set; }
    public double? ChangeClosePercChangeClose { get; set; }
    public double? AvgPrice { get; set; }
    public double? LastTradePrice { get; set; }
    public DateTime? LastTradedDate { get; set; }
    public double? CurrentPe { get; set; }
    public double? TotalValue { get; set; }
    public double? TotalVolume { get; set; }

    public SecurityInfo SecurityInfo { get; set; } = new SecurityInfo();
}
