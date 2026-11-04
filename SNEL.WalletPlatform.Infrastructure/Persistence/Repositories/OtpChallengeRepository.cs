using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Domain.Auth;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class OtpChallengeRepository(ApplicationDbContext context)
    : EntityRepository<OtpChallengeEntity, Guid>(context), IOtpChallengeRepository
{
    public Task<OtpChallengeEntity?> GetPendingOtpAsync(string phoneNumber, OtpPurpose purpose)
    {
        return context.OtpChallenges.Where(o => o.MobileNo == phoneNumber && o.Purpose == purpose && o.Status == OtpStatus.Pending)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync();
    }
}
