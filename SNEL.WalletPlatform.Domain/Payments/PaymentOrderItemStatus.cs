namespace SNEL.WalletPlatform.Domain.Payments;

/// <summary>
/// Status of a payment order item within the billing flow.
/// </summary>
public enum PaymentOrderItemStatus
{
    /// <summary>
    /// Payment is pending completion or payment has failed.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Money transferred from customer account to biller account (on platform).
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment posted on biller system.
    /// </summary>
    Applied = 3,

    /// <summary>
    /// Payment attempt failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Payment attempt expired.
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Payment attempt cancelled.
    /// </summary>
    Cancelled = 6
}

