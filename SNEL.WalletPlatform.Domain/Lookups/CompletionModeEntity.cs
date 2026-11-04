using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CompletionModeEntity : LookupEntity<int>, ITranslatableEntity<CompletionModeTranslationEntity, int>
{
    public ICollection<CompletionModeTranslationEntity> Translations { get; set; } = new List<CompletionModeTranslationEntity>();
    public ICollection<PaymentMethodEntity> PaymentMethods { get; set; } = new List<PaymentMethodEntity>();
}
