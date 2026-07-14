using AwesomeAssertions;
using PseRestApi.Core.Services.DataSync;
using Xunit;

namespace PseRestApi.Test.UnitTests.DataSync;

public class DataSyncExceptionTests
{
    [Fact]
    public void DataSyncException_WithMessage_CreatesInstance()
    {
        // Arrange & Act
        var exception = new TestDataSyncException("Test error message");

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error message");
    }

    [Fact]
    public void DataSyncException_WithMessageAndInnerException_CreatesInstance()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TestDataSyncException("Test error", innerException);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void DataProviderException_InheritsFromDataSyncException()
    {
        // Arrange & Act
        var exception = new DataProviderException("Provider error");

        // Assert
        exception.Should().BeAssignableTo<DataSyncException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void StagingException_InheritsFromDataSyncException()
    {
        // Arrange & Act
        var exception = new StagingException("Staging error");

        // Assert
        exception.Should().BeAssignableTo<DataSyncException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void SyncStrategyException_InheritsFromDataSyncException()
    {
        // Arrange & Act
        var exception = new SyncStrategyException("Strategy error");

        // Assert
        exception.Should().BeAssignableTo<DataSyncException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void BulkCopyException_InheritsFromDataSyncException()
    {
        // Arrange & Act
        var exception = new BulkCopyException("Bulk copy error");

        // Assert
        exception.Should().BeAssignableTo<DataSyncException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void OrchestrationException_InheritsFromDataSyncException()
    {
        // Arrange & Act
        var exception = new OrchestrationException("Orchestration error");

        // Assert
        exception.Should().BeAssignableTo<DataSyncException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    // Helper class to test abstract base
    private sealed class TestDataSyncException : DataSyncException
    {
        public TestDataSyncException(string message) : base(message) { }
        public TestDataSyncException(string message, Exception innerException) : base(message, innerException) { }
    }
}
