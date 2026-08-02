using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(80);
        builder.Property(g => g.Description).HasMaxLength(500);
        builder.Property(g => g.ColorHex).HasMaxLength(7);

        builder.HasIndex(g => g.Name).IsUnique();
        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}