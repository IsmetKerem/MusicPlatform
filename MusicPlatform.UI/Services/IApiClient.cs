using MusicPlatform.Shared.Common;

namespace MusicPlatform.UI.Services;

public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string url);
    Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null);
    Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null);
    Task<ApiResponse<T>> DeleteAsync<T>(string url);

    Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request);
}