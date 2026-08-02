using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class SongConfiguration : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.FileName).IsRequired().HasMaxLength(300);
        builder.Property(s => s.CoverImageUrl).HasMaxLength(500);
        builder.Property(s => s.RequiredPackage).HasConversion<int>();

        builder.HasOne(s => s.Artist)
            .WithMany(a => a.Songs)
            .HasForeignKey(s => s.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Album)
            .WithMany(a => a.Songs)
            .HasForeignKey(s => s.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.RequiredPackage);
        builder.HasIndex(s => s.Title);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}