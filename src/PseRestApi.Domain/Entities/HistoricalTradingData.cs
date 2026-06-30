namespace PseRestApi.Domain.Entities;

public class HistoricalTradingData : AuditableEntity
{
    public int Id { get; set; }
    public int SecurityId { get; set; }
    public string? Symbol { get; set; }
    public string? Currency { get; set; }
    public double? Change { get; set; }
    public double? PercentChange { get; set; }
    public double? Price { get; set; }
    public DateOnly? TradeDate { get; set; }
    public double? Value { get; set; }
    public double? Volume { get; set; }

    public SecurityInfo SecurityInfo { get; set; } = new SecurityInfo();
}
