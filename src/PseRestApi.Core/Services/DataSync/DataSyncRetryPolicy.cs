using Polly;

namespace PseRestApi.Core.Services.DataSync;

/// <summary>
/// Provides retry policies for DataSync operations.
/// </summary>
public static class DataSyncRetryPolicy
{
    /// <summary>
    /// Executes the given action with retry logic for transient errors.
    /// </summary>
    public static async Task ExecuteAsync(Func<Task> action, string operationName)
    {
        var policy = Policy
            .Handle<Exception>(TransientErrorDetector.IsTransient)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));

        try
        {
            await policy.ExecuteAsync(action);
        }
        catch (Exception ex)
        {
            // All retries exhausted - wrap in appropriate exception
            throw new DataProviderException($"Failed to execute {operationName} after 3 retries", ex);
        }
    }

    /// <summary>
    /// Executes the given action with retry logic for transient errors.
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string operationName)
    {
        var policy = Policy
            .Handle<Exception>(TransientErrorDetector.IsTransient)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));

        try
        {
            return await policy.ExecuteAsync(action);
        }
        catch (Exception ex)
        {
            // All retries exhausted - wrap in appropriate exception
            throw new DataProviderException($"Failed to execute {operationName} after 3 retries", ex);
        }
    }
}
