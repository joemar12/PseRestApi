using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Mappers;
using PseRestApi.Core.Services;

namespace PseRestApi.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPseClient(this IServiceCollection services)
        {
            services.AddTransient<IPseClient, PseClient>();
            services.AddAutoMapper(typeof(StockDtoMappingProfile));
            services.AddScoped<IPseApiService, PseApiService>();
            return services;
        }
    }
}