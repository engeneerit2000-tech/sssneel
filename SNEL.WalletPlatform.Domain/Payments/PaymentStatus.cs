namespace SNEL.WalletPlatform.Domain.Payments;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Expired=6
}

public static class PaymentStatusExtensions
{
    public static bool IsFinal(this PaymentStatus status)
    {
        return status == PaymentStatus.Completed ||
               status == PaymentStatus.Failed ||
               status == PaymentStatus.Cancelled ||
               status == PaymentStatus.Expired;
    }
    public static bool IsTermination(this PaymentStatus status)
    {
        return status == PaymentStatus.Failed ||
               status == PaymentStatus.Cancelled ||
               status == PaymentStatus.Expired;
    }
    public static readonly PaymentStatus[] ExpirableStatuses =
    [
        PaymentStatus.Pending,
        PaymentStatus.Processing
    ];
}