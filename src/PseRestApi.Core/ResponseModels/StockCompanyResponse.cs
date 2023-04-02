using Newtonsoft.Json;

namespace PseRestApi.Core.ResponseModels;

public class StockCompanyResponse
{
    public int Count { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<StockCompany>? Records { get; set; }
}

public class StockCompany
{
    public string? SecurityStatus { get; set; }
    [JsonProperty("listedCompany_companyId")]
    public int CompanyId { get; set; }
    public string? Symbol { get; set; }
    [JsonProperty("listedCompany_companyName")]
    public string? CompanyName { get; set; }
    public int SecurityId { get; set; }
    public string? SecurityName { get; set; }
}