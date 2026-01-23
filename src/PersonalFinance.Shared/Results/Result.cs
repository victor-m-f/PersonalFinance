namespace PersonalFinance.Shared.Results;

public sealed record class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success()
    {
        return new Result(true, null, null);
    }

    public static Result Failure(string code, string message)
    {
        return new Result(false, code, message);
    }
}
