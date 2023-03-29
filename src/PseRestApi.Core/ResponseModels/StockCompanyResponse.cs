namespace PseRestApi.Core.ResponseModels;

public class StockCompanyResponse
{
    public int count { get; set; }
    public int totalCount { get; set; }
    public IEnumerable<StockCompany>? records { get; set; }
}

public class StockCompany
{
    public string? securityStatus { get; set; }
    public int listedCompany_companyId { get; set; }
    public string? symbol { get; set; }
    public string? listedCompany_companyname { get; set; }
    public int securityId { get; set; }
    public string? securityName { get; set; }
}