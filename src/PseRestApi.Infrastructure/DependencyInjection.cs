using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Infrastructure.Persistence;
using PseRestApi.Infrastructure.Persistence.Interceptors;
using PseRestApi.Infrastructure.Services;

namespace PseRestApi.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnectionString") ?? string.Empty,
                builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IAppDbContext>(
            provider => provider.GetRequiredService<AppDbContext>());

        services.AddTransient<IDateTime, DateTimeService>();
        return services;
    }
}
