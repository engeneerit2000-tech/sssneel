using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Users.Models.Internal;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Application.Users.Interfaces;

public interface IUserPartnerResolver
{
    Task<Result<PartnerEntity>> ResolveTargetPartnerAsync(
        ActorScope scope,
        long requestedPartnerId,
        CancellationToken cancellationToken = default);
}
