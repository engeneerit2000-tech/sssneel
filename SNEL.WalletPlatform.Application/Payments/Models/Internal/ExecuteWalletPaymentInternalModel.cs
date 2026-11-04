using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed record ExecuteWalletPaymentInternalModel
{
    public AccountEntity AccountEntity { get; set; }
    public AccountEntity BillerAccountEntity { get; set; }
    public CustomerEntity CustomerEntity { get; set; }
    public CustomerSubscriptionEntity CustomerSubscription { get; set; }
    public decimal TotalAmount { get; set; }

    public List<InvoicePaymentDetailsModel> InvoicesPaymentsDetails { get; set; }
}


public sealed record InvoicePaymentDetailsModel
{
    public InvoiceEntity Invoice { get; set; }
    public CustomerSubscriptionEntity? CustomerSubscription { get; set; }
    public decimal PaidAmount { get; set; }
}
