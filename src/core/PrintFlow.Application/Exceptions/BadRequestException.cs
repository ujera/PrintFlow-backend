namespace PrintFlow.Application.Exceptions;

public class BadRequestException : AppException
{
    public List<string> Errors { get; }

    public BadRequestException(string message)
        : base(message, 400)
    {
        Errors = new List<string> { message };
    }

    public BadRequestException(List<string> errors)
        : base(errors.FirstOrDefault() ?? "Invalid request.", 400)
    {
        Errors = errors;
    }
}