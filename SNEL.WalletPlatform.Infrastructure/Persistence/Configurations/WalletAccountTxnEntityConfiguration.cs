// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using SNEL.WalletPlatform.Domain.Wallets;
// using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;
//
// namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;
//
// public sealed class WalletAccountTxnEntityConfiguration : IEntityTypeConfiguration<WalletAccountTxnEntity>
// {
//     public void Configure(EntityTypeBuilder<WalletAccountTxnEntity> builder)
//     {
//         builder.ToTable("WalletAccountTxn");
//
//         builder.HasKey(x => x.Id);
//
//         builder.Property(x => x.WalletAccountId).IsRequired();
//         builder.HasOne(x => x.WalletAccount)
//             .WithMany()
//             .HasForeignKey(x => x.WalletAccountId)
//             .OnDelete(DeleteBehavior.Cascade);
//
//         builder.Property(x => x.Type)
//             .HasConversion<int>()
//             .IsRequired();
//
//         builder.Property(x => x.Debit)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.Credit)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.Balance)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.SourceType).HasMaxLength(50);
//         builder.Property(x => x.SourceRef).HasMaxLength(256);
//
//         builder.ConfigureAuditProperties();
//     }
// }
