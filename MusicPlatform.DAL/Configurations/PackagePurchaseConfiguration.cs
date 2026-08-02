using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class PackagePurchaseConfiguration : IEntityTypeConfiguration<PackagePurchase>
{
    public void Configure(EntityTypeBuilder<PackagePurchase> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.PackageLevel).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.TransactionReference).IsRequired().HasMaxLength(60);

        builder.HasOne(p => p.User)
            .WithMany(u => u.PackagePurchases)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TransactionReference).IsUnique();
        builder.HasQueryFilter(p => !p.IsDeleted && !p.User.IsDeleted);
    }
}