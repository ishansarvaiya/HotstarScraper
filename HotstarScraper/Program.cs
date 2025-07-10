using HotstarScraper.Data;
using HotstarScraper.Interfaces;
using HotstarScraper.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using Serilog;
using Serilog.Debugging;
using System.IO;

internal class Program
{
    private static IConfiguration _configuration;

    private static void Main(string[] args)
    {
        SelfLog.Enable(msg => Console.Error.WriteLine(msg));
        _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

        string currentDirectory = AppContext.BaseDirectory;
        string projectRootDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\.."));
        string logDirectory = Path.Combine(projectRootDirectory, "logs");

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
            Console.WriteLine($"Created log directory: {logDirectory}");
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration) // This line already configures the console sink from appsettings.json
            .WriteTo.File(
                Path.Combine(logDirectory, "hotstar-scraper-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            // Removed: .WriteTo.Console() 
            .CreateLogger();

        Log.Information("Hotstar Scraper application starting up.");

        var optionsBuilder = new DbContextOptionsBuilder<HotstarDbContext>();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

        using (var dbContext = new HotstarDbContext(optionsBuilder.Options))
        {
            try
            {
                dbContext.Database.Migrate();
                Log.Information("Database migrations applied.");

                IScraperService scraperService = new ScraperService(Log.Logger);
                IDataService dataService = new DataService(dbContext, Log.Logger);

                ChromeDriver driver = null;
                try
                {
                    driver = scraperService.GetConfiguredWebDriver();

                    var movies = scraperService.ScrapeMovies(driver);
                    dataService.SaveMoviesAsync(movies).Wait();
                    Log.Information($"Scraped and saved {movies.Count} movies.");

                    var shows = scraperService.ScrapeShows(driver);
                    dataService.SaveShowsAsync(shows).Wait();
                    Log.Information($"Scraped and saved {shows.Count} shows.");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Application terminated unexpectedly during scraping or data saving.");
                }
                finally
                {
                    driver?.Quit();
                    Log.CloseAndFlush();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly during database operations or initial setup.");
            }
        }

        Console.WriteLine("Scraping completed. Press any key to exit.");
        Console.ReadKey();
    }
}