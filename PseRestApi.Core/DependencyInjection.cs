using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Mappers;
using PseRestApi.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
