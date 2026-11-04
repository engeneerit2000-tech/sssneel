using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Partners;
using System.Linq;
using System.Text.Json;

namespace SNEL.WalletPlatform.Application.Accounting;

/// <summary>Resolves platform invoice JSON parameters into display names and related customer/partner ids.</summary>
public static class PlatformInvoiceContextResolver
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static string[]? TryParseParameterStrings(string? descriptionParametersJson)
    {
        if (string.IsNullOrWhiteSpace(descriptionParametersJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(descriptionParametersJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Guid? TryParseGuid(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        return Guid.TryParse(s.Trim(), out var g) ? g : null;
    }

    private static bool TryParseLong(string? s, out long value)
    {
        return long.TryParse(s?.Trim(), out value);
    }

    /// <summary>
    /// Non-platform viewer may match <see cref="PlatformInvoiceEntity.PartnerId"/> or a party extracted from invoice JSON (customer or biller).</summary>
    public static bool ViewerMatchesResolvedParties(
        PlatformInvoiceEntity invoice,
        long viewerPartnerId)
    {
        if (invoice.PartnerId == viewerPartnerId)
        {
            return true;
        }

        var args = TryParseParameterStrings(invoice.DescriptionParametersJson);
        if (args is null
            || !TryGetPartyIds(invoice.DescriptionTemplateKey, args, out var custPid, out var secPid, out _))
        {
            return false;
        }

        if (custPid == viewerPartnerId)
        {
            return true;
        }

        return secPid is { } sid && sid == viewerPartnerId;
    }

    public static bool TryGetPartyIds(
        string descriptionTemplateKey,
        IReadOnlyList<string> args,
        out long customerPartnerId,
        out long? secondaryPartnerId,
        out Guid? paymentOrderItemId)
    {
        customerPartnerId = 0;
        secondaryPartnerId = null;
        paymentOrderItemId = null;

        if (descriptionTemplateKey is LocalizationKeys.PlatformInvoiceDescriptionKeys.TopupFees
            or LocalizationKeys.PlatformInvoiceDescriptionKeys.TopupCost)
        {
            if (args.Count < 3
                || !TryParseLong(args[0], out var cust)
                || !TryParseLong(args[1], out var secondary))
            {
                return false;
            }

            paymentOrderItemId = TryParseGuid(args[2]);
            customerPartnerId = cust;
            secondaryPartnerId = secondary;
            return true;
        }

        if (descriptionTemplateKey == LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCommission)
        {
            if (args.Count < 2
                || !TryParseLong(args[0], out var billerPid)
                || !TryParseLong(args[1], out var custPid))
            {
                return false;
            }

            secondaryPartnerId = billerPid;
            customerPartnerId = custPid;
            if (args.Count >= 3)
            {
                paymentOrderItemId = TryParseGuid(args[2]);
            }

            return true;
        }

        if (descriptionTemplateKey is LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentFees
            or LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCost)
        {
            if (args.Count < 2
                || !TryParseLong(args[0], out var cust)
                || !TryParseLong(args[1], out var secondary))
            {
                return false;
            }

            customerPartnerId = cust;
            secondaryPartnerId = secondary;
            if (args.Count >= 3)
            {
                paymentOrderItemId = TryParseGuid(args[2]);
            }

            return true;
        }

        return false;
    }

    public static string PartnerDisplayName(PartnerEntity? partner)
    {
        if (partner is null)
        {
            return string.Empty;
        }

        return partner.DisplayName ?? partner.LegalName;
    }

    public static void EnrichPlatformInvoiceOutput(
        PlatformInvoiceOutputModel model,
        IStringLocalizer<SharedResource> localizer,
        IReadOnlyDictionary<long, PartnerEntity> partnersById,
        IReadOnlyDictionary<long, CustomerEntity?> customersByPartnerId)
    {
        var args = TryParseParameterStrings(model.DescriptionParametersJson);
        if (args is null
            || !TryGetPartyIds(model.DescriptionTemplateKey, args, out var custPid, out var secPid, out var poiId))
        {
            PopulateDescriptionOnly(model, localizer, args);
            return;
        }

        model.PaymentOrderItemId = poiId;
        model.CustomerPartnerId = custPid;
        partnersById.TryGetValue(custPid, out var customerPartnerRow);
        model.CustomerDisplayName = PartnerDisplayName(customerPartnerRow);
        if (customersByPartnerId.TryGetValue(custPid, out var custEntity) && custEntity != null)
        {
            model.CustomerId = custEntity.Id;
        }

        model.SecondaryPartnerId = secPid;
        if (secPid is { } sid && partnersById.TryGetValue(sid, out var secPartner))
        {
            model.SecondaryPartnerDisplayName = PartnerDisplayName(secPartner);
        }

        if (string.IsNullOrWhiteSpace(model.CustomerDisplayName))
        {
            model.CustomerDisplayName = custPid.ToString();
        }

        var secondaryLabel = model.SecondaryPartnerDisplayName ?? (secPid?.ToString() ?? "—");
        object[] displayArgs =
            model.DescriptionTemplateKey == LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCommission
                ?
                [
                    secondaryLabel,
                    model.CustomerDisplayName
                ]
                :
                model.DescriptionTemplateKey is LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentFees
                    or LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCost
                    ? [model.CustomerDisplayName, secondaryLabel]
                    :
                    [
                        model.CustomerDisplayName,
                        secondaryLabel,
                        poiId?.ToString("D") ?? (args.Length >= 3 ? args[2] : string.Empty)
                    ];
        model.Description = localizer[model.DescriptionTemplateKey, displayArgs];
    }

    public static async Task<PlatformInvoicePrintEnrichment> ResolveForPrintAsync(
        PlatformInvoiceEntity invoice,
        IUnitOfWork unitOfWork,
        IStringLocalizer<SharedResource> localizer,
        CancellationToken cancellationToken)
    {
        var args = TryParseParameterStrings(invoice.DescriptionParametersJson);
        Guid? poiId = null;
        Guid? customerEntityId = null;
        long customerPartnerIdTracked = invoice.PartnerId;
        string customerDisplay = PartnerDisplayName(invoice.Partner);
        string? secondaryDisplay = null;
        long? secondaryId = null;

        var idsToLoad = new HashSet<long> { invoice.PartnerId };

        if (args is not null
            && TryGetPartyIds(invoice.DescriptionTemplateKey, args, out var custPid,
                out var secPid,
                out var parsedPoi))
        {
            poiId = parsedPoi;
            customerPartnerIdTracked = custPid;
            secondaryId = secPid;
            idsToLoad.Add(custPid);
            if (secPid is { } sp)
            {
                idsToLoad.Add(sp);
            }
        }

        var partners =
            await unitOfWork.PartnerRepository.GetByIdsAsync(idsToLoad.ToList(), cancellationToken);
        var dict = partners.ToDictionary(p => p.Id);

        dict.TryGetValue(customerPartnerIdTracked, out var customerPartnerResolved);
        customerDisplay = PartnerDisplayName(customerPartnerResolved);
        if (string.IsNullOrWhiteSpace(customerDisplay))
        {
            customerDisplay = customerPartnerIdTracked.ToString();
        }

        var custEntity = await unitOfWork.CustomerRepository.GetByPartnerIdAsync(
            customerPartnerIdTracked,
            cancellationToken);
        customerEntityId = custEntity?.Id;

        if (secondaryId is { } sid && dict.TryGetValue(sid, out var secondaryPartner))
        {
            secondaryDisplay = PartnerDisplayName(secondaryPartner);
        }

        var secondaryLabel = secondaryDisplay ?? (secondaryId?.ToString() ?? "—");
        object[] fmtArgs =
            invoice.DescriptionTemplateKey == LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCommission
                ? [secondaryLabel, customerDisplay]
                : invoice.DescriptionTemplateKey is LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentFees
                    or LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCost
                    ? [customerDisplay, secondaryLabel]
                    :
                    [
                        customerDisplay,
                        secondaryLabel,
                        poiId?.ToString("D") ?? (args?.Length >= 3 == true ? args[2] : string.Empty)
                    ];
        var description = localizer[invoice.DescriptionTemplateKey, fmtArgs];

        return new PlatformInvoicePrintEnrichment(
            customerEntityId,
            customerDisplay,
            customerPartnerIdTracked,
            secondaryDisplay,
            secondaryId,
            poiId,
            description);
    }

    private static void PopulateDescriptionOnly(
        PlatformInvoiceOutputModel model,
        IStringLocalizer<SharedResource> localizer,
        string[]? args)
    {
        var objArgs = args?.Cast<object>().ToArray() ?? [];
        model.Description = objArgs.Length == 0
            ? localizer[model.DescriptionTemplateKey]
            : localizer[model.DescriptionTemplateKey, objArgs];
    }
}

public sealed record PlatformInvoicePrintEnrichment(
    Guid? CustomerId,
    string CustomerDisplayName,
    long CustomerPartnerId,
    string? SecondaryPartnerDisplayName,
    long? SecondaryPartnerId,
    Guid? PaymentOrderItemId,
    string Description);
