using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ConfirmationTypeEntity : LookupEntity<int>, ITranslatableEntity<ConfirmationTypeTranslationEntity, int>
{
    public ICollection<ConfirmationTypeTranslationEntity> Translations { get; set; } = new List<ConfirmationTypeTranslationEntity>();
    public ICollection<PaymentMethodEntity> PaymentMethods { get; set; } = new List<PaymentMethodEntity>();

}
