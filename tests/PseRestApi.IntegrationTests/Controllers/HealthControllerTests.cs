using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace PseRestApi.IntegrationTests.Controllers;

public class HealthControllerTests : BaseIntegrationTest
{
    public HealthControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetHealth_ReturnsServiceUnavailableWhenDatabaseIsNotConfigured()
    {
        // Act
        using var response = await _httpClient.GetAsync("health", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);

        var body = await response.Content.ReadFromJsonAsync<JsonDocument>(_cancellationToken);
        body.Should().NotBeNull();
        body!.RootElement.GetProperty("status").GetString().Should().Be("unhealthy");
    }

    [Fact]
    public async Task GetDetailedHealth_ReturnsServiceUnavailableWhenDatabaseIsNotConfigured()
    {
        // Act
        using var response = await _httpClient.GetAsync("health/detailed", _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetHealth_EndpointExistsAndIsReachable()
    {
        // Act
        using var response = await _httpClient.GetAsync("health", _cancellationToken);

        // Assert - the endpoint should respond (even if unhealthy without a DB)
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeFalse(); // expected since no DB configured
    }
}
