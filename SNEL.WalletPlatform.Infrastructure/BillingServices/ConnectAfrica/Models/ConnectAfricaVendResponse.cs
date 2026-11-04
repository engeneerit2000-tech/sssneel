using SNEL.WalletPlatform.Application.Payments.Models.ConnectAfrica;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaVendResponse(
    bool IsSuccess,
    string? ReceiptNumber,
    int? TokenCount,
    IReadOnlyList<string> Tokens,
    ConnectAfricaMeterChargeReceiptData? Receipt,
    string RequestBody,
    string ResponseBody,
    string? ResponseCode);
