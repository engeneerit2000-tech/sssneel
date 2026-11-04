using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Auth;

namespace SNEL.WalletPlatform.Application.Auth.Interfaces;

public interface IOtpChallengeRepository : IEntityRepository<OtpChallengeEntity, Guid>
{
    Task<OtpChallengeEntity?> GetPendingOtpAsync(string phoneNumber, OtpPurpose purpose);
}
