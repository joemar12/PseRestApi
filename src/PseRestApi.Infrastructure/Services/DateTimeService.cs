using PseRestApi.Core.Common.Interfaces;

namespace PseRestApi.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
