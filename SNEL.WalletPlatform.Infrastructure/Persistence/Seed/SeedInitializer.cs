using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class SeedInitializer
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder
            .SeedAccountTypeLookup()
            .SeedGenderLookup()
            .SeedAddressLookups()
            .SeedBillingCategoryLookup()
            .SeedBillingServiceTypeLookup()
            .SeedChannelLookup()
            .SeedCenterLookup()
            .SeedCurrencyLookup()
            .SeedFavoriteTypeLookup()
            .SeedPartnerTypeLookup()
            .SeedContactTypeLookup()
            .SeedPaymentCategoryLookup()
            .SeedJournalTypeLookup()
            .SeedJournalLineTypeLookup()
            .SeedPartners()
            .SeedAccounts()
            .SeedUsers()
            .SeedUserNotificationPreferences()
            .SeedPlatformConstantLookup()
            .SeedCompletionModeLookup()
            .SeedConfirmationModeLookup()
            .SeedConfirmationTypeLookup()
            .SeedPaymentInitModeLookup()
            .SeedPaymentMethodPurposeLookup()
            .SeedPaymentTypeLookup()
            .SeedPaymentProviderRouters()
            ;
    }
    private static ModelBuilder SeedAddressLookups(this ModelBuilder modelBuilder)
    {
        return modelBuilder
            .SeedCountryLookup()
            .SeedRegionLookup()
            .SeedCityLookup()
            .SeedAreaLookup();
    }
    
    private static ModelBuilder SeedPartners(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartnerEntity>()
            .HasData(new { Id = 1L, PartnerTypeId = 4, LegalName = "SNEL Wallet Platform", DisplayName = "SNEL Wallet Platform", Active = true, CreatedAtUtc = DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"), UpdatedAtUtc = DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"), CreatedBy = "SYSTEM", UpdatedBy = "SYSTEM" });
        return modelBuilder;
    }
    
    private static ModelBuilder SeedAccounts(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>()
            .HasData(
                    new {Id = Guid.Parse("da5d1565-6352-487e-b17c-e819c168616f"), Name = "Platform Cash",AccountTypeId = (int)AccountTypesEnum.CASH, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM"},
                    new {Id = Guid.Parse("a60adf4a-fc8b-440f-a69a-0dc16b2c7ff4"), Name = "Platform Cash",AccountTypeId = (int)AccountTypesEnum.CASH, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("ea6a0d62-4c39-4f97-9aa6-318c8fb6c5fb"), Name = "Platform Bank Account",AccountTypeId = (int)AccountTypesEnum.BANK, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("4eda20e4-094d-4e44-bb9d-6757d9bb4490"), Name = "Platform Bank Account",AccountTypeId = (int)AccountTypesEnum.BANK, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("ae7c6cc2-3566-460b-aca5-f35439dc874a"), Name = "TOPUP Cost",AccountTypeId = (int)AccountTypesEnum.TOPUP_COST, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("b63f912e-cdfa-482c-88a0-7a6deadb189a"), Name = "TOPUP Cost",AccountTypeId = (int)AccountTypesEnum.TOPUP_COST, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("45eebeaf-e6f1-4934-9010-21d7c9ab69d3"), Name = "TAX Expenses",AccountTypeId = (int)AccountTypesEnum.TAX_EXPENSE, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("5116ca27-8161-4a59-84de-eb824aacd5a1"), Name = "TAX Expenses",AccountTypeId = (int)AccountTypesEnum.TAX_EXPENSE, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("20391f85-3759-426b-8490-71b6d6d05df2"), Name = "TAX Payable",AccountTypeId = (int)AccountTypesEnum.TAX_PAYABLE, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("0f3e0d8d-2748-4afd-966b-1c25a8981867"), Name = "TAX Payable",AccountTypeId = (int)AccountTypesEnum.TAX_PAYABLE, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("b68d7723-039b-4699-a3ad-fcc53a33795a"), Name = "Platform Revenue",AccountTypeId = (int)AccountTypesEnum.REVENUE, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("28fefd2d-9222-4e3e-86cb-15d6c2979c9e"), Name = "Platform Revenue",AccountTypeId = (int)AccountTypesEnum.REVENUE, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("7638d5e8-1a70-459e-a918-0606ab1671ac"), Name = "Payment Cost",AccountTypeId = (int)AccountTypesEnum.PAYMENT_COST, PartnerId = 1L, CurrencyId = 1,Order = 1, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" },
                    new {Id = Guid.Parse("703751f9-32ef-4460-b315-840353d05234"), Name = "Payment Cost",AccountTypeId = (int)AccountTypesEnum.PAYMENT_COST, PartnerId = 1L, CurrencyId = 2,Order = 2, Balance = 0m, Version = 0, CreatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),UpdatedAtUtc=DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),CreatedBy="SYSTEM",UpdatedBy= "SYSTEM" }
                );
        return modelBuilder;
    }

    private static ModelBuilder SeedUsers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasData(new
            {
                Id = Guid.Parse("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01"),
                IamRef = "seed-platform-admin",
                Username = "platform.admin",
                FirstName = "Platform",
                SecondName = (string?)null,
                ThirdName = (string?)null,
                LastName = "Admin",
                GenderId = 1,
                MobileNo = "+243900000001",
                Email = "platform.admin@snel.local",
                NationalNo = "PLATFORM_ADMIN",
                Status = UserStatus.Active,
                Language = "en",
                PartnerId = 1L,
                CreatedAtUtc = DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),
                UpdatedAtUtc = DateTimeOffset.Parse("2026-04-08T00:00:00+00:00"),
                CreatedBy = "SYSTEM",
                UpdatedBy = "SYSTEM"
            });
        return modelBuilder;
    }

    private static ModelBuilder SeedUserNotificationPreferences(this ModelBuilder modelBuilder)
    {
        var baseTime = DateTimeOffset.Parse("2026-04-08T00:00:00+00:00");
        var platformUserId = Guid.Parse("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01");

        modelBuilder.Entity<UserNotificationPreferenceEntity>()
            .HasData(
                BuildPreference(Guid.Parse("e6f34f71-09dc-4f31-a295-cf9ea4f43601"), platformUserId, NotificationChannel.Push, baseTime),
                BuildPreference(Guid.Parse("6dca5ca7-cdb4-43e6-af3f-7ece37bfec4f"), platformUserId, NotificationChannel.SMS, baseTime),
                BuildPreference(Guid.Parse("32a97d01-bf0f-4d6f-9f42-61c8f547f8aa"), platformUserId, NotificationChannel.Email, baseTime));
        return modelBuilder;
    }

    private static object BuildPreference(Guid id, Guid userId, NotificationChannel channel, DateTimeOffset atUtc)
    {
        return new
        {
            Id = id,
            UserId = userId,
            NotificationChannel = channel,
            Active = true,
            CreatedAtUtc = atUtc,
            UpdatedAtUtc = atUtc,
            CreatedBy = "SYSTEM",
            UpdatedBy = "SYSTEM"
        };
    }
}
