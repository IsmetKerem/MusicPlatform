using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(80);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(80);
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(500);
        builder.Property(u => u.PackageLevel).HasConversion<int>();

        builder.Ignore(u => u.FullName);

        builder.HasIndex(u => u.PackageLevel);
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}