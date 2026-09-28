namespace COFIDE.WFF.Api.Configuration;
public class RateLimitOptions
{
    public int PermitLimit { get; set; }
    public int WindowInSeconds { get; set; }
    public int QueueLimit { get; set; }
}
