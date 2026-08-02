using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class SongGenreConfiguration : IEntityTypeConfiguration<SongGenre>
{
    public void Configure(EntityTypeBuilder<SongGenre> builder)
    {
        builder.HasKey(sg => new { sg.SongId, sg.GenreId });

        builder.HasOne(sg => sg.Song)
            .WithMany(s => s.SongGenres)
            .HasForeignKey(sg => sg.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sg => sg.Genre)
            .WithMany(g => g.SongGenres)
            .HasForeignKey(sg => sg.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(sg => !sg.Song.IsDeleted && !sg.Genre.IsDeleted);
    }
}