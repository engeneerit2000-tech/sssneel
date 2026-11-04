using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PlatformConstantSeedExtensions
{
    public static readonly string DefaultPaymentMethodValue =
        $$"""{"paymentServiceProviderCode":"{{AkibaPayPaymentSeedData.ProviderCode}}","paymentMethodCode":"{{AkibaPayPaymentSeedData.MethodCode}}"}""";

    public static ModelBuilder SeedPlatformConstantLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PlatformConstantEntity, PlatformConstantTranslationEntity, int>(
            PlatformConstantSeeds.Lookups,
            PlatformConstantSeeds.Translations);
    }

    private static class PlatformConstantSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)PlatformConstantCodesEnum.MOBILE_APP_VERSION, Code = PlatformConstantCodesEnum.MOBILE_APP_VERSION.ToString(), Name = "Mobile App Version", Order = 1, Active = true, Group = PlatformConstantGroupsEnum.SYSTEM.ToString(), Value = "0.0.17" },
            new { Id = (int)PlatformConstantCodesEnum.DEFAULT_PAYMENT_METHOD, Code = PlatformConstantCodesEnum.DEFAULT_PAYMENT_METHOD.ToString(), Name = "Default Payment Method", Order = 1, Active = true, Group = PlatformConstantGroupsEnum.PAYMENT.ToString(), Value = DefaultPaymentMethodValue },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)PlatformConstantCodesEnum.MOBILE_APP_VERSION, LanguageCode = "fr", Name = "Mobile App Version" },
            new { Id = 2, EntityId = (int)PlatformConstantCodesEnum.DEFAULT_PAYMENT_METHOD, LanguageCode = "fr", Name = "Moyen de paiement par défaut" }
        ];
    }
}
