namespace HotstarScraper.Models
{
    public class MovieScrapeData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReleaseYear { get; set; }
        public string Rating { get; set; }
        public string Duration { get; set; }
        public string Language { get; set; }
        public string Genres { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ShowScrapeData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReleaseYear { get; set; }
        public string Rating { get; set; }
        public string Season { get; set; }
        public string Genres { get; set; }
        public string ImageUrl { get; set; }
    }
}