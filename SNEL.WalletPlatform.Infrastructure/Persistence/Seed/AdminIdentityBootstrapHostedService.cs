using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public sealed class AdminIdentityBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<AdminIdentityBootstrapHostedService> logger) : BackgroundService
{
    private sealed record AdminBootstrap(
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string NationalNo,
        long PartnerId,
        string RoleName,
        string DefaultPassword,
        string MobileNo);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var identityUserManager = scope.ServiceProvider.GetRequiredService<IIdentityUserManager>();

            var admins = new[]
            {
                new AdminBootstrap("platform.admin", "platform.admin@snel.local", "Platform", "Admin", "PLATFORM_ADMIN", 1L, IdentityRoleConstants.PlatformAdmin, "PlatformAdmin#2026", "+243900000001")
            };

            foreach (var admin in admins)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    var localUser = await dbContext.Users
                        .FirstOrDefaultAsync(x => x.Username == admin.Username, stoppingToken);

                    if (localUser is null)
                    {
                        var partnerExists = await dbContext.Partners.AnyAsync(x => x.Id == admin.PartnerId, stoppingToken);
                        if (!partnerExists)
                        {
                            logger.LogWarning("Skipping admin bootstrap for {Username}: partner {PartnerId} does not exist.", admin.Username, admin.PartnerId);
                            continue;
                        }

                        localUser = UserEntity.Create(
                            username: admin.Username,
                            firstName: admin.FirstName,
                            secondName: null,
                            thirdName: null,
                            lastName: admin.LastName,
                            genderId: 1,
                            mobileNo: admin.MobileNo,
                            email: admin.Email,
                            nationalNo: admin.NationalNo,
                            language: "en",
                            notificationChannels: [NotificationChannel.Push, NotificationChannel.SMS, NotificationChannel.Email],
                            iamRef: null);
                        localUser.PartnerId = admin.PartnerId;
                        dbContext.Users.Add(localUser);
                    }

                    // Prefer existing Keycloak users (common after DB reset): resolve by username, then email, then create.
                    var identityUser = await identityUserManager.FindByUsernameAsync(admin.Username);
                    if (identityUser is null && !string.IsNullOrWhiteSpace(localUser.Email))
                    {
                        identityUser = await identityUserManager.FindByEmailAsync(localUser.Email);
                    }

                    if (identityUser is null)
                    {
                        identityUser = await identityUserManager.CreateUserAsync(new CreateIdentityUserRequest(
                            admin.Username,
                            admin.DefaultPassword,
                            localUser.Email,
                            localUser.FirstName,
                            localUser.LastName,
                            localUser.Status == UserStatus.Active));
                    }
                    else
                    {
                        logger.LogInformation(
                            "Linking local user {Username} to existing Keycloak account {KeycloakUserId} (no new Keycloak user created).",
                            admin.Username,
                            identityUser.Id);
                    }

                    if (!string.Equals(localUser.IamRef, identityUser.Id, StringComparison.Ordinal))
                    {
                        localUser.SetIamRef(identityUser.Id);
                    }

                    // Persist user bootstrap even if role assignment fails (e.g. missing realm role).
                    await dbContext.SaveChangesAsync(stoppingToken);

                    try
                    {
                        await identityUserManager.ReplaceRealmRolesAsync(identityUser.Id, [admin.RoleName]);
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.LogWarning(ex, "Could not assign realm role {RoleName} for {Username}.", admin.RoleName, admin.Username);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Admin bootstrap failed for {Username}.", admin.Username);
                }
            }
            logger.LogInformation("Admin identity bootstrap completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin identity bootstrap failed.");
        }
    }
}
