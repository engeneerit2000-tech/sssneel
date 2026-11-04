using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CustomerRegistrationHistoryEntityConfiguration
    : TrackedEntityConfiguration<CustomerRegistrationHistoryEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<CustomerRegistrationHistoryEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.EmployeeId);

        builder.Property(x => x.StatusTimestamp)
            .IsRequired();

        builder.Property(x => x.StatusNotes)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.StatusTimestamp);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
