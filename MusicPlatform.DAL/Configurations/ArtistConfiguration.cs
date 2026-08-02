using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Bio).HasMaxLength(2000);
        builder.Property(a => a.ImageUrl).HasMaxLength(500);
        builder.Property(a => a.Country).HasMaxLength(100);

        builder.HasIndex(a => a.Name);
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}