using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentTypeEntity : LookupEntity<int>,
    ITranslatableEntity<PaymentTypeTranslationEntity, int>
{
   public ICollection<PaymentEntity> Payments { get; private set; }
   public ICollection<PaymentTypeTranslationEntity> Translations { get; set; } =
       new List<PaymentTypeTranslationEntity>();
}
