using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotstarScraper.Data.Configurations
{
    public class ShowGenreConfiguration : IEntityTypeConfiguration<ShowGenre>
    {
        public void Configure(EntityTypeBuilder<ShowGenre> builder)
        {
            builder.HasKey(sg => new { sg.ShowId, sg.GenreId });
            builder.HasOne(sg => sg.Show).WithMany(s => s.ShowGenres).HasForeignKey(sg => sg.ShowId);
            builder.HasOne(sg => sg.Genre).WithMany(g => g.ShowGenres).HasForeignKey(sg => sg.GenreId);
        }
    }
}