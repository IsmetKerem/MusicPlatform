using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicPlatform.DAL.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();


        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=MusicPlatformDb;User Id=sa;Password=Kerembaba44!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False");

        return new AppDbContext(optionsBuilder.Options);
    }
} 