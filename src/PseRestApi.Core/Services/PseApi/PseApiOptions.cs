namespace PseRestApi.Core.Services.PseApi;

public class PseApiOptions
{
    public static string ConfigSectionName = "PseApiConfig";
    public string? FramesUrl { get; set; }
    public string? Referer { get; set; }
}