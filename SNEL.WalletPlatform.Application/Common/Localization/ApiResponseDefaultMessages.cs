using System.Globalization;
using System.Resources;
using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Common.Localization;

/// <summary>
/// Resolves localized default success texts for API responses using <c>SharedResource</c> (.resx) satellite assemblies for the current UI culture.
/// </summary>
public static class ApiResponseDefaultMessages
{
    private static readonly ResourceManager ResourceManager = new(
        "SNEL.WalletPlatform.Application.Resources.SharedResource",
        typeof(global::SNEL.WalletPlatform.Application.SharedResource).Assembly);

    public static string GetForSuccessStatus(AppHttpStatusCodes status)
    {
        var key = status switch
        {
            AppHttpStatusCodes.Created => LocalizationKeys.ApiSuccessCreated,
            AppHttpStatusCodes.Ok => LocalizationKeys.ApiSuccessOk,
            _ => LocalizationKeys.ApiSuccessCompleted,
        };

        var localized = ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        if (!string.IsNullOrWhiteSpace(localized))
            return localized;

        localized = ResourceManager.GetString(key, CultureInfo.GetCultureInfo("en"));
        if (!string.IsNullOrWhiteSpace(localized))
            return localized;

        return Fallback(status);
    }

    private static string Fallback(AppHttpStatusCodes status) =>
        status switch
        {
            AppHttpStatusCodes.Created => "Created successfully.",
            AppHttpStatusCodes.Ok => "Success.",
            _ => "Request completed successfully.",
        };
}
