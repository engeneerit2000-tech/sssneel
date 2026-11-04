// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using SNEL.WalletPlatform.Domain.Wallets;
// using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;
//
// namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;
//
// public sealed class WalletEntityConfiguration : IEntityTypeConfiguration<WalletEntity>
// {
//     public void Configure(EntityTypeBuilder<WalletEntity> builder)
//     {
//         builder.ToTable("Wallets");
//
//         builder.HasKey(x => x.Id);
//
//         builder.Property(x => x.CustomerId).IsRequired();
//         builder.HasOne(x => x.Customer)
//             .WithMany()
//             .HasForeignKey(x => x.CustomerId)
//             .OnDelete(DeleteBehavior.Restrict);
//
//         builder.Property(x => x.WalletNo)
//             .HasMaxLength(50)
//             .IsRequired();
//         builder.HasIndex(x => x.WalletNo).IsUnique();
//
//         builder.Property(x => x.Name)
//             .HasMaxLength(200)
//             .IsRequired();
//
//         builder.Property(x => x.Status)
//             .HasConversion<int>()
//             .IsRequired();
//
//         builder.ConfigureAuditProperties();
//     }
// }
