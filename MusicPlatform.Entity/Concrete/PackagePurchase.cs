using MusicPlatform.Entity.Common;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Entity.Concrete;

public class PackagePurchase : BaseEntity
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public PackageLevel PackageLevel { get; set; }
    public decimal Price { get; set; }
    public int DurationInDays { get; set; } = 30;

    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string TransactionReference { get; set; } = null!;
}