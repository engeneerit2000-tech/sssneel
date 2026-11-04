namespace SNEL.WalletPlatform.Presentation.Controllers.V1.Models;

/// <summary>
/// Request body for manually triggering the biller rollover job.
/// </summary>
public sealed class TriggerBillerRolloverRequest
{
    /// <summary>Start of the invoice date range (inclusive).</summary>
    public DateTime Start { get; init; }

    /// <summary>End of the invoice date range (inclusive).</summary>
    public DateTime End { get; init; }

    /// <summary>Comma-separated regions to process.</summary>
    public string Regions { get; init; } = string.Empty;

    /// <summary>Whether notifications should be registered for newly pulled invoices.</summary>
    public bool RegisterNotificationOnPull { get; init; }
}
