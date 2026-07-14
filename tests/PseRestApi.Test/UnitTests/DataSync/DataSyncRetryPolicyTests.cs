using AwesomeAssertions;
using PseRestApi.Core.Services.DataSync;
using Xunit;

namespace PseRestApi.Test.UnitTests.DataSync;

public class DataSyncRetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution_ReturnsResult()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            return 42;
        };

        // Act
        var result = await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        result.Should().Be(42);
        callCount.Should().Be(1); // Should succeed on first try
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailure_RetriesAndSucceeds()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new HttpRequestException("Transient network error");
            }
            return "success";
        };

        // Act
        var result = await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        result.Should().Be("success");
        callCount.Should().Be(3); // Should succeed on 3rd attempt
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailure_ExhaustsRetries_ThrowsDataProviderException()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            throw new HttpRequestException("Persistent network error");
        };

        // Act
        Func<Task> act = async () => await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        Exception? caughtException = null;
        try
        {
            await act();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<DataProviderException>()
            .Which.Message.Should().Contain("test operation");
        caughtException.InnerException.Should().BeOfType<HttpRequestException>();
        callCount.Should().BeGreaterThan(1); // Should have retried
    }

    [Fact]
    public async Task ExecuteAsync_NonTransientFailure_WrapsInDataProviderException()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            throw new ArgumentException("Invalid argument");
        };

        // Act
        Exception? caughtException = null;
        try
        {
            await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().BeOfType<DataProviderException>();
        caughtException.InnerException.Should().BeOfType<ArgumentException>();
        callCount.Should().Be(1); // Should not retry non-transient errors
    }

    [Fact]
    public async Task ExecuteAsync_VoidAction_SuccessfulExecution()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            // void action
        };

        // Act
        await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_VoidAction_TransientFailure_Retries()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new TimeoutException("Timeout");
            }
        };

        // Act
        await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutException_Retries()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new TimeoutException("Operation timed out");
            }
            return true;
        };

        // Act
        var result = await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        result.Should().BeTrue();
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_DbUpdateException_Retries()
    {
        // Arrange
        int callCount = 0;
        var action = async () =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new Microsoft.EntityFrameworkCore.DbUpdateException("DB error");
            }
            return true;
        };

        // Act
        var result = await DataSyncRetryPolicy.ExecuteAsync(action, "test operation");

        // Assert
        result.Should().BeTrue();
        callCount.Should().Be(2);
    }
}
