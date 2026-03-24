namespace PrintFlow.Application.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string entityName, string conflictField, string value)
        : base($"{entityName} with {conflictField} '{value}' already exists.", 409)
    {
    }
}