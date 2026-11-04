using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Application.Users.Interfaces;

namespace SNEL.WalletPlatform.Application.Users.Services;

public sealed class UserRoleResolver(IIdentityUserManager identityUserManager) : IUserRoleResolver
{
    public async Task<string> ResolveManagedRoleAsync(string? iamRef)
    {
        if (string.IsNullOrWhiteSpace(iamRef))
        {
            return string.Empty;
        }

        var roles = await identityUserManager.GetRealmRolesAsync(iamRef);
        var managed = roles.FirstOrDefault(x => IdentityRoleConstants.ManageableRoles.Contains(x, StringComparer.OrdinalIgnoreCase));
        return managed ?? string.Empty;
    }
}
