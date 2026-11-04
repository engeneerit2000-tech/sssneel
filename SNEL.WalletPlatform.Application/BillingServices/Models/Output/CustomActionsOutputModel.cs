using System.Reflection.Emit;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record CustomActionsOutputModel(
    string Code,
    string Name,    
    IReadOnlyCollection<CustomActionsParamsOutputModel>? Params);

public sealed record CustomActionsParamsOutputModel(
    string Label,
    string Code,
    string Type,
    object Default
    );
