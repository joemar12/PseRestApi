using Flurl.Http.Configuration;
using PseRestApi.Core;
using PseRestApi.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PseApiOptions>(builder.Configuration.GetSection(PseApiOptions.ConfigSectionName));
builder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
builder.Services.AddPseClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();