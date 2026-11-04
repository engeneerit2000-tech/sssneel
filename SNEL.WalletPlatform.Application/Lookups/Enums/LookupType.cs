namespace SNEL.WalletPlatform.Application.Lookups.Enums;

/// <summary>
/// Identifies a translatable lookup table. Used by the generic lookup-translation
/// endpoints to dispatch read / upsert / delete operations against the correct
/// <c>*TranslationEntity</c> table.
/// </summary>
public enum LookupType
{
    Country = 1,
    Region = 2,
    City = 3,
    Area = 4,
    Center = 5,
    Channel = 6,
    Currency = 7,
    Gender = 8,
    PartnerType = 9,
    ContactType = 10,
    BillingServiceType = 11,
    AccountType = 12,
    JournalType = 13,
    JournalLineType = 14,
    BillingCategory = 15,
    FavoriteType = 16,
    PaymentCategory = 17,
    CompletionMode = 18,
    ConfirmationMode = 19,
    ConfirmationType = 20,
    PaymentInitMode = 21,
    PaymentMethodPurpose = 22
}
