using System.Data;
using System.Net.Sockets;

namespace PseRestApi.Core.Services.DataSync;

/// <summary>
/// Determines whether an exception represents a transient error that may succeed on retry.
/// </summary>
public static class TransientErrorDetector
{
    /// <summary>
    /// Returns true if the exception represents a transient error suitable for retry.
    /// </summary>
    public static bool IsTransient(Exception ex)
    {
        // NpgsqlException with specific SQL states indicate transient issues
        if (ex is Npgsql.NpgsqlException npgsqlEx)
            return IsTransientNpgsql(npgsqlEx);

        // HttpRequestException covers network failures when fetching from PSE API
        if (ex is System.Net.Http.HttpRequestException)
            return true;

        // Timeout exceptions are typically transient
        if (ex is TimeoutException)
            return true;

        // OperationCanceledException can be transient if caused by timeout
        if (ex is OperationCanceledException)
            return true;

        // DbUpdateException can wrap transient database errors
        if (ex is Microsoft.EntityFrameworkCore.DbUpdateException)
            return true;

        // SocketException indicates network failure
        if (ex is SocketException)
            return true;

        return false;
    }

    private static bool IsTransientNpgsql(Npgsql.NpgsqlException ex)
    {
        // PostgreSQL error codes that indicate transient issues:
        // 57P01 - admin_shutdown
        // 57P02 - crash_shutdown
        // 57P03 - cannot_connect_now
        // 53300 - out_of_memory
        // 53400 - too_many_connections
        // 40P01 - deadlock_detected (can succeed on retry)
        // Connection-related errors (inner exceptions often contain these)

        var sqlState = ex.SqlState;
        if (sqlState is "57P01" or "57P02" or "57P03" or "53300" or "53400" or "40P01")
            return true;

        // Connection-level failures (no valid SQL state yet)
        if (ex.InnerException is SocketException socketEx)
            return true;

        return false;
    }
}
