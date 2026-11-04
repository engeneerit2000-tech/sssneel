using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Models.Output;

public sealed record ManagedUserOutputModel(
    Guid Id,
    string IamRef,
    string Username,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    int? GenderId,
    string? MobileNo,
    string Email,
    string NationalNo,
    string Language,
    UserStatus Status,
    string Role,
    long? PartnerId,
    int? PartnerTypeId,
    Guid? CustomerId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
