using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Auth.Models.Output;

public sealed record CompleteRegistrationOutputModel(
    Guid CustomerId,
    Guid UserId,
    string Username,
    CustomerStatus Status);
