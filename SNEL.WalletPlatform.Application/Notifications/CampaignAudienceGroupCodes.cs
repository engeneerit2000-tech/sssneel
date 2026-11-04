namespace SNEL.WalletPlatform.Application.Notifications;

public static class CampaignAudienceGroupCodes
{
    public const string AllCustomers = "ALL_CUSTOMERS";
    public const string AllPspAdmins = "ALL_PSP_ADMINS";
    public const string AllBillerAdmins = "ALL_BILLER_ADMINS";

    /// <summary>
    /// Queryable audience group. Criteria are hard-coded for now and will be expanded later.
    /// Current rule: active customers with more than one subscription.
    /// </summary>
    public const string Queryable = "QUERYABLE";
}
