using Flurl.Http.Configuration;
using PseRestApi.Core;
using PseRestApi.Core.Services.Pse;
using PseRestApi.Host;
using PseRestApi.Infrastructure;
using PseRestApi.Infrastructure.Persistence;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PseApiOptions>(builder.Configuration.GetSection(PseApiOptions.ConfigSectionName));
builder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPseClient();
builder.Services.AddDataSyncServices();

builder.Services.AddRateLimiter(options =>
{
    var rateLimitConfig = builder.Configuration.GetSection("RateLimitConfig").Get<RateLimitConfig>();
    rateLimitConfig ??= new RateLimitConfig()
    {
        PermitLimit = 10,
        WindowInMinutes = 1
    };
    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = rateLimitConfig.PermitLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(rateLimitConfig.WindowInMinutes)
            })),
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new ConcurrencyLimiterOptions
            {
                PermitLimit = rateLimitConfig.PermitLimit,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }))
        );
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). ", cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Too many requests. Please try again later. ", cancellationToken: token);
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();