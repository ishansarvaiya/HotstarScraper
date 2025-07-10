using HotstarScraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Concurrent;

namespace HotstarScraper.Interfaces
{
    public interface IScraperService
    {
        ChromeDriver GetConfiguredWebDriver();

        ConcurrentBag<MovieScrapeData> ScrapeMovies(IWebDriver driver);

        ConcurrentBag<ShowScrapeData> ScrapeShows(IWebDriver driver);
    }
}