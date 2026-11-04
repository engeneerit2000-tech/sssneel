using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PaymentInitModeEntity : LookupEntity<int>, ITranslatableEntity<PaymentInitModeTranslationEntity, int>
{
    public ICollection<PaymentInitModeTranslationEntity> Translations { get; set; } = new List<PaymentInitModeTranslationEntity>();
    public ICollection<PaymentMethodEntity> PaymentMethods { get; set; } = new List<PaymentMethodEntity>();

}
