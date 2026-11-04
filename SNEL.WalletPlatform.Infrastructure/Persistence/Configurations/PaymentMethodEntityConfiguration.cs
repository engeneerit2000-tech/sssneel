using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodEntityConfiguration : TrackedEntityConfiguration<PaymentMethodEntity,long>
{
    public override void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.PaymentCategoryId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentCategory)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.PaymentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PaymentProviderId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentProvider)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.PaymentProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PaymentInitModeId)
            .IsRequired();
        
        builder.HasOne(x => x.PaymentInitMode)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.PaymentInitModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ConfirmationModeId)
            .IsRequired();
        
        builder.HasOne(x => x.ConfirmationMode)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.ConfirmationModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CompletionModeId)
            .IsRequired();

        builder.HasOne(x => x.CompletionMode)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.CompletionModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PaymentMethodPurposeId)
            .IsRequired();

        builder.HasOne(x => x.PaymentMethodPurpose)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.PaymentMethodPurposeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ConfirmationTypeId)
            .IsRequired();

        builder.HasOne(x => x.ConfirmationType)
            .WithMany(x => x.PaymentMethods)
            .HasForeignKey(x => x.ConfirmationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Config)
            .HasColumnType("jsonb");

        builder.Property(x => x.Active)
            .IsRequired();

       
    }
}
