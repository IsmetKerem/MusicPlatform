using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}