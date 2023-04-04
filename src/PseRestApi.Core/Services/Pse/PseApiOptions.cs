namespace PseRestApi.Core.Services.Pse;

public class PseApiOptions
{
    public static string ConfigSectionName = "PseApiConfig";
    public string? BaseUrl { get; set; }
    public string? Referer { get; set; }
}