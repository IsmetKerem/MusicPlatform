using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Auth;
using MusicPlatform.UI.Options;

namespace MusicPlatform.UI.Services;


public class AuthTokenHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly ApiSettings _api;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(
        ITokenStore tokenStore,
        IOptions<ApiSettings> api,
        ILogger<AuthTokenHandler> logger)
    {
        _tokenStore = tokenStore;
        _api = api.Value;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenStore.AccessToken;

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            !string.IsNullOrEmpty(_tokenStore.RefreshToken))
        {
            var refreshed = await TryRefreshAsync(cancellationToken);

            if (refreshed)
            {
                response.Dispose();

                var retry = await CloneRequestAsync(request);
                retry.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);

                return await base.SendAsync(retry, cancellationToken);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(_api.BaseUrl) };

            var result = await client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshTokenRequestDto { RefreshToken = _tokenStore.RefreshToken! },
                ct);

            if (!result.IsSuccessStatusCode)
            {
                _tokenStore.Clear();
                return false;
            }

            var body = await result.Content
                .ReadFromJsonAsync<ApiResponse<TokenResponseDto>>(cancellationToken: ct);

            if (body?.Data is null)
            {
                _tokenStore.Clear();
                return false;
            }

            _tokenStore.Save(
                body.Data.AccessToken,
                body.Data.RefreshToken,
                body.Data.AccessTokenExpiresAt);

            _logger.LogInformation("Token otomatik yenilendi.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token yenilenemedi.");
            _tokenStore.Clear();
            return false;
        }
    }

 
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        clone.Version = request.Version;
        return clone;
    }
}