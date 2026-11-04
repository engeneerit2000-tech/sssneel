using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AccountEntityConfiguration : TrackedEntityConfiguration<AccountEntity, Guid>
{
    public override  void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.AccountTypeId).IsRequired();
        builder.HasOne(x => x.AccountType)
            .WithMany()
            .HasForeignKey(x => x.AccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PartnerId)
            .IsRequired();

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        builder.Property(x => x.Order).IsRequired();

        builder.HasOne(x => x.Currency)
            .WithMany(x=>x.Accounts)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Balance)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsConcurrencyToken()
            .IsRequired();
    }
}

