using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class CompletionModeSeedExtensions
{
    public static ModelBuilder SeedCompletionModeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<CompletionModeEntity, CompletionModeTranslationEntity, int>(
            CompletionModeSeeds.Lookups,
            CompletionModeSeeds.Translations);
    }

    private static class CompletionModeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)CompletionModeEnum.IMMEDIATE, Code = CompletionModeEnum.IMMEDIATE.ToString(), Name = "Immediate", Order = 1, Active = true },
            new { Id = (int)CompletionModeEnum.POLLING, Code = CompletionModeEnum.POLLING.ToString(), Name = "Polling", Order = 2, Active = true },
            new { Id = (int)CompletionModeEnum.WEBHOOK, Code = CompletionModeEnum.WEBHOOK.ToString(), Name = "Webhook", Order = 3, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)CompletionModeEnum.IMMEDIATE, LanguageCode = "fr", Name = "Immédiate" },
            new { Id = 2, EntityId = (int)CompletionModeEnum.POLLING, LanguageCode = "fr", Name = "Polling" },
            new { Id = 3, EntityId = (int)CompletionModeEnum.WEBHOOK, LanguageCode = "fr", Name = "Webhook" },
        ];
    }
}





