namespace HotstarScraper.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReleaseYear { get; set; }
        public string Rating { get; set; }
        public string Duration { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<MovieGenre> MovieGenres { get; set; }
        public ICollection<MovieLanguage> MovieLanguages { get; set; }
    }
}