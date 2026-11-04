using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ChannelEntity : LookupEntity<int>, ITranslatableEntity<ChannelTranslationEntity, int>
{
    public ICollection<ChannelTranslationEntity> Translations { get; set; } = new List<ChannelTranslationEntity>();
    public ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
}
