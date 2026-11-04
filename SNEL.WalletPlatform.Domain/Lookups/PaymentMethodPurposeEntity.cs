using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PaymentMethodPurposeEntity : LookupEntity<int>, ITranslatableEntity<PaymentMethodPurposeTranslationEntity, int>
{
    public ICollection<PaymentMethodPurposeTranslationEntity> Translations { get; set; } = new List<PaymentMethodPurposeTranslationEntity>();
    public ICollection<PaymentMethodEntity> PaymentMethods { get; set; } = new List<PaymentMethodEntity>();

}
