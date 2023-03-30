using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PseRestApi.Infrastructure.Persistence;

public class AppDbInitializer
{
    private readonly ILogger<AppDbInitializer> _logger;
    private readonly AppDbContext _context;

    public AppDbInitializer(ILogger<AppDbInitializer> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}