using HotstarScraper.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class HotstarDbContextFactory : IDesignTimeDbContextFactory<HotstarDbContext>
{
    public HotstarDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HotstarDbContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection connection string not found in appsettings.json. Please ensure it's configured for design-time operations.");
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new HotstarDbContext(optionsBuilder.Options);
    }
}