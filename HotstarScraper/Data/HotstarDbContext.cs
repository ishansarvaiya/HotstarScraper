using HotstarScraper.Data.Configurations;
using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;

namespace HotstarScraper.Data
{
    public class HotstarDbContext : DbContext
    {
        public HotstarDbContext(DbContextOptions<HotstarDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<ShowGenre> ShowGenres { get; set; }
        public DbSet<MovieLanguage> MovieLanguages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new MovieConfiguration());
            modelBuilder.ApplyConfiguration(new ShowConfiguration());
            modelBuilder.ApplyConfiguration(new GenreConfiguration());
            modelBuilder.ApplyConfiguration(new LanguageConfiguration());
            modelBuilder.ApplyConfiguration(new MovieGenreConfiguration());
            modelBuilder.ApplyConfiguration(new ShowGenreConfiguration());
            modelBuilder.ApplyConfiguration(new MovieLanguageConfiguration());
        }
    }
}