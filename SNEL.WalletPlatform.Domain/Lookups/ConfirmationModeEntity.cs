using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ConfirmationModeEntity : LookupEntity<int>, ITranslatableEntity<ConfirmationModeTranslationEntity, int>
{
    public ICollection<ConfirmationModeTranslationEntity> Translations { get; set; } = new List<ConfirmationModeTranslationEntity>();
    public ICollection<PaymentMethodEntity> PaymentMethods { get; set; } = new List<PaymentMethodEntity>();

}
