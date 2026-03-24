namespace PrintFlow.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "You are not authenticated.")
        : base(message, 401)
    {
    }
}