namespace TechSpherex.CleanArchitecture.Domain.Common;

// Copyright (c) 2026 TechSpherex
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    internal Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }
}
