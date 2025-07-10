using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotstarScraper.Data.Configurations
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(255);
            builder.Property(m => m.Description).HasMaxLength(4000);
            builder.Property(m => m.ReleaseYear).HasMaxLength(10);
            builder.Property(m => m.Rating).HasMaxLength(10);
            builder.Property(m => m.Duration).HasMaxLength(50);
        }
    }
}