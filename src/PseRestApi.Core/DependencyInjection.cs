using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.Pse;
using System.Reflection;

namespace PseRestApi.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddPseClient(this IServiceCollection services)
    {
        services.AddAutoMapper(
            cfg => cfg.ShouldMapMethod = m => false,
            Assembly.GetExecutingAssembly());
        services.AddScoped<IPseClient, PseClient>();
        services.AddScoped<IPseApiService, PseApiService>();
        return services;
    }
}