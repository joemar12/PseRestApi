namespace PseRestApi.Core.Services.PseApi;

public class PseApiOptions
{
    public static string ConfigSectionName = "PseApiConfig";
    public string? BaseUrl { get; set; }
    public string? FramesUrl { get; set; }
    public string? Referer { get; set; }
}