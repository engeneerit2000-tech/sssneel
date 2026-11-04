using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Common;

public enum AppHttpStatusCodes
{
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    Ok = 200,
    Created = 201,
    NotFound = 404,
    Conflict = 409,
    Failed = 500,
}
public class Result
{
    protected Result(bool isSuccess, string? message, AppHttpStatusCodes statusCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public string? Message { get; }
    public AppHttpStatusCodes StatusCode { get; }

    public static Result Success(AppHttpStatusCodes statusCode = AppHttpStatusCodes.Ok) =>
        new(true, null, statusCode);

    public static Result Failure(string message, AppHttpStatusCodes statusCode) =>
        new(false, message, statusCode);

}
public sealed class Result<T>:Result
{
    public T? Value { get; }

    [JsonConstructor]
    public Result(bool isSuccess, T? value, string? message, AppHttpStatusCodes statusCode):base(isSuccess, message, statusCode)
    {
        Value = value;
    }


    public static Result<T> Success(T value, AppHttpStatusCodes statusCode = AppHttpStatusCodes.Ok) =>
        new(true, value, default, statusCode);
    public static Result<T> Success(T value,string message, AppHttpStatusCodes statusCode = AppHttpStatusCodes.Ok) =>
        new(true, value, message, statusCode);
    public static Result<T> Failure(string message, AppHttpStatusCodes statusCode) =>
        new(false, default, message, statusCode);
    public static Result<T> Failure(string message, AppHttpStatusCodes statusCode, T? value) =>
        new(false, value, message, statusCode);


}

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount);
public sealed record PagedResultWithSummary<T, TSummary>(IReadOnlyCollection<T> Items, TSummary PageSummary, int TotalCount);

public static class ResultExtensions
{
    public static Result<TOut> ToFailure<TOut>(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to failure.");

        return Result<TOut>.Failure(result.Message!, result.StatusCode);
    }
    public static Result<TOut> ToSuccess<TOut>(this Result result,TOut data)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to failure.");

        return Result<TOut>.Success(data,result.Message!, result.StatusCode);
    }
}
