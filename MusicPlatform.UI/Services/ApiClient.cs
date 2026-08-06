using System.Net.Http.Json;
using System.Text.Json;
using MusicPlatform.Shared.Common;

namespace MusicPlatform.UI.Services;

public class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<ApiResponse<T>> GetAsync<T>(string url)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url));

    public Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null)
        => SendAsync<T>(Build(HttpMethod.Post, url, body));

    public Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null)
        => SendAsync<T>(Build(HttpMethod.Put, url, body));

    public Task<ApiResponse<T>> DeleteAsync<T>(string url)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url));

    public Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request)
        => _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    private static HttpRequestMessage Build(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return request;
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage request)
    {
        try
        {
            using var response = await _http.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return response.IsSuccessStatusCode
                    ? ApiResponse<T>.Ok(default!)
                    : ApiResponse<T>.Fail($"Sunucu bos cevap dondu ({(int)response.StatusCode}).");
            }

            var parsed = JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);

            if (parsed is null)
                return ApiResponse<T>.Fail("Cevap cozumlenemedi.");


            return parsed;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API'ye ulasilamadi: {Url}", request.RequestUri);
            return ApiResponse<T>.Fail("Sunucuya ulasilamiyor. Lutfen daha sonra tekrar deneyin.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "API cevabi cozumlenemedi: {Url}", request.RequestUri);
            return ApiResponse<T>.Fail("Sunucudan beklenmeyen bir cevap alindi.");
        }
    }
}