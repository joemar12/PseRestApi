using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PseRestApi.Core;
using PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;
using PseRestApi.Core.Services.DataSync.SecurityInfoSync;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Infrastructure;

try
{
    var host = Host.CreateDefaultBuilder(args)
        //.ConfigureHostConfiguration(configurationBuilder => {
        //    configurationBuilder.AddCommandLine(args);
        //})
        .ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.Sources.Clear();
            var env = context.HostingEnvironment;
            configuration
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
        })
        .ConfigureServices((context, services) =>
        {
            services.Configure<PseApiOptions>(context.Configuration.GetSection(PseApiOptions.ConfigSectionName));
            services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
            services.AddInfrastructure(context.Configuration);
            services.AddPseClient();
            services.AddDataSyncServices();
        })
        .ConfigureLogging((context, loggingBuilder) =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        })
        .Build();

    using var serviceScope = host.Services.CreateScope();
    var securityInfoSyncService = serviceScope.ServiceProvider.GetService<ISecurityInfoDataSyncService>();
    var historicalTradingDataSyncService = serviceScope.ServiceProvider.GetService<IHistoricalTradingDataSyncService>();
    if (securityInfoSyncService != null)
    {
        await securityInfoSyncService.Sync();
        if (historicalTradingDataSyncService != null)
        {
            await historicalTradingDataSyncService.Sync();
        }
    }
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync(ex.Message);
}