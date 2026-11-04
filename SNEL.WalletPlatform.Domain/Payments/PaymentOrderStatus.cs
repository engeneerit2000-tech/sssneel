namespace SNEL.WalletPlatform.Domain.Payments;

public enum PaymentOrderStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
}
