using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MusicPlatform.Business.Extensions;
using MusicPlatform.Business.Options;
using MusicPlatform.DAL.Context;
using MusicPlatform.DAL.Seed;
using MusicPlatform.Entity.Concrete;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------ Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------------------------------------------------ Identity
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ----------------------------------------------------------------- JWT setup
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// -------------------------------------------------------------- App services
builder.Services.AddBusinessServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MusicPlatform API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Sadece token'ı yapıştırın, başına 'Bearer' yazmayın."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var context = sp.GetRequiredService<AppDbContext>();
    var userManager = sp.GetRequiredService<UserManager<AppUser>>();

    var musicFolder = Path.Combine(
        app.Environment.ContentRootPath,
        builder.Configuration["MusicSettings:MusicFolder"]!);

    var coverFolder = Path.Combine(
        app.Environment.ContentRootPath,
        builder.Configuration["MusicSettings:CoverFolder"]!);

    // ======================= GEÇİCİ TEŞHİS =======================
    Console.WriteLine("==================================================");
    Console.WriteLine($"ContentRootPath : {app.Environment.ContentRootPath}");
    Console.WriteLine($"musicFolder     : {musicFolder}");
    Console.WriteLine($"Klasor var mi   : {Directory.Exists(musicFolder)}");

    if (Directory.Exists(musicFolder))
    {
        var files = Directory.GetFiles(musicFolder, "*.mp3");
        Console.WriteLine($"MP3 sayisi      : {files.Length}");

        if (files.Length > 0)
        {
            Console.WriteLine($"Ilk dosya       : {Path.GetFileName(files[0])}");
            try
            {
                using var tag = TagLib.File.Create(files[0]);
                Console.WriteLine($"TagLib sure     : {(int)tag.Properties.Duration.TotalSeconds} sn");
                Console.WriteLine($"Gomulu kapak    : {tag.Tag.Pictures.Length} adet");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TagLib HATASI   : {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
    else
    {
        var parent = Directory.GetParent(musicFolder)?.FullName;
        Console.WriteLine($"Ust klasor      : {parent}");
        if (parent is not null && Directory.Exists(parent))
        {
            Console.WriteLine("Ust klasordeki icerik:");
            foreach (var d in Directory.GetFileSystemEntries(parent))
                Console.WriteLine($"   - {Path.GetFileName(d)}");
        }
    }
    Console.WriteLine("==================================================");
    // ===================== TESHIS SONU ===========================

    // await DbSeeder.SeedAsync(context, userManager, musicFolder, coverFolder);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();