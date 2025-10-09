namespace GymRoutineGenerator.Application.Common;

/// <summary>
/// Patrón Result para manejo de errores funcional
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Un resultado exitoso no puede tener error");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Un resultado fallido debe tener error");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }

    public static Result Success() => new Result(true, string.Empty);

    public static Result Failure(string error) => new Result(false, error);

    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);

    public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);
}

/// <summary>
/// Patrón Result con valor de retorno
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
