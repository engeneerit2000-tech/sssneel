using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class ConfirmationModeSeedExtensions
{
    public static ModelBuilder SeedConfirmationModeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<ConfirmationModeEntity, ConfirmationModeTranslationEntity, int>(
            ConfirmationModeSeeds.Lookups,
            ConfirmationModeSeeds.Translations);
    }

    private static class ConfirmationModeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)ConfirmationModeEnum.EXTERNAL, Code = ConfirmationModeEnum.EXTERNAL.ToString(), Name = "External", Order = 1, Active = true },
            new { Id = (int)ConfirmationModeEnum.PLATFORM, Code = ConfirmationModeEnum.PLATFORM.ToString(), Name = "Platform", Order = 2, Active = true },
            new { Id = (int)ConfirmationModeEnum.NONE, Code = ConfirmationModeEnum.NONE.ToString(), Name = "None", Order = 3, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)ConfirmationModeEnum.EXTERNAL, LanguageCode = "fr", Name = "Externe" },
            new { Id = 2, EntityId = (int)ConfirmationModeEnum.PLATFORM, LanguageCode = "fr", Name = "Plate-forme" },
            new { Id = 3, EntityId = (int)ConfirmationModeEnum.NONE, LanguageCode = "fr", Name = "Aucune" },
        ];
    }
}





