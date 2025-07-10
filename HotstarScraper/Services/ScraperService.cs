using HotstarScraper.Interfaces;
using HotstarScraper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using System.Collections.Concurrent;

namespace HotstarScraper.Services
{
    public class ScraperService : IScraperService
    {
        private readonly ILogger _logger;

        public ScraperService(ILogger logger)
        {
            _logger = logger;
        }

        public ChromeDriver GetConfiguredWebDriver()
        {
            var options = new ChromeOptions();

            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-setuid-sandbox");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return driver;
        }

        public ConcurrentBag<MovieScrapeData> ScrapeMovies(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://www.hotstar.com/in/movies");
            Thread.Sleep(3000);

            for (int i = 0; i < 3; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 2000);");
                Thread.Sleep(800);
            }

            var results = new ConcurrentBag<MovieScrapeData>();
            var contentCards = driver.FindElements(By.CssSelector("div[data-testid='action']"));
            var js = (IJavaScriptExecutor)driver;
            var seenTitles = new HashSet<string>();

            foreach (var card in contentCards)
            {
                string title = null;

                try
                {
                    title = GetTitle(card);

                    if (string.IsNullOrWhiteSpace(title) || seenTitles.Contains(title))
                        continue;

                    _logger.Information("Processing movie: {Title}", title);
                    seenTitles.Add(title);

                    js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", card);
                    Thread.Sleep(200);

                    try
                    {
                        var clickable = card.FindElement(By.CssSelector("article"));
                        js.ExecuteScript("arguments[0].click();", clickable);
                    }
                    catch
                    {
                        _logger.Warning("Unable to click on movie card: {Title}", title);
                        continue;
                    }

                    Thread.Sleep(1500);

                    var initialLanguage = GetMovieLanguage(driver);
                    var (year, rating, duration, singleLanguage) = GetMovieInfoBlock(driver);

                    var movie = new MovieScrapeData
                    {
                        Title = title,
                        Description = GetDescription(driver, js),
                        Genres = GetGenres(driver),
                        ReleaseYear = year,
                        Rating = rating,
                        Duration = duration,
                        Language = !string.IsNullOrWhiteSpace(initialLanguage) ? initialLanguage : singleLanguage,
                        ImageUrl = GetImageUrl(driver)
                    };

                    results.Add(movie);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error scraping movie: {Title}", title ?? "Unknown");
                }
                finally
                {
                    CloseModal(js);
                    Thread.Sleep(500);
                }
            }

            return results;
        }

        public ConcurrentBag<ShowScrapeData> ScrapeShows(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://www.hotstar.com/in/shows");
            Thread.Sleep(3000);

            for (int i = 0; i < 3; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 2000);");
                Thread.Sleep(800);
            }

            var results = new ConcurrentBag<ShowScrapeData>();
            var contentCards = driver.FindElements(By.CssSelector("div[data-testid='action']"));
            var js = (IJavaScriptExecutor)driver;
            var seenTitles = new HashSet<string>();

            foreach (var card in contentCards)
            {
                string title = null;

                try
                {
                    title = GetTitle(card);

                    if (string.IsNullOrWhiteSpace(title) || seenTitles.Contains(title))
                        continue;

                    _logger.Information("Processing show: {Title}", title);
                    seenTitles.Add(title);

                    js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", card);
                    Thread.Sleep(200);

                    try
                    {
                        var clickable = card.FindElement(By.CssSelector("article"));
                        js.ExecuteScript("arguments[0].click();", clickable);
                    }
                    catch
                    {
                        _logger.Warning("Unable to click on show card: {Title}", title);
                        continue;
                    }

                    Thread.Sleep(1500);

                    var (year, rating, seasons) = GetShowInfoBlock(driver);

                    var show = new ShowScrapeData
                    {
                        Title = title,
                        Description = GetDescription(driver, js),
                        Genres = GetGenres(driver),
                        ReleaseYear = year,
                        Rating = rating,
                        Season = seasons,
                        ImageUrl = GetImageUrl(driver)
                    };

                    results.Add(show);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error scraping show: {Title}", title ?? "Unknown");
                }
                finally
                {
                    CloseModal(js);
                    Thread.Sleep(500);
                }
            }

            return results;
        }

        private string GetTitle(IWebElement card)
        {
            var label = card.GetAttribute("aria-label");
            return label?.Split(',')[0]?.Trim() ?? string.Empty;
        }

        private static string GetDescription(IWebDriver driver, IJavaScriptExecutor js)
        {
            try
            {
                var element = driver.FindElement(By.CssSelector("div._1SQXlCXyLucI91Ny_sWM9q > p._1zc788KtPN0EmaoSx7RUA_"));
                if (!string.IsNullOrWhiteSpace(element.Text))
                    return element.Text.Trim();
            }
            catch { }

            try
            {
                var script = @"
            var el = document.querySelector('div._1SQXlCXyLucI91Ny_sWM9q > p._1zc788KtPN0EmaoSx7RUA_');
            return el ? el.textContent.trim() : '';
        ";
                var result = js.ExecuteScript(script) as string;
                if (!string.IsNullOrWhiteSpace(result))
                    return result.Trim();
            }
            catch { }

            return null;
        }

        private (string Year, string Rating, string Duration, string language) GetMovieInfoBlock(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var raw = js.ExecuteScript(@"
            var info = document.querySelector('div[aria-label*=""Release Year""]');
            return info ? info.getAttribute('aria-label') : '';
        ") as string;

                var year = "";
                var rating = "";
                var durationInMinutes = 0;
                var language = "";

                var parts = raw.Split(',').Select(x => x.Trim());

                foreach (var part in parts)
                {
                    if (part.StartsWith("Release Year:")) year = part.Replace("Release Year:", "").Trim();
                    else if (part.StartsWith("Age Rating:")) rating = part.Replace("Age Rating:", "").Trim();
                    else if (part.EndsWith("hours") || part.EndsWith("minutes"))
                    {
                        if (part.Contains("hours") && part.Contains("minutes"))
                        {
                            var hourParts = part.Split(new string[] { "hours" }, StringSplitOptions.RemoveEmptyEntries);
                            int hours = 0;
                            if (int.TryParse(hourParts[0].Trim(), out hours))
                            {
                                durationInMinutes += hours * 60;
                            }
                            var minutePart = hourParts.Length > 1 ? hourParts[1].Replace("minutes", "").Trim() : "";
                            int minutes = 0;
                            if (int.TryParse(minutePart, out minutes))
                            {
                                durationInMinutes += minutes;
                            }
                        }
                        else if (part.Contains("hours"))
                        {
                            var hourStr = part.Replace("hours", "").Trim();
                            int hours = 0;
                            if (int.TryParse(hourStr, out hours))
                            {
                                durationInMinutes += hours * 60;
                            }
                        }
                        else if (part.Contains("minutes"))
                        {
                            var minuteStr = part.Replace("minutes", "").Trim();
                            int minutes = 0;
                            if (int.TryParse(minuteStr, out minutes))
                            {
                                durationInMinutes += minutes;
                            }
                        }
                    }
                    else if (part.StartsWith("Language:")) language = part.Replace("Language:", "").Trim();
                }

                return (year, rating, durationInMinutes.ToString() + " Minutes", language);
            }
            catch
            {
                return (null, null, null, null);
            }
        }

        private (string Year, string Rating, string Seasons) GetShowInfoBlock(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var raw = js.ExecuteScript(@"
            var info = document.querySelector('div[aria-label*=""Release Year""]');
            return info ? info.getAttribute('aria-label') : '';
        ") as string;

                var year = "";
                var rating = "";
                var seasons = "";

                var parts = raw.Split(',').Select(x => x.Trim());

                foreach (var part in parts)
                {
                    if (part.StartsWith("Release Year:")) year = part.Replace("Release Year:", "").Trim();
                    else if (part.StartsWith("Age Rating:")) rating = part.Replace("Age Rating:", "").Trim();
                    else if (part.EndsWith("Seasons") || part.EndsWith("Season")) seasons = part;
                }

                return (year, rating, seasons);
            }
            catch
            {
                return (null, null, null);
            }
        }

        private string GetMovieLanguage(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var result = js.ExecuteScript(@"
            var langContainer = document.querySelector('div[data-testid=""language-picker""]');
            if (!langContainer) return '';
            var spans = langContainer.querySelectorAll('span.BUTTON3_SEMIBOLD');
            return Array.from(spans).map(s => s.textContent.trim()).join('|||');
        ") as string;

                if (string.IsNullOrWhiteSpace(result))
                    return null;

                var languages = result.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(l => l.Trim())
                                      .ToList();

                if (languages.Count > 1)
                {
                    return string.Join(", ", languages);
                }

                var rawInfo = js.ExecuteScript(@"
            var info = document.querySelector('div[aria-label*=""Release Year""]');
            return info ? info.getAttribute('aria-label') : '';
        ") as string;

                if (!string.IsNullOrWhiteSpace(rawInfo) && rawInfo.Contains("Language:"))
                {
                    var parts = rawInfo.Split(',').Select(x => x.Trim());
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("Language:"))
                        {
                            return part.Replace("Language:", "").Trim();
                        }
                    }
                }

                return languages.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private string GetGenres(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;

                var genreScript = @"
                    var genreContainer = document.querySelector('div[data-testid=""tagFlipperEnriched""]') ||
                                         document.querySelector('div[data-testid=""tags-container""]'); // Added fallback for tags-container
                    if (genreContainer) {
                        var spans = genreContainer.querySelectorAll('span');
                        var genres = Array.from(spans).map(s => s.textContent.trim()).filter(t => t);
                        return [...new Set(genres)].join(', ');
                    }
                    return null;
                ";
                var genres = js.ExecuteScript(genreScript) as string;

                if (!string.IsNullOrEmpty(genres))
                {
                    return genres;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting movie genres using JavaScript DOM query.");
            }
            return null;
        }

        private string GetImageUrl(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var script = @"
            var imgEl = document.querySelector('div[data-testid=""autoplay-trailer-image-container""] img');
            return imgEl ? imgEl.src : null;
        ";
                var imageUrl = js.ExecuteScript(script) as string;

                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    return imageUrl.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting image URL.");
            }

            return null;
        }

        private void CloseModal(IJavaScriptExecutor js)
        {
            try
            {
                js.ExecuteScript("var btn = document.querySelector('i.icon-close'); if (btn) btn.click();");
                Thread.Sleep(500);
            }
            catch { }
        }
    }
}