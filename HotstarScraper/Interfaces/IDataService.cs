using HotstarScraper.Models;

namespace HotstarScraper.Interfaces
{
    public interface IDataService
    {
        Task SaveMoviesAsync(IEnumerable<MovieScrapeData> movieDataList);

        Task SaveShowsAsync(IEnumerable<ShowScrapeData> showDataList);
    }
}