using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class BillerConfigRepository(ApplicationDbContext context)
    : EntityRepository<BillerConfigEntity, long>(context), IBillerConfigRepository
{
    public Task UpsertAsync(
        string billerCode,
        string name,
        string billerName,
        string value,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillerCode = billerCode.Trim();
        var normalizedName = name.Trim();
        var normalizedBillerName = billerName.Trim();
        var normalizedValue = value.Trim();
        var now = DateTimeOffset.UtcNow;
        var actor = context.GetCurrentActor();

        return context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "BillerConfigs"
            (
                "BillerCode",
                "Name",
                "BillerName",
                "Value",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "CreatedBy",
                "UpdatedBy"
            )
            VALUES
            (
                {normalizedBillerCode},
                {normalizedName},
                {normalizedBillerName},
                {normalizedValue},
                {now},
                {now},
                {actor},
                {actor}
            )
            ON CONFLICT ("BillerCode", "Name")
            DO UPDATE SET
                "BillerName" = EXCLUDED."BillerName",
                "Value" = EXCLUDED."Value",
                "UpdatedAtUtc" = EXCLUDED."UpdatedAtUtc",
                "UpdatedBy" = EXCLUDED."UpdatedBy"
            """,
            cancellationToken);
    }

    public Task<BillerConfigEntity?> GetByBillerCodeAndNameAsync(
        string billerCode,
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillerCode = billerCode.Trim().ToLower();
        var normalizedName = name.Trim().ToLower();

        return context.BillerConfigs
            .FirstOrDefaultAsync(
                x => x.BillerCode.ToLower() == normalizedBillerCode &&
                     x.Name.ToLower() == normalizedName,
                cancellationToken);
    }

    public async Task<IReadOnlyList<BillerConfigEntity>> GetByBillerCodeAsync(
        string billerCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillerCode = billerCode.Trim().ToLower();

        return await context.BillerConfigs
            .Where(x => x.BillerCode.ToLower() == normalizedBillerCode)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BillerConfigEntity>> GetByBillerCodeForUpdateAsync(
        string billerCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillerCode = billerCode.Trim();

        return await context.BillerConfigs
            .FromSqlInterpolated($"""
                SELECT *
                FROM "BillerConfigs"
                WHERE "BillerCode" = {normalizedBillerCode}
                FOR UPDATE
                """)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
