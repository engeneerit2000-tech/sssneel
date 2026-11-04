namespace SNEL.WalletPlatform.Application.Interfaces.IAM;

public sealed record CreateIdentityUserRequest(
    string Username,
    string Password,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled = true); 

public sealed record UpdateIdentityUserRequest(
    string Id,
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled = true);

public sealed record IdentityUserModel(string Id, string Username, string? Email);
