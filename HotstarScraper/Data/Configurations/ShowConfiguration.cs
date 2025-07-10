using HotstarScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotstarScraper.Data.Configurations
{
    public class ShowConfiguration : IEntityTypeConfiguration<Show>
    {
        public void Configure(EntityTypeBuilder<Show> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Title).IsRequired().HasMaxLength(255);
            builder.Property(s => s.Description).HasMaxLength(4000);
            builder.Property(s => s.ReleaseYear).HasMaxLength(10);
            builder.Property(s => s.Rating).HasMaxLength(10);
            builder.Property(s => s.Season).HasMaxLength(50);
        }
    }
}