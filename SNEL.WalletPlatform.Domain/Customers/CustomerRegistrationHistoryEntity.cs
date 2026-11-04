using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Domain.Customers;

public sealed class CustomerRegistrationHistoryEntity : TrackedEntity<Guid>
{
    private CustomerRegistrationHistoryEntity() { }

    private CustomerRegistrationHistoryEntity(
        CustomerStatus status,
        Guid? employeeId,
        DateTimeOffset statusTimestamp,
        string? statusNotes)
    {
        Status = status;
        EmployeeId = employeeId;
        StatusTimestamp = statusTimestamp;
        StatusNotes = statusNotes;
    }

    public Guid CustomerId { get; private set; }
    public CustomerEntity Customer { get; private set; } = null!;
    public CustomerStatus Status { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public UserEntity? Employee { get; private set; }
    public DateTimeOffset StatusTimestamp { get; private set; }
    public string? StatusNotes { get; private set; }

    public static CustomerRegistrationHistoryEntity Create(
        CustomerStatus status,
        Guid? employeeId = null,
        DateTimeOffset? statusTimestamp = null,
        string? statusNotes = null)
    {
        return new CustomerRegistrationHistoryEntity(
            status,
            employeeId,
            statusTimestamp ?? DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(statusNotes) ? null : statusNotes.Trim());
    }
}
