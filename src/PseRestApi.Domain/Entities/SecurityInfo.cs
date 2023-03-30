namespace PseRestApi.Domain.Entities;
public class SecurityInfo : AuditableEntity
{
    public int SecurityId { get; set; }
    public string? Symbol { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? SecurityStatus { get; set; }
    public string? SecurityName { get; set; }

    public IEnumerable<HistoricalTradingData> HistoricalTradingData { get; set; } = new List<HistoricalTradingData>();
}
