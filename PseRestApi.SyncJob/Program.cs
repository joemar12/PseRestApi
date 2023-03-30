
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Storage;

var builder = new HostBuilder();
builder.ConfigureWebJobs(b =>
{
    b.AddAzureStorageCoreServices();
    b.AddAzureStorage();
});
builder.ConfigureLogging((context, b) =>
{
    b.AddConsole();
});
var host = builder.Build();
using (host)
{
    await host.RunAsync();
}