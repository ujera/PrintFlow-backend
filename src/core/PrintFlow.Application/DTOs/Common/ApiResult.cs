namespace PrintFlow.Application.DTOs.Common;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResult<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResult<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static ApiResult<T> Fail(List<string> errors) => new()
    {
        Success = false,
        Errors = errors,
        Message = errors.FirstOrDefault()
    };
}