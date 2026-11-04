namespace SNEL.WalletPlatform.Application.Common.Security;

public static class IdentityRoleConstants
{
    public const string PlatformAdmin = "PLATFORM_ADMIN";
    public const string BillerAdmin = "BILLER_ADMIN";
    public const string PspAdmin = "PSP_ADMIN";
    public const string CustomerAdmin = "CUSTOMER_ADMIN";
    public const string CustomerUser = "CUSTOMER";
    public const string CustomerUserTitleCase = "Customer";
    public const string WalletOwner = "WALLET_OWNER";
    public const string Customer = $"{CustomerAdmin},{CustomerUser},{CustomerUserTitleCase}";
    public const string Admin = PlatformAdmin;

    public static readonly string[] ManageableRoles =
    [
        PlatformAdmin,
        BillerAdmin,
        PspAdmin,
        CustomerAdmin,
        CustomerUser,
        CustomerUserTitleCase
    ];
    public static string[] PortalAdminRoles =
    [
        PlatformAdmin,
        BillerAdmin,
        PspAdmin
    ];
}
