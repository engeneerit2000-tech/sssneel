namespace SNEL.WalletPlatform.Application.Common.Exceptions;

public class AppException : Exception
{
    public AppException(string message, AppHttpStatusCodes statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public AppHttpStatusCodes StatusCode { get; }
}
