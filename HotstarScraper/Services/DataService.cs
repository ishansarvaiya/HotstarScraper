using HotstarScraper.Data;
using HotstarScraper.Interfaces;
using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HotstarScraper.Services
{
    public class DataService : IDataService
    {
        private readonly HotstarDbContext _dbContext;
        private readonly ILogger _logger;

        public DataService(HotstarDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveMoviesAsync(IEnumerable<MovieScrapeData> movieDataList)
        {
            int savedCount = 0;

            var genreDict = await _dbContext.Genres.ToDictionaryAsync(g => g.Name.ToLower());
            var languageDict = await _dbContext.Languages.ToDictionaryAsync(l => l.Name.ToLower());

            foreach (var movieData in movieDataList)
            {
                try
                {
                    var movie = await GetOrCreateMovieAsync(movieData);

                    if (_dbContext.Entry(movie).State == EntityState.Added)
                    {
                        await _dbContext.SaveChangesAsync();
                        _logger.Information("Added new movie: {Title}", movie.Title);
                    }
                    else
                    {
                        _logger.Information("Updating existing movie: {Title}", movie.Title);
                    }

                    await HandleGenresAsync(movie, movieData.Genres, genreDict);
                    await HandleLanguagesAsync(movie, movieData.Language, languageDict);
                    await _dbContext.SaveChangesAsync();

                    savedCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error saving movie {Title} to database", movieData.Title);
                }
            }

            _logger.Information("Saved {Count} movies to the database.", savedCount);
        }

        private async Task<Movie> GetOrCreateMovieAsync(MovieScrapeData data)
        {
            var movie = await _dbContext.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieLanguages)
                    .ThenInclude(ml => ml.Language)
                .FirstOrDefaultAsync(m => m.Title == data.Title);

            if (movie == null)
            {
                movie = new Movie
                {
                    Title = data.Title,
                    Description = data.Description,
                    ReleaseYear = data.ReleaseYear,
                    Rating = data.Rating,
                    Duration = data.Duration,
                    ImageUrl = data.ImageUrl,
                    MovieGenres = new List<MovieGenre>(),
                    MovieLanguages = new List<MovieLanguage>()
                };

                _dbContext.Movies.Add(movie);
            }
            else
            {
                movie.Description = data.Description;
                movie.ReleaseYear = data.ReleaseYear;
                movie.Rating = data.Rating;
                movie.Duration = data.Duration;
                movie.ImageUrl = data.ImageUrl;
            }

            return movie;
        }

        public async Task SaveShowsAsync(IEnumerable<ShowScrapeData> showDataList)
        {
            int savedCount = 0;

            var genreDict = await _dbContext.Genres.ToDictionaryAsync(g => g.Name.ToLower());

            foreach (var showData in showDataList)
            {
                try
                {
                    var show = await GetOrCreateShowAsync(showData);

                    if (_dbContext.Entry(show).State == EntityState.Added)
                    {
                        await _dbContext.SaveChangesAsync();
                        _logger.Information("Added new show: {Title}", show.Title);
                    }
                    else
                    {
                        _logger.Information("Updating existing show: {Title}", show.Title);
                    }

                    await HandleShowGenresAsync(show, showData.Genres, genreDict);
                    await _dbContext.SaveChangesAsync();

                    savedCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error saving show {Title} to database", showData.Title);
                }
            }

            _logger.Information("Saved {Count} shows to the database.", savedCount);
        }

        private async Task<Show> GetOrCreateShowAsync(ShowScrapeData data)
        {
            var show = await _dbContext.Shows
                .Include(s => s.ShowGenres)
                    .ThenInclude(sg => sg.Genre)
                .FirstOrDefaultAsync(s => s.Title == data.Title);

            if (show == null)
            {
                show = new Show
                {
                    Title = data.Title,
                    Description = data.Description,
                    ReleaseYear = data.ReleaseYear,
                    Rating = data.Rating,
                    Season = data.Season,
                    ImageUrl = data.ImageUrl,
                    ShowGenres = new List<ShowGenre>()
                };

                _dbContext.Shows.Add(show);
            }
            else
            {
                show.Description = data.Description;
                show.ReleaseYear = data.ReleaseYear;
                show.Rating = data.Rating;
                show.Season = data.Season;
                show.ImageUrl = data.ImageUrl;
            }

            return show;
        }

        private async Task HandleGenresAsync(Movie movie, string genreStringInput, Dictionary<string, Genre> genreDict)
        {
            if (string.IsNullOrWhiteSpace(genreStringInput) || genreStringInput == "(not found)")
            {
                if (movie.MovieGenres.Any())
                {
                    movie.MovieGenres.Clear();
                    _logger.Information("Removed all genres from movie {MovieTitle}", movie.Title);
                }
                return;
            }

            var incomingGenreNames = new HashSet<string>(genreStringInput.Split(',').Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g)), StringComparer.OrdinalIgnoreCase);
            var currentGenreIds = new HashSet<int>(movie.MovieGenres.Select(mg => mg.GenreId));

            var genresToRemove = movie.MovieGenres
                .Where(mg => mg.Genre != null && !incomingGenreNames.Contains(mg.Genre.Name))
                .ToList();

            foreach (var mg in genresToRemove)
            {
                movie.MovieGenres.Remove(mg);
                _logger.Information("Unlinked genre {GenreName} from movie {MovieTitle}", mg.Genre.Name, movie.Title);
            }

            foreach (var genreName in incomingGenreNames)
            {
                var key = genreName.ToLower();
                if (!genreDict.TryGetValue(key, out var genre))
                {
                    genre = new Genre { Name = genreName };
                    _dbContext.Genres.Add(genre);
                    await _dbContext.SaveChangesAsync();
                    genreDict[key] = genre;
                    _logger.Information("Added new genre: {GenreName}", genre.Name, genre.Id);
                }

                if (!currentGenreIds.Contains(genre.Id))
                {
                    movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genre.Id });
                    _logger.Information("Linked genre {GenreName} to movie {Title}", genre.Name, movie.Title);
                }
            }
        }

        private async Task HandleLanguagesAsync(Movie movie, string languageStringInput, Dictionary<string, Language> languageDict)
        {
            if (string.IsNullOrWhiteSpace(languageStringInput) || languageStringInput == "(not found)")
            {
                if (movie.MovieLanguages.Any())
                {
                    movie.MovieLanguages.Clear();
                    _logger.Information("Removed all languages from movie {MovieTitle}", movie.Title);
                }
                return;
            }

            var desiredLanguageName = languageStringInput.Trim();

            if (string.IsNullOrEmpty(desiredLanguageName))
            {
                if (movie.MovieLanguages.Any())
                {
                    movie.MovieLanguages.Clear();
                    _logger.Information("Removed all languages from movie {MovieTitle}", movie.Title);
                }
                return;
            }

            var key = desiredLanguageName.ToLower();
            Language targetLanguage;

            if (!languageDict.TryGetValue(key, out targetLanguage))
            {
                targetLanguage = new Language { Name = desiredLanguageName };
                _dbContext.Languages.Add(targetLanguage);
                await _dbContext.SaveChangesAsync();
                languageDict[key] = targetLanguage;
                _logger.Information("Added new language: {LanguageName}", targetLanguage.Name, targetLanguage.Id);
            }

            var currentPrimaryLanguage = movie.MovieLanguages.FirstOrDefault();
            if (currentPrimaryLanguage == null || currentPrimaryLanguage.LanguageId != targetLanguage.Id)
            {
                movie.MovieLanguages.Clear();
                movie.MovieLanguages.Add(new MovieLanguage { MovieId = movie.Id, LanguageId = targetLanguage.Id });
                _logger.Information("Set primary language to {LanguageName} for movie {Title}", targetLanguage.Name, movie.Title);
            }
        }

        private async Task HandleShowGenresAsync(Show show, string genreStringInput, Dictionary<string, Genre> genreDict)
        {
            if (string.IsNullOrWhiteSpace(genreStringInput) || genreStringInput == "(not found)")
            {
                if (show.ShowGenres.Any())
                {
                    show.ShowGenres.Clear();
                    _logger.Information("Removed all genres from show {ShowTitle}", show.Title);
                }
                return;
            }

            var incomingGenreNames = new HashSet<string>(genreStringInput.Split(',').Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g)), StringComparer.OrdinalIgnoreCase);
            var currentGenreIds = new HashSet<int>(show.ShowGenres.Select(sg => sg.GenreId));

            var genresToRemove = show.ShowGenres
                .Where(sg => sg.Genre != null && !incomingGenreNames.Contains(sg.Genre.Name))
                .ToList();

            foreach (var sg in genresToRemove)
            {
                show.ShowGenres.Remove(sg);
                _logger.Information("Unlinked genre {GenreName} from show {ShowTitle}", sg.Genre.Name, show.Title);
            }

            foreach (var genreName in incomingGenreNames)
            {
                var key = genreName.ToLower();
                if (!genreDict.TryGetValue(key, out var genre))
                {
                    genre = new Genre { Name = genreName };
                    _dbContext.Genres.Add(genre);
                    await _dbContext.SaveChangesAsync();
                    genreDict[key] = genre;
                    _logger.Information("Added new genre: {GenreName}", genre.Name, genre.Id);
                }

                if (!currentGenreIds.Contains(genre.Id))
                {
                    show.ShowGenres.Add(new ShowGenre { ShowId = show.Id, GenreId = genre.Id });
                    _logger.Information("Linked genre {GenreName} to show {Title}", genre.Name, show.Title);
                }
            }
        }
    }
}