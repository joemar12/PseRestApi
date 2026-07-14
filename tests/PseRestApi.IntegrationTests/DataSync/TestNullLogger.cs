using Microsoft.Extensions.Logging;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// A simple null logger that discards all log output.
/// Used in integration tests where logging is not needed.
/// </summary>
internal static class TestNullLogger
{
    private static readonly ILogger _logger = LoggerFactory.Create(builder => { }).CreateLogger("TestNullLogger");

    /// <summary>
    /// Gets a null logger instance that discards all log output.
    /// </summary>
    public static ILogger Instance => _logger;
}
