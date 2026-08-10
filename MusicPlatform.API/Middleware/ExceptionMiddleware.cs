using System.Net;
using System.Text.Json;
using MusicPlatform.Shared.Common;

namespace MusicPlatform.API.Middleware;


public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Yakalanmamis hata: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted) return;

        context.Response.ContentType = "application/json";

        var (status, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim."),
            KeyNotFoundException        => (HttpStatusCode.NotFound, "Kayıt bulunamadı."),
            ArgumentException           => (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException   => (HttpStatusCode.BadRequest, ex.Message),
            TaskCanceledException       => (HttpStatusCode.RequestTimeout, "İstek zaman aşımına uğradı."),
            _                           => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.")
        };

        context.Response.StatusCode = (int)status;

        var response = ApiResponse.Fail(message);

        if (_env.IsDevelopment())
            response.Errors.Add(ex.ToString());

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}