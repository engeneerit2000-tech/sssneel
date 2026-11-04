namespace SNEL.WalletPlatform.Application.Users.Interfaces;

public interface IUserRoleResolver
{
    Task<string> ResolveManagedRoleAsync(string? iamRef);
}
