namespace HotstarScraper.Models
{
    public class ShowGenre
    {
        public int ShowId { get; set; }
        public Show Show { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}