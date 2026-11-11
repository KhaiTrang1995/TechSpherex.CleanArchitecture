namespace TechSpherex.CleanArchitecture.Domain.Common;

/// <summary>
/// A failure description. This is a C# 15 closed hierarchy: only the error kinds declared
/// in this assembly can exist, so a <c>switch</c> over an <see cref="Error"/> is checked
/// for exhaustiveness by the compiler. Adding a new kind here turns every mapping that
/// forgot to handle it (HTTP status, gRPC status, ...) into a compiler warning.
/// </summary>
public closed record Error(string Code, string Message)
{
    public static Error NotFound(string code, string message) => new NotFoundError(code, message);
    public static Error Validation(string code, string message) => new ValidationError(code, message);
    public static Error Conflict(string code, string message) => new ConflictError(code, message);
    public static Error Failure(string code, string message) => new FailureError(code, message);
}

public sealed record NotFoundError(string Code, string Message) : Error(Code, Message);

public sealed record ValidationError(string Code, string Message) : Error(Code, Message);

public sealed record ConflictError(string Code, string Message) : Error(Code, Message);

public sealed record FailureError(string Code, string Message) : Error(Code, Message);
