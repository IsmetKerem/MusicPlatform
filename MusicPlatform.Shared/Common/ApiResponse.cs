namespace MusicPlatform.Shared.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message)
        => new() { Success = true, Message = message };

    public new static ApiResponse Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}