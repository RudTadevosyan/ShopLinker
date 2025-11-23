namespace authService.Domain.CustomExceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}