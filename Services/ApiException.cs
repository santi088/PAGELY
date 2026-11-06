namespace PAGELY.Services;

public enum ApiErrorKind
{
    NetworkUnreachable,
    Timeout,
    HttpError,
    InvalidResponse,
    Authentication,
    Configuration
}

public sealed class ApiException : Exception
{
    public ApiErrorKind Kind { get; }

    public ApiException(ApiErrorKind kind, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
    }
}
