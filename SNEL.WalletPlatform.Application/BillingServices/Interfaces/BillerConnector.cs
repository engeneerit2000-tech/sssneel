using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public abstract class BillerConnector
{
    protected IStringLocalizer<SharedResource> Localizer { get; }

    protected BillerConnector(IStringLocalizer<SharedResource> localizer)
    {
        Localizer = localizer;
    }

    public abstract string BillerCode { get; }
    public abstract Task<Result<InquireOutputModel>> HandleInquireAsync(
        BillerInquiryRequestModel request,
        CancellationToken cancellationToken = default);
    public abstract Task<Result<PagedInquireOutputModel>> HandleInquirePagedAsync(
        BillerPagedInquiryRequestModel request,
        CancellationToken cancellationToken = default);
    public abstract Task<Result<(bool Status, string? Info)>> HandleCheckSubscriptionExistsAsync(BillerSubscriptionExistsRequestModel request);
    public abstract Task<Result<int>> ApplyPaymentsAsync(IReadOnlyCollection<PaymentOrderItemEntity> pois);

    public virtual Task<Result<(bool eligible, string message)>> HandleIsEligible(IsEligibleRequestModel request)
    {
        return Task.FromResult(Result<(bool eligible, string message)>.Success(
            (true, Localizer[LocalizationKeys.BillerIsEligibleByDefault])));
    }

    public Task<Result<InquireOutputModel>> InquireAsync(
        BillerInquiryRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var x = HandleInquireAsync(request, cancellationToken);
        return x;
    }

    public Task<Result<PagedInquireOutputModel>> InquirePagedAsync(
        BillerPagedInquiryRequestModel request,
        CancellationToken cancellationToken = default)
    {
        return HandleInquirePagedAsync(request, cancellationToken);
    }

    public async Task<Result<(bool Status, string? Info)>> CheckSubscriptionExistsAsync(BillerSubscriptionExistsRequestModel request)
    {
        return await HandleCheckSubscriptionExistsAsync(request);
    }

    public Task<Result<(bool eligible, string message)>> IsEligible(IsEligibleRequestModel model)
    {
        return HandleIsEligible(model);
    }

    public virtual Result<List<CustomActionsOutputModel>> GetCustomActions(string billingServiceCode) {
        return Result<List<CustomActionsOutputModel>>.Success(new List<CustomActionsOutputModel>());
    }

    public virtual Task<Result<object>> ExecuteCustomAction(string billingServiceCode, string billingRef, string actionCode, Dictionary<string, object> parameters)
    {
        throw new NotImplementedException();
    }
}
