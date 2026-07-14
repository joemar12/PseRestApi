using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PseRestApi.IntegrationTests;

/// <summary>
/// Base class for integration tests. Provides an <see cref="HttpClient"/>
/// configured to talk to the test server.
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<TestWebApplicationFactory>
{
    protected readonly HttpClient _httpClient;
    protected readonly CancellationToken _cancellationToken;

    protected BaseIntegrationTest(TestWebApplicationFactory factory)
    {
        _cancellationToken = TestContext.Current.CancellationToken;
        // Disable automatic redirects so we can assert on 3xx responses if needed.
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}
