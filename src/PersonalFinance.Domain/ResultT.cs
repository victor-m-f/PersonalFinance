namespace PersonalFinance.Domain;

public sealed record class Result<T>
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null);
    }

    public static Result<T> Failure(string code, string message)
    {
        return new Result<T>(false, default, code, message);
    }
}
