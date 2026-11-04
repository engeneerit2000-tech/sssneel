using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class ConfirmationTypeSeedExtensions
{
    public static ModelBuilder SeedConfirmationTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<ConfirmationTypeEntity, ConfirmationTypeTranslationEntity, int>(
            ConfirmationTypeSeeds.Lookups,
            ConfirmationTypeSeeds.Translations);
    }

    private static class ConfirmationTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)ConfirmationTypeEnum.NONE, Code = ConfirmationTypeEnum.NONE.ToString(), Name = "None", Order = 1, Active = true },
            new { Id = (int)ConfirmationTypeEnum.OTP, Code = ConfirmationTypeEnum.OTP.ToString(), Name = "OTP", Order = 2, Active = true },
            new { Id = (int)ConfirmationTypeEnum.PROMPT, Code = ConfirmationTypeEnum.PROMPT.ToString(), Name = "Prompt", Order = 3, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)ConfirmationTypeEnum.NONE, LanguageCode = "fr", Name = "Aucune" },
            new { Id = 2, EntityId = (int)ConfirmationTypeEnum.OTP, LanguageCode = "fr", Name = "OTP" },
            new { Id = 3, EntityId = (int)ConfirmationTypeEnum.PROMPT, LanguageCode = "fr", Name = "Prompt" },
        ];
    }
}





