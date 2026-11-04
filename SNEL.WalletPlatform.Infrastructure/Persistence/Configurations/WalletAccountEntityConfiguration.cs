// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using SNEL.WalletPlatform.Domain.Wallets;
// using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;
//
// namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;
//
// public sealed class WalletAccountEntityConfiguration : IEntityTypeConfiguration<WalletAccountEntity>
// {
//     public void Configure(EntityTypeBuilder<WalletAccountEntity> builder)
//     {
//         builder.ToTable("WalletAccounts");
//
//         builder.HasKey(x => x.Id);
//
//         builder.Property(x => x.WalletId).IsRequired();
//         builder.HasOne(x => x.Wallet)
//             .WithMany()
//             .HasForeignKey(x => x.WalletId)
//             .OnDelete(DeleteBehavior.Cascade);
//
//         builder.Property(x => x.CurrencyId).IsRequired();
//         builder.HasOne(x => x.Currency)
//             .WithMany()
//             .HasForeignKey(x => x.CurrencyId)
//             .OnDelete(DeleteBehavior.Restrict);
//
//         builder.Property(x => x.Name)
//             .HasMaxLength(200)
//             .IsRequired();
//
//         builder.Property(x => x.Status)
//             .HasConversion<int>()
//             .IsRequired();
//
//         builder.Property(x => x.Balance)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.BlockedBalance)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.Version)
//             .IsConcurrencyToken()
//             .IsRequired();
//
//         builder.ConfigureAuditProperties();
//     }
// }
