namespace MusicPlatform.Shared.DTOs.Package;

public class PackageInfoDto
{
    public int Level { get; set; }
    public string Name { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public string Description { get; set; } = null!;
    public List<string> Features { get; set; } = new();
    public int AccessibleSongCount { get; set; }
    public bool IsCurrent { get; set; }
    public bool CanUpgradeTo { get; set; }
}

public class PurchaseRequestDto
{
    public int TargetPackageLevel { get; set; }
    public int DurationInDays { get; set; } = 30;

    public string CardHolderName { get; set; } = null!;
    public string CardNumber { get; set; } = null!;
    public string ExpiryMonth { get; set; } = null!;
    public string ExpiryYear { get; set; } = null!;
    public string Cvv { get; set; } = null!;
}

public class PurchaseResultDto
{
    public string TransactionReference { get; set; } = null!;
    public string PackageName { get; set; } = null!;
    public decimal AmountPaid { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime ExpiresAt { get; set; }


    public bool RequiresTokenRefresh { get; set; } = true;
}