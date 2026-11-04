namespace SNEL.WalletPlatform.Application.Users.Models.Internal;

public sealed record ActorScope(
    bool IsPlatformAdmin,
    bool IsBillerAdmin,
    bool IsPspAdmin,
    long? ScopePartnerId);
