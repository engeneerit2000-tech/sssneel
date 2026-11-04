namespace SNEL.WalletPlatform.Application.Reporting.Models;

/// <summary>
/// Optional page layout overrides for HTML → PDF conversion.
/// When null, Gotenberg Chrome defaults (A4) are used.
/// </summary>
public sealed class PdfPageOptions
{
    /// <summary>Paper width in millimeters (e.g. 80 for thermal receipts).</summary>
    public double? WidthMillimeters { get; init; }

    /// <summary>Paper height in millimeters. Ignored when <see cref="SinglePage"/> is true.</summary>
    public double? HeightMillimeters { get; init; }

    /// <summary>
    /// Fit all HTML content on one page by expanding page height to content.
    /// </summary>
    public bool SinglePage { get; init; }

    /// <summary>Zero margins on all sides.</summary>
    public bool NoMargins { get; init; } = true;

    /// <summary>Print CSS backgrounds (needed for watermarks).</summary>
    public bool PrintBackground { get; init; } = true;

    /// <summary>Prefer @page size from CSS over explicit paper dimensions.</summary>
    public bool PreferCssPageSize { get; init; }

    public static PdfPageOptions Thermal80Mm { get; } = new()
    {
        WidthMillimeters = 80,
        SinglePage = true,
        NoMargins = true,
        PrintBackground = true,
        PreferCssPageSize = false
    };
}
