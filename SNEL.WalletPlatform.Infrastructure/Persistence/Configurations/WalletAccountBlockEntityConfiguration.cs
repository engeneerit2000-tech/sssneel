// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using SNEL.WalletPlatform.Domain.Wallets;
// using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;
//
// namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;
//
// public sealed class WalletAccountBlockEntityConfiguration : TrackedEntityConfiguration<WalletAccountBlockEntity>
// {
//     public void Configure(EntityTypeBuilder<WalletAccountBlockEntity> builder)
//     {
//         builder.ToTable("WalletAccountBlocks");
//
//         builder.HasKey(x => x.Id);
//
//         builder.Property(x => x.WalletAccountId).IsRequired();
//         builder.HasOne(x => x.WalletAccount)
//             .WithMany()
//             .HasForeignKey(x => x.WalletAccountId)
//             .OnDelete(DeleteBehavior.Cascade);
//
//         builder.Property(x => x.Amount)
//             .HasPrecision(18, 4)
//             .IsRequired();
//
//         builder.Property(x => x.Status)
//             .HasConversion<int>()
//             .IsRequired();
//
//         builder.Property(x => x.SourceType).HasMaxLength(50);
//         builder.Property(x => x.SourceRef).HasMaxLength(256);
//
//         builder.ConfigureAuditProperties();
//     }
// }
