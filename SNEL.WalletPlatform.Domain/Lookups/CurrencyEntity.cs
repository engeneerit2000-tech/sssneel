using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CurrencyEntity : LookupEntity<int>, ITranslatableEntity<CurrencyTranslationEntity, int>
{
    public string Abbreviation { get; set; }
    public int Minor { get; set; }
    public ICollection<CurrencyTranslationEntity> Translations { get; set; } = new List<CurrencyTranslationEntity>();
    
    public ICollection<InvoiceEntity> Invoices { get; set; } = new List<InvoiceEntity>();
    public ICollection<BillingServiceCurrencyEntity> BillingServiceCurrencies { get; set; } =
        new List<BillingServiceCurrencyEntity>();
    public ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
    public ICollection<PaymentOrderEntity> PaymentOrders { get; set; } = new List<PaymentOrderEntity>();
    public ICollection<PaymentOrderItemEntity> PaymentOrderItems { get; set; } = new List<PaymentOrderItemEntity>();
    public ICollection<PaymentMethodCurrencyEntity> PaymentMethodCurrencies { get; set; } = new List<PaymentMethodCurrencyEntity>();
    public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();


}
