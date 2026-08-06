using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicPlatform.Business.ML;
using MusicPlatform.Business.Options;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Business.Services.Concrete;

namespace MusicPlatform.Business.Extensions;

public static class BusinessServiceRegistration
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPackageAuthorizationService, PackageAuthorizationService>();
        services.AddScoped<IStreamService, StreamService>();
        services.AddScoped<ISongService, SongService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IAlbumService, AlbumService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IPlaylistService, PlaylistService>();
        services.AddScoped<IPackageService, PackageService>();
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.SectionName));

        services.AddScoped<IMailService, MailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<MatrixFactorizationRecommender>();
        

        return services;
    }
}