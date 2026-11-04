using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed record CustomerOutputModel(
    Guid Id,
    Guid UserId,
    long PartnerId,
    string Username,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    int? GenderId,
    string MobileNo,
    string NationalNo,
    string Email,
    int? AreaId,
    string? Address,
    string Language,
    IReadOnlyCollection<int> NotificationChannels,
    UserStatus UserStatus,
    CustomerStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string CreatedBy,
    string UpdatedBy);
