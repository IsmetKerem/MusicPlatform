using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(f => new { f.UserId, f.SongId });

        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Song)
            .WithMany(s => s.Favorites)
            .HasForeignKey(f => f.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(f => !f.User.IsDeleted && !f.Song.IsDeleted);
    }
}