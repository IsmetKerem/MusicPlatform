using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class ListeningHistoryConfiguration : IEntityTypeConfiguration<ListeningHistory>
{
    public void Configure(EntityTypeBuilder<ListeningHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.User)
            .WithMany(u => u.ListeningHistories)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Song)
            .WithMany(s => s.ListeningHistories)
            .HasForeignKey(h => h.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.UserId, h.SongId });
        builder.HasIndex(h => h.ListenedAt);

        builder.HasQueryFilter(h => !h.IsDeleted && !h.User.IsDeleted && !h.Song.IsDeleted);
    }
}