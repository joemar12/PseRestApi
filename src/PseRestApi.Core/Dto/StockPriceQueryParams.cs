namespace PseRestApi.Core.Dto;

public class StockPriceQueryParams
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}
