namespace HotstarScraper.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReleaseYear { get; set; }
        public string Rating { get; set; }
        public string Season { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<ShowGenre> ShowGenres { get; set; }
    }
}