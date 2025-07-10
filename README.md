
# HotstarScraper

This project is a C# application designed to scrape movie and show data from Hotstar and store it in a SQL Server database.

## Screenshots

## Run Locally

Clone the project

```bash
git clone https://github.com/ishansarvaiya/HotstarScraper.git
```

Go to the project directory

```bash
cd HotstarScraper
```

Install dependencies

```bash
dotnet restore
```

Start the server

```bash
dotnet run
```


## Features

- Web Scraping: Utilizes Selenium with ChromeDriver to navigate Hotstar, scroll through content, click on items, and extract details like titles, descriptions, release years, ratings, durations/seasons, languages, genres, and image URLs for both movies and shows.
- Data Persistence: Employs Entity Framework Core to interact with a SQL Server database.
- Database Schema: Defines models for Movie, Show, Genre, Language, and their many-to-many relationships (MovieGenre, ShowGenre, MovieLanguage).
- Data Handling: Includes a DataService responsible for saving scraped data, checking for existing entries, and managing relationships with genres and languages, adding new ones if they don't exist.
- Logging: Integrates Serilog for comprehensive logging of application activities, including information messages, warnings, and errors, with output directed to both console and a daily rolling file.
- Configuration: Reads database connection strings and Serilog settings from an appsettings.json file.
- Headless Browse: Configures ChromeDriver to run in headless mode for efficient scraping without a visible browser UI.
## Structure

- Program.cs: The main entry point of the application, handling setup, database migrations, and orchestrating the scraping and data saving processes.
- HotstarDbContext.cs: The Entity Framework Core DbContext for interacting with the database, defining DbSet properties for all models.
- Data/Configurations: Contains EF Core fluent API configurations for each model, defining primary keys, required properties, maximum lengths, and unique indexes.
- Interfaces: Defines IDataService and IScraperService interfaces for abstraction.
- Models: Contains the POCO classes representing the data entities (Movie, Show, Genre, Language) and data transfer objects for scraped data (MovieScrapeData, ShowScrapeData).
- Services/DataService.cs: Implements IDataService, responsible for saving scraped movie and show data to the database, including handling associated genres and languages.
- Services/ScraperService.cs: Implements IScraperService, containing the core logic for configuring the web driver, navigating Hotstar, and extracting data for movies and shows.
- appsettings.json: Configuration file for database connection strings and Serilog settings.
## Database Schema

- Movies (Id: INT (Primary Key), Title: NVARCHAR(255) (Required), Description: NVARCHAR(4000), ReleaseYear: NVARCHAR(10), Rating: NVARCHAR(10), Duration: NVARCHAR(50), ImageUrl: NVARCHAR(MAX))
- Shows (Id: INT (Primary Key), Title: NVARCHAR(255) (Required), Description: NVARCHAR(4000), ReleaseYear: NVARCHAR(10), Rating: NVARCHAR(10), Season: NVARCHAR(50), ImageUrl: NVARCHAR(MAX))
- Genres (Id: INT (Primary Key), Name: NVARCHAR(255) (Required))
- Languages (Id: INT (Primary Key), Name: NVARCHAR(255) (Required))
- MovieGenres (MovieId: INT (Primary Key, Foreign Key to Movies), GenreId (Primary Key, Foreign Key to Genres))
- MovieLanguages (MovieId: INT (Primary Key, Foreign Key to Movies), LanguageId (Primary Key, Foreign Key to Languages))
- ShowGenres (ShowId: INT (Primary Key, Foreign Key to Shows), GenreId (Primary Key, Foreign Key to Genres))
## NuGet Packages

- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Extensions.Configuration.Json
- Selenium.WebDriver
- Selenium.WebDriver.ChromeDriver
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Process
- Serilog.Enrichers.Thread
- Serilog.Settings.Configuration
- Serilog.Sinks.Console
- Serilog.Sinks.File
