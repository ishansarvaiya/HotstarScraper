using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotstarScraper.Data.Configurations
{
    public class MovieLanguageConfiguration : IEntityTypeConfiguration<MovieLanguage>
    {
        public void Configure(EntityTypeBuilder<MovieLanguage> builder)
        {
            builder.HasKey(ml => new { ml.MovieId, ml.LanguageId });
            builder.HasOne(ml => ml.Movie).WithMany(m => m.MovieLanguages).HasForeignKey(ml => ml.MovieId);
            builder.HasOne(ml => ml.Language).WithMany(l => l.MovieLanguages).HasForeignKey(ml => ml.LanguageId);
        }
    }
}