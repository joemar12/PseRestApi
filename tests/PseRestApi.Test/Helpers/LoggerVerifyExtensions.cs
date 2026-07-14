using Microsoft.Extensions.Logging;
using NSubstitute;

namespace PseRestApi.Test.Helpers;

public static class LoggerVerifyExtensions
{
    /// <summary>
    /// Verifies that a log call was received at the given level, optionally
    /// asserting whether an exception was (or wasn't) passed.
    /// hasException: null = don't care, true = must have an exception, false = must NOT have one.
    /// </summary>
    public static void VerifyLog<T>(
        this ILogger<T> logger,
        LogLevel level,
        bool? hasException = null,
        string? messageContains = null,
        int callCount = 1)
    {
        logger.Received(callCount).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(state =>
                messageContains == null ||
                state.ToString()!.Contains(messageContains, StringComparison.OrdinalIgnoreCase)),
            Arg.Is<Exception?>(ex => hasException == null || (hasException.Value ? ex != null : ex == null)),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Same as VerifyLog, but returns the actual exception(s) passed for that
    /// level so you can assert on the exception itself (type, message, etc).
    /// </summary>
    public static List<Exception?> GetLoggedExceptions<T>(this ILogger<T> logger, LogLevel level)
    {
        var calls = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log)
                        && (LogLevel)c.GetArguments()[0]! == level);

        return calls.Select(c => (Exception?)c.GetArguments()[3]).ToList();
    }

    /// <summary>
    /// Verifies that a log call was received at the given level, optionally
    /// asserting whether an exception was (or wasn't) passed.
    /// hasException: null = don't care, true = must have an exception, false = must NOT have one.
    /// </summary>
    public static void VerifyLog<T>(
        this ILogger logger,
        LogLevel level,
        bool? hasException = null,
        string? messageContains = null,
        int callCount = 1)
    {
        logger.Received(callCount).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(state =>
                messageContains == null ||
                state.ToString()!.Contains(messageContains, StringComparison.OrdinalIgnoreCase)),
            Arg.Is<Exception?>(ex => hasException == null || (hasException.Value ? ex != null : ex == null)),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Same as VerifyLog, but returns the actual exception(s) passed for that
    /// level so you can assert on the exception itself (type, message, etc).
    /// </summary>
    public static List<Exception?> GetLoggedExceptions<T>(this ILogger logger, LogLevel level)
    {
        var calls = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log)
                        && (LogLevel)c.GetArguments()[0]! == level);

        return calls.Select(c => (Exception?)c.GetArguments()[3]).ToList();
    }
}