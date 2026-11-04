using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class UserEntityConfiguration : TrackedEntityConfiguration<UserEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.IamRef)
            .HasMaxLength(128);

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SecondName)
            .HasMaxLength(100);

        builder.Property(x => x.ThirdName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.GenderId).IsRequired();
        builder.HasOne(x => x.Gender)
            .WithMany()
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MobileNo)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.NationalNo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany(x => x.NotificationPreferences)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.PartnerId);

        builder.HasIndex(x => x.PartnerId);

        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.IamRef).IsUnique();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.MobileNo);
        builder.HasIndex(x => x.NationalNo);
        builder.HasIndex(x => x.Username);
    }
}
