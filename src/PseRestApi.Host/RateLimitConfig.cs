namespace PseRestApi.Host;

public class RateLimitConfig
{
    public int PermitLimit { get; set; }
    public int WindowInMinutes { get; set; }
}
