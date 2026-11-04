namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record SnelConfigModel(
    string PosCode,
    decimal PosId,
    decimal JournalNumber,
    decimal JournalItemNumber,
    string PaymentMode);
