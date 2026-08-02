using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.DAL.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ToEmail).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.TemplateName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.IsSent);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}