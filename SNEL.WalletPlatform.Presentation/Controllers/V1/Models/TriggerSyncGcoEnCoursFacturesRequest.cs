namespace SNEL.WalletPlatform.Presentation.Controllers.V1.Models;

/// <summary>
/// Request body for manually triggering the Sync GCO En Cours Factures job with a date range.
/// </summary>
public sealed class TriggerSyncGcoEnCoursFacturesRequest
{
    /// <summary>Start of the invoice date range (inclusive).</summary>
    public DateTime Start { get; init; }

    /// <summary>End of the invoice date range (inclusive).</summary>
    public DateTime End { get; init; }

    /// <summary>Regions to run the rollover at.</summary>
    public string Regions { get; set; } = string.Empty;
}
