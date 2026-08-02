using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(200);
        builder.Property(r => r.CreatedByIp).HasMaxLength(60);

        builder.Ignore(r => r.IsActive);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}