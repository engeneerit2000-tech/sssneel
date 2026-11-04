using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentCategoryEntity : LookupEntity<int>,
    ITranslatableEntity<PaymentCategoryTranslationEntity, int>
{
   public ICollection<PaymentMethodEntity> PaymentMethods { get; private set; }
   public ICollection<PaymentCategoryTranslationEntity> Translations { get; set; } =
       new List<PaymentCategoryTranslationEntity>();
}
