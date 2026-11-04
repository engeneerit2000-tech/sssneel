using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;

namespace SNEL.WalletPlatform.Presentation.Common;

public sealed record ApiResponse<T>
{
    public int Status { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    // Primary constructor that covers all fields
    public ApiResponse(AppHttpStatusCodes status, string? message = null, T? data = default)
    {
        Status = (int)status;
        Message = ResolveMessage(status, message);
        Data = data;
    }

    /// <summary>
    /// When <paramref name="message"/> is omitted, successful responses (2xx) get a stable default message so clients never see <c>null</c> for success.
    /// </summary>
    private static string? ResolveMessage(AppHttpStatusCodes status, string? message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            return message;

        if (!IsSuccessStatusCode(status))
            return message;

        return ApiResponseDefaultMessages.GetForSuccessStatus(status);
    }

    private static bool IsSuccessStatusCode(AppHttpStatusCodes status)
    {
        var code = (int)status;
        return code is >= 200 and < 300;
    }

    // Backward compatible constructor for existing callers that used the old shape
    public ApiResponse(bool success, T? data, string? message = null)
        : this(success ? AppHttpStatusCodes.Ok : AppHttpStatusCodes.Failed, message, data)
    {
    }

    // Convenience factory methods
    public static ApiResponse<T> SuccessResponse(T? data, string? message = null) =>
        new(AppHttpStatusCodes.Ok,  message, data);

    public static ApiResponse<T> FailResponse(string message, T? data = default) =>
        new(AppHttpStatusCodes.Failed, message, data);

    public static ApiResponse<T> ErrorResponse(string message,  T? data = default) =>
        new(AppHttpStatusCodes.Failed, message, data);

    public static ApiResponse<T> NotFoundResponse(string message) =>
        new(AppHttpStatusCodes.NotFound, message, default);
}