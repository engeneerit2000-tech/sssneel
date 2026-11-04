// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using SNEL.WalletPlatform.Domain.Customers;
// using SNEL.WalletPlatform.Domain.Seeds;
// using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;
//
// namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;
//
// public sealed class SeedMigrationHistoryEntityConfiguration : IEntityTypeConfiguration<SeedMigrationHistoryEntity>
// {
//     public void Configure(EntityTypeBuilder<SeedMigrationHistoryEntity> builder)
//     {
//         builder.ToTable("SeedMigrationHistories");
//
//         builder.HasKey(x => x.Id);
//         builder.Property(x => x.Version).IsRequired();
//         builder.Property(x => x.Name).IsRequired();
//         builder.Property(x => x.Group).IsRequired();
//         builder.Property(x => x.AppliedOnUtc).IsRequired();
//
//         builder.ConfigureAuditProperties();
//         builder.HasIndex(x => new { x.Group, x.Version })
//             .IsUnique();
//
//         
//     }
// }
